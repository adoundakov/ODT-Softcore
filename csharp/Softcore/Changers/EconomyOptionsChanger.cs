using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Orchestrator that applies all economy-related changers in the correct order:
/// 1. Price rebalance (sync flea to handbook)
/// 2. Pacifist flea market (restrict to pacifist categories)
/// 3. Barter economy (configure barter settings)
/// 4. Other flea changes (misc tweaks)
/// </summary>
[Injectable]
public class EconomyOptionsChanger
{
    private readonly ISptLogger<EconomyOptionsChanger> _logger;
    private readonly BarterEconomyChanger _barterEconomyChanger;
    private readonly PacifistFleaMarketChanger _pacifistFleaChanger;
    private readonly PriceRebalanceChanger _priceRebalanceChanger;
    private readonly OtherFleaMarketChangesChanger _otherFleaChanger;

    public EconomyOptionsChanger(
        ISptLogger<EconomyOptionsChanger> logger,
        BarterEconomyChanger barterEconomyChanger,
        PacifistFleaMarketChanger pacifistFleaChanger,
        PriceRebalanceChanger priceRebalanceChanger,
        OtherFleaMarketChangesChanger otherFleaChanger)
    {
        _logger = logger;
        _barterEconomyChanger = barterEconomyChanger;
        _pacifistFleaChanger = pacifistFleaChanger;
        _priceRebalanceChanger = priceRebalanceChanger;
        _otherFleaChanger = otherFleaChanger;
    }

    public void Apply(EconomyOptionsConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Economy options disabled");
            return;
        }

        if (config.DisableFleaMarketCompletely)
        {
            _logger.Info("[Softcore] Flea market completely disabled - setting unlock to level 99");
            _otherFleaChanger.UpdateRagfairMinUserLevel(99);
            return;
        }

        _logger.Info("[Softcore] Applying economy options...");

        // Apply changers in order (order matters!)
        _priceRebalanceChanger.Apply(config.PriceRebalance);
        _pacifistFleaChanger.Apply(config.PacifistFleaMarket);
        _barterEconomyChanger.Apply(config.BarterEconomy);
        _otherFleaChanger.Apply(config.OtherFleaMarketChanges);

        _logger.Success("[Softcore] Economy options applied");
    }
}
