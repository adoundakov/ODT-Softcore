using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
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

    private void DoBetterSalesToTraders()
    {
        _logger.Info("[Softcore] Better sales to traders: not implemented yet");
    }

    private void DoAlternativeCategories()
    {
        _logger.Info("[Softcore] Alternative categories: not implemented yet");
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

    private void DoBiggerLimits(double multiplier)
    {
        _logger.Info("[Softcore] Bigger limits: not implemented yet");
    }
}
