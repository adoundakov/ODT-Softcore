using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

/// <summary>
/// Configures barter economy settings in RagfairConfig.
/// Controls barter chance, price variance, item counts, and blacklists.
/// </summary>
[Injectable]
public class BarterEconomyChanger(
    ISptLogger<BarterEconomyChanger> logger,
    RagfairConfig ragfairConfig,
    TemplateTable templateTable,
    ItemHelper itemHelper)
{
    private readonly ISptLogger<BarterEconomyChanger> _logger = logger;
    private readonly RagfairConfig _ragfairConfig = ragfairConfig;
    private readonly TemplateTable _templateTable = templateTable;
    private readonly ItemHelper _itemHelper = itemHelper;

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

            DoBarterEconomy(config);
            AdjustCashOffers(config.CashOffersPercentage);
            SetupRandomCurrencyDistribution();
            AdjustBarterPriceVariance(config.BarterPriceVariance);
            AdjustItemCountMax(config.ItemCountMax);
            AdjustOfferItemCount(config.OfferItemCount);
            AdjustNonStackableAmount(config.NonStackableCount);

            _logger.Success("[Softcore] Barter economy applied successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Barter economy failed: {ex.Message}");
        }
    }

    private void DoBarterEconomy(BarterEconomyConfig config)
    {
        // 1. Compute barter blacklist = all base classes NOT in whitelist
        var barterBlacklist = FleaMarketData.ActualBaseClasses
            .Where(bc => !FleaMarketData.FleaBarterRequestWhitelist.Contains(bc))
            .ToHashSet();

        // 2. Apply to ragfair config
        _ragfairConfig.Dynamic.Barter.ItemTypeBlacklist = barterBlacklist;
        _ragfairConfig.Dynamic.Barter.MinRoubleCostToBecomeBarter = 100;

        // 3. Adjust flea prices for quest items and non-sellable items
        var items = _templateTable.Items;
        var fleaPrices = _templateTable.Prices;

        foreach (var (itemId, item) in items)
        {
            if (item.Type == "Item" &&
                !IsOfBaseClasses(itemId, barterBlacklist) &&
                item.Parent != BaseClasses.MONEY)
            {
                if (item.Properties?.QuestItem == true)
                {
                    fleaPrices[itemId] = 0;
                }
                else if (!item.Properties?.CanSellOnRagfair.GetValueOrDefault(false) ?? false)
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
        _ragfairConfig.Dynamic.Barter.ChancePercent = 100 - cashOffersPercentage;
    }

    private void SetupRandomCurrencyDistribution()
    {
        // Set equal distribution across all three currencies for random selection
        _ragfairConfig.Dynamic.OfferCurrencyChangePercent[ItemTpl.MONEY_ROUBLES] = 33;
        _ragfairConfig.Dynamic.OfferCurrencyChangePercent[ItemTpl.MONEY_EUROS] = 33;
        _ragfairConfig.Dynamic.OfferCurrencyChangePercent[ItemTpl.MONEY_DOLLARS] = 34;

        _logger.Info("[Softcore] Currency distribution: 33% RUB, 33% EUR, 34% USD");
    }

    private void AdjustBarterPriceVariance(int variancePercent)
    {
        _ragfairConfig.Dynamic.Barter.PriceRangeVariancePercent = variancePercent;
    }

    private void AdjustItemCountMax(int max)
    {
        _ragfairConfig.Dynamic.Barter.ItemCountMax = max;
    }

    private void AdjustOfferItemCount(MinMax range)
    {
        // OfferItemCount is a Dictionary<string, MinMax<int>> - update the "default" key
        _ragfairConfig.Dynamic.OfferItemCount["default"] = new MinMax<int>(range.Min, range.Max);
    }

    private void AdjustNonStackableAmount(MinMax range)
    {
        // NonStackableCount is a MinMax<int> - replace the entire object (records are immutable)
        _ragfairConfig.Dynamic.NonStackableCount = new MinMax<int>(range.Min, range.Max);
    }

    private bool IsOfBaseClasses(MongoId itemId, HashSet<MongoId> baseClasses)
    {
        return _itemHelper.IsOfBaseclasses(itemId, baseClasses);
    }
}
