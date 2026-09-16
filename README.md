# Experimental single player Tarkov "survival RPG vibes" rebalance focused on economy and crafting with a sprinkle of random extra features. 

BETA VERSION. WORK IN PROGRESS. Looking for community feedback. Use at your own risk. New profile highly recommended. 

## Flea market rebalance features: 
- "Pacifist" flea market (and Fence), only meds, barter items, food and info items can be bought.
- Random barter ONLY economy - you can only purchase those items on the flea using FIR (or crafted) items.
- Items you can barter with are also from food, meds, info and barter categories only. 
- "Live" flea prices are completely removed and matched to in-game BSG handbook (Traders) data. 
- You cannot sell items on flea anymore. 

This results in major change of gameplay dynamic. 
Now Tarkov feels like a proper single player focused survival game.
Begone "most profit per slot" mentality.
Begone "rush to lvl 15" to run meta gear only.

All items, even the cheapest, are somewhat valuable now. 
Ever thought about looting AA batteries or Crickents? 
Well, now you will actually look for them to trade in similar value tiers on flea. 

Barters are random and vary a lot in value. 
This results in higly engaging simulation of a war broken society, with some wild trades included. 
Trully immersive experience.

Hideout crafts are great and usefull (toilet paper craft nerfed, was too OP).

Leveling traders and crafting is your only hope of survival. 

## Hideout features:
- 100x faster hideout construction.
- 100x faster hiteout crafts.
- 10x faster bitcoin mining, purified water and moonshine production.
- 10x bigger fuel draw to compensate. (Now you will have to think about fuel. A little, but still.)
- New crafts (Ophthalmoscope, Zagustin, CALOK, Adrenaline, 3bTG, AHF1) and rebalanced some of the vanilla ones (Clin, Paracord, Water filter, Toilet paper, EWR, MULE, Surv12, AFAK, LEDX, GRIZZLY). Working on more (Workbench and intelligence center crafts), suggestions and discussion are encouraged!
- Did you always want to run your own personal underground meth lab in Tarkov? Obdolbos is now craftable at massive profit. 

## Various tweaks:
- Flea is open on level 5.
- Reshala always has his Golden TT.
- Remove backpack restrictions (for containers [ammo, med, etc] mostly). Never again I'll see an unlootable medcase in 314.
- 5x faster item examine time.
- Redo insurance. Prapor is an instant return with 50% chance, costs 10% of item value, Therapist has 2 hour return with 80% chance, costs 20%.
- Keytool buff to make it 5x5.
- Buff to SICC case to make it actually better and a direct upgrade to Docs. And while we are here, allow it to hold keytool. It's Softcore, who cares.
- Buff Vitality, Sniper and Surgery skill leveling (looking for more input)
- Random-only QUEST keys are availiable on flea
- Small list of items used in crafts is availiable on flea (and headsets. Because it's mostly preference, cmon.)
- Custom config for HIGLY recommended mod - [TRAP'S PROGRESSIVE STASH](https://hub.sp-tarkov.com/files/file/917-trap-s-progressive-stash-3-4-0-temp). You will need the space.
- Bigger Ammo Stacks x10

## Configuration:
All options live in `user/mods/Softcore/config/config.json`. The file must stay plain JSON (no comments) because the
SPT server dashboard's config editor rewrites it through `System.Text.Json`. Changes take effect after a server restart.

### `general`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | Enable or disable the mod. |
| `debug` | `false` | Enable debugging mode. Currently does nothing. |

### `economyOptions`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | Master toggle for all economy options below. |
| `disableFleaMarketCompletely` | `false` | Completely disable flea market for a true HARDCORE experience. Still allows you to use the interface and see trader offers. Overrides all other flea changes below. |

#### `economyOptions.priceRebalance`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | **CORE feature.** Completely removes the SPT flea price snapshot from LIVE and matches prices to the internal handbook/trader prices. Everything else is balanced around this. NOT recommended to disable. |
| `itemFixes` | `true` | Handbook price fixes for important items like the intel folder and military flash drive. |

#### `economyOptions.pacifistFleaMarket`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | **CORE feature.** Only meds, barter items, food and info items can be bought on the flea market. Uses the hardcoded handbook-category whitelist as filter. NOT recommended to disable. |
| `whitelist.enabled` | `true` | A small list of items used in crafts and trader barters is available on flea. Uses the hardcoded item whitelist. |
| `whitelist.priceMultiplier` | `2` | Flea price multiplier for those items. |
| `questKeys.enabled` | `true` | Random-only QUEST keys are available on flea. Uses the hardcoded quest-key list. |
| `questKeys.priceMultiplier` | `2` | Flea price multiplier for quest keys. |
| `markedKeys.enabled` | `true` | Marked keys are available on flea. |
| `markedKeys.priceMultiplier` | `2` | Flea price multiplier for marked keys. |

#### `economyOptions.barterEconomy`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | **CORE feature.** Only allows buying items on flea using other random FiR or crafted items. Uses the hardcoded barter blacklist as filter for allowed items (meds, barter items, food and info items are enabled; exceptions are stimulants and fuel). NOT recommended to disable. |
| `cashOffersPercentage` | `15` | Allow a small, random percentage of listings to be buyable for cash. 0 is a true barter-only economy (except the cheapest items like AA battery, an SPT limitation) — the preferred way to play, but sometimes a little too hard. 15% makes life just a bit easier and avoids item deadlocks. Recommendation: 0 to 15. |
| `barterPriceVariance` | `50` | ± percent of price variance between an item listing and its barter value. Bigger number — more wild and varied random trades, e.g. a Defibrillator (224k) offered for a Lion (162k) or a Tank Battery (330k). This CORE feature makes the whole mod tick. More variance also means it is easier to find an offer you have an item for. Recommendation: 20–50. |
| `offerItemCount` | `{ min: 10, max: 20 }` | Number of different offers per item. Too low a number breaks the SPT server with constant client errors on completed trades. |
| `nonStackableCount` | `{ min: 1, max: 2 }` | Items available per individual offer. Max 2 feels nice — loot more, it might come in handy. |
| `itemCountMax` | `2` | Maximum number of items asked for in a barter. Default 2 means 2-for-1 barters at most. |
| `currencyDistribution` | `{ rub: 33, eur: 33, usd: 34 }` | Which currency the cash offers are listed in. Percentages, should add up to 100. SPT default is 78/14/8. |

#### `economyOptions.otherFleaMarketChanges`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | Master toggle for all other flea market changes below. |
| `sellingOnFlea` | `false` | NOT RECOMMENDED TO CHANGE. Default `false` forces the flea sell chance to 0 — no selling on flea at all. Setting it `true` allows selling weapons and other Softcore-blocked items like in vanilla. Not playtested or balanced around. |
| `fleaMarketOpenAtLevel` | `5` | PMC level the flea market opens at. |
| `fleaPricesIncreased` | `1.3` | Slightly increase flea prices to stimulate looting and crafting instead of buying everything on flea. With barter economy and variance enabled you still get many great trades below actual item value. Hustle! |
| `fleaPristineItems` | `true` | Only pristine-condition items are offered on flea. |
| `onlyFoundInRaidItemsAllowedForBarters` | `true` | Be a man, don't change this. Disabling it is borderline cheating: infinite money because of the variance changes. |

### `craftingChanges`
| Option | Default | Description |
|---|---|---|
| `enabled` | `true` | Master toggle for all crafting changes below. |
| `craftingRebalance` | `true` | Major rebalance of crafting recipes around component rarity, usefulness, trader prices and plain "lore" logic. Some nerfs, but a lot of huge buffs. The idea is to make most crafts useful and/or profitable. |
| `additionalCraftingRecipes` | `true` | New custom lore-friendly and balanced crafting recipes for 3-(b-TG), Adrenaline, L1, AHF1, CALOK, Ophthalmoscope, Zagustin, Obdolbos, OLOLO and the secure-container upgrades. |

## Notes:
- No, you cannot use your new fastly mined bitcoins for barters. Because of well, reasons. Like inflation, man. It hits all of us. No one cares about crypto anymore, except you and your nerd friend Mechanic. 
