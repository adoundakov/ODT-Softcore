using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies Ref changes lifted from Geko's Better Progression: a Streamer Item Case barter for GP.
/// Runs at Preload + 1, after <see cref="TraderChangesChanger"/>; Ref's assort is never snapshotted
/// (trader resets clone the live table), so a single edit here survives every restock.
/// </summary>
[Injectable]
public class RefChangesChanger(
    ISptLogger<RefChangesChanger> logger,
    TradersTable tradersTable,
    ItemHelper itemHelper)
{
    private readonly ISptLogger<RefChangesChanger> _logger = logger;
    private readonly TradersTable _tradersTable = tradersTable;
    private readonly ItemHelper _itemHelper = itemHelper;

    public void Apply(RefChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Ref changes disabled");
            return;
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
