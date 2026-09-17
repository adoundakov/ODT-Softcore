using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// Talks to the server's skill point routes and pushes the answer into <see cref="SkillLevels"/> and the
/// live skills. The server owns the rules; this only renders and asks. Calls are synchronous on the UI
/// thread, like every other SPT client mod's <see cref="RequestHandler"/> use.
/// </summary>
internal static class SkillPointsClient
{
    private const string StateRoute = "/softcore/skillpoints/state";
    private const string AllocateRoute = "/softcore/skillpoints/allocate";

    /// <summary>Last answer from the server; null until the first successful <see cref="Refresh"/>.</summary>
    public static SkillPointsState State { get; private set; }

    /// <summary>Feature usable: the server answered, has it enabled and speaks our version.</summary>
    public static bool Enabled => State != null && State.Enabled && State.Version == SkillPointsState.SupportedVersion;

    /// <summary>Raised after <see cref="State"/> changed; the UI patches re-render from it.</summary>
    public static event Action StateChanged;

    private static bool _versionWarned;

    public static void Refresh()
    {
        try
        {
            Apply(Parse(RequestHandler.GetJson(StateRoute)));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Skill points: state request failed, feature off until the next skills screen: {ex.Message}");
            Apply(null);
        }
    }

    /// <summary>Asks for ±1 on a skill. Returns false and logs when the server refused; the state is replaced either way.</summary>
    public static bool Allocate(ESkillId skill, int delta)
    {
        if (!Enabled) return false;

        try
        {
            var body = JsonConvert.SerializeObject(new { skill = skill.ToString(), delta });
            var state = Parse(RequestHandler.PostJson(AllocateRoute, body));
            Apply(state);
            if (state.Error != null)
            {
                Plugin.Log.LogInfo($"Skill points: {skill} {delta:+#;-#;0} refused: {state.Error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Skill points: allocate request failed: {ex.Message}");
            return false;
        }
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

        var allocated = new Dictionary<ESkillId, int>();
        if (Enabled)
        {
            foreach (var pair in state.Allocated ?? new Dictionary<string, int>())
            {
                if (Enum.TryParse<ESkillId>(pair.Key, out var id)) allocated[id] = pair.Value;
                else Plugin.Log.LogWarning($"Skill points: server allocated points to unknown skill '{pair.Key}', ignored");
            }
        }

        var changed = Diff(SkillLevels.Allocated, allocated);
        SkillLevels.Allocated = allocated;
        RecomputeSkills(changed);
        StateChanged?.Invoke();
    }

    private static List<ESkillId> Diff(Dictionary<ESkillId, int> before, Dictionary<ESkillId, int> after)
    {
        var changed = new List<ESkillId>();
        foreach (var pair in after)
        {
            if (!before.TryGetValue(pair.Key, out var old) || old != pair.Value) changed.Add(pair.Key);
        }

        foreach (var key in before.Keys)
        {
            if (!after.ContainsKey(key)) changed.Add(key);
        }

        return changed;
    }

    /// <summary>
    /// Re-runs the buff rules and the UI bindings of every skill whose allocation changed. Not
    /// <c>LevelChanged()</c>: that would grant AnySkillUp XP for a level the player did not earn.
    /// </summary>
    private static void RecomputeSkills(List<ESkillId> changed)
    {
        if (changed.Count == 0) return;

        var skills = SkillLevels.PlayerSkills();
        if (skills == null) return; // before login: the SkillManager is built later and reads Allocated on its own

        foreach (var id in changed)
        {
            if (!skills.TryGetSkill(id, out var skill)) continue;
            skill.UpdateRules();
            skill.SkillLevelChanged.Invoke();
            skill.SkillExperienceChanged.Invoke();
        }
    }
}
