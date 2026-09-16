using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
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
public class OtherFleaMarketChangesChanger(
    ISptLogger<OtherFleaMarketChangesChanger> logger,
    RagfairConfig ragfairConfig,
    GlobalTable globalTable)
{
    private readonly ISptLogger<OtherFleaMarketChangesChanger> _logger = logger;
    private readonly RagfairConfig _ragfairConfig = ragfairConfig;
    private readonly GlobalTable _globalTable = globalTable;

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
        if (!enabled)
        {
            _ragfairConfig.Sell.Chance.Base = 0;
            _ragfairConfig.Sell.Chance.MaxSellChancePercent = 0;
        }
    }

    private void AdjustOnlyFIRforBarters(bool enabled)
    {
        _globalTable.Configuration.RagFair.IsOnlyFoundInRaidAllowed = enabled;
    }

    private void AdjustPristineItems(bool enabled)
    {
        if (enabled)
        {
            foreach (var condition in _ragfairConfig.Dynamic.Condition.Values)
            {
                condition.ConditionChance = 0;
            }
        }
    }

    private void IncreaseFleaPrices(double multiplier)
    {
        _ragfairConfig.Dynamic.PriceRanges.Default.Max *= multiplier;
        _ragfairConfig.Dynamic.PriceRanges.Default.Min *= multiplier;
    }

    public void UpdateRagfairMinUserLevel(int level)
    {
        _globalTable.Configuration.RagFair.MinUserLevel = level;
    }
}
