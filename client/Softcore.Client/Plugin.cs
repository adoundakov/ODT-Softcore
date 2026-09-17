using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using Softcore.Client.SkillPoints;

namespace Softcore.Client;

/// <summary>
/// Client half of Softcore: renders what the server decides (skill points, later). Optional — the server
/// mod works without it. Patches are <see cref="ModulePatch"/> subclasses under Patches/; PatchManager
/// picks up every one in this assembly, so a config flag gates a patch's behaviour, not its registration.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.dukewendigo.softcore.client";
    public const string Name = "Softcore Client";
    // Keep in step with <Version> in Softcore.Client.csproj and the server's ModMetadata
    public const string Version = "4.1.0";

    /// <summary>
    /// EFT build the patches were written against (plans/8a-dnspy-guide.md). A mismatch only warns:
    /// a patch whose target moved fails on its own with the member name in the log.
    /// </summary>
    public const int EftBuild = 40743;

    internal static ManualLogSource Log { get; private set; }

    private static PatchManager _patchManager;

    private void Awake()
    {
        Log = Logger;

        var eftBuild = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
        if (eftBuild != EftBuild)
        {
            Log.LogWarning($"Built against EFT {EftBuild}, running on {eftBuild} — patches may not bind");
        }

        try
        {
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();
        }
        catch (Exception ex)
        {
            Log.LogError($"Enabling patches failed, plugin inactive: {ex}");
            return;
        }

        Log.LogInfo($"{Name} {Version} loaded (EFT {eftBuild})");
    }

    /// <summary>
    /// First state fetch. The session id travels in a header RequestHandler fills from the launcher
    /// args, so this works before login; the profile's SkillManager is built later and reads the
    /// allocations through the Level patch. Re-fetched on every skills screen open.
    /// </summary>
    private void Start()
    {
        if (_patchManager == null) return;

        SkillPointsClient.Refresh();
        var state = SkillPointsClient.State;
        Log.LogInfo(SkillPointsClient.Enabled
            ? $"Skill points: {state.Available}/{state.Total} available, {state.Allocated.Count} skill(s) allocated"
            : "Skill points: off (server has it disabled, is missing the mod, or answered a different version)");
    }
}
