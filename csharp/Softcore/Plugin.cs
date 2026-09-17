using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;
using Softcore.Config;
using Softcore.Changers;

namespace Softcore;

/// <summary>
/// Entry point. Runs at Preload + 1: every table and SPT config is loaded by then, but nothing has
/// consumed them yet. In particular RagfairCallbacks (900000) snapshots prices and generates all
/// flea offers from RagfairConfig, so our economy edits must land before it — PostLoad is too late.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class Plugin(
    ISptLogger<Plugin> logger,
    Configuration config,
    EconomyOptionsChanger economyChanger,
    TraderChangesChanger traderChanger,
    CraftingChangesChanger craftingChanger,
    RefChangesChanger refChanger,
    QuestRewardsChanger questRewardsChanger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!config.LoadedFromDisk)
        {
            logger.Error($"[Softcore] Config file not found at: {config.ConfigPath} — using default configuration");
        }

        if (!config.General.Enabled)
        {
            logger.Warning("[Softcore] Mod is disabled in config");
            return Task.CompletedTask;
        }

        logger.Success("[Softcore] Configuration loaded successfully");
        logger.Info($"[Softcore] Economy enabled: {config.EconomyOptions.Enabled}");
        logger.Info($"[Softcore] Barter economy enabled: {config.EconomyOptions.BarterEconomy.Enabled}");
        logger.Info($"[Softcore] Trader changes enabled: {config.TraderChanges.Enabled}");

        economyChanger.Apply(config.EconomyOptions);
        traderChanger.Apply(config.TraderChanges);
        craftingChanger.Apply(config.CraftingChanges);
        refChanger.Apply(config.RefChanges);
        questRewardsChanger.Apply(config.QuestRewards);

        logger.Success("[Softcore] All changes applied successfully");
        return Task.CompletedTask;
    }
}
