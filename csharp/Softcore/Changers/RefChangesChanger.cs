using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies the table side of the Ref changes lifted from Geko's Better Progression: what Ref buys and
/// in which currency, and a Streamer Item Case barter for GP. The matching patches are enabled by
/// <see cref="Plugin"/>. Runs at Preload + 1, after <see cref="TraderChangesChanger"/> (which leaves
/// Ref alone); Ref's assort is never snapshotted (trader resets clone the live table), so a single
/// edit here survives every restock.
/// </summary>
[Injectable]
public class RefChangesChanger(
    ISptLogger<RefChangesChanger> logger,
    TradersTable tradersTable,
    TemplateTable templateTable,
    ItemHelper itemHelper)
{
    private readonly ISptLogger<RefChangesChanger> _logger = logger;
    private readonly TradersTable _tradersTable = tradersTable;
    private readonly TemplateTable _templateTable = templateTable;
    private readonly ItemHelper _itemHelper = itemHelper;

    public void Apply(RefChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Ref changes disabled");
            return;
        }

        if (config.BuysInGpCoins || config.OnlyBuysDogtags || config.AlsoBuysLegaMedals)
        {
            try
            {
                DoPurchasingOptions(config);
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Ref purchasing options failed: {ex.Message}");
            }
        }

        if (config.StreamerItemCase.Enabled)
        {
            try
            {
                DoStreamerItemCase(config.StreamerItemCase);
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Streamer Item Case at Ref failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Currency: PaymentService converts every sale through the trader's currency and the GP handbook
    /// price, so the server needs nothing else; the client needs <c>GpCurrencyCoursePatch</c>.
    /// Buy list: vanilla Ref buys the WEAPON, MOD and AMMO categories; dogtags come from
    /// <see cref="ItemHelper.IsDogtag"/> (14 tpls incl. EOD/Unheard/prestige).
    /// </summary>
    private void DoPurchasingOptions(RefChangesConfig config)
    {
        var refBase = _tradersTable.GetTrader(Traders.REF)?.Base;
        if (refBase == null)
        {
            _logger.Warning("[Softcore] Ref not found, skipping");
            return;
        }

        if (config.BuysInGpCoins)
        {
            refBase.Currency = CurrencyType.GP;
        }

        var itemsBuy = refBase.ItemsBuy;
        if (itemsBuy == null)
        {
            _logger.Warning("[Softcore] Ref items_buy not found, skipping buy list changes");
        }
        else
        {
            if (config.OnlyBuysDogtags)
            {
                itemsBuy.Category.Clear();
                itemsBuy.IdList = _templateTable.Items.Keys.Where(_itemHelper.IsDogtag).ToHashSet();
            }

            if (config.AlsoBuysLegaMedals)
            {
                itemsBuy.IdList.Add(ItemTpl.BARTER_LEGA_MEDAL);
            }
        }

        var buys = itemsBuy == null ? "?" : $"{itemsBuy.Category.Count} categories, {itemsBuy.IdList.Count} items";
        _logger.Success($"[Softcore] Ref purchasing options: pays in {refBase.Currency}, buys {buys}");
    }

    /// <summary>Ref's whole assort is GP/Lega priced already, so a flat GP ask is in keeping.</summary>
    private void DoStreamerItemCase(StreamerItemCaseConfig config)
    {
        if (config.LoyaltyLevel is < 1 or > 4 || config.GpPrice <= 0 || config.BuyLimit <= 0)
        {
            _logger.Warning($"[Softcore] streamerItemCase needs loyaltyLevel 1–4 and positive gpPrice/buyLimit, got LL{config.LoyaltyLevel}, {config.GpPrice} GP, limit {config.BuyLimit}; skipping");
            return;
        }

        var assort = _tradersTable.GetTrader(Traders.REF)?.Assort;
        if (assort == null)
        {
            _logger.Warning("[Softcore] Ref assort not found, skipping");
            return;
        }

        var tpl = ItemTpl.CONTAINER_STREAMER_ITEM_CASE;
        if (assort.Items.Any(item => item.Template == tpl && item.ParentId == "hideout"))
        {
            _logger.Warning($"[Softcore] Ref already sells {_itemHelper.GetItemName(tpl)}, skipping");
            return;
        }

        AssortHelper.CreateBarter(assort, tpl, config.LoyaltyLevel, config.BuyLimit,
            [new BarterScheme { Template = Money.GP, Count = config.GpPrice }]);

        _logger.Success($"[Softcore] Added {_itemHelper.GetItemName(tpl)} barter at Ref LL{config.LoyaltyLevel}: {config.GpPrice} GP, limit {config.BuyLimit}");
    }
}
