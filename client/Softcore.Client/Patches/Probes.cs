// Throwaway diagnostics for plans/8a-dnspy-guide.md §5: logs the live Unity hierarchy of one skill row and of the
// skills screen header once each. Compiled only with `dotnet build -p:Probe=true` (defines PROBE); never in Release.
#if PROBE
using System.Linq;
using System.Reflection;
using System.Text;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Softcore.Client.Patches;

internal static class ProbeDump
{
    public static void Hierarchy(Transform t, StringBuilder sb, string indent = "", int depth = 0, int maxDepth = 6)
    {
        var comps = string.Join(", ", t.GetComponents<Component>().Select(c => c.GetType().Name));
        sb.Append(indent).Append(t.name).Append("  [").Append(comps).Append(']');
        if (t is RectTransform rt)
        {
            sb.Append("  pos=").Append(rt.anchoredPosition).Append(" size=").Append(rt.sizeDelta)
              .Append(" anchors=").Append(rt.anchorMin).Append('-').Append(rt.anchorMax);
        }
        sb.Append(t.gameObject.activeSelf ? "\n" : "  (inactive)\n");
        if (depth >= maxDepth) return;
        for (var i = 0; i < t.childCount; i++)
            Hierarchy(t.GetChild(i), sb, indent + "  ", depth + 1, maxDepth);
    }

    public static void Fields(object o, StringBuilder sb)
    {
        foreach (var f in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            sb.Append("  ").Append(f.IsPublic ? "public " : "private ").Append(f.FieldType.Name).Append(' ').Append(f.Name).Append('\n');
    }

    public static Transform FieldTransform(object o, string field) =>
        (AccessTools.Field(o.GetType(), field)?.GetValue(o) as Component)?.transform;
}

/// One skill row of the list view. Its hierarchy includes the SkillIcon (_skillIcon) and the level text (_level).
internal class SkillPanelProbe : ModulePatch
{
    private static bool _done;

    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.Show));

    [PatchPostfix]
    private static void Postfix(SkillPanel __instance, object __0)
    {
        if (_done) return;
        _done = true;
        var sb = new StringBuilder("[Softcore probe] SkillPanel.Show(").Append(__0?.GetType().FullName).Append(")\n");
        sb.Append("fields:\n");
        ProbeDump.Fields(__instance, sb);
        sb.Append("hierarchy from the SkillPanel's transform:\n");
        ProbeDump.Hierarchy(__instance.transform, sb);
        var icon = ProbeDump.FieldTransform(__instance, "_skillIcon");
        sb.Append("_skillIcon is ").Append(icon == null ? "null" : icon == __instance.transform ? "the same GameObject" : "child " + icon.name).Append('\n');
        Plugin.Log.LogInfo(sb.ToString());
    }
}

/// The screen header: _playerExperiencePanel holds the "current:" XP text the points label will be cloned from.
internal class SkillsScreenProbe : ModulePatch
{
    private static bool _done;

    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(SkillsAndMasteringScreen), nameof(SkillsAndMasteringScreen.Show));

    [PatchPostfix]
    private static void Postfix(SkillsAndMasteringScreen __instance)
    {
        if (_done) return;
        _done = true;
        var sb = new StringBuilder("[Softcore probe] SkillsAndMasteringScreen.Show\nfields:\n");
        ProbeDump.Fields(__instance, sb);
        var xp = ProbeDump.FieldTransform(__instance, "_playerExperiencePanel");
        sb.Append("hierarchy of _playerExperiencePanel (depth 4):\n");
        if (xp != null) ProbeDump.Hierarchy(xp, sb, maxDepth: 4); else sb.Append("  (field not found)\n");
        sb.Append("hierarchy of the screen (depth 2):\n");
        ProbeDump.Hierarchy(__instance.transform, sb, maxDepth: 2);
        Plugin.Log.LogInfo(sb.ToString());
    }
}
#endif
