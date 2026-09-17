using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using Softcore.Config;

namespace Softcore.Services;

/// <summary>
/// The pure rules from plans/8-skill-points-plan.md §2, separated from profile loading and saving so
/// they can be driven from a test harness with a bare <see cref="PmcData"/>.
/// </summary>
public static class SkillPointsRules
{
    /// <summary>Elite level; a skill cannot go past it, allocated or not.</summary>
    public const int MaxLevel = 51;

    private const double ProgressPerLevel = 100;

    /// <summary>Total points = floor(PMC level × pointsPerLevel). Retroactive by construction.</summary>
    public static int Total(PmcData pmc, SkillPointsConfig config) =>
        (int)Math.Floor((pmc.Info?.Level ?? 1) * config.PointsPerLevel);

    /// <summary>Server-side level: Progress is already clamped to 5100 on set. A skill missing from Common is level 0.</summary>
    public static int NativeLevel(PmcData pmc, SkillTypes skill)
    {
        var progress = pmc.Skills?.Common?.FirstOrDefault(s => s.Id == skill)?.Progress ?? 0;
        return Math.Min(MaxLevel, (int)Math.Floor(progress / ProgressPerLevel));
    }

    /// <summary>
    /// Refunds whatever the natural level has grown into: <c>alloc := MAX − native</c> wherever
    /// <c>native + alloc &gt; MAX</c>. Unknown skill names (a renamed enum member) are refunded whole.
    /// Returns the number of points refunded.
    /// </summary>
    public static int Normalise(PmcData pmc, SkillPointsData data, out List<string> unknownSkills)
    {
        var refunded = 0;
        unknownSkills = [];
        foreach (var (key, allocated) in data.Allocated.ToList())
        {
            if (!Enum.TryParse<SkillTypes>(key, out var skill))
            {
                unknownSkills.Add(key);
                data.Allocated.Remove(key);
                refunded += allocated;
                continue;
            }

            var room = Math.Max(0, MaxLevel - NativeLevel(pmc, skill));
            if (allocated <= room) continue;

            refunded += allocated - room;
            if (room == 0) data.Allocated.Remove(key);
            else data.Allocated[key] = room;
        }

        return refunded;
    }

    /// <summary>Applies <paramref name="delta"/> (+1 or −1) to <paramref name="skill"/>. Returns the refusal, or null when applied.</summary>
    public static string? Apply(PmcData pmc, SkillPointsData data, SkillPointsConfig config, SkillTypes skill, int delta)
    {
        var key = skill.ToString();
        var allocated = data.Allocated.GetValueOrDefault(key);
        var available = Total(pmc, config) - data.Allocated.Values.Sum();

        switch (delta)
        {
            case 1:
                if (available < 1) return "No skill points available";
                if (NativeLevel(pmc, skill) + allocated >= MaxLevel) return $"{skill} is already at the maximum level";
                data.Allocated[key] = allocated + 1;
                return null;
            case -1:
                if (!config.EnableDeallocation) return "Deallocation is disabled";
                if (allocated < 1) return $"No points allocated to {skill}";
                if (allocated == 1) data.Allocated.Remove(key);
                else data.Allocated[key] = allocated - 1;
                return null;
            default:
                return $"Delta must be +1 or -1, got {delta}";
        }
    }

    public static SkillPointsState Build(PmcData pmc, SkillPointsData data, SkillPointsConfig config)
    {
        var total = Total(pmc, config);
        return new SkillPointsState
        {
            Version = data.Version,
            Enabled = true,
            Total = total,
            // Negative only after pointsPerLevel was lowered under spent points; reads as 0 until levels catch up
            Available = Math.Max(0, total - data.Allocated.Values.Sum()),
            DeallocationEnabled = config.EnableDeallocation,
            Allocated = new Dictionary<string, int>(data.Allocated),
        };
    }
}
