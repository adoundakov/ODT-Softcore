using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Assets;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies trader changes: sell-price coefficients, buy categories, pacifist Fence, case barters,
/// Skier in Euros and bigger buy limits. Ported from <c>src/changers/TraderChangesChanger.ts</c>.
/// Runs at Preload + 1: trader assorts are never snapshotted (the hourly reset re-clones the live
/// table) and Fence reads <see cref="TraderConfig.Fence"/> at generation time, so a single edit here
/// is persistent.
/// </summary>
[Injectable]
public class TraderChangesChanger(
    ISptLogger<TraderChangesChanger> logger,
    TradersTable tradersTable,
    TemplateTable templateTable,
    TraderConfig traderConfig,
    HandbookHelper handbookHelper,
    ItemHelper itemHelper)
{
    private readonly ISptLogger<TraderChangesChanger> _logger = logger;
    private readonly TradersTable _tradersTable = tradersTable;
    private readonly TemplateTable _templateTable = templateTable;
    private readonly TraderConfig _traderConfig = traderConfig;
    private readonly HandbookHelper _handbookHelper = handbookHelper;
    private readonly ItemHelper _itemHelper = itemHelper;

    public void Apply(TraderChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Trader changes disabled");
            return;
        }

        if (config.BetterSalesToTraders)
        {
            try
            {
                DoBetterSalesToTraders();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Better sales to traders failed: {ex.Message}");
            }
        }

        if (config.AlternativeCategories)
        {
            try
            {
                DoAlternativeCategories();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Alternative categories failed: {ex.Message}");
            }
        }

        if (config.PacifistFence.Enabled)
        {
            try
            {
                DoPacifistFence(config.PacifistFence.NumberOfFenceOffers);
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Pacifist Fence failed: {ex.Message}");
            }
        }

        if (config.ReasonablyPricedCases)
        {
            try
            {
                DoReasonablyPricedCases();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Reasonably priced cases failed: {ex.Message}");
            }
        }

        if (config.SkierUsesEuros)
        {
            try
            {
                DoSkierUsesEuros();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Skier uses Euros failed: {ex.Message}");
            }
        }

        if (config.BiggerLimits.Enabled)
        {
            try
            {
                DoBiggerLimits(config.BiggerLimits.Multiplier);
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Bigger limits failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// buy_price_coef = 35 + adjustment at LL1, then 5 less per loyalty level. Vanilla 4.1.5 is flat per
    /// trader (Prapor 50, Therapist 37, ... Peacekeeper 55), so this is better at LL1 and much better at LL4.
    /// </summary>
    private void DoBetterSalesToTraders()
    {
        foreach (var (traderId, adjustment) in TraderData.BuyPriceAdjustment)
        {
            var loyaltyLevels = _tradersTable.GetTrader(traderId)?.Base.LoyaltyLevels;
            if (loyaltyLevels == null)
            {
                _logger.Warning($"[Softcore] Loyalty levels for trader {traderId} not found, skipping");
                continue;
            }

            for (var i = 0; i < loyaltyLevels.Count; i++)
            {
                loyaltyLevels[i].BuyPriceCoefficient = 35 + adjustment - 5 * i;
            }
        }

        _logger.Success($"[Softcore] Better sales to traders applied for {TraderData.BuyPriceAdjustment.Count} traders");
    }

    private void DoAlternativeCategories()
    {
        var therapist = GetBuyCategories(Traders.THERAPIST);
        if (therapist != null)
        {
            therapist.Remove(BaseClasses.BARTER_ITEM);
            therapist.Add(BaseClasses.MEDICAL_SUPPLIES);
            therapist.Add(BaseClasses.HOUSEHOLD_GOODS);
        }

        GetBuyCategories(Traders.RAGMAN)?.Add(BaseClasses.JEWELRY);
        GetBuyCategories(Traders.SKIER)?.Add(BaseClasses.INFO);

        _logger.Success("[Softcore] Alternative trader buy categories applied");
    }

    private HashSet<MongoId>? GetBuyCategories(MongoId traderId)
    {
        var categories = _tradersTable.GetTrader(traderId)?.Base.ItemsBuy?.Category;
        if (categories == null)
        {
            _logger.Warning($"[Softcore] items_buy categories for trader {traderId} not found, skipping");
        }

        return categories;
    }

    private void DoPacifistFence(int numberOfFenceOffers)
    {
        _logger.Info("[Softcore] Pacifist Fence: not implemented yet");
    }

    private void DoReasonablyPricedCases()
    {
        _logger.Info("[Softcore] Reasonably priced cases: not implemented yet");
    }

    private void DoSkierUsesEuros()
    {
        _logger.Info("[Softcore] Skier uses Euros: not implemented yet");
    }

    /// <summary>
    /// Multiplies every buy restriction on the vanilla traders' assorts. Fence's table assort is empty
    /// at this point and his generated offers carry no restriction, so he is a no-op, as in TS.
    /// </summary>
    private void DoBiggerLimits(double multiplier)
    {
        if (multiplier <= 0)
        {
            _logger.Warning($"[Softcore] biggerLimits.multiplier must be positive, got {multiplier}; skipping");
            return;
        }

        var count = 0;
        foreach (var traderId in TraderData.VanillaTraders)
        {
            var items = _tradersTable.GetTrader(traderId)?.Assort?.Items;
            if (items == null)
            {
                _logger.Warning($"[Softcore] Assort for trader {traderId} not found, skipping");
                continue;
            }

            foreach (var item in items)
            {
                if (item.Upd?.BuyRestrictionMax > 0)
                {
                    item.Upd.BuyRestrictionMax = (int)Math.Round(item.Upd.BuyRestrictionMax.Value * multiplier);
                    count++;
                }
            }
        }

        _logger.Success($"[Softcore] Bigger limits: {count} trader offers multiplied by {multiplier}");
    }
}
