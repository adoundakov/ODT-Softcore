using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Common;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Syncs flea market prices to handbook prices and applies item price fixes.
/// </summary>
[Injectable]
public class PriceRebalanceChanger(
    ISptLogger<PriceRebalanceChanger> logger,
    TemplateTable templateTable)
{
    private readonly ISptLogger<PriceRebalanceChanger> _logger = logger;
    private readonly TemplateTable _templateTable = templateTable;

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
        // Set specific item prices in handbook.
        // HandbookHelper caches prices lazily on first GetTemplatePrice() call and has no public
        // re-hydrate, so this must run before any caller — Plugin's Preload + 1 priority is the mitigation.
        var handbook = _templateTable.Handbook;
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
        var handbook = _templateTable.Handbook;
        var fleaPrices = _templateTable.Prices;

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
