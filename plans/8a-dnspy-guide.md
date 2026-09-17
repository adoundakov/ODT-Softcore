# dnSpy guide for plan 8 — what to do on the Windows box

**Written:** 2026-09-17
**Purpose:** answer §6.2 of `8-skill-points-plan.md` and hand the raw material back so the client patches can be
written on the Mac. Nothing here needs the game running under a debugger — static decompilation covers items 1–4, and
one throwaway logging patch covers item 5 (Unity hierarchy names never appear in a decompiler).
**Status (2026-09-17):** §1–§4 done — build **40743**, full `Export to Project` dump at
`~/Documents/git/dnspy-dump/Assembly-CSharp/` on the Mac (option A), findings recorded in plan 8 §6.3. §1 resolved: `Managed\Assembly-CSharp.dll` is deobfuscated in place (the client project builds against it). §5 probe
run on 2026-09-17, output in `8a-probe-output.log`, layout facts in §6. Nothing open.

The wiki's `spt-wiki/modding/tutorials/debug_dnSpy.md` is about *live* debugging (development `UnityPlayer.dll`,
attach to process, breakpoints). Skip it; it is slower to set up and gives nothing we need. Its Chapter 3 note about
`BepInEx\DumpedAssemblies` is the only relevant line, and it is covered below.

---

## 0. Install

- **dnSpyEx** — <https://github.com/dnSpyEx/dnSpy/releases>. Take `dnSpy-net-win64.zip`, unzip anywhere, run
  `dnSpy.exe`. No install. (The original `dnSpy/dnSpy` is abandoned; dnSpyEx is the maintained fork the wiki links.)
- Optional but handy for step 5: **UnityExplorer** BepInEx 5 Mono build —
  <https://github.com/sinai-dev/UnityExplorer/releases> (`UnityExplorer.BepInEx5.Mono.zip`). Drop it in
  `BepInEx\plugins\`, press F7 in game, and you can click through the live scene hierarchy. Remove it when done; it
  spams the log.

## 1. Which `Assembly-CSharp.dll` to open

1. `File → Open` (Ctrl+O) → `<SPT>\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll`.
2. Ctrl+Shift+K, type `EFT.Skill`, Enter.
   - If `EFT.Skill` shows up as a class with members like `Level`, `SummaryLevel`, `OnTrigger`: **this is the file**.
     Note this in the plan (§4.1 says to prefer `Managed\`).
   - If you only see `SkillClass` / `GClassNNNN`-style names: the real names are applied by SPT's prepatcher at
     launch. Set `DumpAssemblies = true` in `BepInEx\config\BepInEx.cfg`, launch the game once to the main menu,
     quit, and open `<SPT>\BepInEx\DumpedAssemblies\EscapeFromTarkov\Assembly-CSharp.dll` instead. Update §4.1 to
     use `$(SptDir)\BepInEx\DumpedAssemblies\EscapeFromTarkov\` like Skills Extended does.

Either way, write down the game build (`EscapeFromTarkov.exe` → Properties → Details → File version, last number,
e.g. `40743`) so the findings are tied to a build.

## 2. dnSpy in five keystrokes

| Want | Do |
|---|---|
| Find a type or member | Ctrl+Shift+K, type the name. Set the dropdown to "All of the above". |
| Read a class | Click it in the tree; the right pane is the C# decompilation. `View → Options → Decompiler` → language C#. |
| Who reads/calls this member? | Right-click the member (in the tree or in code) → **Analyze**. The Analyzer pane at the bottom opens with **Used By** (callers/readers) and **Uses** (callees). Expand `Used By`. This is the whole game for §6.2 item 1. |
| Jump to definition | Ctrl+click, or F12 on a symbol in the code pane. |
| Copy code | Click in the code pane, Ctrl+A, Ctrl+C. |
| Dump everything | `File → Export to Project` — see §3, option A. |

Obfuscated leftovers look like `method_3`, `smethod_1`, `int_0`, `gclass1234_0`. dnSpy shows them as-is; there is
nothing to "resolve", they simply have no name. The mapping doc in the wiki only covers *types*.

## 3. Deliverable — pick one

### Option A (preferred): export the whole assembly as C# once

`File → Export to Project`, target folder **outside any repo**, e.g. `C:\eft-src\<build>\`. Untick "Create .sln"
if offered; leave "Decompile everything". Expect several hundred MB and 10–20 minutes. Then zip the `EFT\` and
`EFT.UI\` subfolders (or the lot) and put them on the Mac, e.g. `~/Documents/git/eft-decompiled/<build>/`. Add that
path to `.claude/settings.local.json` → `additionalDirectories` and every remaining §6.2 question becomes a `grep`
on the Mac: readers of `SummaryLevel`, the buff-recompute method, `SkillIcon`'s fields, the lot.

This is the one that lets the client patches be written and re-checked without another round trip.

### Option B: copy the handful of classes we need

If exporting is impractical, paste each of these into its own file under `plans/dnspy/<build>/` (Ctrl+A, Ctrl+C in
the code pane):

| File | dnSpy target |
|---|---|
| `Skill.cs` | `EFT.Skill` — whole class |
| `BaseSkill.cs` | `EFT.BaseSkill` — whole class (ex `AbstractSkillClass`; `Level` may be declared here, not on `Skill`) |
| `SkillManager.cs` | `EFT.SkillManager` — whole class. It is big; if it is thousands of lines, the nested `Buff`, `FloatBuff`, `SkillAction` classes plus every method that mentions `Level`, `SummaryLevel`, `Buffs` is enough |
| `SkillIcon.cs` | `EFT.UI.SkillIcon` — whole class |
| `SkillPanel.cs` | `EFT.UI.SkillPanel` — whole class |
| `SkillLevelPanel.cs` | `EFT.UI.SkillLevelPanel` — whole class |
| `SkillsAndMasteringScreen.cs` | `EFT.UI.SkillsAndMasteringScreen` — whole class |
| `UsedBy-Level.txt` | Analyze → Used By on the `Level` getter (of `Skill` or `BaseSkill`, wherever it is declared). Right-click the Used By node → Copy, or type the list by hand: `Type.Member` per line |
| `UsedBy-SummaryLevel.txt` | same, for `SummaryLevel` getter |
| `UsedBy-IsEliteLevel.txt` | same, for `IsEliteLevel` |

With option A none of this is needed, the files are already there.

## 4. What to look at while you are in there (so nothing obvious is missed)

Mapped to `8-skill-points-plan.md` §6.2. No need to write conclusions; the files from §3 carry them. These are just
the places where a surprise would change the plan, worth a glance on the spot:

1. **`Level` vs `SummaryLevel`** — in the `Used By` list for each: does `SkillLevelPanel.SetLevel` read
   `SummaryLevel`? Does the method that iterates `Buffs` (Gekos' `method_3`) read `SummaryLevel` or `Level`? Do
   `LevelExp` / `CalculateExpOnFirstLevels` / `BaseProgress` / `ProgressValue` / `LevelProgress` / `OnTrigger` read
   `Level`? If the split is UI+buffs → `SummaryLevel`, XP → `Level`, the client is one patch (§10 Q4).
2. **Clamping** — does the `Level` getter contain a `Math.Min(..., MaxLevel)` / `51`? What is the constant called?
3. **Obfuscated names** — inside `Skill`, note which `method_N` iterates the buff array and calls something like
   `Update`/`Recalculate` on each; inside `SkillPanel`, which `method_N` refreshes the panel (called from `Show`).
4. **`SkillIcon` fields** — the `[SerializeField]`/private fields: is there still a `_levelPanel` of type
   `SkillLevelPanel` and a field of type `Skill` (Gekos called it `skillClass`)?
5. **`SkillsAndMasteringScreen.Show`** — its parameter list, and any `[SerializeField]` fields of TMP text type
   that could be the "current XP" label.

## 5. Unity hierarchy names — runtime probe (optional now)

The dump made most of this unnecessary: every UI object the patches need is a `[SerializeField]` field with a name
(`SkillIcon._levelPanel`, `SkillPanel._level`, `SkillsAndMasteringScreen._playerExperiencePanel._currentExperience`),
so no `transform.Find("...")` path is needed. What a probe still gives is the *layout* — where the buttons can be
parented so they land next to the level number without overlapping — and that is a nice-to-have for §7 step 6, not a
blocker for step 5.

### 5a. Probe patch — in the repo's client project

The probe is committed as `client/Softcore.Client/Patches/Probes.cs`, wrapped in `#if PROBE`, so a normal build (Debug
or Release) never compiles it. `Plugin.cs` enables every `ModulePatch` in the assembly through `PatchManager`, so
nothing else needs registering.

1. The repo must sit at `<SPT>\Development\ODT-Softcore`, or pass `-p:SptDir=<SPT root>\` to every `dotnet build`.
2. From `client/Softcore.Client`: `dotnet build -p:Probe=true`. The post-build step copies `Softcore.Client.dll` to
   `<SPT>\BepInEx\plugins\Softcore\`. A "not found" error from the `CheckSptDir` target means `SptDir` is wrong;
   200 "type not found" errors mean `Managed\Assembly-CSharp.dll` is not the deobfuscated one — add
   `-p:AssemblyCSharpDir=<SPT>\BepInEx\DumpedAssemblies\EscapeFromTarkov\` (see §1) and note that in plan 8 §4.1.
3. Launch SPT, log in, open the **Skills** tab in the character screen once (list view), quit.
4. Send `<SPT>\BepInEx\LogOutput.log` — the blocks start with `[Softcore probe]`.
5. Rebuild without the flag (`dotnet build`) to install a probe-free DLL again.

`Plugin.Log` is the plugin's `ManualLogSource`. Fields are read through `AccessTools.Field` so the probe compiles
whether the game's `[SerializeField]` fields are public (as in the dump) or private. The file, for reference:

```csharp
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
```

`__0` is Harmony's "first argument" injection, so the patch binds whatever the parameter is called. Both `Show`
methods have a single overload in build 40743 (`SkillPanel.Show(Skill, IHealthController)`,
`SkillsAndMasteringScreen.Show(Profile, InventoryController, IHealthController)`); if a later build adds one,
`AccessTools.Method` throws at `EnablePatches()` and `Plugin.Awake` logs it — switch to
`AccessTools.FirstMethod(typeof(X), m => m.Name == "Show")`.

### 5b. UnityExplorer (no build needed, nothing to hand back)

Install it (§0), open the Skills tab in game, press F7 → Object Explorer → Scene Explorer, search `SkillPanel`,
expand one. Good for eyeballing; for a record, screenshot the expanded tree for one `SkillPanel` and for
`PlayerExperiencePanel` under `SkillsAndMasteringScreen`.

## 6. Hand-back checklist

- [x] Game build number — **40743**. Which `Assembly-CSharp.dll` was exported (§1): `Managed\` — the client project
  compiles `EFT.UI.SkillPanel` / `SkillsAndMasteringScreen` straight from `Managed\Assembly-CSharp.dll` with the
  default `AssemblyCSharpDir`, so it is the in-place deobfuscated one; §4.1 stays on `Managed\`.
- [x] Option A folder on the Mac (`~/Documents/git/dnspy-dump`) + path added to `additionalDirectories`
- [x] `BepInEx\LogOutput.log` after the probe run (§5a) — saved as `plans/8a-probe-output.log` (2026-09-17, EFT 40743).
  Layout facts for §7 step 6:
  - `SkillPanel` row is `Detailed Skill Panel(Clone)` 824×100, and **`SkillIcon` sits on the same GameObject as
    `SkillPanel`** (`_skillIcon == this`); the icon's visuals are the child `Skill Icon` (96×96 at bottom-left),
    whose `Level Panel` (`SkillLevelPanel`, 25×25 at the icon's bottom-left corner) holds the small level number.
  - The big level text `_level` is the child `Level` (anchored to the top edge, x=113, y=-70, stretches to the
    right edge); `Fill Bar` runs along the bottom (y=34, stretching from x=112). Free space for +/− buttons is the
    right end of the `Level` row, or the row's `Buffs` corner (top-right, `HorizontalLayoutGroup`).
  - `_playerExperiencePanel` = `Progress Panel` (full width, 159 tall): `Current Text` (300×27,
    `CustomTextMeshProUGUI`, left-anchored at x=344, y=-19) is the "current:" XP label to clone for the points label;
    `Remaining Text` mirrors it on the right (x=-386). Both sit above the `Bar` (`TwoValueBar`, y=-60).
  - Field visibility: every `[SerializeField]` on both classes is **public** (`_level`, `_skillIcon`,
    `_playerExperiencePanel`, …) except `_tooltip`, `_skill`, `_healthController`, `_skillMasterTabGroup`.
- [x] §4 surprises — recorded in plan 8 §6.3

With those, §6.2 gets filled in and §7 steps 5–6 can be written on the Mac and only *built* on Windows.
