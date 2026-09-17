using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;
using Softcore.Config;

namespace Softcore.Services;

/// <summary>
/// What the client renders. One shape for both routes; <see cref="Error"/> is set when an allocation
/// was refused and the rest is the unchanged state.
/// </summary>
public record SkillPointsState
{
    /// <summary>Wire version, matches <see cref="SkillPointsData.Version"/>. The client refuses a mismatch.</summary>
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    /// <summary>False when the feature is off in config: the client then shows nothing.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("available")]
    public int Available { get; init; }

    [JsonPropertyName("maxLevel")]
    public int MaxLevel { get; init; } = SkillPointsRules.MaxLevel;

    [JsonPropertyName("deallocationEnabled")]
    public bool DeallocationEnabled { get; init; }

    /// <summary>Allocated levels per skill, keyed by skill name (<see cref="SkillTypes"/>, same names as the client's ESkillId).</summary>
    [JsonPropertyName("allocated")]
    public Dictionary<string, int> Allocated { get; init; } = new();

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

/// <summary>
/// Owns the skill point state (plans/8-skill-points-plan.md §2, rules in <see cref="SkillPointsRules"/>).
/// The client only renders and asks. Every read normalises: an allocation that the skill's natural
/// level has grown into is refunded to the pool and the profile saved, so a restart never re-charges
/// a refund.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SkillPointsService(
    ISptLogger<SkillPointsService> logger,
    ProfileHelper profileHelper,
    SaveServer saveServer,
    Configuration configuration)
{
    private readonly ISptLogger<SkillPointsService> _logger = logger;
    private readonly ProfileHelper _profileHelper = profileHelper;
    private readonly SaveServer _saveServer = saveServer;
    private readonly SkillPointsConfig _config = configuration.SkillChanges.SkillPoints;
    private readonly bool _enabled = configuration.SkillChanges.Enabled && configuration.SkillChanges.SkillPoints.Enabled;

    public async Task<SkillPointsState> GetStateAsync(MongoId sessionId)
    {
        if (!_enabled)
        {
            return Disabled();
        }

        var (profile, pmc, data) = Load(sessionId);
        if (Normalise(pmc, data) > 0)
        {
            await SaveAsync(sessionId, profile, data);
        }

        return SkillPointsRules.Build(pmc, data.SkillPoints, _config);
    }

    /// <summary>Applies <paramref name="delta"/> (+1 or −1) to <paramref name="skill"/>; a refusal comes back as <see cref="SkillPointsState.Error"/>.</summary>
    public async Task<SkillPointsState> AllocateAsync(MongoId sessionId, SkillTypes skill, int delta)
    {
        if (!_enabled)
        {
            return Disabled() with { Error = "Skill points are disabled" };
        }

        var (profile, pmc, data) = Load(sessionId);
        var refunded = Normalise(pmc, data);

        var error = SkillPointsRules.Apply(pmc, data.SkillPoints, _config, skill, delta);
        if (error == null || refunded > 0)
        {
            await SaveAsync(sessionId, profile, data);
        }

        var state = SkillPointsRules.Build(pmc, data.SkillPoints, _config) with { Error = error };
        if (error == null)
        {
            _logger.Info($"[Softcore] Skill points: {skill} {delta:+#;-#;0}, {state.Available}/{state.Total} available");
        }

        return state;
    }

    private (SptProfile profile, PmcData pmc, SoftcoreProfileData data) Load(MongoId sessionId)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);
        var pmc = profile.CharacterData?.PmcData ?? throw new InvalidOperationException("Profile has no PMC data");
        return (profile, pmc, SoftcoreProfileData.Get(profile));
    }

    private int Normalise(PmcData pmc, SoftcoreProfileData data)
    {
        var refunded = SkillPointsRules.Normalise(pmc, data.SkillPoints, out var unknownSkills);
        foreach (var skill in unknownSkills)
        {
            _logger.Warning($"[Softcore] Skill points: unknown skill '{skill}' in profile, its points were refunded");
        }

        if (refunded > 0)
        {
            _logger.Info($"[Softcore] Skill points: {refunded} refunded by natural levelling");
        }

        return refunded;
    }

    private async Task SaveAsync(MongoId sessionId, SptProfile profile, SoftcoreProfileData data)
    {
        SoftcoreProfileData.Set(profile, data);
        await _saveServer.SaveProfileAsync(sessionId);
    }

    private static SkillPointsState Disabled() => new() { Enabled = false, Total = 0, Available = 0 };
}
