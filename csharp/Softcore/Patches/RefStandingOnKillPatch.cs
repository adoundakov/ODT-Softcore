using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Traders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.InRaid;
using Softcore.Config;

namespace Softcore.Patches;

/// <summary>
/// Raises Ref standing for every PMC killed in a raid, scaled by the victim's level. Enabled by
/// <see cref="Plugin"/> when refChanges.standingOnKill is on.
/// <para>
/// Postfix on <see cref="LocationLifecycleService.EndLocalRaidAsync"/>, not a prefix: the original
/// overwrites every trader's server standing with the client's post-raid value (HandlePostRaidPmc →
/// ApplyTraderStandingAdjustments), which would wipe an earlier bump. The postfix returns a
/// continuation of the original task so the caller awaits our work too; another mod postfixing the
/// same method composes, each sees the previous one's task.
/// </para>
/// </summary>
[Injectable]
public class RefStandingOnKillPatch : AbstractPatch
{
    private static readonly HashSet<string> PmcRoles = new(StringComparer.OrdinalIgnoreCase) { Sides.PmcUsec, Sides.PmcBear };

    private static ISptLogger<RefStandingOnKillPatch> _logger = default!;
    private static ProfileHelper _profileHelper = default!;
    private static TraderHelper _traderHelper = default!;
    private static SaveServer _saveServer = default!;
    private static StandingOnKillConfig _config = default!;

    public RefStandingOnKillPatch(
        ISptLogger<RefStandingOnKillPatch> logger,
        ProfileHelper profileHelper,
        TraderHelper traderHelper,
        SaveServer saveServer,
        Configuration config)
    {
        _logger = logger;
        _profileHelper = profileHelper;
        _traderHelper = traderHelper;
        _saveServer = saveServer;
        _config = config.RefChanges.StandingOnKill;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocationLifecycleService).GetMethod(nameof(LocationLifecycleService.EndLocalRaidAsync))
            ?? throw new InvalidOperationException("LocationLifecycleService.EndLocalRaidAsync not found");
    }

    [PatchPostfix]
    private static async Task Postfix(Task __result, MongoId sessionId, EndLocalRaidRequestData request)
    {
        await __result;

        try
        {
            await CreditPmcKillsAsync(sessionId, request);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Ref standing on kill failed: {ex.Message}");
        }
    }

    /// <summary>
    /// <c>request.Results.Profile</c> is whichever character ran the raid (PMC or Scav) and its Victims
    /// are per-raid; the standing always goes to the PMC profile, the one traders use.
    /// </summary>
    private static async Task CreditPmcKillsAsync(MongoId sessionId, EndLocalRaidRequestData request)
    {
        if (request.Results == null) return;
        if (_config.RequireSurvival && !request.Results.IsPlayerSurvived()) return;

        var victims = (request.Results.Profile?.Stats?.Eft?.Victims ?? [])
            .Where(victim => victim.Role != null && PmcRoles.Contains(victim.Role))
            .ToList();
        if (victims.Count == 0) return;

        var gained = victims.Sum(victim => RepFor(victim.Level ?? 0));
        if (gained <= 0) return;

        var pmc = _profileHelper.GetFullProfile(sessionId).CharacterData?.PmcData;
        if (pmc?.TradersInfo == null || !pmc.TradersInfo.TryGetValue(Traders.REF, out var refInfo))
        {
            _logger.Warning("[Softcore] Ref not in profile TradersInfo, no standing credited");
            return;
        }

        refInfo.Standing = (refInfo.Standing ?? 0) + gained;
        _traderHelper.LevelUp(Traders.REF, pmc); // recompute Ref LL, as vanilla does for Fence car extracts

        // The original saved before our bump
        await _saveServer.SaveProfileAsync(sessionId);

        _logger.Info($"[Softcore] Ref standing +{gained:0.###} for {victims.Count} PMC kill(s), now {refInfo.Standing:0.###}");
    }

    private static double RepFor(double level) =>
        _config.RepByKillLevel.FirstOrDefault(range => level >= range.MinLevel && level < range.MaxLevel)?.Rep ?? 0;
}
