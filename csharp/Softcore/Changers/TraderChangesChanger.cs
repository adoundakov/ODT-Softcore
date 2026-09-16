using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
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

    /// <summary>
    /// Applies <see cref="TraderData.CaseAdjustments"/>, then <see cref="TraderData.CaseBarterSets"/>,
    /// then <see cref="TraderData.CaseBarterAdds"/>. On clean 4.1.5 data nothing is skipped; a skip
    /// means BSG moved a barter again.
    /// </summary>
    private void DoReasonablyPricedCases()
    {
        var applied = 0;
        var skipped = 0;

        foreach (var adjust in TraderData.CaseAdjustments)
        {
            if (AdjustBarters(adjust)) applied++; else skipped++;
        }

        foreach (var set in TraderData.CaseBarterSets)
        {
            if (SetBarter(set)) applied++; else skipped++;
        }

        foreach (var add in TraderData.CaseBarterAdds)
        {
            AddBarter(add);
            applied++;
        }

        _logger.Success($"[Softcore] Reasonably priced cases: {applied} applied, {skipped} skipped");
    }

    private bool AdjustBarters(CaseBarterAdjust adjust)
    {
        var assort = _tradersTable.GetTrader(adjust.Trader)?.Assort;
        var roots = assort == null ? [] : CaseRoots(assort, adjust.Item).ToList();
        if (assort == null || roots.Count == 0)
        {
            _logger.Warning($"[Softcore] Barter for {ItemName(adjust.Item)} at {TraderName(adjust.Trader)} not found, skipping");
            return false;
        }

        var touched = 0;
        foreach (var (adjustmentTpl, callback) in adjust.Adjustments)
        {
            foreach (var root in roots)
            {
                if (!assort.BarterScheme.TryGetValue(root.Id, out var schemes)) continue;
                if (!schemes.Any(scheme => scheme.Any(requirement => requirement.Template == adjustmentTpl))) continue;

                // TS semantic: every requirement of the barter, not just the matching one
                foreach (var requirement in schemes[0])
                {
                    callback(requirement);
                }
                touched++;
            }
        }

        if (touched == 0)
        {
            _logger.Warning($"[Softcore] No barter for {ItemName(adjust.Item)} at {TraderName(adjust.Trader)} contains the adjusted items, skipping");
            return false;
        }

        _logger.Info($"[Softcore] Adjusted {touched} {ItemName(adjust.Item)} barter(s) at {TraderName(adjust.Trader)}");
        return true;
    }

    private bool SetBarter(CaseBarterSet set)
    {
        var assort = _tradersTable.GetTrader(set.Trader)?.Assort;
        if (assort == null)
        {
            _logger.Warning($"[Softcore] Assort for trader {set.Trader} not found, skipping");
            return false;
        }

        var wanted = set.Requirements.Select(requirement => requirement.Template).ToHashSet();
        var match = CaseRoots(assort, set.Item).FirstOrDefault(root =>
            assort.BarterScheme.TryGetValue(root.Id, out var schemes) &&
            schemes[0].Select(requirement => requirement.Template).ToHashSet().SetEquals(wanted));

        if (match != null)
        {
            assort.BarterScheme[match.Id] = [set.Requirements];
            _logger.Info($"[Softcore] Set {ItemName(set.Item)} barter at {TraderName(set.Trader)}: {Describe(set.Requirements)}");
            return true;
        }

        _logger.Warning($"[Softcore] Barter for {ItemName(set.Item)} at {TraderName(set.Trader)} not found, creating it at LL{set.LoyaltyLevel}");
        CreateBarter(assort, set.Item, set.LoyaltyLevel, 1, set.Requirements);
        return true;
    }

    private void AddBarter(CaseBarterAdd add)
    {
        var assort = _tradersTable.GetTrader(add.Trader)?.Assort;
        if (assort == null)
        {
            _logger.Warning($"[Softcore] Assort for trader {add.Trader} not found, skipping");
            return;
        }

        CreateBarter(assort, add.Item, add.LoyaltyLevel, add.BuyLimit, add.Requirements);
        _logger.Info($"[Softcore] Added {ItemName(add.Item)} barter at {TraderName(add.Trader)} LL{add.LoyaltyLevel}: {Describe(add.Requirements)}");
    }

    /// <summary>
    /// Same shape as the vanilla case barters (unlimited stack, per-restock buy limit). The id is minted
    /// on every server start; that is fine because the live table is what trader resets clone from and
    /// TraderPurchasePersisterService drops purchases whose assort id no longer exists.
    /// </summary>
    private static void CreateBarter(TraderAssort assort, MongoId tpl, int loyaltyLevel, int buyLimit, List<BarterScheme> requirements)
    {
        var root = new Item
        {
            Id = new MongoId(),
            Template = tpl,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                UnlimitedCount = true,
                StackObjectsCount = 9999999,
                BuyRestrictionMax = buyLimit,
                BuyRestrictionCurrent = 0,
            },
        };

        assort.Items.Add(root);
        assort.BarterScheme[root.Id] = [requirements];
        assort.LoyalLevelItems[root.Id] = loyaltyLevel;
    }

    private static IEnumerable<Item> CaseRoots(TraderAssort assort, MongoId tpl) =>
        assort.Items.Where(item => item.Template == tpl && item.ParentId == "hideout");

    private string Describe(List<BarterScheme> requirements) =>
        string.Join(" + ", requirements.Select(requirement => $"{requirement.Count} {ItemName(requirement.Template)}"));

    private string ItemName(MongoId tpl)
    {
        var name = _itemHelper.GetItemName(tpl);
        return string.IsNullOrEmpty(name) ? tpl.ToString() : name;
    }

    private string TraderName(MongoId traderId) =>
        _tradersTable.GetTrader(traderId)?.Base.Nickname ?? traderId.ToString();

    /// <summary>
    /// Skier trades in EUR: currency and balance, loyalty thresholds (PaymentService converts every sale
    /// into the trader's currency before adding to SalesSum, so MinSalesSum must shrink by the same
    /// factor), every RUB-priced assort and the RUB stacks in his quests' Success rewards.
    /// </summary>
    private void DoSkierUsesEuros()
    {
        var skier = _tradersTable.GetTrader(Traders.SKIER);
        if (skier == null)
        {
            _logger.Warning("[Softcore] Skier not found, skipping");
            return;
        }

        var euroPrice = _handbookHelper.GetTemplatePrice(ItemTpl.MONEY_EUROS);
        if (euroPrice <= 0)
        {
            _logger.Warning($"[Softcore] EUR handbook price is {euroPrice}, skipping");
            return;
        }

        skier.Base.Currency = CurrencyType.EUR;
        skier.Base.BalanceEuro = 700000;

        foreach (var loyaltyLevel in skier.Base.LoyaltyLevels ?? [])
        {
            if (loyaltyLevel.MinSalesSum.HasValue)
            {
                loyaltyLevel.MinSalesSum = (long)Math.Round(loyaltyLevel.MinSalesSum.Value / euroPrice);
            }
        }

        // Skier sells EUR for RUB; that offer must stay in RUB
        var eurOfferId = skier.Assort.Items.FirstOrDefault(item => item.Template == ItemTpl.MONEY_EUROS)?.Id;

        var converted = 0;
        foreach (var (assortId, schemes) in skier.Assort.BarterScheme)
        {
            if (assortId == eurOfferId) continue;

            var price = schemes[0][0];
            if (price.Template != ItemTpl.MONEY_ROUBLES || !price.Count.HasValue) continue;

            price.Count = Math.Round(price.Count.Value / euroPrice, 2);
            price.Template = ItemTpl.MONEY_EUROS;
            converted++;
        }

        var rewards = 0;
        foreach (var quest in _templateTable.Quests.Values)
        {
            if (quest.TraderId != Traders.SKIER) continue;

            if (quest.Rewards == null || !quest.Rewards.TryGetValue("Success", out var successRewards))
            {
                _logger.Warning($"[Softcore] Quest {quest.Id} has no Success rewards, skipping");
                continue;
            }

            foreach (var reward in successRewards)
            {
                foreach (var item in reward.Items ?? [])
                {
                    if (item.Template != ItemTpl.MONEY_ROUBLES) continue;

                    item.Template = ItemTpl.MONEY_EUROS;
                    if (item.Upd?.StackObjectsCount == null)
                    {
                        _logger.Warning($"[Softcore] Quest {quest.Id} rouble reward has no stack count, skipping");
                        continue;
                    }

                    item.Upd.StackObjectsCount = Math.Ceiling(item.Upd.StackObjectsCount.Value / euroPrice);
                    if (reward.Value == null)
                    {
                        _logger.Warning($"[Softcore] Quest {quest.Id} rouble reward has no value, skipping");
                        continue;
                    }

                    reward.Value = Math.Ceiling(reward.Value.Value / euroPrice);
                    rewards++;
                }
            }
        }

        _logger.Success($"[Softcore] Skier uses Euros: {converted} offers and {rewards} quest rewards converted at {euroPrice} RUB/EUR");
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
