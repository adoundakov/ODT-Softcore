using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace Softcore.Assets;

/// <summary>
/// TS <c>modifyTraderBarters</c> semantic: for every assort root of <see cref="Item"/> at
/// <see cref="Trader"/>, for each adjustment whose key tpl the barter contains, run the callback on
/// <b>every</b> requirement of that barter (not just the matching one).
/// </summary>
public record CaseBarterAdjust(MongoId Trader, MongoId Item, Dictionary<MongoId, Action<BarterScheme>> Adjustments);

/// <summary>
/// Full requirement list for a barter of <see cref="Item"/> at <see cref="Trader"/>. Matched by the
/// set of requirement tpls; on a match the requirements are replaced and the vanilla loyalty level
/// kept, on a miss the barter is created at <see cref="LoyaltyLevel"/>.
/// </summary>
public record CaseBarterSet(MongoId Trader, MongoId Item, int LoyaltyLevel, List<BarterScheme> Requirements);

/// <summary>A new assort root for <see cref="Item"/> at <see cref="Trader"/>, always created.</summary>
public record CaseBarterAdd(MongoId Trader, MongoId Item, int LoyaltyLevel, int BuyLimit, List<BarterScheme> Requirements);

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
    /// The only item base classes Fence may sell. Every other class an item template uses is blacklisted
    /// (derived at runtime, see <c>TraderChangesChanger.DoPacifistFence</c>).
    /// </summary>
    public static readonly HashSet<MongoId> PacifistFenceWhitelist =
    [
        BaseClasses.DRINK,
        BaseClasses.INFO,
        BaseClasses.FOOD,
        BaseClasses.DRUGS,
        BaseClasses.MED_KIT,
        BaseClasses.MEDICAL,
        BaseClasses.BATTERY,
        BaseClasses.ELECTRONICS,
        BaseClasses.BUILDING_MATERIAL,
        BaseClasses.HOUSEHOLD_GOODS,
        BaseClasses.JEWELRY,
        BaseClasses.LUBRICANT,
        BaseClasses.OTHER,
        BaseClasses.TOOL,
        BaseClasses.MEDICAL_SUPPLIES,
        BaseClasses.FUEL,
        BaseClasses.STIMULATOR,
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

    /// <summary>
    /// <c>reasonablyPricedCases</c> step 1 — TS adjustments that still apply on 4.1.5 data. The ones the
    /// data outgrew (Item Case Ophthalmoscope/dogtag, both THICC cases) were re-specified as
    /// <see cref="CaseBarterSets"/> because those barters are multi-item now.
    /// </summary>
    public static readonly List<CaseBarterAdjust> CaseAdjustments =
    [
        new(Traders.THERAPIST, ItemTpl.CONTAINER_ITEM_CASE, new()
        {
            [ItemTpl.MONEY_EUROS] = requirement => requirement.Count = 7256, // vanilla 15 192.74
        }),

        new(Traders.THERAPIST, ItemTpl.CONTAINER_LUCKY_SCAV_JUNK_BOX, new()
        {
            [ItemTpl.MONEY_ROUBLES] = requirement => requirement.Count = 961138, // vanilla 1 106 138
            [ItemTpl.BARTER_DOGTAG_USEC] = requirement => requirement.Count = 15, // vanilla 40
        }),

        new(Traders.THERAPIST, ItemTpl.CONTAINER_MEDICINE_CASE, new()
        {
            [ItemTpl.MONEY_ROUBLES] = requirement => requirement.Count = 290610, // vanilla 548 610
        }),

        // 20 (lvl 39+) + 140 (lvl 10+, Usec) → 2 + 14
        new(Traders.THERAPIST, ItemTpl.BARTER_LEDX_SKIN_TRANSILLUMINATOR, new()
        {
            [ItemTpl.BARTER_DOGTAG_USEC] = requirement => requirement.Count /= 10,
        }),

        // Every-requirement semantic: 15 Defib + 15 LEDX + 15 Ibuprofen + 15 Toothpaste → 5 of each
        new(Traders.THERAPIST, ItemTpl.CONTAINER_THICC_ITEM_CASE, new()
        {
            [ItemTpl.BARTER_LEDX_SKIN_TRANSILLUMINATOR] = requirement => requirement.Count = 5,
        }),

        // Every-requirement semantic: 10 Moonshine + 10 Vodka + 5 Slickers → 4 of each
        new(Traders.SKIER, ItemTpl.CONTAINER_WEAPON_CASE, new()
        {
            [ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE] = requirement => requirement.Count = 4,
        }),
    ];

    /// <summary><c>reasonablyPricedCases</c> step 2 — explicit requirement lists.</summary>
    public static readonly List<CaseBarterSet> CaseBarterSets =
    [
        // vanilla 50 / 50 / 30
        new(Traders.THERAPIST, ItemTpl.CONTAINER_THICC_ITEM_CASE, 4,
        [
            new() { Template = ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE, Count = 15 },
            new() { Template = ItemTpl.DRINK_BOTTLE_OF_TARKOVSKAYA_VODKA, Count = 15 },
            new() { Template = ItemTpl.DRINK_BOTTLE_OF_DAN_JACKIEL_WHISKEY, Count = 15 },
        ]),

        // vanilla 20 / 30 / 30
        new(Traders.PEACEKEEPER, ItemTpl.CONTAINER_THICC_ITEM_CASE, 4,
        [
            new() { Template = ItemTpl.INFO_TERRAGROUP_BLUE_FOLDERS_MATERIALS, Count = 5 },
            new() { Template = ItemTpl.INFO_SECURE_MAGNETIC_TAPE_CASSETTE, Count = 5 },
            new() { Template = ItemTpl.INFO_SECURE_FLASH_DRIVE, Count = 10 },
        ]),

        // vanilla 80. "Any dogtag" is Template = USEC dogtag + Side = Any, as vanilla does it
        new(Traders.THERAPIST, ItemTpl.CONTAINER_ITEM_CASE, 3,
        [
            new() { Template = ItemTpl.BARTER_DOGTAG_USEC, Count = 20, Level = 15, Side = DogtagExchangeSide.Any },
        ]),

        // vanilla 10 / 25
        new(Traders.THERAPIST, ItemTpl.CONTAINER_ITEM_CASE, 3,
        [
            new() { Template = ItemTpl.BARTER_OPHTHALMOSCOPE, Count = 2 },
            new() { Template = ItemTpl.BARTER_PILE_OF_MEDS, Count = 10, OnlyFunctional = true },
        ]),
    ];

    /// <summary><c>reasonablyPricedCases</c> step 3 — new barters.</summary>
    public static readonly List<CaseBarterAdd> CaseBarterAdds =
    [
        // Nobody sells the chain on 4.1.5. Therapist, because all her case barters are dogtag-priced.
        new(Traders.THERAPIST, ItemTpl.BARTER_GOLDEN_NECK_CHAIN, 2, 3,
        [
            new() { Template = ItemTpl.BARTER_DOGTAG_USEC, Count = 10, Level = 10, Side = DogtagExchangeSide.Any },
        ]),
    ];
}
