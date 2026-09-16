using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace Softcore.Assets;

/// <summary>
/// Static data for trader changes. Ported from <c>src/changers/TraderChangesChanger.ts</c> and
/// <c>src/assets/fleamarket.ts</c> (<c>pacifistFenceItemBaseWhitelist</c>).
/// </summary>
public static class TraderData
{
    /// <summary>The vanilla traders, so custom traders are left alone (TS <c>stacticTraderList</c>).</summary>
    public static readonly List<MongoId> VanillaTraders =
    [
        Traders.PRAPOR,
        Traders.THERAPIST,
        Traders.FENCE,
        Traders.SKIER,
        Traders.PEACEKEEPER,
        Traders.MECHANIC,
        Traders.RAGMAN,
        Traders.JAEGER,
        Traders.REF,
    ];

    /// <summary>
    /// Added to the base <c>buy_price_coef</c> of 35 at LL1 (−5 per further LL). The coefficient is the
    /// discount the trader takes, so a bigger number means a worse deal for the player.
    /// </summary>
    public static readonly Dictionary<MongoId, int> BuyPriceAdjustment = new()
    {
        [Traders.PEACEKEEPER] = 7,
        [Traders.SKIER] = 6,
        [Traders.PRAPOR] = 5,
        [Traders.MECHANIC] = 4,
        [Traders.JAEGER] = 3,
        [Traders.RAGMAN] = 2,
        [Traders.THERAPIST] = 1,
    };
}
