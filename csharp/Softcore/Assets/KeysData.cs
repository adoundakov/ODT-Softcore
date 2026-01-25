namespace Softcore.Assets;

/// <summary>
/// Quest keys and marked keys for pacifist flea market.
/// NOTE: Item IDs need to be populated with SPT 4.0 values in Phase 4.
/// TypeScript source uses ItemTpl enums which need to be mapped to actual IDs.
/// </summary>
public static class KeysData
{
    /// <summary>
    /// Quest-only keys available on flea
    /// TODO Phase 4: Populate with actual SPT 4.0 item IDs from ItemTpl enum
    /// </summary>
    public static readonly HashSet<string> QuestKeys = new()
    {
        // Factory
        // "KEY_FACTORY_EMERGENCY_EXIT",
        // "KEYCARD_TERRAGROUP_STORAGE_ROOM",

        // Customs
        // "KEY_DORM_OVERSEER",
        // "KEY_DORM_ROOM_114",
        // "KEY_DORM_ROOM_214",
        // "KEY_DORM_ROOM_220",
        // "KEY_DORM_ROOM_303",
        "5d80cbd886f77470855c26c2",  // Dorm 314 marked (known ID)

        // Woods
        // "KEY_ZB014",

        // Shoreline
        // "KEY_COTTAGE_BACK_DOOR",
        // "KEY_HEALTH_RESORT_EAST_WING_ROOM_306",
        // ... more keys

        // Interchange, Labs, Reserve, Lighthouse, Streets, Ground Zero
        // ... (60+ keys total - to be added in Phase 4)
    };

    /// <summary>
    /// High-value marked keys
    /// TODO Phase 4: Populate with actual SPT 4.0 item IDs
    /// </summary>
    public static readonly HashSet<string> MarkedKeys = new()
    {
        "5d80cbd886f77470855c26c2",  // Dorm 314 marked (known ID)
        // "KEY_RBBK_MARKED",
        // "KEY_RBVO_MARKED",
        // "KEY_SHARED_BEDROOM_MARKED",
        // "KEY_RBPKPM_MARKED",
        // "KEY_MYSTERIOUS_ROOM_MARKED",
        // "KEY_ABANDONED_FACTORY_MARKED",
    };
}
