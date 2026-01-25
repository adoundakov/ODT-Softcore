# Merged Migration Plan: Softcore SPT Mod (TypeScript → C#)

**Goal:** Port the Softcore mod from TypeScript (SPT 3.x) to C# (SPT 4.0+)

**Priority Features:**
1. Barter Economy (flea market changes)
2. Crafting Recipe Output Increases
3. Everything else is lower priority / may be skipped

**Out of Scope:**
- Insurance changes (user doesn't use)
- Item ID validation (human task)

**Context:**

This plan was merged from two implementation plans. You'll see references to two throughout the merged plan, along with mentions of those plans diverging / conflicting at points. If you're working on a phase with a conflict, let's talk it out before commencing work on that phase.

---

## Repository Structure

The C# implementation will live in a separate `csharp/` subdirectory to avoid conflicts with the existing TypeScript codebase:

```
ODT-Softcore/
├── src/                    # Current TypeScript mod (SPT 3.x)
├── config/                 # TypeScript config (JSON5)
├── package.json
├── tsconfig.json
├── csharp/                 # New C# mod (SPT 4.0+)
│   ├── Softcore.sln
│   ├── Softcore/
│   │   ├── Softcore.csproj
│   │   ├── Plugin.cs
│   │   ├── ModMetadata.cs
│   │   ├── Config/
│   │   ├── Changers/
│   │   ├── Assets/
│   │   └── config/
│   └── README.md
├── plans/
├── CLAUDE.md
└── README.md
```

This allows:
- Both versions to coexist during migration
- Independent build systems (npm vs dotnet)
- Easy side-by-side testing
- Clean deletion of TypeScript version when complete

---

## Prerequisites: C# Development Setup

### Install .NET SDK

**macOS (via Homebrew):**
```bash
brew install dotnet
```

Verify installation:
```bash
dotnet --version  # Should show 9.x or later
```

### How NuGet Packages Work

Unlike npm where you install packages manually, .NET uses **NuGet** which works like this:

1. You declare package dependencies in `Softcore.csproj`:
   ```xml
   <ItemGroup>
     <PackageReference Include="SPTarkov.Common" Version="4.0.5" />
   </ItemGroup>
   ```

2. When you run `dotnet build` or `dotnet restore`, NuGet automatically:
   - Downloads packages from nuget.org
   - Caches them in `~/.nuget/packages/`
   - Makes them available to your project

**You don't need to manually download SPT modules** - they're published as NuGet packages and will be fetched automatically when you build.

### VSCode Setup (You're Already Set!)

You mentioned you have the "C# Dev Kit" extension installed - that's all you need. It provides:
- IntelliSense (autocomplete)
- Debugging
- Build task integration

When you open the `csharp/` folder, VSCode should automatically detect the `.sln` file and offer to set it as the active solution.

### First-Time Build

After creating the project structure in Phase 0:
```bash
cd csharp
dotnet restore   # Downloads NuGet packages (optional, build does this)
dotnet build     # Compiles the project
```

---

## Architecture Mapping: Old → New

### Dependency Injection

| Old (TypeScript/tsyringe) | New (C#/SPT DI) |
|---------------------------|-----------------|
| `container.resolve<T>("ServiceName")` | Constructor injection via `[Injectable]` |
| `IPreSptLoadMod.preSptLoad()` | `IOnLoad` with `TypePriority = OnLoadOrder.PostDBModLoader + 1` |
| `IPostDBLoadMod.postDBLoad()` | `IOnLoad` with `TypePriority = OnLoadOrder.PostDBModLoader + 1` |

### Database Access

| Old Pattern | New Pattern |
|-------------|-------------|
| `databaseServer.getTables()` | `databaseService.GetXxx()` methods |
| `tables.templates.items` | `databaseService.GetItems()` |
| `tables.templates.prices` | `databaseService.GetPrices()` |
| `tables.hideout` | `databaseService.GetHideout()` |
| `tables.globals` | `databaseService.GetGlobals()` |

### Config Access

| Old Pattern | New Pattern |
|-------------|-------------|
| `configServer.getConfig<IRagfairConfig>(ConfigTypes.RAGFAIR)` | `configServer.GetConfig<RagfairConfig>()` |

### Logging

| Old Pattern | New Pattern |
|-------------|-------------|
| `this.logger.info("msg")` | `logger.Info("msg")` |
| `this.logger.warning("msg")` | `logger.Warning("msg")` |
| Custom `PrefixLogger` class | Use `ISptLogger<T>` directly |

---

## Project Structure

> **⚠️ CONFLICT:** The plans differ slightly on structure

| Aspect | Artifact Plan | Chat Plan |
|--------|---------------|-----------|
| Main entry file | `Plugin.cs` | `SoftcorePlugin.cs` |
| Logger wrapper | None (use ISptLogger directly) | `Utils/SoftcoreLogger.cs` |
| Config service | Built-in JSON loading | `Services/ConfigService.cs` |

**Resolution:** Use the simpler artifact approach (no custom logger wrapper, no separate ConfigService class). The chat plan's `SoftcoreLogger.cs` adds unnecessary abstraction since `ISptLogger<T>` already provides what we need.

### Final C# Project Structure
```
csharp/
├── Softcore.sln
├── Softcore/
│   ├── Softcore.csproj
│   ├── ModMetadata.cs
│   ├── Plugin.cs                      # Main entry point
│   ├── Config/
│   │   └── Configuration.cs           # All config models
│   ├── Changers/
│   │   ├── BarterEconomyChanger.cs
│   │   ├── CraftingChangesChanger.cs
│   │   ├── PacifistFleaMarketChanger.cs
│   │   ├── PriceRebalanceChanger.cs
│   │   └── OtherFleaMarketChangesChanger.cs
│   ├── Assets/
│   │   ├── FleaMarketData.cs
│   │   └── CraftingData.cs
│   └── config/
│       └── config.json
└── README.md
```

---

## Phase 0: Minimal Building Mod

**Goal:** Create a mod that compiles, loads in SPT 4.0, and logs a message.

### Tasks

1. **Create solution and project**
   ```bash
   # Run from repository root (ODT-Softcore/)
   mkdir -p csharp/Softcore
   cd csharp
   dotnet new sln -n Softcore
   cd Softcore
   dotnet new classlib -n Softcore -f net9.0
   cd ..
   dotnet sln add Softcore/Softcore.csproj
   ```

2. **Create `Softcore.csproj`**
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <RootNamespace>Softcore</RootNamespace>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
       <OutputType>Library</OutputType>
       <Version>4.0.0</Version>
       <OutputPath>bin\$(Configuration)\[Softcore]\</OutputPath>
       <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="SPTarkov.Common" Version="4.0.5" />
       <PackageReference Include="SPTarkov.DI" Version="4.0.5" />
       <PackageReference Include="SPTarkov.Server.Core" Version="4.0.5" />
     </ItemGroup>
   </Project>
   ```

3. **Create `ModMetadata.cs`**
   ```csharp
   using SPTarkov.Server.Core.Models.Spt.Mod;

   namespace Softcore;

   public record ModMetadata : AbstractModMetadata
   {
       public override string ModGuid { get; init; } = "com.softcore.spt";
       public override string Name { get; init; } = "Softcore";
       public override string Author { get; init; } = "YourName";
       public override List<string>? Contributors { get; init; }
       public override SemanticVersioning.Version Version { get; init; } = new("4.0.0");
       public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
       public override List<string>? Incompatibilities { get; init; }
       public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
       public override string? Url { get; init; }
       public override bool? IsBundleMod { get; init; }
       public override string License { get; init; } = "MIT";
   }
   ```

4. **Create `Plugin.cs`**
   ```csharp
   using SPTarkov.DI.Annotations;
   using SPTarkov.Server.Core.DI;
   using SPTarkov.Server.Core.Models.Utils;

   namespace Softcore;

   [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
   public class Plugin(ISptLogger<Plugin> logger) : IOnLoad
   {
       public Task OnLoad()
       {
           logger.Success("[Softcore] Mod loaded successfully!");
           return Task.CompletedTask;
       }
   }
   ```

5. **Build and test**
   ```bash
   # From csharp/ directory
   dotnet build

   # Output will be in: csharp/Softcore/bin/Debug/[Softcore]/
   # Copy to your SPT installation:
   cp -r Softcore/bin/Debug/[Softcore]/ /path/to/SPT/user/mods/
   ```
   - Start SPT server
   - Verify log message "[Softcore] Mod loaded successfully!" appears in console

### Success Criteria
- Mod compiles without errors
- Log message "[Softcore] Mod loaded successfully!" appears

---

## Phase 1: Configuration System

**Goal:** Port configuration loading and models.

### Config Models

```csharp
// Config/Configuration.cs
namespace Softcore.Config;

public class Configuration
{
    public GeneralConfig General { get; set; } = new();
    public EconomyOptionsConfig EconomyOptions { get; set; } = new();
    public CraftingChangesConfig CraftingChanges { get; set; } = new();
}

public class GeneralConfig
{
    public bool Enabled { get; set; } = true;
    public bool Debug { get; set; } = false;
}

public class EconomyOptionsConfig
{
    public bool Enabled { get; set; } = true;
    public BarterEconomyConfig BarterEconomy { get; set; } = new();
}

public class BarterEconomyConfig
{
    public bool Enabled { get; set; } = true;
    public int CashOffersPercentage { get; set; } = 10;
    public int BarterPriceVariance { get; set; } = 10;
    public MinMax OfferItemCount { get; set; } = new();
    public MinMax NonStackableCount { get; set; } = new();
    public int ItemCountMax { get; set; } = 5;
}

public class MinMax
{
    public int Min { get; set; }
    public int Max { get; set; }
}

public class CraftingChangesConfig
{
    public bool Enabled { get; set; } = true;
    public bool CraftingRebalance { get; set; } = true;
    public bool AdditionalCraftingRecipes { get; set; } = true;
}
```

### Config Loading

Load from `config/config.json` using `System.Text.Json`. Handle missing file by creating defaults.

### Success Criteria
- Config file loads successfully
- Invalid config shows helpful error
- Default values work when config missing

---

## Phase 2: Barter Economy

**Goal:** Port the core barter economy functionality.

> **⚠️ CONFLICT:** Chat plan includes more changers

| Artifact Plan | Chat Plan |
|---------------|-----------|
| `BarterEconomyChanger.cs` only | Also includes `PriceRebalanceChanger.cs`, `OtherFleaMarketChangesChanger.cs` |

**Resolution:** Include all changers from chat plan - they're part of the complete barter economy system.

### SPT Configs Touched (NEEDS INVESTIGATION)
- `RagfairConfig.Dynamic.Barter.*` - Barter chance, blacklist, price variance
- `RagfairConfig.Dynamic.OfferItemCount`
- `RagfairConfig.Dynamic.NonStackableCount`
- `RagfairConfig.Dynamic.Blacklist.Custom`

### Files to Create

1. **`Assets/FleaMarketData.cs`** - Static data:
   - `FleaBarterRequestWhitelist` (BaseClasses that CAN be requested)
   - `ActualBaseClasses` (all base classes)
   - `RequestWhitelist` (items with custom prices)
   - `FleaListingsWhitelistHandbook` (handbook category IDs)
   - `BSGblacklist` (items BSG blacklists)
   - Mark all with `// TODO: Validate for SPT 4.0`

2. **`Changers/BarterEconomyChanger.cs`**:
   - `DoBarterEconomy()` - set barter blacklist, adjust prices
   - `AdjustCashOffers()` - set barter.chancePercent
   - `AdjustBarterPriceVariance()`
   - `AdjustItemCountMax()`
   - `AdjustOfferItemCount()`
   - `AdjustNonStackableAmount()`

3. **`Changers/PacifistFleaMarketChanger.cs`**:
   - `PacifistFleaMarket()` - blacklist items not in handbook whitelist
   - `AllowOnRagfair()` - whitelist specific items

4. **`Changers/PriceRebalanceChanger.cs`**:
   - `DoItemFixes()` - fix specific item prices
   - `DoPriceRebalance()` - sync flea prices to handbook

5. **`Changers/OtherFleaMarketChangesChanger.cs`**:
   - `DoSellingOnFlea()` - disable player sales
   - `AdjustOnlyFIRforBarters()` - require FIR
   - `AdjustPristineItems()` - disable condition randomization
   - `IncreaseFleaPrices()` - price multiplier

### Key Logic (from artifact plan)

```csharp
public void Apply(BarterEconomyConfig config)
{
    if (!config.Enabled) return;

    // 1. Set barter type blacklist
    var barterBlacklist = ActualBaseClasses
        .Where(bc => !FleaBarterRequestWhitelist.Contains(bc))
        .ToList();
    
    ragfairConfig.Dynamic.Barter.ItemTypeBlacklist = barterBlacklist;
    ragfairConfig.Dynamic.Barter.MinRoubleCostToBecomeBarter = 100;

    // 2. Adjust flea prices
    var items = databaseService.GetItems();
    var fleaPrices = databaseService.GetPrices();

    foreach (var item in items.Values)
    {
        if (item.Type == "Item" && 
            !IsOfBaseClasses(item.Id, barterBlacklist) &&
            item.Parent != BaseClasses.Money)
        {
            if (item.Properties.QuestItem == true)
                fleaPrices[item.Id] = 0;
            else if (!item.Properties.CanSellOnRagfair)
                fleaPrices[item.Id] = 0;
        }
    }

    // 3. Apply whitelist overrides
    foreach (var (itemId, price) in RequestWhitelist)
        fleaPrices[itemId] = price;

    // 4. Adjust other settings
    ragfairConfig.Dynamic.Barter.ChancePercent = 100 - config.CashOffersPercentage;
    ragfairConfig.Dynamic.Barter.PriceRangeVariancePercent = config.BarterPriceVariance;
    ragfairConfig.Dynamic.Barter.ItemCountMax = config.ItemCountMax;
}
```

### Error Handling (from chat plan)

```csharp
try
{
    DoBarterEconomy();
}
catch (Exception ex)
{
    logger.Warning($"BarterEconomy failed: {ex.Message}");
}
```

### Success Criteria
- Flea shows mostly barter offers (not cash)
- Quest items cannot be requested in barters
- Config options work correctly

---

## Phase 3: Crafting Recipe Changes

**Goal:** Port crafting rebalance and new recipes.

### Database Tables Touched
- `databaseService.GetHideout().Production.Recipes`

### Files to Create

1. **`Assets/CraftingData.cs`**:
   - `CraftingAdjustments` - recipe modifications
   - `ContainerRecipes` - secure container upgrade recipes
   - `StimRecipes` - new stim recipes
   - Mark all with `// TODO: Validate for SPT 4.0`

2. **`Changers/CraftingChangesChanger.cs`**:
   - `DoCraftingRebalance()` - apply adjustments
   - `DoAdditionalCraftingRecipes()` - add new recipes

### Recipe Outputs Changed

| Item | New Output |
|------|------------|
| Toilet Paper | 1 |
| Paracord | 2 |
| Bottle of Water | 16 |
| SJ6 | 3 |
| 5.45x39 BP | 180 |
| 5.56x45 M855A1 | 180 |
| *(and more from productionAdjustments.ts)* |

### Key Logic

```csharp
private HideoutProduction? GetCraftByEndProduct(string endProductId)
{
    return hideout.Production.Recipes
        .FirstOrDefault(p => p.EndProduct == endProductId 
            && p.AreaType != HideoutAreas.ChristmasTree);
}

public void DoCraftingRebalance()
{
    foreach (var adjustment in CraftingData.Adjustments)
    {
        var craft = GetCraftByEndProduct(adjustment.EndProductId);
        if (craft != null)
            adjustment.Apply(craft);
        else
            logger.Warning($"Craft not found for {adjustment.EndProductId}");
    }
}
```

### Success Criteria
- Modified recipes show new output counts
- New recipes appear in hideout
- No errors on server start

---

## Phase 4: Item ID Validation (HUMAN TASK)

**Goal:** Verify all hardcoded item IDs work in SPT 4.0.

### Files to Validate

| File | Approx Count |
|------|--------------|
| `FleaMarketData.cs` whitelists | ~40 |
| `FleaMarketData.cs` BSGblacklist | ~300 |
| `FleaMarketData.cs` actualBaseClasses | ~100 |
| `CraftingData.cs` adjustments | ~50 recipes |
| `CraftingData.cs` new recipes | ~20 |

### Process
1. Export all item IDs to a list
2. Cross-reference with SPT 4.0 `ItemTpl` enum or database
3. Update changed IDs, remove deleted items
4. Remove all `// TODO: Validate for SPT 4.0` comments

---

## Future Phases (Backlog)

| Phase | Feature | Notes |
|-------|---------|-------|
| 5 | Trader Changes | Better sales, Skier→Euros, Fence pacifist |
| 6 | Hideout Options | Stash, containers, bitcoin, scav case |
| 7 | Secure Containers | Progressive unlocks, Collector quest |
| 8 | Other Tweaks | Skills, backpacks, ammo stacks |
| 9 | Insurance | SKIPPED per user request |

---

## Research Questions

Both plans agree these need investigation during implementation:

1. Exact C# type for `RagfairConfig` - is it in `SPTarkov.Server.Core.Models.Spt.Config`?
2. How to access `tables.templates.prices` in C#?
3. Does `ItemTpl` enum exist in NuGet packages, or use raw strings?
4. C# equivalent of `tables.templates.handbook`?
5. How are hideout production recipes structured in C#?

Use [DeepWiki](https://deepwiki.com/sp-tarkov/server-csharp/1-overview) to research.

---

## File Mapping Reference

| Old TypeScript | New C# | Phase |
|----------------|--------|-------|
| `Softcore.ts` | `Plugin.cs` | 0 |
| `types.ts` | `Config/Configuration.cs` | 1 |
| `ConfigServer.ts` | Built-in JSON loading | 1 |
| `PrefixLogger.ts` | Use `ISptLogger<T>` directly | 0 |
| `fleamarket.ts` | `Assets/FleaMarketData.cs` | 2 |
| `BarterEconomyChanger.ts` | `Changers/BarterEconomyChanger.cs` | 2 |
| `PacifistFleaMarketChanger.ts` | `Changers/PacifistFleaMarketChanger.cs` | 2 |
| `PriceRebalanceChanger.ts` | `Changers/PriceRebalanceChanger.cs` | 2 |
| `OtherFleaMarketChangesChanger.ts` | `Changers/OtherFleaMarketChangesChanger.cs` | 2 |
| `productionAdjustments.ts` | `Assets/CraftingData.cs` | 3 |
| `recipes.ts` | `Assets/CraftingData.cs` | 3 |
| `CraftingChangesChanger.ts` | `Changers/CraftingChangesChanger.cs` | 3 |
