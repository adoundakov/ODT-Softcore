using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Common;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

/// <summary>
/// Configures barter economy settings in RagfairConfig.
/// Controls barter chance, price variance, item counts, and blacklists.
/// </summary>
[Injectable]
#pragma warning disable CS0618 // ConfigServer replacement API not yet available in current SPT version
public class BarterEconomyChanger
{
    private readonly ISptLogger<BarterEconomyChanger> _logger;
    private readonly ConfigServer _configServer;
    private readonly DatabaseService _databaseService;
    private readonly ItemHelper _itemHelper;

    public BarterEconomyChanger(
        ISptLogger<BarterEconomyChanger> logger,
        ConfigServer configServer,
        DatabaseService databaseService,
        ItemHelper itemHelper)
    {
        _logger = logger;
        _configServer = configServer;
        _databaseService = databaseService;
        _itemHelper = itemHelper;
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
        var barterBlacklistStrings = FleaMarketData.ActualBaseClasses
            .Where(bc => !FleaMarketData.FleaBarterRequestWhitelist.Contains(bc))
            .ToList();

        var barterBlacklist = barterBlacklistStrings
            .Select(bc => (MongoId)bc)
            .ToHashSet();

        // 2. Apply to ragfair config
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.ItemTypeBlacklist = barterBlacklist;
        ragfairConfig.Dynamic.Barter.MinRoubleCostToBecomeBarter = 100;

        // 3. Adjust flea prices for quest items and non-sellable items
        var items = _databaseService.GetItems();
        var fleaPrices = _databaseService.GetPrices();

        foreach (var (itemId, item) in items)
        {
            if (item.Type == "Item" &&
                !IsOfBaseClasses(itemId, barterBlacklist) &&
                item.Parent != (MongoId)"543be5dd4bdc2deb348b4569") // MONEY base class
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
            fleaPrices[(MongoId)itemId] = price;
        }

        _logger.Info($"[Softcore] Barter blacklist: {barterBlacklistStrings.Count} base classes");
    }

    private void AdjustCashOffers(int cashOffersPercentage)
    {
        // Inverse: cashOffersPercentage=0 means 100% barter
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Dynamic.Barter.ChancePercent = 100 - cashOffersPercentage;
    }

    private void SetupRandomCurrencyDistribution()
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();

        // Set equal distribution across all three currencies for random selection
        // Roubles: 5449016a4bdc2d6f028b456f
        // Euros: 569668774bdc2da2298b4568
        // Dollars: 5696686a4bdc2da3298b456a
        ragfairConfig.Dynamic.OfferCurrencyChangePercent[(MongoId)"5449016a4bdc2d6f028b456f"] = 33; // RUB
        ragfairConfig.Dynamic.OfferCurrencyChangePercent[(MongoId)"569668774bdc2da2298b4568"] = 33; // EUR
        ragfairConfig.Dynamic.OfferCurrencyChangePercent[(MongoId)"5696686a4bdc2da3298b456a"] = 34; // USD

        _logger.Info("[Softcore] Currency distribution: 33% RUB, 33% EUR, 34% USD");
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
        // OfferItemCount is a Dictionary<string, MinMax<int>> - update the "default" key
        ragfairConfig.Dynamic.OfferItemCount["default"] = new MinMax<int>(range.Min, range.Max);
    }

    private void AdjustNonStackableAmount(MinMax range)
    {
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        // NonStackableCount is a MinMax<int> - replace the entire object (records are immutable)
        ragfairConfig.Dynamic.NonStackableCount = new MinMax<int>(range.Min, range.Max);
    }

    private bool IsOfBaseClasses(MongoId itemId, HashSet<MongoId> baseClasses)
    {
        return _itemHelper.IsOfBaseclasses(itemId, baseClasses);
    }
}
#pragma warning restore CS0618
