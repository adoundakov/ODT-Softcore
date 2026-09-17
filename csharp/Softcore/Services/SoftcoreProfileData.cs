using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace Softcore.Services;

/// <summary>
/// Softcore's own state in the SPT profile, stored as the <c>"softcore"</c> key of the top-level
/// <see cref="SptProfile"/>'s extension data. The top-level record never leaves the server (the client
/// only sees and echoes back PmcData), and it saves, backs up and wipes with the profile.
/// </summary>
public class SoftcoreProfileData
{
    public const string Key = "softcore";

    [JsonPropertyName("skillPoints")]
    public SkillPointsData SkillPoints { get; set; } = new();

    /// <summary>
    /// After a profile load the value is a <see cref="JsonElement"/> (the extension dictionary is
    /// <c>&lt;string, object&gt;</c>); after <see cref="Set"/> it is the live object. Missing means a
    /// profile that has never used the feature.
    /// </summary>
    public static SoftcoreProfileData Get(SptProfile profile)
    {
        if (!profile.GetExtensionData().TryGetValue(Key, out var value))
        {
            return new SoftcoreProfileData();
        }

        return value switch
        {
            SoftcoreProfileData data => data,
            JsonElement element => element.ToObject<SoftcoreProfileData>() ?? new SoftcoreProfileData(),
            _ => throw new InvalidOperationException($"Unexpected type in profile extension data '{Key}': {value.GetType().FullName}"),
        };
    }

    /// <summary>Replaces the stored value; the next profile save writes it. AddToExtensionData throws on an existing key.</summary>
    public static void Set(SptProfile profile, SoftcoreProfileData data)
    {
        profile.RemoveFromExtensionData(Key);
        profile.AddToExtensionData(Key, data);
    }
}

public class SkillPointsData
{
    /// <summary>Bumped on any incompatible change to this shape.</summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>Allocated levels per skill, keyed by <see cref="SPTarkov.Server.Core.Models.Enums.SkillTypes"/> name.</summary>
    [JsonPropertyName("allocated")]
    public Dictionary<string, int> Allocated { get; set; } = new();
}
