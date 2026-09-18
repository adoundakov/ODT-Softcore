using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// Talks to the server's skill point routes and pushes the answer into <see cref="SkillLevels"/> and the
/// live skills. The server owns the rules; this only renders and asks. Requests run on the thread pool
/// and their results are applied on the main thread from <see cref="Update"/>, so a fetch from a UI
/// callback never stalls a frame. One request in flight at a time; a click while one is pending is
/// dropped, the state that comes back re-renders the buttons anyway.
/// </summary>
internal static class SkillPointsClient
{
    private const string StateRoute = "/softcore/skillpoints/state";
    private const string AllocateRoute = "/softcore/skillpoints/allocate";

    /// <summary>Last answer from the server; null until the first successful fetch.</summary>
    public static SkillPointsState State { get; private set; }

    /// <summary>Feature usable: the server answered, has it enabled and speaks our version.</summary>
    public static bool Enabled => State != null && State.Enabled && State.Version == SkillPointsState.SupportedVersion;

    /// <summary>Raised on the main thread after <see cref="State"/> changed; the UI patches re-render from it.</summary>
    public static event Action StateChanged;

    private static readonly ConcurrentQueue<Action> MainThread = new ConcurrentQueue<Action>();
    private static bool _inFlight; // main thread only
    private static bool _versionWarned;
    private static bool _firstStateLogged;

    /// <summary>Fetches the state. Safe to call often; coalesces with a pending request.</summary>
    public static void Refresh()
    {
        Send(() => RequestHandler.GetJson(StateRoute), "state", disableOnFailure: true);
    }

    /// <summary>Asks for ±1 on a skill. The server's answer (or refusal) arrives through <see cref="StateChanged"/>.</summary>
    public static void Allocate(ESkillId skill, int delta)
    {
        if (!Enabled) return;

        var body = JsonConvert.SerializeObject(new { skill = skill.ToString(), delta });
        Send(() => RequestHandler.PostJson(AllocateRoute, body), $"{skill} {delta:+#;-#;0}", disableOnFailure: false);
    }

    /// <summary>Drains finished requests. Called from the plugin's <c>Update</c>.</summary>
    public static void Update()
    {
        while (MainThread.TryDequeue(out var apply)) apply();
    }

    private static void Send(Func<string> request, string what, bool disableOnFailure)
    {
        if (_inFlight) return;
        _inFlight = true;

        Task.Run(() =>
        {
            SkillPointsState state = null;
            Exception error = null;
            try
            {
                state = Parse(request());
            }
            catch (Exception ex)
            {
                error = ex;
            }

            MainThread.Enqueue(() =>
            {
                _inFlight = false;
                if (error != null)
                {
                    Plugin.Log.LogWarning($"Skill points: {what} request failed: {error.Message}");
                    if (disableOnFailure) Apply(null);
                    return;
                }

                if (state.Error != null) Plugin.Log.LogInfo($"Skill points: {what} refused: {state.Error}");
                else if (what != "state") Plugin.Log.LogInfo($"Skill points: {what}, {state.Available}/{state.Total} available");
                Apply(state);
            });
        });
    }

    private static SkillPointsState Parse(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new InvalidOperationException("empty response — is the Softcore server mod installed?");
        }

        return JsonConvert.DeserializeObject<SkillPointsState>(json)
               ?? throw new InvalidOperationException("response is not a skill points state");
    }

    private static void Apply(SkillPointsState state)
    {
        if (state != null && state.Enabled && state.Version != SkillPointsState.SupportedVersion && !_versionWarned)
        {
            _versionWarned = true;
            Plugin.Log.LogError($"Skill points: server state version {state.Version}, this client understands {SkillPointsState.SupportedVersion} — update both halves; feature off");
        }

        State = state;

        if (!_firstStateLogged)
        {
            _firstStateLogged = true;
            Plugin.Log.LogInfo(Enabled
                ? $"Skill points: {state.Available}/{state.Total} available, {state.Allocated?.Count ?? 0} skill(s) allocated"
                : "Skill points: off (server has it disabled, is missing the mod, or answered a different version)");
        }

        var allocated = new Dictionary<ESkillId, int>();
        if (Enabled)
        {
            foreach (var pair in state.Allocated ?? new Dictionary<string, int>())
            {
                if (Enum.TryParse<ESkillId>(pair.Key, out var id)) allocated[id] = pair.Value;
                else Plugin.Log.LogWarning($"Skill points: server allocated points to unknown skill '{pair.Key}', ignored");
            }
        }

        RecomputeSkills(SkillLevels.SetAllocated(allocated));
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Re-runs the buff rules and the UI bindings of every skill whose allocation changed. Not
    /// <c>LevelChanged()</c>: that would grant AnySkillUp XP for a level the player did not earn.
    /// </summary>
    private static void RecomputeSkills(List<ESkillId> changed)
    {
        if (changed.Count == 0) return;

        var skills = SkillLevels.PlayerSkills();
        if (skills == null) return; // before login: the SkillManager is built later and reads the allocations on its own

        foreach (var id in changed)
        {
            if (!skills.TryGetSkill(id, out var skill)) continue;
            skill.UpdateRules();
            skill.SkillLevelChanged.Invoke();
            skill.SkillExperienceChanged.Invoke();
        }
    }
}
