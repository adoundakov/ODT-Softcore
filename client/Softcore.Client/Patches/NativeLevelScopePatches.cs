using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;

namespace Softcore.Client.Patches;

// N1–N7 (plans/8-skill-points-plan.md §4.5): the seven members that turn skill XP into progress read
// `Level` and must keep seeing the natural one, so the XP curve, fatigue and the first-levels bonus run
// at the skill's own pace under an allocation. Each pair brackets the call with the native scope; the
// depth counter makes the nesting (`BaseProgress` → `LevelProgress`, `OnTrigger` → `SetCurrent` →
// `LevelChanged` → `UpdateRules`) harmless. One class per target because PatchManager only looks at a
// patch class's own declared static methods.

/// <c>Skill.LevelExp</c>: XP needed for the current level.
internal class NativeLevelExpPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.PropertyGetter(typeof(Skill), nameof(Skill.LevelExp));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>Skill.CalculateExpOnFirstLevels</c>: the cheaper levels below 9.
internal class NativeCalculateExpOnFirstLevelsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(Skill), nameof(Skill.CalculateExpOnFirstLevels));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>Skill.BaseProgress</c>: the progress bar's base fill.
internal class NativeBaseProgressPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.PropertyGetter(typeof(Skill), nameof(Skill.BaseProgress));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>Skill.ProgressValue</c>: the "+N" earned-this-session text.
internal class NativeProgressValuePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.PropertyGetter(typeof(Skill), nameof(Skill.ProgressValue));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>Skill.CalculateRealEarnedExpForLobby</c>: Gekos' <c>method_4</c>.
internal class NativeCalculateRealEarnedExpForLobbyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(Skill), nameof(Skill.CalculateRealEarnedExpForLobby));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>Skill.OnTrigger</c>: every XP grant; also covers the level-up compare in <c>SetCurrent</c>.
internal class NativeOnTriggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(Skill), nameof(Skill.OnTrigger));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}

/// <c>BaseSkill.LevelProgress</c>: fraction into the current level (progress bar fill).
internal class NativeLevelProgressPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.PropertyGetter(typeof(BaseSkill), nameof(BaseSkill.LevelProgress));
    [PatchPrefix] private static void Prefix() => SkillLevels.EnterNative();
    [PatchPostfix] private static void Postfix() => SkillLevels.ExitNative();
}
