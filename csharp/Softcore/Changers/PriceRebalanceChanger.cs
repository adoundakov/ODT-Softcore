using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Common;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Syncs flea market prices to handbook prices and applies item price fixes.
/// </summary>
[Injectable]
public class PriceRebalanceChanger
{
    private readonly ISptLogger<PriceRebalanceChanger> _logger;
    private readonly DatabaseService _databaseService;

    public PriceRebalanceChanger(
        ISptLogger<PriceRebalanceChanger> logger,
        DatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public void Apply(PriceRebalanceConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Price rebalance disabled");
            return;
        }

        try
        {
            _logger.Info("[Softcore] Applying price rebalance...");

            if (config.ItemFixes)
                DoItemFixes();

            DoPriceRebalance();

            _logger.Success("[Softcore] Price rebalance applied successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Price rebalance failed: {ex.Message}");
        }
    }

    private void DoItemFixes()
    {
        // Set specific item prices in handbook
        var handbook = _databaseService.GetHandbook();
        var handbookItems = handbook.Items;

        // Example: fix Bitcoin price
        var bitcoinId = (MongoId)"59faff1d86f7746c51718c9c";
        var bitcoinEntry = handbookItems.FirstOrDefault(h => h.Id == bitcoinId);
        if (bitcoinEntry != null)
        {
            bitcoinEntry.Price = 100000;
        }

        // Additional item fixes can be added here
        _logger.Info("[Softcore] Applied handbook item price fixes");
    }

    private void DoPriceRebalance()
    {
        // Sync all flea prices to handbook prices
        var handbook = _databaseService.GetHandbook();
        var fleaPrices = _databaseService.GetPrices();

        foreach (var item in handbook.Items)
        {
            if (item.Price.HasValue)
            {
                fleaPrices[item.Id] = item.Price.Value;
            }
        }

        // Note: Handbook cache is auto-populated - no need to call HydrateLookup()

        _logger.Info($"[Softcore] Synced {handbook.Items.Count} flea prices to handbook");
    }
}
