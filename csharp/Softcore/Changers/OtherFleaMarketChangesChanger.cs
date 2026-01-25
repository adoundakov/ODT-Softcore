using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Spt.Config;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies miscellaneous flea market tweaks:
/// - Disable player selling on flea
/// - FIR-only for barters
/// - Pristine items
/// - Increased flea prices
/// - Flea unlock level
/// </summary>
[Injectable]
public class OtherFleaMarketChangesChanger
{
    private readonly ISptLogger<OtherFleaMarketChangesChanger> _logger;
    private readonly ConfigServer _configServer;
    private readonly DatabaseService _databaseService;

    public OtherFleaMarketChangesChanger(
        ISptLogger<OtherFleaMarketChangesChanger> logger,
        ConfigServer configServer,
        DatabaseService databaseService)
    {
        _logger = logger;
        _configServer = configServer;
        _databaseService = databaseService;
    }

    public void Apply(OtherFleaMarketChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Other flea market changes disabled");
            return;
        }

        try
        {
            DoSellingOnFlea(config.SellingOnFlea);
            AdjustOnlyFIRforBarters(config.OnlyFoundInRaidItemsAllowedForBarters);
            AdjustPristineItems(config.FleaPristineItems);
            IncreaseFleaPrices(config.FleaPricesIncreased);
            UpdateRagfairMinUserLevel(config.FleaMarketOpenAtLevel);

            _logger.Success("[Softcore] Other flea market changes applied successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Other flea market changes failed: {ex.Message}");
        }
    }

    private void DoSellingOnFlea(bool enabled)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        if (!enabled)
        {
            ragfairConfig.Sell.Chance.Base = 0;
            ragfairConfig.Sell.Chance.MaxSellChancePercent = 0;
        }
    }

    private void AdjustOnlyFIRforBarters(bool enabled)
    {
        var globals = _databaseService.GetGlobals();
        globals.Configuration.RagFair.IsOnlyFoundInRaidAllowed = enabled;
    }

    private void AdjustPristineItems(bool enabled)
    {
        if (enabled)
        {
            var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
            foreach (var condition in ragfairConfig.Dynamic.Condition.Values)
            {
                condition.ConditionChance = 0;
            }
        }
    }

    private void IncreaseFleaPrices(double multiplier)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.PriceRanges.Default.Max *= multiplier;
        ragfairConfig.Dynamic.PriceRanges.Default.Min *= multiplier;
    }

    public void UpdateRagfairMinUserLevel(int level)
    {
        var globals = _databaseService.GetGlobals();
        globals.Configuration.RagFair.MinUserLevel = level;
    }
}
