using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace Softcore.Assets;

/// <summary>
/// Static data for flea market whitelists, blacklists, and pricing.
/// Item IDs are from SPT 3.x - will need validation for SPT 4.0 in Phase 4.
/// </summary>
public static class FleaMarketData
{
    /// <summary>
    /// Items that can be bought on flea (specific IDs)
    /// </summary>
    public static readonly HashSet<string> Whitelist = new()
    {
        "572b7d8524597762b472f9d1",  // GP-7 gas mask
        "59e770b986f7742cbd762754",  // Security vest
        "59e763f286f7742ee57895da",  // Pilgrim tourist backpack
        "5d80cbd886f77470855c26c2",  // Dorm 314 marked key
        "5447e1d04bdc2dff2f8b4567",  // Knife
        "5a0dc45586f77f39e400e7c1",  // Bars A-2607 95kh18 knife
        "5a16bb52fcdbcb001a3b00dc",  // Bars A-2607 Damascus knife
        "59f32c3b86f77472a31742f0",  // Dogtag USEC
        "59f32bb586f774757e1e8442",  // Dogtag BEAR
        "5c093ca986f7740a1867ab12",  // Secure container Kappa
        "5e00c1ad86f774747333222c",  // Team Wendy EXFIL helmet
        "5ea05cf85ad9772e6624305d",  // Tac-Kek FAST MT helmet (Replica)
        "67449b6c89d5e1ddc603f504",  // Contraband case key
    };

    /// <summary>
    /// Base class IDs that can be REQUESTED in barters (food, barter items, meds, etc.)
    /// </summary>
    public static readonly HashSet<string> FleaBarterRequestWhitelist = new()
    {
        "5448e8d04bdc2ddf718b4569",  // FOOD
        "5448e8d64bdc2dce718b4568",  // DRINK
        "5448eb774bdc2d0a728b4567",  // BARTER_ITEM
        "5448ecbe4bdc2d60728b4568",  // INFO
        "5448f39d4bdc2d0a728b4568",  // MEDKIT
        "5448f3a14bdc2d27728b4569",  // DRUGS
        "5448f3ac4bdc2dce718b4569",  // MEDICAL
        "57864c8c245977548867e7f1",  // MEDICAL_SUPPLIES
        "5d650c3e815116009f6201d2",  // FUEL
        "543be5664bdc2dd4348b4569",  // MEDS
        "543be5dd4bdc2deb348b4569",  // MONEY
        "543be6674bdc2df1348b4569",  // FOOD_DRINK
        "57864a3d24597754843f8721",  // JEWELRY
        "57864a66245977548f04a81f",  // ELECTRONICS
        "57864ada245977548638de91",  // BUILDING_MATERIAL
        "57864bb7245977548b3b66c2",  // TOOL
        "57864c322459775490116fbf",  // HOUSEHOLD_GOODS
        "57864e4c24597754843f8723",  // LUBRICANT
        "57864ee62459775490116fc1",  // BATTERY
        "590c745b86f7743cc433c5f2",  // OTHER
        "54009119af1c881c07000029",  // ITEM
        "5661632d4bdc2d903d8b456b",  // STACKABLE_ITEM
    };

    /// <summary>
    /// Specific items with custom flea prices (for barter requests)
    /// </summary>
    public static readonly Dictionary<string, int> RequestWhitelist = new()
    {
        { "6389c7750ef44505c87f5996", 1480000 },  // Microcontroller board
        { "6389c7f115805221fb410466", 2126000 },  // Far-forward GPS Signal Amplifier Unit
        { "6389c85357baa773a825b356", 4924000 },  // Advanced current converter
        { "59faff1d86f7746c51718c9c", 100000 },   // Physical Bitcoin
        { "6389c8fb46b54c634724d847", 500000 },   // Silicon Optoelectronic Integrated Circuits textbook
        { "6389c92d52123d5dd17f8876", 490000 },   // Advanced Electronic Materials textbook
        { "6656560053eaaa7a23349c86", 900000 },   // Lega Medal
        { "6331ba83f2ab4f3f09502983", 0 },        // Secure Flash drive V2 (blacklist)
        { "67449b6c89d5e1ddc603f504", 0 },        // Case key (blacklist)
        { "5e99711486f7744bfc4af328", 0 },        // Sanitar's first aid kit (blacklist)
        { "5e99735686f7744bfc4af32c", 0 },        // Sanitar kit (blacklist)
        { "5b9b9020e7ef6f5716480215", 0 },        // dogtag (blacklist)
    };

    /// <summary>
    /// Handbook category IDs allowed for buying on flea (pacifist categories)
    /// </summary>
    public static readonly HashSet<string> FleaListingsWhitelistHandbook = new()
    {
        "5b47574386f77428ca22b2ed",  // Energy elements
        "5b47574386f77428ca22b2ee",  // Building materials
        "5b47574386f77428ca22b2ef",  // Electronics
        "5b47574386f77428ca22b2f0",  // Household materials
        "5b47574386f77428ca22b2f1",  // Valuables
        "5b47574386f77428ca22b2f2",  // Flammable materials
        "5b47574386f77428ca22b2f3",  // Medical supplies
        "5b47574386f77428ca22b2f4",  // Others
        "5b47574386f77428ca22b2f6",  // Tools
        "5b47574386f77428ca22b335",  // Drinks
        "5b47574386f77428ca22b336",  // Food
        "5b47574386f77428ca22b337",  // Pills
        "5b47574386f77428ca22b338",  // Medkits
        "5b47574386f77428ca22b339",  // Injury treatment
        "5b47574386f77428ca22b33a",  // Injectors
        "5b47574386f77428ca22b33e",  // Barter items
        "5b47574386f77428ca22b340",  // Provisions
        "5b47574386f77428ca22b341",  // Info items
        "5b47574386f77428ca22b343",  // Maps
        "5b47574386f77428ca22b344",  // Medication
    };

    /// <summary>
    /// All base class IDs in the game (used to compute barter blacklist)
    /// This is a subset - full list has 159 items (will be completed in Phase 4)
    /// </summary>
    public static readonly HashSet<string> ActualBaseClasses = new()
    {
        "566162e44bdc2d3f298b4573",  // CompoundItem
        "54009119af1c881c07000029",  // Item
        "5661632d4bdc2d903d8b456b",  // StackableItem
        "5447b5f14bdc2d61278b4567",  // AssaultRifle
        "543be5cb4bdc2deb348b4568",  // AmmoBox
        "5422acb9af1c889c16000029",  // Weapon
        "5c99f98d86f7745c314214b3",  // KeyMechanical
        "55802f3e4bdc2de7118b4584",  // GearMod
        "5447b5cf4bdc2d65278b4567",  // Pistol
        "543be6564bdc2df4348b4568",  // ThrowWeap
        "566168634bdc2d144c8b456c",  // SearchableItem
        "5448bc234bdc2d3c308b4569",  // Magazine
        "57bef4c42459772e8d35a53b",  // ArmoredEquipment
        "543be6674bdc2df1348b4569",  // FoodDrink
        "543be5664bdc2dd4348b4569",  // Meds
        "550aa4154bdc2dd8348b456b",  // FunctionalMod
        "5448e8d64bdc2dce718b4568",  // Drink
        "5448e8d04bdc2ddf718b4569",  // Food
        "543be5dd4bdc2deb348b4569",  // Money
        "55818b164bdc2ddc698b456c",  // TacticalCombo
        "550aa4cd4bdc2dd8348b456c",  // Silencer
        "5447e1d04bdc2dff2f8b4567",  // Knife
        "5447b6094bdc2dc3278b4567",  // Shotgun
        "5448bf274bdc2dfc2f8b456a",  // MobContainer
        "550aa4bf4bdc2dd6348b456b",  // FlashHider
        "55818add4bdc2d5b648b456f",  // AssaultScope
        "55818ae44bdc2dde698b456c",  // OpticScope
        "5448e5284bdc2dcb718b4567",  // Vest
        "5448e53e4bdc2d60728b4567",  // Backpack
        "5448f3ac4bdc2dce718b4569",  // Medical
        "5448f3a14bdc2d27728b4569",  // Drugs
        "5448f39d4bdc2d0a728b4568",  // MedKit
        "5485a8684bdc2da71d8b4567",  // Ammo
        "5448e54d4bdc2dcc718b4568",  // Armor
        "5448fe124bdc2da5018b4567",  // Mod
        "5448fe394bdc2d0d028b456c",  // Muzzle
        "55802f4a4bdc2ddb688b4569",  // MasterMod
        "5448e5724bdc2ddf718b4568",  // Visors
        "557596e64bdc2dc2118b4571",  // Pockets
        "555ef6e44bdc2de9068b457e",  // Barrel
        "5447b6254bdc2dc3278b4568",  // SniperRifle
        "55818ad54bdc2ddc698b4569",  // Collimator
        "550aa4dd4bdc2dc9348b4569",  // MuzzleCombo
        "55818a684bdc2ddd698b456d",  // PistolGrip
        "55818af64bdc2d5b648b4570",  // Foregrip
        "5448fe7a4bdc2d6f028b456b",  // Sights
        "55818a304bdc2db5418b457d",  // Receiver
        "55818a6f4bdc2db9688b456b",  // Charge
        "55818a104bdc2db9688b4569",  // Handguard
        "55818b224bdc2dde698b456f",  // Mount
        "55818a594bdc2db9688b456a",  // Stock
        "55818ac54bdc2d5b648b456e",  // IronSight
        "55d720f24bdc2d88028b456d",  // Inventory
        "5a74651486f7744e73386dd1",  // AuxiliaryMod
        "5a341c4086f77401f2541505",  // Headwear
        "543be5f84bdc2dd4348b456a",  // Equipment
        "5645bcb74bdc2ded0b8b4578",  // Headphones
        "55818b014bdc2ddc698b456b",  // Launcher
        "566965d44bdc2d814c8b4571",  // LootContainer
        "566abbb64bdc2d144c8b457d",  // Stash
        "5671435f4bdc2d96058b4569",  // LockableContainer
        "57864ee62459775490116fc1",  // Battery
        "57864a66245977548f04a81f",  // Electronics
        "57864e4c24597754843f8723",  // Lubricant
        "567583764bdc2d98058b456e",  // StationaryContainer
        "55818afb4bdc2dde698b456d",  // Bipod
        "56ea9461d2720b67698b456f",  // Gasblock
        "5a2c3a9486f774688b05e574",  // NightVision
        "5a341c4686f77469e155819e",  // FaceCover
        "57864a3d24597754843f8721",  // Jewelry
        "590c745b86f7743cc433c5f2",  // Other
        "57864ada245977548638de91",  // BuildingMaterial
        "57864c322459775490116fbf",  // HouseholdGoods
        "5447b5fc4bdc2d87278b4567",  // AssaultCarbine
        "567849dd4bdc2d150f8b456e",  // Map
        "5c164d2286f774194c5e69fa",  // Keycard
        "55818acf4bdc2dde698b456b",  // CompactCollimator
        "5447b6194bdc2d67278b4567",  // MarksmanRifle
        "5795f317245977243854e041",  // SimpleContainer
        "5448eb774bdc2d0a728b4567",  // BarterItem
        "5447b5e04bdc2d62278b4567",  // Smg
        "55818b084bdc2d5b648b4571",  // Flashlight
        "57864bb7245977548b3b66c2",  // Tool
        "5448ecbe4bdc2d60728b4568",  // Info
        "616eb7aea207f41933308f46",  // RepairKits
        "5447e0e74bdc2d3c308b4567",  // SpecItem
        "57864c8c245977548867e7f1",  // MedicalSupplies
        "55818aeb4bdc2ddc698b456a",  // SpecialScope
        "5b3f15d486f77432d0509248",  // ArmBand
        "5447bed64bdc2d97278b4568",  // MachineGun
        "5448f3a64bdc2d60728b456a",  // Stimulator
        "5d21f59b6dbe99052b54ef83",  // ThermalVision
        "543be5e94bdc2df1348b4568",  // Key
        "5d650c3e815116009f6201d2",  // Fuel
        "5447bedf4bdc2d87278b4568",  // GrenadeLauncher
        // More classes exist - will be added in Phase 4
    };

    /// <summary>
    /// BSG's hardcoded flea market blacklist (secure containers, OP items, etc.)
    /// This is a subset - full list has 700+ items (will be completed in Phase 4)
    /// </summary>
    public static readonly HashSet<string> BSGBlacklist = new()
    {
        "544a11ac4bdc2d470e8b456a",  // Secure container Alpha
        "5857a8b324597729ab0a0e7d",  // Secure container Beta
        "5857a8bc2459772bad15db29",  // Secure container Gamma
        "59db794186f77448bc595262",  // Secure container Epsilon
        "5c093ca986f7740a1867ab12",  // Secure container Kappa
        "59faff1d86f7746c51718c9c",  // Physical Bitcoin
        "5aafbcd986f7745e590fff23",  // Medicine case
        "5b6d9ce188a4501afc1b2b25",  // T H I C C Weapon case
        "5b7c710788a4506dec015957",  // Lucky Scav Junk box
        "5c0a840b86f7742ffa4f2482",  // T H I C C item case
        "59f32bb586f774757e1e8442",  // Dogtag BEAR
        "59f32c3b86f77472a31742f0",  // Dogtag USEC
        "5df8a6a186f77412640e2e80",  // Christmas tree ornament (Red)
        "5df8a72c86f77412640e2e83",  // Christmas tree ornament (Silver)
        "5df8a77486f77412672a1e3f",  // Christmas tree ornament (Violet)
        "665ee77ccf2d642e98220bca",  // Secure container Gamma
        "6662e9aca7e0b43baa3d5f74",  // Dogtag BEAR
        "6662e9cda7e0b43baa3d5f76",  // Dogtag BEAR
        "6662e9f37fa79a6d83730fa0",  // Dogtag USEC
        "6662ea05f6259762c56f3189",  // Dogtag USEC
        // More items exist - will be added in Phase 4
    };
}
