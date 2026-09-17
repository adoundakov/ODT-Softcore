using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies the skill globals. Runs at Preload + 1; the client reads globals on login, so a single edit
/// here is persistent.
/// </summary>
[Injectable]
public class SkillChangesChanger(
    ISptLogger<SkillChangesChanger> logger,
    GlobalTable globalTable)
{
    private readonly ISptLogger<SkillChangesChanger> _logger = logger;
    private readonly GlobalTable _globalTable = globalTable;

    public void Apply(SkillChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Skill changes disabled");
            return;
        }

        if (config.Fatigue.Enabled)
        {
            try
            {
                DoFatigue(config.Fatigue);
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Skill fatigue failed: {ex.Message}");
            }
        }
    }

    private void DoFatigue(SkillFatigueConfig config)
    {
        var globals = _globalTable.Configuration;
        globals.SkillFreshEffectiveness = config.SkillFreshEffectiveness;
        globals.SkillFreshPoints = config.SkillFreshPoints;
        globals.SkillPointsBeforeFatigue = config.SkillPointsBeforeFatigue;
        globals.SkillMinEffectiveness = config.SkillMinEffectiveness;

        _logger.Success($"[Softcore] Skill fatigue: fresh x{config.SkillFreshEffectiveness} for {config.SkillFreshPoints} pt, " +
                        $"fatigue after {config.SkillPointsBeforeFatigue} pt down to x{config.SkillMinEffectiveness}");
    }
}
