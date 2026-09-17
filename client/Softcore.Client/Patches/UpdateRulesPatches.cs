using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;

namespace Softcore.Client.Patches;

/// <summary>
/// U1 (plans/8-skill-points-plan.md §4.5): <c>Skill.UpdateRules</c> — the buff recompute — always sees the
/// effective level, even when a natural level-up runs it from inside <c>OnTrigger</c>'s native scope.
/// </summary>
internal class UpdateRulesEffectivePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(Skill), nameof(Skill.UpdateRules));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterEffective();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitEffective();
}

/// <summary>
/// U2: <c>UpdateRules</c> ends with <c>if (Level &gt; 50) Unsubscribe()</c>, which stops the skill's XP for
/// good. With an allocation pushing the effective level to 51 that would freeze the natural level and
/// the refund could never happen, so the unsubscribe is skipped unless the <em>natural</em> level is
/// elite. <c>SkillManager.StartClientMode</c>/<c>Dispose</c> unsubscribe from outside <c>UpdateRules</c> and
/// keep working.
/// </summary>
internal class UnsubscribeOnlyWhenNativelyElitePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(BaseSkill), nameof(BaseSkill.Unsubscribe));

    [PatchPrefix]
    private static bool Prefix(BaseSkill __instance)
    {
        if (!SkillLevels.InUpdateRules) return true;
        return SkillLevels.NativeLevel(__instance) > Skill.MAX_LEVEL;
    }
}
