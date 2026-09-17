using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using SPT.Reflection.Utils;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// The one piece of state the level patches read (plans/8-skill-points-plan.md §4.5). <see cref="Allocated"/>
/// is replaced whole by <see cref="SkillPointsClient"/>; the two depth counters are toggled by the
/// prefix/postfix pairs around the game's XP methods and <c>Skill.UpdateRules</c>. The game touches
/// skills on the main thread only, so plain statics are enough.
/// </summary>
internal static class SkillLevels
{
    public static Dictionary<ESkillId, int> Allocated = new Dictionary<ESkillId, int>();

    private static int _nativeDepth;
    private static int _effectiveDepth;

    // Bots and the Scav have SkillManagers with the same ESkillIds; only the session profile's gets the
    // allocations. The player's manager is cached once matched; every other one is remembered as foreign
    // (weakly, they die with the raid) so the per-frame Level reads of thirty bots stay two lookups.
    private static SkillManager _playerSkills;
    private static readonly ConditionalWeakTable<SkillManager, object> Foreign = new ConditionalWeakTable<SkillManager, object>();
    private static readonly object ForeignMarker = new object();

    /// <summary>
    /// True inside an XP method: <c>BaseSkill.Level</c> then answers the natural level so the per-level XP
    /// curve, fatigue and the "fresh skill" bonus keep working off it. <c>UpdateRules</c> nested inside an
    /// XP-driven level-up wins and sees the effective level again, so buffs never drop to the natural one.
    /// </summary>
    public static bool ExposeNative => _nativeDepth > 0 && _effectiveDepth == 0;

    /// <summary>True while <c>Skill.UpdateRules</c> runs; <c>BaseSkill.Unsubscribe</c> reads it.</summary>
    public static bool InUpdateRules => _effectiveDepth > 0;

    public static void EnterNative() => _nativeDepth++;
    public static void ExitNative() => _nativeDepth = Math.Max(0, _nativeDepth - 1);
    public static void EnterEffective() => _effectiveDepth++;
    public static void ExitEffective() => _effectiveDepth = Math.Max(0, _effectiveDepth - 1);

    public static int AllocatedFor(ESkillId id) => Allocated.TryGetValue(id, out var levels) ? levels : 0;

    /// <summary>The level without allocations, bypassing the patched getter.</summary>
    public static int NativeLevel(BaseSkill skill) => skill.GetLevelForValue(skill.ClampCurrent(skill.Current));

    /// <summary>Whether <paramref name="manager"/> is the session profile's (the PMC's, in the menu and in raid).</summary>
    public static bool IsPlayer(SkillManager manager)
    {
        if (manager == null) return false;
        if (ReferenceEquals(manager, _playerSkills)) return true;
        if (Foreign.TryGetValue(manager, out _)) return false;

        var current = PlayerSkills();
        if (ReferenceEquals(manager, current))
        {
            _playerSkills = manager;
            return true;
        }

        if (current != null) Foreign.GetValue(manager, _ => ForeignMarker); // before login nothing is known yet
        return false;
    }

    public static SkillManager PlayerSkills()
    {
        try
        {
            return ClientAppUtils.GetClientApp()?.Session?.Profile?.Skills;
        }
        catch (Exception)
        {
            return null; // no client app / session yet
        }
    }
}
