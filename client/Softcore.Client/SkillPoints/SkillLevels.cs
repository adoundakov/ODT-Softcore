using System;
using System.Collections.Generic;
using EFT;
using SPT.Reflection.Utils;
using UnityEngine;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// The one piece of state the level patches read (plans/8-skill-points-plan.md §4.5). Allocations are
/// replaced whole by <see cref="SkillPointsClient"/>; the two depth counters are toggled by the
/// prefix/postfix pairs around the game's XP methods and <c>Skill.UpdateRules</c>. The game touches
/// skills on the main thread only, so plain statics are enough.
/// <para>
/// This is a hot path: <c>BaseSkill.Level</c> is read every frame for every skill of every bot, so nothing
/// here may allocate — no enum-keyed dictionary (Mono boxes the key per lookup), no weak table (Boehm GC
/// pays for every ephemeron on every collection). Both showed up as GC hitches in raids with bots.
/// </para>
/// </summary>
internal static class SkillLevels
{
    // Indexed by (byte)ESkillId; the enum is a byte, so 256 covers it
    private static readonly int[] AllocatedByIndex = new int[256];

    private static int _nativeDepth;
    private static int _effectiveDepth;

    // Bots and the Scav have SkillManagers with the same ESkillIds; only the session profile's gets the
    // allocations. Resolved at most once per frame: the first mismatch in a frame refreshes the cache, every
    // later mismatch in the same frame is a bot by elimination.
    private static SkillManager _playerSkills;
    private static int _resolvedFrame = -1;

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

    public static int AllocatedFor(ESkillId id) => AllocatedByIndex[(byte)id];

    /// <summary>Replaces every allocation. Returns the ids whose value changed.</summary>
    public static List<ESkillId> SetAllocated(Dictionary<ESkillId, int> allocated)
    {
        var changed = new List<ESkillId>();
        for (var i = 0; i < AllocatedByIndex.Length; i++)
        {
            var id = (ESkillId)(byte)i;
            var value = allocated.TryGetValue(id, out var levels) ? levels : 0;
            if (AllocatedByIndex[i] == value) continue;

            AllocatedByIndex[i] = value;
            changed.Add(id);
        }

        return changed;
    }

    /// <summary>The level without allocations, bypassing the patched getter.</summary>
    public static int NativeLevel(BaseSkill skill) => skill.GetLevelForValue(skill.ClampCurrent(skill.Current));

    /// <summary>Whether <paramref name="manager"/> is the session profile's (the PMC's, in the menu and in raid).</summary>
    public static bool IsPlayer(SkillManager manager)
    {
        if (manager == null) return false;
        if (ReferenceEquals(manager, _playerSkills)) return true;

        var frame = Time.frameCount;
        if (frame == _resolvedFrame) return false;

        _resolvedFrame = frame;
        _playerSkills = PlayerSkills();
        return ReferenceEquals(manager, _playerSkills);
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
