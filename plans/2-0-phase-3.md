# Phase 3: Crafting Recipe Changes - Implementation Plan

## Overview

Port crafting rebalance system from TypeScript to C# for SPT 4.0 migration. This includes:
- **98 crafting adjustments** to existing recipes (output counts, requirements)
- **12 new recipes** (container progression, stims)

## Prerequisites

- Phases 0-2 completed (Plugin infrastructure, Config system, Economy changers)
- Current project compiles successfully
- TypeScript source files: `src/assets/productionAdjustments.ts`, `src/assets/recipes.ts`, `src/changers/CraftingChangesChanger.ts`

## Architecture Decision: Action<T> Delegates

**Pattern chosen:** Use `Action<HideoutProduction>` delegates to represent adjustment functions

**Rationale:**
- Direct 1:1 mapping from TypeScript closures: `adjust: (craft) => { craft.count = 2 }`
- Maintains functional programming style of original
- Cleaner than creating 98 separate classes
- Easy to port line-by-line from TypeScript

**Data structure:**
```csharp
public class CraftingAdjustment
{
    public string ItemId { get; init; }
    public Action<HideoutProduction> Apply { get; init; }
}
```

## Files to Create/Modify

### Create: `csharp/Softcore/Assets/CraftingData.cs`
Static data class containing:
1. **CraftingAdjustments.All** - List of 98 adjustments
2. **NewRecipes.All** - List of 12 new recipes

**Structure:**
```csharp
public static class CraftingData
{
    public static class Adjustments
    {
        public static readonly List<CraftingAdjustment> All = new() { ... };
    }

    public static class NewRecipes
    {
        public static readonly List<HideoutProduction> All = new() { ... };
    }
}
```

### Create: `csharp/Softcore/Changers/CraftingChangesChanger.cs`
Orchestrator class with:
- `Apply(CraftingChangesConfig)` - Main entry point
- `DoCraftingRebalance()` - Apply 98 adjustments
- `DoAdditionalCraftingRecipes()` - Add 12 new recipes
- `GetCraftByEndProduct(string)` - Helper to find recipe (excludes CHRISTMAS_TREE)

**Error handling:** Individual try-catch per adjustment for graceful degradation

### Modify: `csharp/Softcore/Plugin.cs`
1. Add `CraftingChangesChanger` to constructor parameters (DI)
2. Call `_craftingChanger.Apply(_config.CraftingChanges)` in `OnLoad()`

## Implementation Steps

### Step 1: Create CraftingData.cs Structure
```csharp
namespace Softcore.Assets;

public class CraftingAdjustment
{
    public string ItemId { get; init; }
    public Action<HideoutProduction> Apply { get; init; }

    public CraftingAdjustment(string itemId, Action<HideoutProduction> apply)
    {
        ItemId = itemId;
        Apply = apply;
    }
}

public static class CraftingData
{
    public static class Adjustments
    {
        public static readonly List<CraftingAdjustment> All = new()
        {
            // Step 3-5 will populate this
        };
    }

    public static class NewRecipes
    {
        public static readonly List<HideoutProduction> All = new()
        {
            // Step 2 will populate this
        };
    }
}
```

**Test:** Verify compilation

### Step 2: Port New Recipes
Port 12 recipes from `src/assets/recipes.ts`:
- alpha, beta, epsilon, gamma, kappa, waist, omega, tau, mechanic_intel, sj6_recipe, etc.

**TypeScript → C# mapping:**
```typescript
// TypeScript (recipes.ts:3-42)
export const alpha: IHideoutProduction = {
    _id: "63da4dbee8fa73e22500001a",
    areaType: 10,
    requirements: [
        { areaType: 10, requiredLevel: 1, type: "Area" },
        { templateId: "567143bf4bdc2d1a0f8b4567", count: 2, isFunctional: false, type: "Item" }
    ],
    productionTime: 5600,
    endProduct: "544a11ac4bdc2d470e8b456a",
    count: 1,
    // ... other properties
}
```

```csharp
// C# (CraftingData.cs)
new HideoutProduction
{
    Id = "63da4dbee8fa73e22500001a", // TODO: Phase 4 - Validate
    AreaType = 10,
    Requirements = new List<ProductionRequirement>
    {
        new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
        new() { TemplateId = "567143bf4bdc2d1a0f8b4567", Count = 2, IsFunctional = false, Type = "Item" }
        // TODO: Phase 4 - Validate item ID
    },
    ProductionTime = 5600,
    EndProduct = "544a11ac4bdc2d470e8b456a", // TODO: Phase 4 - Validate
    Count = 1,
    IsEncoded = false,
    Locked = false,
    NeedFuelForAllProductionTime = true,
    Continuous = false,
    ProductionLimitCount = 0,
    IsCodeProduction = false
}
```

**Property name mappings:** camelCase → PascalCase (_id → Id, endProduct → EndProduct)

**Add to NewRecipes.All:** Populate the list with all 12 recipe instances

**Test:** Verify compilation, count = 12

### Step 3: Port Simple Adjustments
Port adjustments with simple count changes (approx 30-40 adjustments)

**Pattern:**
```typescript
// TypeScript (productionAdjustments.ts:4-9)
{
    id: ItemTpl.BARTER_TOILET_PAPER,
    adjust: (craft: IHideoutProduction) => {
        craft.count = 1
    },
}
```

```csharp
// C# (CraftingData.cs)
new CraftingAdjustment(
    "5bc9b355d4351e6d1509862a", // TODO: Phase 4 - Validate item ID
    craft => craft.Count = 1
)
```

**Examples:**
- Toilet paper: count = 1
- Clin cleaner: count = 4
- Paracord: count = 2
- Bottle of water: count = 16

**Test:** Compilation after each batch of 10

### Step 4: Port Find+Modify Adjustments
Port adjustments that find and modify specific requirements (approx 30-40 adjustments)

**Pattern:**
```typescript
// TypeScript (productionAdjustments.ts:34-42)
{
    id: ItemTpl.BARTER_WATER_FILTER,
    adjust: (craft: IHideoutProduction) => {
        const requirement = craft.requirements.find(r => r.templateId === ItemTpl.BARTER_GAS_MASK_AIR_FILTER)
        if (!requirement) return
        requirement.count = 2
    },
}
```

```csharp
// C# (CraftingData.cs)
new CraftingAdjustment(
    "590c60fc86f77412b13fddcf", // TODO: Phase 4 - Validate
    craft =>
    {
        var requirement = craft.Requirements.FirstOrDefault(r =>
            r.TemplateId == "5c06779c86f77426e00dd782"); // TODO: Phase 4 - Validate
        if (requirement == null) return;
        requirement.Count = 2;
    }
)
```

**Key points:**
- Use `FirstOrDefault()` instead of `find()`
- Null check with early return
- Multiple find operations may be chained

**Test:** Compilation after each batch

### Step 5: Port Complex Adjustments
Port adjustments with complete requirement replacements or loops (approx 20-30 adjustments)

**Pattern A - Bulk loop:**
```typescript
// TypeScript (productionAdjustments.ts:23-32)
{
    id: ItemTpl.BARTER_CORRUGATED_HOSE,
    adjust: (craft: IHideoutProduction) => {
        for (const requirement of craft.requirements) {
            if (requirement.count) {
                requirement.count = 1
            }
        }
        craft.count = 1
    },
}
```

```csharp
// C# (CraftingData.cs)
new CraftingAdjustment(
    "5d1b371186f774253763a656", // TODO: Phase 4 - Validate
    craft =>
    {
        foreach (var requirement in craft.Requirements)
        {
            if (requirement.Count.HasValue)
            {
                requirement.Count = 1;
            }
        }
        craft.Count = 1;
    }
)
```

**Pattern B - Complete replacement:**
```csharp
new CraftingAdjustment(
    "5c1127bdd7f00c44744416e3", // TODO: Phase 4 - Validate
    craft =>
    {
        craft.Requirements = new List<ProductionRequirement>
        {
            new() { AreaType = 11, RequiredLevel = 2, Type = "Area" },
            new() { TemplateId = "5c0e530286f7747fa1419862", Count = 1, IsFunctional = false, Type = "Item" }
            // TODO: Phase 4 - Validate all item IDs
        };
    }
)
```

**Test:** Compilation, verify total count = 98

### Step 6: Create CraftingChangesChanger.cs
```csharp
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

[Injectable]
public class CraftingChangesChanger
{
    private readonly ISptLogger<CraftingChangesChanger> _logger;
    private readonly DatabaseService _databaseService;

    public CraftingChangesChanger(
        ISptLogger<CraftingChangesChanger> logger,
        DatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public void Apply(CraftingChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Crafting changes disabled");
            return;
        }

        if (config.CraftingRebalance)
        {
            try
            {
                DoCraftingRebalance();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Crafting rebalance failed: {ex.Message}");
            }
        }

        if (config.AdditionalCraftingRecipes)
        {
            try
            {
                DoAdditionalCraftingRecipes();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Additional recipes failed: {ex.Message}");
            }
        }
    }

    private void DoCraftingRebalance()
    {
        // TODO: Phase 4 - Verify DatabaseService method name
        var recipes = _databaseService.GetHideout().Production.Recipes;
        int successCount = 0;
        int failCount = 0;

        foreach (var adjustment in CraftingData.Adjustments.All)
        {
            try
            {
                var craft = GetCraftByEndProduct(recipes, adjustment.ItemId);
                if (craft == null)
                {
                    _logger.Warning($"[Softcore] Craft not found for {adjustment.ItemId}, skipping");
                    failCount++;
                    continue;
                }

                adjustment.Apply(craft);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Failed to adjust craft {adjustment.ItemId}: {ex.Message}");
                failCount++;
            }
        }

        _logger.Success($"[Softcore] Crafting rebalance: {successCount} applied, {failCount} skipped");
    }

    private void DoAdditionalCraftingRecipes()
    {
        // TODO: Phase 4 - Verify DatabaseService method name
        var recipes = _databaseService.GetHideout().Production.Recipes;

        foreach (var newRecipe in CraftingData.NewRecipes.All)
        {
            recipes.Add(newRecipe);
        }

        _logger.Success($"[Softcore] Added {CraftingData.NewRecipes.All.Count} new crafting recipes");
    }

    private HideoutProduction? GetCraftByEndProduct(List<HideoutProduction> recipes, string endProductId)
    {
        // Exclude CHRISTMAS_TREE area (type 16) as per TypeScript implementation
        // TODO: Phase 4 - Verify CHRISTMAS_TREE constant/enum value
        return recipes.FirstOrDefault(r =>
            r.EndProduct == endProductId &&
            r.AreaType != 16
        );
    }
}
```

**Test:** Compilation

### Step 7: Wire into Plugin.cs
```csharp
// Add to constructor parameters:
private readonly CraftingChangesChanger _craftingChanger;

public Plugin(
    ISptLogger<Plugin> logger,
    EconomyOptionsChanger economyChanger,
    CraftingChangesChanger craftingChanger) // ADD THIS
{
    _logger = logger;
    _economyChanger = economyChanger;
    _craftingChanger = craftingChanger; // ADD THIS
}

// Add to OnLoad() method after economy changes:
public async Task OnLoad()
{
    // ... existing config loading ...

    _economyChanger.Apply(_config.EconomyOptions);

    // ADD THIS:
    _craftingChanger.Apply(_config.CraftingChanges);

    _logger.Success("[Softcore] All changes applied successfully");
}
```

**Test:** Full project compilation

## Porting Examples Reference

### Example 1: Simple Count
```typescript
// TS: productionAdjustments.ts:17-21
{ id: ItemTpl.BARTER_PARACORD, adjust: (craft) => { craft.count = 2 } }
```
```csharp
// C#: CraftingData.cs
new("5c12613b86f7743bbe2c3f76", craft => craft.Count = 2) // TODO: Phase 4 - Validate
```

### Example 2: Multiple Finds
```typescript
// TS: productionAdjustments.ts:79-92
adjust: (craft) => {
    let req = craft.requirements.find(r => r.templateId === ItemTpl.MEDKIT_IFAK)
    if (!req) return
    req.count = 1
    req = craft.requirements.find(r => r.templateId === ItemTpl.MEDICAL_ARMY_BANDAGE)
    if (!req) return
    req.templateId = ItemTpl.MEDICAL_CALOKB_HEMOSTATIC_APPLICATOR
}
```
```csharp
// C#: CraftingData.cs
craft =>
{
    var req = craft.Requirements.FirstOrDefault(r => r.TemplateId == "590c657e86f77412b013051d");
    if (req == null) return;
    req.Count = 1;
    req = craft.Requirements.FirstOrDefault(r => r.TemplateId == "5755356824597772cb798962");
    if (req == null) return;
    req.TemplateId = "5c0e530286f7747fa1419862";
    // TODO: Phase 4 - Validate all item IDs
}
```

### Example 3: Complete Replacement
```typescript
// TS: Large requirements array replacement
craft.requirements = [
    { areaType: 11, requiredLevel: 2, type: "Area" },
    { templateId: ItemTpl.ITEM_A, count: 1, type: "Item" },
    { templateId: ItemTpl.ITEM_B, type: "Tool" }
]
```
```csharp
// C#: CraftingData.cs
craft.Requirements = new List<ProductionRequirement>
{
    new() { AreaType = 11, RequiredLevel = 2, Type = "Area" },
    new() { TemplateId = "item_a_id", Count = 1, IsFunctional = false, Type = "Item" },
    new() { TemplateId = "item_b_id", Type = "Tool" }
    // TODO: Phase 4 - Validate all item IDs
};
```

## Research Questions for Phase 4

These assumptions need verification when SPT 4.0 documentation/testing is available:

1. **Database API**: What is the exact method signature?
   - Assumption: `_databaseService.GetHideout().Production.Recipes`
   - Alternative: `_databaseService.GetHideoutProduction()`

2. **HideoutProduction model**: Exact property names?
   - Assumptions: `Id`, `AreaType`, `EndProduct`, `Count`, `Requirements`, `ProductionTime`
   - C# typically uses PascalCase for properties

3. **ProductionRequirement model**: Structure?
   - Assumptions: `Type`, `AreaType?`, `RequiredLevel?`, `TemplateId?`, `Count?`, `IsFunctional?`, `QuestId?`

4. **HideoutAreas enum**: CHRISTMAS_TREE value?
   - Assumption: Type 16 (from TypeScript HideoutAreas.CHRISTMAS_TREE)
   - Alternative: Use enum constant if available

5. **Recipe mutability**: Can we modify in place?
   - Assumption: Yes (based on database patterns in Phase 2)
   - Alternative: Clone-modify-replace if immutable

6. **Item ID format**: String vs typed?
   - Current: Using string literals
   - Phase 4: Convert to enum/MongoId if SPT provides

## TODO Comment Strategy

Mark every uncertainty with TODO for Phase 4:
```csharp
// TODO: Phase 4 - Validate item ID "544a11ac4bdc2d470e8b456a" exists in SPT 4.0
// TODO: Phase 4 - Verify DatabaseService.GetHideout() method signature
// TODO: Phase 4 - Confirm HideoutProduction property names
// TODO: Phase 4 - Test in SPT 4.0 runtime environment
```

## Verification (Post-Implementation)

1. **Compilation**: `dotnet build` succeeds with no errors
2. **Count verification**:
   - `CraftingData.Adjustments.All.Count == 98`
   - `CraftingData.NewRecipes.All.Count == 12`
3. **Pattern check**: All adjustments use correct delegate syntax
4. **TODO audit**: Search for "TODO: Phase 4" confirms all uncertainties marked

## Phase 4 Integration Plan

After Phase 3 completion:
1. Research SPT 4.0 API documentation (use DeepWiki if available)
2. Validate all item IDs against SPT 4.0 database
3. Update database access methods if needed
4. Test in SPT 4.0 runtime environment
5. Verify recipes appear correctly in-game hideout
6. Remove all TODO comments once validated

## Critical Files

**Create:**
- `/Users/alex/Documents/git/ODT-Softcore/csharp/Softcore/Assets/CraftingData.cs` (~1500 lines)
- `/Users/alex/Documents/git/ODT-Softcore/csharp/Softcore/Changers/CraftingChangesChanger.cs` (~120 lines)

**Modify:**
- `/Users/alex/Documents/git/ODT-Softcore/csharp/Softcore/Plugin.cs` (~5 lines added)

**Reference:**
- `/Users/alex/Documents/git/ODT-Softcore/src/assets/productionAdjustments.ts` (source for 98 adjustments)
- `/Users/alex/Documents/git/ODT-Softcore/src/assets/recipes.ts` (source for 12 recipes)
- `/Users/alex/Documents/git/ODT-Softcore/src/changers/CraftingChangesChanger.ts` (source for logic)
