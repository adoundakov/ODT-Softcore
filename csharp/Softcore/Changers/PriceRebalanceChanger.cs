using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
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

    /// <summary>
    /// Handbook price overrides for items whose vanilla handbook value is far off their real worth
    /// (ported 1:1 from the TS mod's PriceRebalanceChanger.doItemFixes).
    /// </summary>
    private static readonly Dictionary<MongoId, double> ItemFixes = new()
    {
        [ItemTpl.VISORS_ROUND_FRAME_SUNGLASSES] = 3084 * 5,
        [ItemTpl.AMMO_40MMRU_VOG25] = 6750 * 5,
        [ItemTpl.VISORS_ANTIFRAGMENTATION_GLASSES] = 2181 * 2,
        [ItemTpl.BACKPACK_LOLKEK_3F_TRANSFER_TOURIST] = 18000 * 2,
        [ItemTpl.FOOD_EMELYA_RYE_CROUTONS] = 1500,
        [ItemTpl.FOOD_RYE_CROUTONS] = 2000,
        [ItemTpl.INFO_INTELLIGENCE_FOLDER] = 588000,
        [ItemTpl.INFO_MILITARY_FLASH_DRIVE] = 224400,
        [ItemTpl.BARTER_CASE_KEY] = 32524 * 20, // Skier contraband case key
    };

    private void DoItemFixes()
    {
        // HandbookHelper caches prices lazily on first GetTemplatePrice() call and has no public
        // re-hydrate (the TS mod called hydrateLookup() here). Running Plugin at Preload + 1, before
        // any caller, is the mitigation — see Plugin.cs.
        var handbookItems = _templateTable.Handbook.Items;

        var fixedCount = 0;
        foreach (var (itemId, price) in ItemFixes)
        {
            var entry = handbookItems.FirstOrDefault(h => h.Id == itemId);
            if (entry == null)
            {
                _logger.Error($"[Softcore] Item {itemId} not found in handbook, skipping price fix");
                continue;
            }

            entry.Price = price;
            fixedCount++;
        }

        _logger.Info($"[Softcore] Applied {fixedCount}/{ItemFixes.Count} handbook item price fixes");
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
