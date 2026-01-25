namespace Softcore.Config;

// Root configuration
public class Configuration
{
    public GeneralConfig General { get; set; } = new();
    public EconomyOptionsConfig EconomyOptions { get; set; } = new();
    public CraftingChangesConfig CraftingChanges { get; set; } = new();
}

// General settings
public class GeneralConfig
{
    public bool Enabled { get; set; } = true;
    public bool Debug { get; set; } = false;
}

// Economy options (CORE for Phase 2)
public class EconomyOptionsConfig
{
    public bool Enabled { get; set; } = true;
    public bool DisableFleaMarketCompletely { get; set; } = false;
    public PriceRebalanceConfig PriceRebalance { get; set; } = new();
    public PacifistFleaMarketConfig PacifistFleaMarket { get; set; } = new();
    public BarterEconomyConfig BarterEconomy { get; set; } = new();
    public OtherFleaMarketChangesConfig OtherFleaMarketChanges { get; set; } = new();
}

// Barter economy settings
public class BarterEconomyConfig
{
    public bool Enabled { get; set; } = true;
    public int CashOffersPercentage { get; set; } = 15;       // 15% cash, 85% barter
    public int BarterPriceVariance { get; set; } = 50;         // ±50%
    public MinMax OfferItemCount { get; set; } = new() { Min = 10, Max = 20 };
    public MinMax NonStackableCount { get; set; } = new() { Min = 1, Max = 2 };
    public int ItemCountMax { get; set; } = 2;
    public bool UnbanBitcoinsForBarters { get; set; } = false;
}

// Price rebalance settings
public class PriceRebalanceConfig
{
    public bool Enabled { get; set; } = true;
    public bool ItemFixes { get; set; } = true;
}

// Pacifist flea market settings
public class PacifistFleaMarketConfig
{
    public bool Enabled { get; set; } = true;
    public EconomyTogglesConfig Whitelist { get; set; } = new();
    public EconomyTogglesConfig QuestKeys { get; set; } = new();
    public EconomyTogglesConfig MarkedKeys { get; set; } = new();
}

public class EconomyTogglesConfig
{
    public bool Enabled { get; set; } = true;
    public double PriceMultiplier { get; set; } = 2.0;
}

// Other flea market settings
public class OtherFleaMarketChangesConfig
{
    public bool Enabled { get; set; } = true;
    public bool SellingOnFlea { get; set; } = false;
    public int FleaMarketOpenAtLevel { get; set; } = 5;
    public double FleaPricesIncreased { get; set; } = 1.3;
    public bool FleaPristineItems { get; set; } = true;
    public bool OnlyFoundInRaidItemsAllowedForBarters { get; set; } = true;
}

// Crafting changes settings
public class CraftingChangesConfig
{
    public bool Enabled { get; set; } = true;
    public bool CraftingRebalance { get; set; } = true;
    public bool AdditionalCraftingRecipes { get; set; } = true;
}

// Utility class for ranges
public class MinMax
{
    public int Min { get; set; }
    public int Max { get; set; }
}
