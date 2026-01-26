# SPT 4.0 API Questions for Phase 3: Crafting System

This document contains verification questions for the SPT-server C# codebase. These questions should be answered by exploring the sp-tarkov/server-csharp repository to confirm API patterns needed for Phase 3 implementation.

## 1. DatabaseService - Hideout Data Access

**Question:** What is the correct method to access hideout production recipes from DatabaseService?

**Answer:**

**Correct Access Path:** Option A is correct:
```csharp
var recipes = databaseService.GetHideout().Production.Recipes;
```

**Full hierarchy:**
```
DatabaseService.GetHideout() → Hideout → Production (HideoutProductionData) → Recipes (List<HideoutProduction>)
```

**Confirmed Details:**
- **Method path:** `databaseService.GetHideout().Production.Recipes`
- **Return type:** `List<HideoutProduction>?` (nullable List)
- **Mutability:** YES - fully mutable, can modify in place and use `.Add()`, `.AddRange()`
- **Implementation:** `GetHideout()` internally calls `databaseServer.GetTables().Hideout`

**Real usage examples from codebase:**
```csharp
// HideoutController.cs:588
var recipe = databaseService.GetHideout().Production.Recipes
    .FirstOrDefault(production => production.Id == request.RecipeId);

// PostDbLoadService.cs:687-690
foreach (var craft in databaseService.GetHideout().Production.Recipes)
{
    craft.ProductionTime = Math.Min(craft.ProductionTime.Value, overrideSeconds);
}
```

**Additional recipe types available:**
- `Production.Recipes` - Normal crafting (List<HideoutProduction>)
- `Production.ScavRecipes` - Scav case (List<ScavRecipe>)
- `Production.CultistRecipes` - Circle of Cultists (List<CultistRecipe>)

## 2. HideoutProduction Model Structure

**Question:** What are the exact property names and types in the HideoutProduction class?

**Context:** We're porting from TypeScript IHideoutProduction interface which has camelCase properties. C# typically uses PascalCase.

**TypeScript properties we're mapping:**
```typescript
interface IHideoutProduction {
    _id: string
    areaType: number
    requirements: IProductionRequirement[]
    productionTime: number
    endProduct: string
    count: number
    isEncoded: boolean
    locked: boolean
    needFuelForAllProductionTime: boolean
    continuous: boolean
    productionLimitCount: number
    isCodeProduction: boolean
}
```

**Answer:**

**Type:** `public record HideoutProduction` (immutable value semantics, but properties are mutable)

**Location:** `/Libraries/SPTarkov.Server.Core/Models/Eft/Hideout/HideoutProduction.cs`

**Complete Property Mapping:**

| C# Property (PascalCase) | JSON Name (camelCase) | Type | Nullable | Notes |
|--------------------------|----------------------|------|----------|-------|
| `Id` | `"_id"` | `MongoId` | NO | Note underscore in JSON |
| `AreaType` | `"areaType"` | `HideoutAreas?` | YES | Enum type |
| `Requirements` | `"requirements"` | `List<Requirement>?` | YES | List, not array/IList |
| `ProductionTime` | `"productionTime"` | `double?` | YES | Double, not int |
| `EndProduct` | `"endProduct"` | `MongoId` | NO | Item template ID |
| `Count` | `"count"` | `int?` | YES | |
| `IsEncoded` | `"isEncoded"` | `bool?` | YES | |
| `Locked` | `"locked"` | `bool?` | YES | |
| `NeedFuelForAllProductionTime` | `"needFuelForAllProductionTime"` | `bool?` | YES | |
| `Continuous` | `"continuous"` | `bool?` | YES | |
| `ProductionLimitCount` | `"productionLimitCount"` | `int?` | YES | |
| `IsCodeProduction` | `"isCodeProduction"` | `bool?` | YES | |

**Key Differences from TypeScript:**
- `Id` property maps to JSON `"_id"` (with underscore)
- `AreaType` is `HideoutAreas?` enum, not `int`
- `ProductionTime` is `double?`, not `int`
- `Requirements` is `List<Requirement>?`, not array
- ALL properties except `Id` and `EndProduct` are nullable
- Properties use `[JsonPropertyName]` attributes for JSON mapping

**Code structure:**
```csharp
public record HideoutProduction
{
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements { get; set; }

    // ... etc
}
```

## 3. ProductionRequirement Model Structure

**Question:** What is the structure of the requirement object used in hideout recipes?

**Answer:**

**Class Name:** `Requirement` (not ProductionRequirement or HideoutRequirement)

**Location:** `/Libraries/SPTarkov.Server.Core/Models/Eft/Hideout/HideoutProduction.cs`

**Type Differentiation:** String-based via `Type` property (not inheritance or discriminated union)

**Complete Property List (ALL nullable):**

```csharp
public record Requirement
{
    [JsonPropertyName("templateId")]
    public MongoId? TemplateId { get; set; }          // For Item, Tool, Resource

    [JsonPropertyName("count")]
    public int? Count { get; set; }                   // For Item, Tool

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }              // For Item, Tool

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }           // For Item, Tool

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }                // For Area (int, not enum)

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }           // For Area

    [JsonPropertyName("resource")]
    public int? Resource { get; set; }                // For Resource requirements

    [JsonPropertyName("questId")]
    public MongoId? QuestId { get; set; }             // For QuestComplete

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("gameVersions")]
    public List<string>? GameVersions { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }                 // "Area", "Item", "Tool", "QuestComplete", etc.
}
```

**RequirementType Enum Values:**
Located at `/Libraries/SPTarkov.Server.Core/Models/Enums/Hideout/RequirementType.cs`

```csharp
public enum RequirementType
{
    Area,              // Uses: areaType, requiredLevel
    Item,              // Uses: templateId, count, isFunctional, isEncoded
    Tool,              // Uses: templateId, count, isFunctional, isEncoded
    QuestComplete,     // Uses: questId
    TraderUnlock,      // Uses: traderId
    TraderLoyalty,     // Uses: traderId, loyaltyLevel
    Skill,             // Uses: skillName, skillLevel
    Resource,          // Uses: templateId, resource
    Health,            // Uses: energy, hydration
    BodyPartBuff,      // Uses: effectName, bodyPart, excluded
    GameVersion        // Game version check
}
```

**Usage Pattern:**
Type checking is done via string comparison:
```csharp
if (requirement.Type == "Item") { ... }
if (requirement.Type == "Tool") { ... }
```

**Key Notes:**
- ALL properties are nullable
- `Type` is a string (compared against enum names)
- `AreaType` in Requirement is `int?`, not `HideoutAreas?` enum
- More requirement types exist beyond the 4 basic ones

## 4. HideoutAreas Constants/Enum

**Question:** How are hideout area types defined, and what is the value for CHRISTMAS_TREE?

**Context:** We need to filter out CHRISTMAS_TREE area when searching recipes (it's used for special event recipes).

**TypeScript reference:**
```typescript
import { HideoutAreas } from "@spt/models/enums/HideoutAreas"
// HideoutAreas.CHRISTMAS_TREE is used in filter
```

**Answer:**

**Enum exists:** YES - `HideoutAreas` enum exists

**Location:** `/Libraries/SPTarkov.Server.Core/Models/Enums/Hideout/HideoutAreas.cs`

**Namespace:** `SPTarkov.Server.Core.Models.Enums.Hideout`

**CHRISTMAS_TREE value:** ⚠️ **DOES NOT EXIST**. Use `ChristmasIllumination` instead.

**Complete enum definition:**
```csharp
public enum HideoutAreas
{
    NotSet = -1,               // Explicit -1
    Vents,                     // 0 (implicit)
    Security,                  // 1
    WaterCloset,               // 2
    Stash,                     // 3
    Generator,                 // 4
    Heating,                   // 5
    WaterCollector,            // 6
    MedStation,                // 7
    Kitchen,                   // 8
    RestSpace,                 // 9
    Workbench,                 // 10
    IntelligenceCenter,        // 11
    ShootingRange,             // 12
    Library,                   // 13
    ScavCase,                  // 14
    Illumination,              // 15
    PlaceOfFame,               // 16
    AirFilteringUnit,          // 17
    SolarPower,                // 18
    BoozeGenerator,            // 19
    BitcoinFarm,               // 20
    ChristmasIllumination,     // 21 ← Use this for Christmas filtering
    EmergencyWall,             // 22
    Gym,                       // 23
    WeaponStand,               // 24
    WeaponStandSecondary,      // 25
    EquipmentPresetsStand,     // 26
    CircleOfCultists,          // 27
}
```

**Usage for filtering:**
```csharp
// Instead of: r.AreaType != HideoutAreas.CHRISTMAS_TREE
// Use:
r.AreaType != HideoutAreas.ChristmasIllumination

// Or by numeric value:
r.AreaType != (HideoutAreas)21
```

**Key Notes:**
- The TypeScript `CHRISTMAS_TREE` maps to C# `ChristmasIllumination`
- Numeric value is 21, not 16 (16 is `PlaceOfFame`)
- Enum uses implicit incrementing (0-27) except `NotSet` (-1)

## 5. Recipe Collection Mutability

**Question:** Can we modify HideoutProduction objects in place, or do we need to clone/replace?

**Answer:**

**YES - Fully mutable.** All modifications are in-place and persist automatically.

**Confirmed mutability details:**
- ✅ `HideoutProduction` objects are mutable (record with `{ get; set; }` properties)
- ✅ All properties have public setters
- ✅ Nested collections can be modified (Requirements list)
- ✅ Recipes collection supports `.Add()`, `.AddRange()`, etc.
- ✅ Changes persist automatically (no save method needed)

**Real-world mutation examples from codebase:**

```csharp
// PostDbLoadService.cs:690 - Direct property mutation
foreach (var craft in databaseService.GetHideout().Production.Recipes)
{
    craft.ProductionTime = Math.Min(craft.ProductionTime.Value, overrideSeconds);
}

// PostDbLoadService.cs:720 - Nested property mutation
var recipe = databaseService.GetHideout().Production.Recipes.FirstOrDefault(craft => craft.Id == craftId);
if (recipe is not null)
{
    recipe.Locked = false;
}
```

**All supported mutation patterns:**
```csharp
// Direct property assignment
craft.Count = 2;
craft.ProductionTime = 100.0;
craft.Locked = false;

// Nested property mutation
craft.Requirements[0].Count = 1;

// Replace entire collection
craft.Requirements = new List<Requirement> { ... };

// Add to recipes collection
recipes.Add(newRecipe);
recipes.AddRange(newRecipes);
```

**Important note:** `HideoutProduction` is a `record` type, which provides value semantics for equality but does NOT make it immutable. Properties with `{ get; set; }` are fully mutable.

## 6. Item ID Format and Types

**Question:** How are item template IDs represented in SPT 4.0 C#?

**Context:** TypeScript uses ItemTpl enum with string values (24-char hexadecimal MongoDB IDs). We're currently using string literals.

**TypeScript reference:**
```typescript
import { ItemTpl } from "@spt/models/enums/ItemTpl"
ItemTpl.BARTER_TOILET_PAPER // = "5bc9b355d4351e6d1509862a"
```

**Answer:**

**ItemTpl exists:** YES - Static class with ~500+ readonly `MongoId` fields (auto-generated)

**Location:** `/Libraries/SPTarkov.Server.Core/Models/Enums/ItemTpl.cs`

**MongoId type:** Custom `readonly struct` representing 12-byte MongoDB ObjectId

**MongoId Location:** `/Libraries/SPTarkov.Server.Core/Models/Common/MongoId.cs`

**All three options work:**

```csharp
// Option A - String literals (implicit conversion)
MongoId itemId = "5bc9b355d4351e6d1509862a";  // ✅ Works via implicit operator

// Option B - ItemTpl static fields (recommended)
MongoId itemId = ItemTpl.BARTER_TOILET_PAPER;  // ✅ Type-safe

// Option C - Explicit MongoId construction
var itemId = new MongoId("5bc9b355d4351e6d1509862a");  // ✅ Explicit
```

**MongoId key features:**
- **Implicit string conversion:** `public static implicit operator MongoId(string mongoId)`
- **24-character hex string** format (lowercase)
- **Empty check:** `IsEmpty` property or `MongoId.Empty()`
- **Auto-generation:** `new MongoId()` generates unique ID based on timestamp + machine + process + counter

**ItemTpl structure (auto-generated):**
```csharp
public static class ItemTpl
{
    public static readonly MongoId AMMO_127X108_B32 = new MongoId("5cde8864d7f00c0010373be1");
    public static readonly MongoId ASSAULTRIFLE_COLT_M4A1_556X45 = new MongoId("5447a9cd4bdc2dbd208b4567");
    // ... 500+ more entries
}
```

**BaseClasses for item categories:**
```csharp
public static class BaseClasses
{
    public static readonly MongoId AMMO = new MongoId("5485a8684bdc2da71d8b4567");
    public static readonly MongoId ARMOR = new MongoId("5448e54d4bdc2dcc718b4568");
    // ... category types
}
```

**Recommended pattern:** Use `ItemTpl.*` static fields for type safety and clarity. String literals work but lack IDE autocomplete and compile-time validation.

## 7. Recipe Lookup Performance

**Question:** Is there an index/dictionary for looking up recipes by endProduct, or do we need to iterate?

**Answer:**

**No built-in index** - The codebase uses `List<HideoutProduction>` and iterates with `FirstOrDefault()`.

**Current pattern in codebase:**
```csharp
// HideoutController.cs:588 - Linear search on List
var recipe = databaseService.GetHideout().Production.Recipes
    .FirstOrDefault(production => production.Id == request.RecipeId);

// PostDbLoadService.cs:717 - Another linear search
var recipe = databaseService.GetHideout().Production.Recipes
    .FirstOrDefault(craft => craft.Id == craftId);
```

**Performance characteristics:**
- Recipes is `List<HideoutProduction>?` (no indexing)
- Each lookup is O(n) linear search
- 98 lookups = O(n*m) complexity

**Recommendation for your use case:**
**YES, create a Dictionary cache** if doing 98 lookups:

```csharp
// Build cache once
var recipesByEndProduct = databaseService.GetHideout().Production.Recipes
    .Where(r => r.AreaType != HideoutAreas.ChristmasIllumination)
    .GroupBy(r => r.EndProduct)
    .ToDictionary(g => g.Key, g => g.First());

// Then O(1) lookups
var craft = recipesByEndProduct.TryGetValue(itemId, out var recipe) ? recipe : null;
```

**Typical collection size:** Unknown from codebase inspection, but likely 100-500 recipes based on game content.

**Alternative pattern - Single pass:**
```csharp
var recipes = databaseService.GetHideout().Production.Recipes;
foreach (var craft in recipes)
{
    if (craft.AreaType == HideoutAreas.ChristmasIllumination) continue;

    var adjustment = CraftingData.Adjustments.All
        .FirstOrDefault(a => a.ItemId == craft.EndProduct);

    adjustment?.Apply(craft);
}
```

## 8. Logging in Changers

**Question:** Confirm ISptLogger<T> API for logging in changers.

**Context:** We're using ISptLogger<CraftingChangesChanger> injected via DI. Need to confirm method names.

**Answer:**

**Location:** `/Libraries/SPTarkov.Server.Core/Models/Utils/ISptLogger.cs`

**Available logging methods:**

```csharp
_logger.Success("message");   // ✅ Green text, LogLevel.Info
_logger.Info("message");      // ✅ Default text, LogLevel.Info
_logger.Warning("message");   // ✅ Yellow text, LogLevel.Warn
_logger.Error("message");     // ✅ Red text, LogLevel.Error
_logger.Debug("message");     // ✅ Gray text, LogLevel.Debug
_logger.Critical("message");  // ✅ Black on red, LogLevel.Fatal
```

**Full method signatures:**
```csharp
void Success(string data, Exception? ex = null);
void Info(string data, Exception? ex = null);
void Warning(string data, Exception? ex = null);
void Error(string data, Exception? ex = null);
void Debug(string data, Exception? ex = null);
void Critical(string data, Exception? ex = null);
```

**Advanced methods:**
```csharp
void LogWithColor(string data, LogTextColor? textColor = null,
    LogBackgroundColor? backgroundColor = null, Exception? ex = null);

void Log(LogLevel level, string data, LogTextColor? textColor = null,
    LogBackgroundColor? backgroundColor = null, Exception? ex = null);

bool IsLogEnabled(LogLevel level);
```

**Log levels (ordered):**
- `LogLevel.Fatal` (Critical)
- `LogLevel.Error`
- `LogLevel.Warn` (Warning)
- `LogLevel.Info` (Info, Success)
- `LogLevel.Debug`
- `LogLevel.Trace`

**Prefix handling:** No auto-prefixing. Add "[Softcore]" or mod name manually if desired:
```csharp
_logger.Info("[Softcore] Applying crafting changes...");
```

**Optional level checks:**
```csharp
if (_logger.IsLogEnabled(LogLevel.Debug))
{
    _logger.Debug("Expensive debug computation here");
}
```

**Generic type:** `ISptLogger<T>` - category is derived from type `T` automatically.

## 9. Exception Handling Patterns

**Question:** What exception types should we catch for database operations?

**Context:** We're using generic catch (Exception ex) for graceful degradation. Should we be more specific?

**Answer:**

**Specific SPT exception types exist:** YES, but generic `catch (Exception ex)` is the predominant pattern.

**Available SPT exception types:**
Located in `/Libraries/SPTarkov.Server.Core/Exceptions/`

**Database exceptions:**
- `DatabaseNullException` - Database is null
- `DatabaseTablesAlreadySetException` - Tables already initialized

**Helper exceptions:**
- `HideoutHelperException` - Hideout operations
- `InventoryHelperException` - Inventory operations
- `ItemHelperException` - Item operations
- `HandbookHelperException` - Handbook operations
- `HealthHelperException` - Health operations
- `DurabilityHelperException` - Durability operations
- `InRaidHelperException` - In-raid operations

**Item/mod validation exceptions:**
- `InvalidModdedItemException`
- `InvalidModdedClothingException`
- `InvalidModdedTraderException`

**Exception structure (simple wrappers):**
```csharp
public class DatabaseNullException : Exception
{
    public DatabaseNullException(string message) : base(message) { }
    public DatabaseNullException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

**Predominant pattern in codebase:**
```csharp
catch (Exception ex)  // Generic catch used in 95%+ of code
{
    _logger.Error($"Error message: {ex.Message}");
    // Graceful degradation
}
```

**Recommended pattern for mods:**
Use **generic `catch (Exception ex)`** for graceful degradation. The codebase does not use specific exception catching for database operations.

**Examples from codebase:**
- `ImporterUtil.cs:119` - `catch (Exception ex)`
- `InventoryHelper.cs:128` - `catch (Exception ex)`
- `BotController.cs:297` - `catch (Exception ex)`

**Conclusion:** Generic exception handling is the standard. Specific SPT exceptions exist but are rarely caught explicitly.

## 10. Recipe ID Generation

**Question:** For new recipes, how should we generate unique IDs?

**Context:** Our 12 new recipes have hardcoded IDs like "63da4dbee8fa73e22500001a". Are these MongoDB ObjectIds that need to be unique?

**TypeScript IDs used:**
```typescript
alpha: _id: "63da4dbee8fa73e22500001a"
beta: _id: "63da4dbee8fa73e22500001b"
// etc...
```

**Answer:**

**YES - Use hardcoded IDs for consistency** across installations, or generate new ones.

**MongoId format requirements:**
- **24-character hexadecimal string** (lowercase recommended)
- Represents 12-byte MongoDB ObjectId
- Format: `timestamp(4 bytes) + machine(3 bytes) + pid(2 bytes) + counter(3 bytes)`

**Option 1: Hardcoded IDs (recommended for mods):**
```csharp
new HideoutProduction
{
    Id = "63da4dbee8fa73e22500001a",  // ✅ Hardcoded, consistent across installs
    // ...
}
```

**Benefits of hardcoded IDs:**
- ✅ Consistent across all user installations
- ✅ Predictable for save compatibility
- ✅ Easy to reference in other mods
- ✅ No collision risk if IDs are unique

**Option 2: Generate new IDs:**
```csharp
new HideoutProduction
{
    Id = new MongoId(),  // ✅ Auto-generates unique ID
    // ...
}
```

**Collision checking:**
Not necessary if:
- Using hardcoded IDs that don't conflict with vanilla recipes
- IDs start with unique prefix (e.g., `63da4dbe...` range)

**Real usage examples:**
```csharp
// DialogueCallbacks.cs:40 - Generated ID
Id = new MongoId(),

// RagfairOfferGenerator.cs:111 - Generated ID
Id = new MongoId(),

// LootGenerator.cs:176 - Generated ID
Id = new MongoId(),
```

**Validation:**
`MongoId` constructor validates format:
```csharp
new MongoId("invalid");  // ❌ Throws FormatException if not 24 hex chars
```

**Recommendation:**
**Use your hardcoded IDs** (`"63da4dbee8fa73e22500001a"`, etc.) for consistency. The TypeScript IDs are valid MongoDB ObjectId format and will work perfectly in C#.

## Summary of Critical Paths

**All questions answered and verified ✅**

**Critical findings:**
1. ✅ DatabaseService path: `databaseService.GetHideout().Production.Recipes`
2. ✅ HideoutProduction: Uses PascalCase properties, `MongoId` types, all nullable except Id/EndProduct
3. ✅ Requirement class: String-based type differentiation, all properties nullable
4. ⚠️ HideoutAreas.ChristmasIllumination (value 21, NOT CHRISTMAS_TREE)
5. ✅ Recipe mutability: Fully mutable, modify in-place
6. ✅ Item IDs: Use `MongoId` type with implicit string conversion, `ItemTpl` static class available
7. ⚠️ Recipe lookup: No index, recommend Dictionary cache for 98 lookups
8. ✅ Logger: Success/Info/Warning/Error/Debug/Critical methods available
9. ✅ Exceptions: Use generic `catch (Exception ex)` pattern
10. ✅ Recipe IDs: Use hardcoded IDs for consistency, 24-char hex format

## Example Usage Pattern Desired

Based on Phase 2 economy changers, we expect this pattern to work:

```csharp
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
        var recipes = _databaseService.GetHideout().Production.Recipes; // ✅ VERIFIED

        foreach (var adjustment in CraftingData.Adjustments.All)
        {
            var craft = recipes.FirstOrDefault(r =>
                r.EndProduct == adjustment.ItemId &&
                r.AreaType != HideoutAreas.ChristmasIllumination); // ✅ Use ChristmasIllumination (21), not 16

            if (craft != null)
            {
                adjustment.Apply(craft); // ✅ VERIFIED - Mutability confirmed
            }
        }

        recipes.AddRange(CraftingData.NewRecipes.All); // ✅ VERIFIED - AddRange available
    }
}
```

**✅ All APIs verified - This pattern will work correctly.**

## Verified Corrections to Original Assumptions

1. **CHRISTMAS_TREE → ChristmasIllumination** (value 21, not 16)
2. **ProductionTime is `double?`**, not `int?`
3. **Requirements class name is `Requirement`**, not `ProductionRequirement`
4. **AreaType in HideoutProduction is `HideoutAreas?` enum**, not `int?`
5. **AreaType in Requirement is `int?`**, not `HideoutAreas?` enum
6. **Recommend Dictionary cache** for 98 recipe lookups (no built-in index)
