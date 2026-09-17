# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is "ODT-Softcore", a mod for SPT (Single Player Tarkov) that rebalances the game into a survival RPG focused on economy, crafting, and barter-only trading. The mod implements a "pacifist flea market" where only meds, barter items, food, and info items can be bought using random barters with found-in-raid items.

## Two implementations

- `csharp/` — **the active mod**, a C# server mod for SPT 4.1.5 (`SPTushonka.*` 4.1.5 NuGet packages, net10.0).
  Currently ports `economyOptions`, `traderChanges` and `craftingChanges`, plus two sections with no TS counterpart lifted
  from Geko's Better Progression (`../geckos-better-progression`): `refChanges` and `questRewards`. Structure mirrors the
  TS mod: `Plugin.cs` (entry, `IOnLoad` at `Preload + 1`, also enables the patches behind their config flags), `Changers/`
  (`AssortHelper` is shared), `Patches/` (Harmony `AbstractPatch` subclasses via `SPTushonka.Reflection`; `[Injectable]`,
  no config checks inside), `Assets/`, `Config/` (`Configuration.cs` POCO + `config.json`).
  Migration plan and status live in `plans/` (untracked).
- `src/` — the original TypeScript mod for SPT 3.11. Kept as the reference for porting; the rest of this file
  describes it.

## Build Commands

C# (SPT 4.1):
```bash
cd csharp && dotnet build                 # Debug build, 0 warnings expected
cd csharp && dotnet build -c Release      # + csharp/Softcore/ReleaseZip/DukeWendigo-Softcore-{version}.zip
```
Local SPT server source for API lookups: `../server-csharp` (tag 4.1.5); examples: `../spt-server-mod-examples`.

TypeScript (SPT 3.11, legacy):
```bash
npm run setup      # Install dependencies (first time setup)
npm run build      # Build and package mod to dist/softcore-{version}.zip
npm run buildinfo  # Build with verbose logging
```

The TS build process:
- Compiles TypeScript to JavaScript
- Packages files according to `.buildignore` rules
- Creates a ZIP file in `dist/` directory that can be placed in SPT's `user/mods/` folder

## Architecture Overview

### SPT Mod Integration Pattern

This mod uses SPT's standard lifecycle hooks defined in `src/Softcore.ts`:

1. **`preSptLoad(container)`** - Initializes logger, loads and validates configuration from `config/config.json5`
2. **`postDBLoad(container)`** - Applies all database modifications through "Changer" classes

Entry point: `src/Softcore.ts` exports a singleton `mod` object that SPT loads.

### Changer Pattern (Core Architecture)

All game modifications are organized into "Changer" classes in `src/changers/`. Each changer:
- Receives SPT's `DependencyContainer` to resolve services (`DatabaseServer`, `ConfigServer`, etc.)
- Exposes an `apply(config)` method that performs database mutations
- Wraps modifications in try-catch for graceful degradation
- Logs operations using the singleton `PrefixLogger`

**Top-level Changers** (orchestrated in `Softcore.ts:postDBLoad`):
- `SecureContainerOptionsChanger` - Container size modifications
- `HideoutOptionsChanger` - Stash, containers, crafting/fuel speed
- `EconomyOptionsChanger` - Flea market, barter system, prices
- `TraderChangesChanger` - Trader pricing, assortments, Fence modifications
- `CraftingChangesChanger` - Recipe additions and rebalances
- `InsuranceChangesChanger` - Insurance timing and costs
- `OtherTweaksChanger` - Skills, examine time, ammo stacks, misc tweaks

**Hierarchical Composition**: Top-level changers delegate to specialized sub-changers. For example, `HideoutOptionsChanger` composes:
- `StashOptionsChanger`
- `HideoutContainersChanger`
- `FasterBitcoinFarmingChanger`
- `FasterCraftingTimeChanger`
- `FasterConstructionTimeChanger`
- `FasterScavCaseChanger`

### Database Modification Pattern

The mod modifies SPT's in-memory database after it loads. Access pattern:

```typescript
const databaseServer = container.resolve<DatabaseServer>("DatabaseServer")
const tables = databaseServer.getTables()

// Modify item templates
tables.templates.items[itemId]._props.property = newValue

// Modify hideout
tables.hideout.areas.find(a => a.type === HideoutAreas.STASH).level = 1

// Modify globals
tables.globals.config.RagFair.minUserLevel = 5

// Modify trader assortments
tables.traders[traderEnum.FENCE].assort.items = newItems
```

**Key Database Tables**:
- `templates.items` - Item properties, grids, slots
- `templates.prices` - Flea market price data
- `templates.handbook` - Item handbook (base trader prices)
- `hideout.areas` - Hideout area requirements and levels
- `hideout.production.recipes` - Crafting recipes
- `traders[id].assort` - Trader inventory and barter offers
- `traders[id].base` - Trader configuration (buy rates, currency)
- `globals.config` - Global game settings
- `profiles[side]` - Character profile templates

### Configuration System

Configuration file: `config/config.json5` (JSON5 format with comments)

**Structure mirrors Changer hierarchy**:
```
general                 → Global enable/disable, debug mode
secureContainersOptions → Container modifications
hideoutOptions          → Hideout-related features
economyOptions          → Flea market and economy
traderChanges           → Trader modifications
craftingChanges         → Recipe changes
insuranceChanges        → Insurance tweaks
otherTweaks            → Miscellaneous features
```

**Type safety**: All config sections have corresponding TypeScript interfaces in `src/types.ts`.

**Loading**: `ConfigServer` class handles reading/parsing config.json5 using the `json5` library.

### Static Asset Data

`src/assets/` contains hardcoded data for modifications:
- `fleamarket.ts` - Item whitelists/blacklists for flea market
- `keys.ts` - Quest key definitions
- `recipes.ts` - Custom crafting recipe definitions
- `productionAdjustments.ts` - Functions to modify existing recipes
- `itemBaseClasses.ts` - Item category/type definitions

These use SPT's internal item template IDs from the `ItemTpl` enum.

### Error Handling Strategy

Each feature modification is wrapped in try-catch blocks. If one feature fails, others continue to apply. This provides graceful degradation - partial mod functionality is preferred over total failure.

```typescript
try {
  new FeatureChanger(this.container).apply(config)
} catch (e) {
  this.logger.warning(`Failed to apply feature: ${e.message}`)
}
```

### Singleton Logger

`PrefixLogger` is a singleton wrapper around Winston logger that adds `[Softcore]` prefix to all log messages. Initialize once in `preSptLoad`, then retrieve via `PrefixLogger.getInstance()` in all changers.

## Adding New Features

1. Add config interface to `src/types.ts`
2. Add default config values to `config/config.json5`
3. Create new changer class in `src/changers/NewFeatureChanger.ts`:
   ```typescript
   export class NewFeatureChanger {
     constructor(private container: DependencyContainer) {
       this.logger = PrefixLogger.getInstance()
       const databaseServer = container.resolve<DatabaseServer>("DatabaseServer")
       this.tables = databaseServer.getTables()
     }

     public apply(config: NewFeatureConfig): void {
       if (!config.enabled) return
       // Implement modifications
     }
   }
   ```
4. Instantiate and call in `Softcore.ts:postDBLoad()`
5. Test by building and installing in SPT

## Key SPT Services

Access via dependency container:
- `DatabaseServer` - In-memory game database
- `ConfigServer` - SPT core configuration
- `ItemHelper` - Item utility functions
- `HandbookHelper` - Handbook/price utilities
- `FenceService` - Fence trader management
- `TraderHelper` - Trader utility functions
- `RagfairPriceService` - Flea market pricing

## Mod Metadata

Defined in `package.json`:
- `sptVersion`: "~3.11" - Compatible SPT version
- `main`: "src/Softcore.js" - Entry point after compilation
- `version`: Mod version (e.g., "3.3.0")
