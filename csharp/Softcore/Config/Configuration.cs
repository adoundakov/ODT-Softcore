using System.Text.Json.Serialization;

namespace Softcore.Config;

// Every property carries [JsonPropertyName] because the server's JsonUtil (used by the dashboard
// config editor) has no naming policy and is case-sensitive — config.json is camelCase.
// Option docs live here as <summary> and in README.md, not in config.json: the editor rewrites
// that file through System.Text.Json and would strip comments on the first save.

/// <summary>Root of config/config.json.</summary>
public class Configuration
{
    /// <summary>
    /// Shown as the first line in the dashboard config editor, which has no other way to display
    /// a description. Round-trips to disk; harmless.
    /// </summary>
    [JsonPropertyName("_note")]
    public string Note { get; set; } = "Edits take effect after a server restart.";

    [JsonPropertyName("general")]
    public GeneralConfig General { get; set; } = new();

    [JsonPropertyName("economyOptions")]
    public EconomyOptionsConfig EconomyOptions { get; set; } = new();

    [JsonPropertyName("traderChanges")]
    public TraderChangesConfig TraderChanges { get; set; } = new();

    [JsonPropertyName("craftingChanges")]
    public CraftingChangesConfig CraftingChanges { get; set; } = new();

    /// <summary>False when config.json was missing and class-initializer defaults are in use.</summary>
    [JsonIgnore]
    public bool LoadedFromDisk { get; set; } = true;

    [JsonIgnore]
    public string? ConfigPath { get; set; }
}

public class GeneralConfig
{
    /// <summary>Enable or disable the mod.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Enable debugging mode. Currently does nothing.</summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;
}

public class EconomyOptionsConfig
{
    /// <summary>Master toggle for all economy options below.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Completely disable flea market for a true HARDCORE experience. Still allows you to use the
    /// interface and see trader offers. Overrides all other flea changes below.
    /// </summary>
    [JsonPropertyName("disableFleaMarketCompletely")]
    public bool DisableFleaMarketCompletely { get; set; } = false;

    [JsonPropertyName("priceRebalance")]
    public PriceRebalanceConfig PriceRebalance { get; set; } = new();

    [JsonPropertyName("pacifistFleaMarket")]
    public PacifistFleaMarketConfig PacifistFleaMarket { get; set; } = new();

    [JsonPropertyName("barterEconomy")]
    public BarterEconomyConfig BarterEconomy { get; set; } = new();

    [JsonPropertyName("otherFleaMarketChanges")]
    public OtherFleaMarketChangesConfig OtherFleaMarketChanges { get; set; } = new();
}

public class PriceRebalanceConfig
{
    /// <summary>
    /// CORE feature of this mod. Completely removes the SPT flea price snapshot from LIVE and matches
    /// prices to the internal handbook/trader prices. Everything else is balanced around this.
    /// NOT recommended to disable.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Handbook price fixes for important items like the intel folder and military flash drive.</summary>
    [JsonPropertyName("itemFixes")]
    public bool ItemFixes { get; set; } = true;
}

public class PacifistFleaMarketConfig
{
    /// <summary>
    /// CORE feature of this mod. Only meds, barter items, food and info items can be bought on the flea
    /// market. Uses the hardcoded handbook-category whitelist as filter. NOT recommended to disable.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// A small list of items used in crafts and trader barters is available on flea.
    /// Uses the hardcoded item whitelist.
    /// </summary>
    [JsonPropertyName("whitelist")]
    public EconomyTogglesConfig Whitelist { get; set; } = new();

    /// <summary>Random-only QUEST keys are available on flea. Uses the hardcoded quest-key list.</summary>
    [JsonPropertyName("questKeys")]
    public EconomyTogglesConfig QuestKeys { get; set; } = new();

    /// <summary>Marked keys are available on flea.</summary>
    [JsonPropertyName("markedKeys")]
    public EconomyTogglesConfig MarkedKeys { get; set; } = new();
}

/// <summary>An on/off switch plus a flea price multiplier for the items it re-allows.</summary>
public class EconomyTogglesConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Flea price multiplier applied to the re-allowed items.</summary>
    [JsonPropertyName("priceMultiplier")]
    public double PriceMultiplier { get; set; } = 2.0;
}

public class BarterEconomyConfig
{
    /// <summary>
    /// CORE feature of this mod. Only allows buying items on flea using other random FiR or crafted
    /// items. Uses the hardcoded barter blacklist as filter for allowed items (meds, barter items, food
    /// and info items are enabled; exceptions are stimulants and fuel). NOT recommended to disable.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Allow a small, random percentage of listings to be buyable for cash. 0 is a true barter-only
    /// economy (except the cheapest items like AA battery, an SPT limitation) — the preferred way to
    /// play, but sometimes a little too hard. 15% makes life just a bit easier and avoids item
    /// deadlocks. Recommendation: 0 to 15.
    /// </summary>
    [JsonPropertyName("cashOffersPercentage")]
    public int CashOffersPercentage { get; set; } = 15;

    /// <summary>
    /// ± percent of price variance between an item listing and its barter value. Bigger number — more
    /// wild and varied random trades, e.g. a Defibrillator (224k) offered for a Lion (162k) or a Tank
    /// Battery (330k). This CORE feature makes the whole mod tick: you can get lucky and get a great
    /// deal, or desperately need an overvalued item and only have the expensive item it asks for.
    /// More variance also means it is easier to find an offer you have an item for. Recommendation: 20–50.
    /// </summary>
    [JsonPropertyName("barterPriceVariance")]
    public int BarterPriceVariance { get; set; } = 50;

    /// <summary>
    /// Number of different offers per item. Too low a number breaks the SPT server with constant
    /// client errors on completed trades. More offers means more random trade variance anyway.
    /// </summary>
    [JsonPropertyName("offerItemCount")]
    public MinMax OfferItemCount { get; set; } = new() { Min = 10, Max = 20 };

    /// <summary>Items available per individual offer. Max 2 feels nice — loot more, it might come in handy.</summary>
    [JsonPropertyName("nonStackableCount")]
    public MinMax NonStackableCount { get; set; } = new() { Min = 1, Max = 2 };

    /// <summary>Maximum number of items asked for in a barter. Default 2 means 2-for-1 barters at most.</summary>
    [JsonPropertyName("itemCountMax")]
    public int ItemCountMax { get; set; } = 2;

    /// <summary>
    /// Which currency the cash offers (see <see cref="CashOffersPercentage"/>) are listed in.
    /// Percentages, should add up to 100. SPT default is 78/14/8.
    /// </summary>
    [JsonPropertyName("currencyDistribution")]
    public CurrencyDistributionConfig CurrencyDistribution { get; set; } = new();
}

public class CurrencyDistributionConfig
{
    [JsonPropertyName("rub")]
    public int Rub { get; set; } = 33;

    [JsonPropertyName("eur")]
    public int Eur { get; set; } = 33;

    [JsonPropertyName("usd")]
    public int Usd { get; set; } = 34;
}

public class OtherFleaMarketChangesConfig
{
    /// <summary>Master toggle for all other flea market changes below.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// NOT RECOMMENDED TO CHANGE. Default false forces the flea sell chance to 0 — no selling on flea at
    /// all. Setting it true allows selling weapons and other Softcore-blocked items like in vanilla.
    /// Not playtested or balanced around.
    /// </summary>
    [JsonPropertyName("sellingOnFlea")]
    public bool SellingOnFlea { get; set; } = false;

    /// <summary>PMC level the flea market opens at.</summary>
    [JsonPropertyName("fleaMarketOpenAtLevel")]
    public int FleaMarketOpenAtLevel { get; set; } = 5;

    /// <summary>
    /// Slightly increase flea prices to stimulate looting and crafting instead of buying everything
    /// on flea. With barter economy and variance enabled you still get many great trades below actual
    /// item value. Hustle!
    /// </summary>
    [JsonPropertyName("fleaPricesIncreased")]
    public double FleaPricesIncreased { get; set; } = 1.3;

    /// <summary>Only pristine-condition items are offered on flea.</summary>
    [JsonPropertyName("fleaPristineItems")]
    public bool FleaPristineItems { get; set; } = true;

    /// <summary>
    /// Be a man, don't change this. Disabling it is borderline cheating: infinite money because of
    /// the variance changes.
    /// </summary>
    [JsonPropertyName("onlyFoundInRaidItemsAllowedForBarters")]
    public bool OnlyFoundInRaidItemsAllowedForBarters { get; set; } = true;
}

public class TraderChangesConfig
{
    /// <summary>Master toggle for all trader changes below.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Traders pay more when you sell to them. At loyalty level 1: Prapor 50% → 60%, Therapist 63% → 64%,
    /// Ragman 62% → 63%, Jaeger 60% → 62%, Mechanic 56% → 61%, Peacekeeper 45% → 58%, Skier 49% → 59%.
    /// Every further loyalty level adds 5%, so at LL4 Prapor pays 75%. Fence and Ref are unchanged.
    /// </summary>
    [JsonPropertyName("betterSalesToTraders")]
    public bool BetterSalesToTraders { get; set; } = true;

    /// <summary>
    /// Nerfs Therapist's buying categories (instead of all barter items she buys only medical supplies
    /// and household goods, good for trader diversity), allows Ragman to buy valuables and Skier to buy
    /// info items.
    /// </summary>
    [JsonPropertyName("alternativeCategories")]
    public bool AlternativeCategories { get; set; } = true;

    [JsonPropertyName("pacifistFence")]
    public PacifistFenceConfig PacifistFence { get; set; } = new();

    /// <summary>
    /// Rebalances the hideout case barters (Item Case, THICC Item Case, Lucky Scav Junk Box, Medicine
    /// Case, Weapon Case) to fair and reasonable prices, changes the LEDX dogtag barter and adds a
    /// Golden neck chain dogtag barter at Therapist.
    /// </summary>
    [JsonPropertyName("reasonablyPricedCases")]
    public bool ReasonablyPricedCases { get; set; } = true;

    /// <summary>
    /// EXPERIMENTAL. Makes Skier use Euros for all trades and quest rewards, mostly for fun and diversity.
    /// Adjusts assorts and loyalty levels accordingly. Off by default because existing profiles need
    /// their Skier salesSum adjusted by hand: in your profile json find TradersInfo →
    /// 58330581ace78e27b8b10cee and divide salesSum by the EUR handbook price (134 on 4.1.5), dropping
    /// the remainder. Profiles started with this enabled need nothing.
    /// </summary>
    [JsonPropertyName("skierUsesEuros")]
    public bool SkierUsesEuros { get; set; } = false;

    [JsonPropertyName("biggerLimits")]
    public BiggerLimitsConfig BiggerLimits { get; set; } = new();
}

public class PacifistFenceConfig
{
    /// <summary>
    /// To go along with the theme of this mod, Fence also sells only pacifist items. His prices depend on
    /// scav karma, so at 6 karma he sells items at almost the same price Therapist buys them from you.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Number of items in Fence's regular assort. The discount assort (6 karma) is twice that.</summary>
    [JsonPropertyName("numberOfFenceOffers")]
    public int NumberOfFenceOffers { get; set; } = 30;
}

public class BiggerLimitsConfig
{
    /// <summary>Multiply every trader item's buy limit (e.g. "3 per restock") by <see cref="Multiplier"/>.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("multiplier")]
    public double Multiplier { get; set; } = 2.0;
}

public class CraftingChangesConfig
{
    /// <summary>Master toggle for all crafting changes below.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Major rebalance of crafting recipes around component rarity, usefulness, trader prices and plain
    /// "lore" logic. Some nerfs, but a lot of huge buffs. The idea is to make most crafts useful and/or
    /// profitable.
    /// </summary>
    [JsonPropertyName("craftingRebalance")]
    public bool CraftingRebalance { get; set; } = true;

    /// <summary>
    /// New custom lore-friendly and balanced crafting recipes for 3-(b-TG), Adrenaline, L1, AHF1, CALOK,
    /// Ophthalmoscope, Zagustin, Obdolbos, OLOLO and the secure-container upgrades.
    /// </summary>
    [JsonPropertyName("additionalCraftingRecipes")]
    public bool AdditionalCraftingRecipes { get; set; } = true;
}

/// <summary>Inclusive integer range.</summary>
public class MinMax
{
    [JsonPropertyName("min")]
    public int Min { get; set; }

    [JsonPropertyName("max")]
    public int Max { get; set; }
}
