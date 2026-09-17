using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;

namespace Softcore.Client.Patches;

/// <summary>
/// L1 (plans/8-skill-points-plan.md §4.5): <c>BaseSkill.Level</c> answers natural + allocated, capped at
/// the elite level, everywhere except inside the XP methods (<see cref="SkillLevels.ExposeNative"/>).
/// <c>SummaryLevel</c> and <c>IsEliteLevel</c> read this getter, so they need nothing. Only the
/// player's <see cref="Skill"/>s get allocations: <see cref="Mastering"/> shares the base class, and bots
/// and the Scav have their own SkillManagers with the same skill ids.
/// </summary>
internal class EffectiveLevelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.PropertyGetter(typeof(BaseSkill), nameof(BaseSkill.Level));

    [PatchPostfix]
    private static void Postfix(BaseSkill __instance, ref int __result)
    {
        if (SkillLevels.ExposeNative || !(__instance is Skill skill)) return;

        var allocated = SkillLevels.AllocatedFor(skill.Id);
        if (allocated > 0 && SkillLevels.IsPlayer(skill.SkillManager))
        {
            __result = System.Math.Min(__result + allocated, Skill.MAX_LEVEL + 1);
        }
    }
}
