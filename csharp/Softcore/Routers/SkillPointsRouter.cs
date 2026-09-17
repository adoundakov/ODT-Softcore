using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using Softcore.Services;

namespace Softcore.Routers;

/// <summary>
/// The client's two calls. The session id comes from the request header SPT fills in, so the client
/// sends nothing identifying. Responses are the raw <see cref="SkillPointsState"/> JSON (no SPT
/// envelope), a refusal is an ordinary response with <see cref="SkillPointsState.Error"/> set.
/// Always registered: with the feature disabled the state answers <c>enabled = false</c>, so a client
/// plugin left installed degrades to "no buttons" instead of an HTTP error.
/// </summary>
[Injectable]
public class SkillPointsRouter(JsonUtil jsonUtil, SkillPointsCallbacks callbacks)
    : StaticRouter(jsonUtil, [
        new RouteAction<EmptyRequestData>(
            "/softcore/skillpoints/state",
            async (url, info, sessionId, output, cancellationToken) => await callbacks.GetStateAsync(sessionId)),
        new RouteAction<SkillPointsAllocateRequest>(
            "/softcore/skillpoints/allocate",
            async (url, info, sessionId, output, cancellationToken) => await callbacks.AllocateAsync(sessionId, info)),
    ])
{ }

[Injectable]
public class SkillPointsCallbacks(JsonUtil jsonUtil, SkillPointsService skillPoints)
{
    public async ValueTask<string> GetStateAsync(MongoId sessionId)
    {
        return jsonUtil.Serialize(await skillPoints.GetStateAsync(sessionId))!;
    }

    public async ValueTask<string> AllocateAsync(MongoId sessionId, SkillPointsAllocateRequest request)
    {
        if (!Enum.TryParse<SkillTypes>(request.Skill, ignoreCase: false, out var skill))
        {
            var state = await skillPoints.GetStateAsync(sessionId);
            return jsonUtil.Serialize(state with { Error = $"Unknown skill '{request.Skill}'" })!;
        }

        return jsonUtil.Serialize(await skillPoints.AllocateAsync(sessionId, skill, request.Delta))!;
    }
}

/// <summary>Body of <c>/softcore/skillpoints/allocate</c>.</summary>
public record SkillPointsAllocateRequest : IRequestData
{
    /// <summary><see cref="SkillTypes"/> name, e.g. "Endurance".</summary>
    [JsonPropertyName("skill")]
    public string? Skill { get; init; }

    /// <summary>+1 or −1.</summary>
    [JsonPropertyName("delta")]
    public int Delta { get; init; }
}
