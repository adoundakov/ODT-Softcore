using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using Softcore.Config;
using Softcore.Assets;

namespace Softcore.Changers;

/// <summary>
/// Restricts flea market to "pacifist" categories (barter items, meds, food, info).
/// Allows whitelisted items, quest keys, and marked keys with price multipliers.
/// </summary>
[Injectable]
public class PacifistFleaMarketChanger
{
    private readonly ISptLogger<PacifistFleaMarketChanger> _logger;
    // TODO: Inject when API is confirmed:
    // - IConfigServer (RagfairConfig)
    // - IDatabaseService (GetItems(), GetHandbook(), GetPrices())

    public PacifistFleaMarketChanger(ISptLogger<PacifistFleaMarketChanger> logger)
    {
        _logger = logger;
    }

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

            // TODO: Implement after API research
            // PacifistFleaMarket(config);
            // AllowWhitelistedItems(config.Whitelist);
            // AllowQuestKeys(config.QuestKeys);
            // AllowMarkedKeys(config.MarkedKeys);

            _logger.Warning("[Softcore] Pacifist flea market - NOT YET IMPLEMENTED (awaiting API research)");
        }
        catch (Exception ex)
        {
            _logger.Warning($"[Softcore] Pacifist flea market failed: {ex.Message}");
        }
    }

    // TODO: Implement after confirming API
    /*
    private void PacifistFleaMarket(PacifistFleaMarketConfig config)
    {
        var items = _databaseService.GetItems();
        var handbook = _databaseService.GetHandbook();
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();

        int blacklistedCount = 0;

        // Blacklist all items NOT in whitelisted handbook categories
        foreach (var (itemId, item) in items)
        {
            var handbookEntry = handbook.Items.FirstOrDefault(h => h.Id == itemId);
            if (handbookEntry == null)
                continue;

            if (!FleaMarketData.FleaListingsWhitelistHandbook.Contains(handbookEntry.ParentId))
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

        var items = _databaseService.GetItems();
        var prices = _databaseService.GetPrices();
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();

        foreach (var itemId in FleaMarketData.Whitelist)
        {
            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(itemId);

            // Mark as sellable
            if (items.TryGetValue(itemId, out var item))
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

        var items = _databaseService.GetItems();
        var prices = _databaseService.GetPrices();
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();

        foreach (var keyId in KeysData.QuestKeys)
        {
            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(keyId);

            // Mark as sellable
            if (items.TryGetValue(keyId, out var item))
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

        var items = _databaseService.GetItems();
        var prices = _databaseService.GetPrices();
        var ragfairConfig = _configServer.GetConfig<RagfairConfig>();

        foreach (var keyId in KeysData.MarkedKeys)
        {
            // Remove from blacklist
            ragfairConfig.Dynamic.Blacklist.Custom.Remove(keyId);

            // Mark as sellable
            if (items.TryGetValue(keyId, out var item))
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
    */
}
