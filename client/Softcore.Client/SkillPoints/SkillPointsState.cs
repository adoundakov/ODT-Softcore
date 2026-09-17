using System.Collections.Generic;
using Newtonsoft.Json;

namespace Softcore.Client.SkillPoints;

/// <summary>
/// Wire shape of the server's <c>/softcore/skillpoints/*</c> routes (csharp/Softcore/Services/SkillPointsService.cs).
/// <see cref="Error"/> is set when an allocation was refused; the rest is then the unchanged state.
/// </summary>
public class SkillPointsState
{
    /// <summary>Shape version the client understands; the server's must match.</summary>
    public const int SupportedVersion = 1;

    [JsonProperty("version")] public int Version;
    [JsonProperty("enabled")] public bool Enabled;
    [JsonProperty("total")] public int Total;
    [JsonProperty("available")] public int Available;
    [JsonProperty("maxLevel")] public int MaxLevel = 51;
    [JsonProperty("deallocationEnabled")] public bool DeallocationEnabled;
    /// <summary>Allocated levels per skill, keyed by ESkillId name.</summary>
    [JsonProperty("allocated")] public Dictionary<string, int> Allocated = new Dictionary<string, int>();
    [JsonProperty("error")] public string Error;
}
