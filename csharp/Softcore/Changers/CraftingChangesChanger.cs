using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Assets;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies crafting recipe changes:
/// - Rebalances 47 existing recipes (output counts, requirements)
/// - Adds 16 new recipes (container progression + stims)
/// </summary>
[Injectable]
public class CraftingChangesChanger(
    ISptLogger<CraftingChangesChanger> logger,
    HideoutTable hideoutTable)
{
    private readonly ISptLogger<CraftingChangesChanger> _logger = logger;
    private readonly HideoutTable _hideoutTable = hideoutTable;

    public void Apply(CraftingChangesConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Crafting changes disabled");
            return;
        }

        if (config.CraftingRebalance)
        {
            try
            {
                DoCraftingRebalance();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Crafting rebalance failed: {ex.Message}");
            }
        }

        if (config.AdditionalCraftingRecipes)
        {
            try
            {
                DoAdditionalCraftingRecipes();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Additional recipes failed: {ex.Message}");
            }
        }
    }

    private void DoCraftingRebalance()
    {
        var recipes = _hideoutTable.Production.Recipes;
        if (recipes == null)
        {
            _logger.Warning("[Softcore] Hideout recipes not found in database");
            return;
        }

        int successCount = 0;
        int failCount = 0;

        foreach (var adjustment in CraftingData.Adjustments.All)
        {
            try
            {
                var craft = GetCraftByEndProduct(recipes, adjustment.ItemId);
                if (craft == null)
                {
                    _logger.Warning($"[Softcore] Craft not found for {adjustment.ItemId}, skipping");
                    failCount++;
                    continue;
                }

                adjustment.Apply(craft);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Failed to adjust craft {adjustment.ItemId}: {ex.Message}");
                failCount++;
            }
        }

        _logger.Success($"[Softcore] Crafting rebalance: {successCount} applied, {failCount} skipped");
    }

    private void DoAdditionalCraftingRecipes()
    {
        var recipes = _hideoutTable.Production.Recipes;
        if (recipes == null)
        {
            _logger.Warning("[Softcore] Hideout recipes not found in database");
            return;
        }

        foreach (var newRecipe in CraftingData.NewRecipes.All)
        {
            recipes.Add(newRecipe);
        }

        _logger.Success($"[Softcore] Added {CraftingData.NewRecipes.All.Count} new crafting recipes");
    }

    /// <summary>
    /// Finds a craft recipe by its end product ID.
    /// Excludes ChristmasIllumination area (special event recipes).
    /// </summary>
    private HideoutProduction? GetCraftByEndProduct(List<HideoutProduction> recipes, string endProductId)
    {
        return recipes.FirstOrDefault(r =>
            r.EndProduct == endProductId &&
            r.AreaType != HideoutAreas.ChristmasIllumination
        );
    }
}
