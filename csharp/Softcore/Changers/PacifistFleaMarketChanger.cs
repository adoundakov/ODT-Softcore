using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Common;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

/// <summary>
/// Restricts flea market to "pacifist" categories (barter items, meds, food, info).
/// Allows whitelisted items, quest keys, and marked keys with price multipliers.
/// </summary>
[Injectable]
public class PacifistFleaMarketChanger(
    ISptLogger<PacifistFleaMarketChanger> logger,
    RagfairConfig ragfairConfig,
    TemplateTable templateTable)
{
    private readonly ISptLogger<PacifistFleaMarketChanger> _logger = logger;
    private readonly RagfairConfig _ragfairConfig = ragfairConfig;
    private readonly TemplateTable _templateTable = templateTable;

    public void Apply(PacifistFleaMarketConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Pacifist flea market disabled");
            return;
        }

        try
        {
            _logger.Info("[Softcore] Applying pacifist flea market restrictions...");

            PacifistFleaMarket();

            if (config.Whitelist.Enabled)
                AllowOnRagfair(FleaMarketData.Whitelist, config.Whitelist.PriceMultiplier, "whitelisted items");
            if (config.QuestKeys.Enabled)
                AllowOnRagfair(KeysData.QuestKeys, config.QuestKeys.PriceMultiplier, "quest keys");
            if (config.MarkedKeys.Enabled)
                AllowOnRagfair(KeysData.MarkedKeys, config.MarkedKeys.PriceMultiplier, "marked keys");

            _logger.Success("[Softcore] Pacifist flea market applied successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Pacifist flea market failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Ban everything on flea except whitelisted handbook categories, plus every quest item.
    /// Filtering is done by handbook category (not base class) on purpose — that is what the
    /// original mod does and it is the grouping the flea UI actually uses.
    /// </summary>
    private void PacifistFleaMarket()
    {
        var items = _templateTable.Items;
        var whitelistedCategories = FleaMarketData.FleaListingsWhitelistHandbook;
        var customBlacklist = _ragfairConfig.Dynamic.Blacklist.Custom;

        var blacklistedCount = 0;
        foreach (var handbookItem in _templateTable.Handbook.Items)
        {
            var itemId = handbookItem.Id;
            var isQuestItem = items.TryGetValue(itemId, out var item) && item.Properties?.QuestItem == true;

            if (!whitelistedCategories.Contains(handbookItem.ParentId) || isQuestItem)
            {
                // Better semantics than CanSellOnRagfair: the item is hidden from offers but stays usable
                customBlacklist.Add(itemId);
                blacklistedCount++;
            }
        }

        _logger.Info($"[Softcore] Pacifist flea: blacklisted {blacklistedCount} items");
    }

    /// <summary>
    /// Re-allow a set of items on flea: remove them from the custom blacklist, mark them sellable
    /// and scale their flea price.
    /// </summary>
    private void AllowOnRagfair(IEnumerable<MongoId> itemIds, double priceMultiplier, string label)
    {
        var items = _templateTable.Items;
        var prices = _templateTable.Prices;
        var customBlacklist = _ragfairConfig.Dynamic.Blacklist.Custom;

        var allowedCount = 0;
        foreach (var itemId in itemIds)
        {
            if (!items.TryGetValue(itemId, out var item))
            {
                _logger.Warning($"[Softcore] AllowOnRagfair ({label}): item {itemId} not found, skipping");
                continue;
            }

            if (prices.TryGetValue(itemId, out var price))
            {
                prices[itemId] = Math.Round(price * priceMultiplier);
            }

            if (item.Properties != null)
            {
                item.Properties.CanSellOnRagfair = true;
            }

            customBlacklist.Remove(itemId);
            allowedCount++;
        }

        _logger.Info($"[Softcore] Allowed {allowedCount} {label} on flea with {priceMultiplier}x price multiplier");
    }
}
