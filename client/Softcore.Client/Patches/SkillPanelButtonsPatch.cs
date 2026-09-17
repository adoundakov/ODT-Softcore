using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;
using TMPro;

namespace Softcore.Client.Patches;

/// <summary>
/// P9 (plans/8-skill-points-plan.md §4.5): every list-view row gets its +/− buttons after the game has
/// shown it. The thumbs view (<c>SkillIcon</c> only) gets none in v1. <c>_level</c> is only borrowed for its
/// font; it is public on build 40743 but injected by name so either visibility compiles.
/// </summary>
internal class SkillPanelButtonsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.Show));

    [PatchPostfix]
    private static void Postfix(SkillPanel __instance, Skill skill, TextMeshProUGUI ____level)
    {
        if (!SkillPointsClient.Enabled) return;

        var buttons = __instance.GetComponent<SkillPointButtons>() ?? __instance.gameObject.AddComponent<SkillPointButtons>();
        buttons.Bind(skill, ____level);
    }
}
