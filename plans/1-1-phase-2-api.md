# SPT 4.0 API Confirmation - ANSWERED

## Required Service Injections

### ✓ CONFIRMED - All are classes (not interfaces):

1. **Config Server**
   - **ANSWER:** Use `ConfigServer` (class, NOT interface)
   - Namespace: `SPTarkov.Server.Core.Servers`
   - File: `Libraries/SPTarkov.Server.Core/Servers/ConfigServer.cs:14`
   - Usage: Accessing `RagfairConfig`
   - Method: `GetConfig<RagfairConfig>()` ✓
   - Note: Marked as `[Obsolete]` - will be removed in SPT 4.2 in favor of direct injection

2. **Database Service**
   - **ANSWER:** Use `DatabaseService` (class, NOT interface)
   - Namespace: `SPTarkov.Server.Core.Services`
   - File: `Libraries/SPTarkov.Server.Core/Services/DatabaseService.cs:20`
   - Usage: Accessing game data
   - Methods confirmed:
     - `GetItems()` - Returns `Dictionary<MongoId, TemplateItem>` ✓
     - `GetPrices()` - Returns `Dictionary<MongoId, double>` ✓
     - `GetHandbook()` - Returns `HandbookBase` ✓
     - `GetGlobals()` - Returns `Globals` ✓

3. **Item Helper**
   - **ANSWER:** Use `ItemHelper` (class, NOT interface)
   - Namespace: `SPTarkov.Server.Core.Helpers`
   - File: `Libraries/SPTarkov.Server.Core/Helpers/ItemHelper.cs:20`
   - Usage: Item type checking
   - Method: `IsOfBaseclasses(MongoId tpl, IEnumerable<MongoId> baseClassTpls)` ✓

4. **Handbook Helper**
   - **ANSWER:** Use `HandbookHelper` (class, NOT interface)
   - Namespace: `SPTarkov.Server.Core.Helpers`
   - File: `Libraries/SPTarkov.Server.Core/Helpers/HandbookHelper.cs:16`
   - Usage: Refreshing handbook cache
   - Method: **DOES NOT EXIST** - The cache is hydrated automatically
   - Instead: The cache is auto-populated via `HydrateHandbookCache()` (protected method)
   - Public methods available:
     - `GetTemplatePrice(MongoId tpl)` - Get price from cache
     - `TemplatesWithParent(MongoId parentId)` - Get items by parent category

## Required Type Definitions

### RagfairConfig ✓ CONFIRMED
**File:** `Libraries/SPTarkov.Server.Core/Models/Spt/Config/RagfairConfig.cs`

**Confirmed properties:**
- `RagfairConfig.Dynamic.Barter.ChancePercent` (double) ✓
- `RagfairConfig.Dynamic.Barter.PriceRangeVariancePercent` (double) ✓
- `RagfairConfig.Dynamic.Barter.ItemCountMax` (int) ✓
- `RagfairConfig.Dynamic.Barter.ItemTypeBlacklist` (HashSet<MongoId>) ✓
- `RagfairConfig.Dynamic.Barter.MinRoubleCostToBecomeBarter` (double) ✓

**ANSWERS for blacklist:**
- **Exact path:** `RagfairConfig.Dynamic.Blacklist.Custom` ✓
- **Type:** `HashSet<MongoId>` ✓
- Other blacklist properties in `RagfairConfig.Dynamic.Blacklist`:
  - `DamagedAmmoPacks` (bool)
  - `EnableBsgList` (bool)
  - `EnableQuestList` (bool)
  - `TraderItems` (bool)
  - `ArmorPlate` (ArmorPlateBlacklistSettings)
  - `EnableCustomItemCategoryList` (bool)
  - `CustomItemCategoryList` (HashSet<MongoId>)

### TemplateItem and TemplateItemProperties ✓ CONFIRMED
**File:** `Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/TemplateItem.cs`

**Confirmed properties:**
- `item.Properties.QuestItem` (bool?) ✓
- `item.Properties.CanSellOnRagfair` (bool?) ✓

**Full class definitions:**
```csharp
public record TemplateItem
{
    public MongoId Id { get; set; }
    public string? Name { get; set; }
    public MongoId Parent { get; set; }
    public string? Type { get; set; }
    public TemplateItemProperties? Properties { get; set; }
    public string? Prototype { get; set; }
    // ... see file for complete definition (1959 lines)
}

public record TemplateItemProperties
{
    // Has 300+ properties including:
    public bool? QuestItem { get; set; }
    public bool? CanSellOnRagfair { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? StackMaxSize { get; set; }
    public double? MaxDurability { get; set; }
    public IEnumerable<Slot>? Slots { get; set; }
    public HashSet<MongoId>? ConflictingItems { get; set; }
    // ... see file for complete definition
}
```

### HandbookBase, HandbookItem, HandbookCategory ✓ CONFIRMED
**File:** `Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/HandbookBase.cs`

**ANSWERS:**
- `HandbookBase.Items` - **`List<HandbookItem>`** (NOT Dictionary)
- `HandbookBase.Categories` - **`List<HandbookCategory>`** (NOT Dictionary)

**Full class definitions:**
```csharp
public record HandbookBase
{
    public required List<HandbookCategory> Categories { get; init; }
    public required List<HandbookItem> Items { get; init; }
}

public record HandbookCategory
{
    public MongoId Id { get; set; }
    public MongoId? ParentId { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public string Order { get; set; }
}

public record HandbookItem
{
    public MongoId Id { get; set; }
    public MongoId ParentId { get; set; }  // NOT nullable
    public double? Price { get; set; }
}
```

**Note:** `HandbookItem.ParentId` is `MongoId` (not nullable), while `HandbookCategory.ParentId` is `MongoId?` (nullable).

### Globals ✓ CONFIRMED
**File:** `Libraries/SPTarkov.Server.Core/Models/Eft/Common/Globals.cs`

**Confirmed properties:**
- `globals.Configuration.RagFair.MinUserLevel` (int) ✓
- `globals.Configuration.RagFair.IsOnlyFoundInRaidAllowed` (bool) ✓

**Partial class definitions:**
```csharp
public record Globals
{
    public required Config Configuration { get; init; }
    public required Dictionary<string, int> LocationInfection { get; init; }
    public required IEnumerable<BotPreset> BotPresets { get; init; }
    public required IEnumerable<BotWeaponScattering> BotWeaponScatterings { get; init; }
    public required Dictionary<MongoId, Preset> ItemPresets { get; init; }
}

// Config has nested RagFair property
public record RagFair
{
    public int MinUserLevel { get; set; }
    public bool IsOnlyFoundInRaidAllowed { get; set; }
    // ... plus many other properties
}
```

## Service Injection Pattern ✓ CONFIRMED

```csharp
// Example for BarterEconomyChanger
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

private readonly ConfigServer _configServer;
private readonly DatabaseService _databaseService;
private readonly ISptLogger<BarterEconomyChanger> _logger;

public BarterEconomyChanger(
    ConfigServer configServer,
    DatabaseService databaseService,
    ISptLogger<BarterEconomyChanger> logger
)
{
    _configServer = configServer;
    _databaseService = databaseService;
    _logger = logger;
}
```

**For other changers:**
- `PacifistFleaMarketChanger` - needs `ConfigServer`, `DatabaseService`, `ItemHelper`
- `PriceRebalanceChanger` - needs `DatabaseService`, `HandbookHelper`
- `OtherFleaMarketChangesChanger` - needs `ConfigServer`, `DatabaseService`

**Note:** For logging, use `ISptLogger<T>` (interface with generic type parameter)

## Questions Summary - ANSWERS

1. **Are service interfaces prefixed with `I`?**
   - **ANSWER:** NO - Use concrete classes: `ConfigServer`, `DatabaseService`, `ItemHelper`, `HandbookHelper`
   - Exception: Logging uses `ISptLogger<T>` interface

2. **What are the exact namespaces for these services?**
   - **ANSWER:**
     - `ConfigServer` → `SPTarkov.Server.Core.Servers`
     - `DatabaseService` → `SPTarkov.Server.Core.Services`
     - `ItemHelper` → `SPTarkov.Server.Core.Helpers`
     - `HandbookHelper` → `SPTarkov.Server.Core.Helpers`

3. **What is the exact structure of `RagfairConfig.Dynamic.Blacklist`?**
   - **ANSWER:** `RagfairConfig.Dynamic.Blacklist.Custom` is `HashSet<MongoId>`
   - Full type: `RagfairBlacklist` with multiple properties (see above)

4. **Are handbook collections dictionaries or lists?**
   - **ANSWER:** Both are `List<T>` (NOT dictionaries)
     - `HandbookBase.Items` → `List<HandbookItem>`
     - `HandbookBase.Categories` → `List<HandbookCategory>`

5. **What type is used for item IDs throughout?**
   - **ANSWER:** Yes, consistently `MongoId` ✓
