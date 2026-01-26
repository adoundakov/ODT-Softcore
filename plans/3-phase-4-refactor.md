# Phase 4: Refactor C# to Use SPT Constants

**Goal:** Replace all hardcoded MongoDB ID strings in C# files with proper SPT enum constants (ItemTpl, BaseClasses, QuestTpl).

## Background

The TypeScript source uses ~653 unique MongoDB IDs scattered across asset files. SPT 4.0 provides auto-generated enums for most of these:

- **ItemTpl** - All item template IDs (4,355 constants)
- **BaseClasses** - Item category/type IDs (119 constants)
- **QuestTpl** - Quest IDs (544 constants)

We've validated the TypeScript IDs and created a mapping file that maps each MongoDB ID to its corresponding C# constant.

## Validation Results

| Type | Count | Notes |
|------|-------|-------|
| ItemTpl | 423 | Items, ammo, containers, weapons, etc. |
| BaseClasses | 111 | Item categories (WEAPON, AMMO, BACKPACK, etc.) |
| QuestTpl | 3 | Quest unlock requirements for crafts |
| NOT_FOUND | 116 | Handbook category IDs (no enum - keep as strings) |
| **TOTAL** | **653** | **82% successfully mapped** |

## Required Files

### Input: ID Mapping (for Claude)
**Location:** `scripts/id-mapping.json`

This file provides a lookup table from MongoDB ID to C# constant:
```json
{
  "5449016a4bdc2d6f028b456f": "ItemTpl.MONEY_ROUBLES",
  "5696686a4bdc2da3298b456a": "ItemTpl.MONEY_DOLLARS",
  "5485a8684bdc2da71d8b4567": "BaseClasses.AMMO",
  ...
}
```

**Usage:** Read this file at the start of refactoring to get all mappings in context.

### Reference: CSV (for humans)
**Location:** `scripts/id-mapping.csv`

Human-readable version sorted by constant name. Use for manual lookups.

## Files to Refactor

All files in `csharp/Softcore/Assets/`:

1. **`FleaMarketData.cs`** (~300 IDs)
   - FleaBarterRequestWhitelist (BaseClasses)
   - ActualBaseClasses (BaseClasses)
   - RequestWhitelist (ItemTpl, with custom prices)
   - FleaListingsWhitelistHandbook (Handbook IDs - **keep as strings**)
   - BSGblacklist (ItemTpl)

2. **`CraftingData.cs`** (~353 IDs)
   - CraftingAdjustments (ItemTpl for endProduct, requirements, tools)
   - ContainerRecipes (ItemTpl)
   - StimRecipes (ItemTpl)
   - Quest unlock requirements (QuestTpl - only 3 instances)

## Refactoring Strategy

### Step 1: Read Mapping File
```
Read scripts/id-mapping.json once at the start
Parse into memory for fast lookups during refactoring
```

### Step 2: Process Each File

For each C# file in `csharp/Softcore/Assets/`:

1. **Read the file**
2. **Find all MongoDB IDs** (24-character hex strings)
3. **Replace with constants:**
   ```csharp
   // Before:
   "5449016a4bdc2d6f028b456f"

   // After:
   ItemTpl.MONEY_ROUBLES
   ```

4. **Handle NOT_FOUND IDs:**
   - These are handbook category IDs (no enum available)
   - Keep as strings with a comment:
   ```csharp
   // Handbook category IDs (no enum - from handbook.json)
   "5b47574386f77428ca22b2ed" // Keep as-is
   ```
   - If a comment with a human-readable name is present, make sure to preserve it
   - If no pretty name exists, look in `src/assets/fleamarket.ts` to find it

5. **Add using statements** at the top of each file:
   ```csharp
   using SPTarkov.Server.Core.Models.Enums;
   using SPTarkov.Server.Core.Models.Common;
   ```

### Step 3: Special Cases

#### Dictionary/Collection Syntax
```csharp
// Before:
new Dictionary<string, int>
{
    ["5449016a4bdc2d6f028b456f"] = 1000,
}

// After:
new Dictionary<MongoId, int>
{
    [ItemTpl.MONEY_ROUBLES] = 1000,
}
```

**Note:** Change key type from `string` to `MongoId` when keys are replaced with constants.

#### Array Initialization
```csharp
// Before:
new[] { "5485a8684bdc2da71d8b4567", "543be5cb4bdc2deb348b4568" }

// After:
new[] { BaseClasses.AMMO, BaseClasses.AMMO_BOX }
```

#### Quest Requirements (rare - only 3 instances)
```csharp
// Before:
questId: "63966fccac6f8f3c677b9d89"

// After:
questId: QuestTpl.SNATCH
```

### Step 4: Verify Build
```bash
cd csharp
dotnet build
```

Fix any type mismatches (e.g., string vs MongoId).

## Example Refactor

### Before:
```csharp
public static class FleaMarketData
{
    public static readonly string[] FleaBarterRequestWhitelist = new[]
    {
        "5485a8684bdc2da71d8b4567", // Ammo
        "543be5cb4bdc2deb348b4568", // Ammo box
        "5448e53e4bdc2d60728b4567", // Backpack
    };

    public static readonly Dictionary<string, int> RequestWhitelist = new()
    {
        ["5449016a4bdc2d6f028b456f"] = 1000, // Roubles
        ["5696686a4bdc2da3298b456a"] = 100,  // Dollars
    };
}
```

### After:
```csharp
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Common;

public static class FleaMarketData
{
    public static readonly MongoId[] FleaBarterRequestWhitelist = new[]
    {
        BaseClasses.AMMO,
        BaseClasses.AMMO_BOX,
        BaseClasses.BACKPACK,
    };

    public static readonly Dictionary<MongoId, int> RequestWhitelist = new()
    {
        [ItemTpl.MONEY_ROUBLES] = 1000,
        [ItemTpl.MONEY_DOLLARS] = 100,
    };
}
```

## Expected Outcomes

### Benefits
✅ **Type safety** - Compile-time errors for typos/invalid IDs
✅ **Readability** - `ItemTpl.AMMO_762X39_BP` vs `"59e0d99486f7744a32234762"`
✅ **Autocomplete** - IDE suggestions for available items
✅ **Refactoring** - Easy to rename/track usage
✅ **Validation** - SPT 4.0 compatibility confirmed (82% of IDs found)

### Known Limitations
- **116 handbook category IDs** remain as strings (SPT doesn't provide enum)
- These are used only in `FleaListingsWhitelistHandbook` array
- Acceptable to keep as hardcoded strings with comments

## Testing After Refactor

1. **Build succeeds** without errors
2. **No runtime errors** when mod loads
3. **Arrays/dictionaries** have correct type signatures (MongoId vs string)
4. **Flea market behavior** unchanged (barter system still works)
5. **Crafting recipes** unchanged (outputs/requirements correct)

## Success Criteria

- [ ] All 537 mappable IDs replaced with constants
- [ ] 116 handbook IDs kept as strings with comments
- [ ] All C# asset files use proper using statements
- [ ] Project builds without errors
- [ ] No functional regressions in mod behavior

## Notes

- **MongoId has implicit string conversion**, so comparing MongoId to string will work at runtime
- If you see type mismatch errors, check if the collection should be `MongoId` instead of `string`
- The mapping JSON has ~5-6K tokens - read once at the start, not per file
