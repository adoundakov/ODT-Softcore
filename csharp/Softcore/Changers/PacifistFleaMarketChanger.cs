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

            PacifistFleaMarket(config);
            AllowWhitelistedItems(config.Whitelist);
            AllowQuestKeys(config.QuestKeys);
            AllowMarkedKeys(config.MarkedKeys);

            _logger.Success("[Softcore] Pacifist flea market applied successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Pacifist flea market failed: {ex.Message}");
        }
    }

    private void PacifistFleaMarket(PacifistFleaMarketConfig config)
    {
        var items = _templateTable.Items;
        var handbook = _templateTable.Handbook;
        var ragfairConfig = _ragfairConfig;

        // Convert whitelist handbook IDs to MongoId for comparison
        var whitelistedCategories = FleaMarketData.FleaListingsWhitelistHandbook
            .Select(id => (MongoId)id)
            .ToHashSet();

        int blacklistedCount = 0;

        // Blacklist all items NOT in whitelisted handbook categories
        foreach (var (itemId, item) in items)
        {
            var handbookEntry = handbook.Items.FirstOrDefault(h => h.Id == itemId);
            if (handbookEntry == null)
                continue;

            if (!whitelistedCategories.Contains(handbookEntry.ParentId))
            {
                // Add to ragfair blacklist
                ragfairConfig.Dynamic.Blacklist.Custom.Add(itemId);
                blacklistedCount++;
            }
        }

        _logger.Info($"[Softcore] Pacifist flea: blacklisted {blacklistedCount} items");
    }

    private void AllowWhitelistedItems(EconomyTogglesConfig config)
    {
        if (!config.Enabled)
            return;

        var items = _templateTable.Items;
        var prices = _templateTable.Prices;
        var ragfairConfig = _ragfairConfig;

        foreach (var itemIdStr in FleaMarketData.Whitelist)
        {
            var itemId = (MongoId)itemIdStr;

            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(itemId);

            // Mark as sellable
            if (items.TryGetValue(itemId, out var item) && item.Properties != null)
            {
                item.Properties.CanSellOnRagfair = true;
            }

            // Apply price multiplier
            if (prices.ContainsKey(itemId))
            {
                prices[itemId] = (int)Math.Round(prices[itemId] * config.PriceMultiplier);
            }
        }

        _logger.Info($"[Softcore] Whitelisted {FleaMarketData.Whitelist.Count} items with {config.PriceMultiplier}x multiplier");
    }

    private void AllowQuestKeys(EconomyTogglesConfig config)
    {
        if (!config.Enabled)
            return;

        var items = _templateTable.Items;
        var prices = _templateTable.Prices;
        var ragfairConfig = _ragfairConfig;

        foreach (var keyIdStr in KeysData.QuestKeys)
        {
            var keyId = (MongoId)keyIdStr;

            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(keyId);

            // Mark as sellable
            if (items.TryGetValue(keyId, out var item) && item.Properties != null)
            {
                item.Properties.CanSellOnRagfair = true;
            }

            // Apply price multiplier
            if (prices.ContainsKey(keyId))
            {
                prices[keyId] = (int)Math.Round(prices[keyId] * config.PriceMultiplier);
            }
        }

        _logger.Info($"[Softcore] Allowed {KeysData.QuestKeys.Count} quest keys with {config.PriceMultiplier}x multiplier");
    }

    private void AllowMarkedKeys(EconomyTogglesConfig config)
    {
        if (!config.Enabled)
            return;

        var items = _templateTable.Items;
        var prices = _templateTable.Prices;
        var ragfairConfig = _ragfairConfig;

        foreach (var keyIdStr in KeysData.MarkedKeys)
        {
            var keyId = (MongoId)keyIdStr;

            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(keyId);

            // Mark as sellable
            if (items.TryGetValue(keyId, out var item) && item.Properties != null)
            {
                item.Properties.CanSellOnRagfair = true;
            }

            // Apply price multiplier
            if (prices.ContainsKey(keyId))
            {
                prices[keyId] = (int)Math.Round(prices[keyId] * config.PriceMultiplier);
            }
        }

        _logger.Info($"[Softcore] Allowed {KeysData.MarkedKeys.Count} marked keys with {config.PriceMultiplier}x multiplier");
    }
}
