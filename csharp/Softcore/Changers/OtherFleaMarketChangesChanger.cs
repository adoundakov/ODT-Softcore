using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
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
    // TODO: Add SPT service dependencies when API is confirmed
    // private readonly IConfigServer _configServer;
    // private readonly IDatabaseService _databaseService;

    public OtherFleaMarketChangesChanger(ISptLogger<OtherFleaMarketChangesChanger> logger)
    {
        _logger = logger;
        // TODO: Inject SPT services: IConfigServer, IDatabaseService
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
            // TODO: Implement when SPT APIs are available
            // DoSellingOnFlea(config.SellingOnFlea);
            // AdjustOnlyFIRforBarters(config.OnlyFoundInRaidItemsAllowedForBarters);
            // AdjustPristineItems(config.FleaPristineItems);
            // IncreaseFleaPrices(config.FleaPricesIncreased);
            // UpdateRagfairMinUserLevel(config.FleaMarketOpenAtLevel);

            _logger.Warning("[Softcore] Other flea market changes - NOT YET IMPLEMENTED (awaiting API research)");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Other flea market changes failed: {ex.Message}");
        }
    }

    // TODO: Implement these methods after API research
    /*
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
    */
}
