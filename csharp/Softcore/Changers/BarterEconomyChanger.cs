using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

/// <summary>
/// Configures barter economy settings in RagfairConfig.
/// Controls barter chance, price variance, item counts, and blacklists.
/// </summary>
[Injectable]
public class BarterEconomyChanger
{
    private readonly ISptLogger<BarterEconomyChanger> _logger;
    // TODO: Inject when SPT API namespaces are confirmed:
    // - IConfigServer (for RagfairConfig)
    // - IDatabaseService (for GetItems(), GetPrices())
    // - IItemHelper (for IsOfBaseclasses())

    public BarterEconomyChanger(ISptLogger<BarterEconomyChanger> logger)
    {
        _logger = logger;
    }

    public void Apply(BarterEconomyConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Barter economy disabled");
            return;
        }

        try
        {
            _logger.Info("[Softcore] Applying barter economy settings...");

            // TODO: Implement after API research confirms exact types
            // DoBarterEconomy(config);
            // AdjustCashOffers(config.CashOffersPercentage);
            // AdjustBarterPriceVariance(config.BarterPriceVariance);
            // AdjustItemCountMax(config.ItemCountMax);
            // AdjustOfferItemCount(config.OfferItemCount);
            // AdjustNonStackableAmount(config.NonStackableCount);

            _logger.Warning("[Softcore] Barter economy - NOT YET IMPLEMENTED (awaiting API research)");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Barter economy failed: {ex.Message}");
        }
    }

    // TODO: Implement these methods after confirming SPT 4.0 API
    /*
    private void DoBarterEconomy(BarterEconomyConfig config)
    {
        // 1. Compute barter blacklist = all base classes NOT in whitelist
        var barterBlacklist = FleaMarketData.ActualBaseClasses
            .Where(bc => !FleaMarketData.FleaBarterRequestWhitelist.Contains(bc))
            .ToList();

        // 2. Apply to ragfair config
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.ItemTypeBlacklist = barterBlacklist.ToHashSet();
        ragfairConfig.Dynamic.Barter.MinRoubleCostToBecomeBarter = 100;

        // 3. Adjust flea prices for quest items and non-sellable items
        var items = _databaseService.GetItems();
        var fleaPrices = _databaseService.GetPrices();

        foreach (var (itemId, item) in items)
        {
            if (item.Type == "Item" &&
                !IsOfBaseClasses(itemId, barterBlacklist) &&
                item.Parent != "543be5dd4bdc2deb348b4569") // MONEY base class
            {
                if (item.Properties.QuestItem == true)
                {
                    fleaPrices[itemId] = 0;
                }
                else if (!item.Properties.CanSellOnRagfair.GetValueOrDefault(false))
                {
                    fleaPrices[itemId] = 0;
                }
            }
        }

        // 4. Apply whitelist overrides
        foreach (var (itemId, price) in FleaMarketData.RequestWhitelist)
        {
            fleaPrices[itemId] = price;
        }

        _logger.Info($"[Softcore] Barter blacklist: {barterBlacklist.Count} base classes");
    }

    private void AdjustCashOffers(int cashOffersPercentage)
    {
        // Inverse: cashOffersPercentage=0 means 100% barter
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.ChancePercent = 100 - cashOffersPercentage;
    }

    private void AdjustBarterPriceVariance(int variancePercent)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.PriceRangeVariancePercent = variancePercent;
    }

    private void AdjustItemCountMax(int max)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.ItemCountMax = max;
    }

    private void AdjustOfferItemCount(MinMax range)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.OfferItemCount = new { Min = range.Min, Max = range.Max };
    }

    private void AdjustNonStackableAmount(MinMax range)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.NonStackableCount = new { Min = range.Min, Max = range.Max };
    }

    private bool IsOfBaseClasses(string itemId, List<string> baseClasses)
    {
        // Use ItemHelper.IsOfBaseclasses() when available
        return _itemHelper.IsOfBaseclasses(itemId, baseClasses);
    }
    */
}
