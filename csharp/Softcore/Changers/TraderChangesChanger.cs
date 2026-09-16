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

    /// <summary>
    /// Fence sells only the pacifist categories, no weapon/equipment presets, at handbook price
    /// (0.82× in the 6-karma discount assort). All of it goes through <see cref="TraderConfig.Fence"/>,
    /// which FenceBaseAssortGenerator / FenceService read at generation time, after us.
    /// </summary>
    private void DoPacifistFence(int numberOfFenceOffers)
    {
        if (numberOfFenceOffers <= 0)
        {
            _logger.Warning($"[Softcore] pacifistFence.numberOfFenceOffers must be positive, got {numberOfFenceOffers}; skipping");
            return;
        }

        var fence = _traderConfig.Fence;
        var whitelist = TraderData.PacifistFenceWhitelist;

        // Blacklist = every base class an item template uses, minus the whitelist. Derived at runtime
        // (the TS shipped a static list generated the same way) so categories added after 3.11 and by
        // other mods are blacklisted too, which is the failure mode we want for a pacifist Fence.
        // FenceBaseAssortGenerator checks Blacklist with IsOfBaseclasses, so base-class ids work here.
        // Quest items are not added: itemHelper.IsValidItem already rejects them before the blacklist
        // is consulted, so the TS union was redundant.
        var fenceBlacklist = _templateTable.Items.Values
            .Where(item => item.Type == "Item")
            .Select(item => item.Parent)
            .Where(parent => !whitelist.Contains(parent))
            .ToHashSet();

        fence.Blacklist.UnionWith(fenceBlacklist);
        fence.Blacklist.UnionWith(FleaMarketData.BSGBlacklist);
        fence.Blacklist.Add(ItemTpl.INFO_ENCRYPTED_FLASH_DRIVE);

        // Vanilla caps several whitelisted classes at 0 (INFO, STIMULATOR, ...), which would hide them
        // from Fence entirely. Replace the table with numberOfFenceOffers per whitelisted class; every
        // other class is blacklisted anyway. Note that on 4.1.5 FenceService never increments the
        // per-class counter (it mutates a tuple copy), so any non-zero value means "unlimited" for now.
        // MEDICAL_SUPPLIES is left out of the limits and the duplicate guard: the TS found that
        // including it broke Fence generation ("SPT GITM BUG ... wasted 3 hours"). Absent = unlimited.
        fence.ItemTypeLimits.Clear();
        fence.PreventDuplicateOffersOfCategory.Clear();
        foreach (var baseClass in whitelist.Where(cls => cls != BaseClasses.MEDICAL_SUPPLIES))
        {
            fence.ItemTypeLimits[baseClass] = numberOfFenceOffers;
            fence.PreventDuplicateOffersOfCategory.Add(baseClass);
        }

        fence.AssortSize = numberOfFenceOffers;
        fence.EquipmentPresetMinMax.Min = 0;
        fence.EquipmentPresetMinMax.Max = 0;
        fence.WeaponPresetMinMax.Min = 0;
        fence.WeaponPresetMinMax.Max = 0;
        fence.ItemPriceMult = 1;
        fence.DiscountOptions.AssortSize = numberOfFenceOffers * 2;
        fence.DiscountOptions.ItemPriceMult = 0.82;
        fence.DiscountOptions.WeaponPresetMinMax.Min = 0;
        fence.DiscountOptions.WeaponPresetMinMax.Max = 0;
        fence.DiscountOptions.EquipmentPresetMinMax.Min = 0;
        fence.DiscountOptions.EquipmentPresetMinMax.Max = 0;

        _logger.Success($"[Softcore] Pacifist Fence: {numberOfFenceOffers} offers, {fenceBlacklist.Count} base classes blacklisted");
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
