using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;
using TMPro;

namespace Softcore.Client.Patches;

/// <summary>
/// P10 (plans/8-skill-points-plan.md §4.5): re-fetches the state every time the skills screen opens (so a
/// raid's refunds show without a restart) and puts the "available points" label in the header. Runs
/// before the rows are built, so <see cref="SkillPanelButtonsPatch"/> sees fresh state.
/// </summary>
internal class AvailablePointsLabelPatch : ModulePatch
{
    private static readonly FieldInfo CurrentExperienceField =
        AccessTools.Field(typeof(PlayerExperiencePanel), "_currentExperience");

    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(SkillsAndMasteringScreen), nameof(SkillsAndMasteringScreen.Show));

    [PatchPrefix]
    private static void Prefix()
    {
        SkillPointsClient.Refresh();
    }

    [PatchPostfix]
    private static void Postfix(PlayerExperiencePanel ____playerExperiencePanel)
    {
        if (!SkillPointsClient.Enabled) return;

        if (!(CurrentExperienceField?.GetValue(____playerExperiencePanel) is TMP_Text currentExperience))
        {
            Plugin.Log.LogWarning("Skill points: PlayerExperiencePanel._currentExperience not found, no points label");
            return;
        }

        var label = ____playerExperiencePanel.GetComponent<AvailablePointsLabel>()
                    ?? ____playerExperiencePanel.gameObject.AddComponent<AvailablePointsLabel>();
        label.Bind(currentExperience);
    }
}
