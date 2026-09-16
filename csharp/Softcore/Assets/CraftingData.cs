using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace Softcore.Assets;

/// <summary>
/// Represents an adjustment to an existing crafting recipe, matched by its end product.
/// </summary>
public class CraftingAdjustment
{
    public MongoId ItemId { get; init; }
    public Action<HideoutProduction> Apply { get; init; }

    public CraftingAdjustment(MongoId itemId, Action<HideoutProduction> apply)
    {
        ItemId = itemId;
        Apply = apply;
    }
}

/// <summary>
/// Static data for crafting recipe adjustments and new recipes.
/// Ported 1:1 from <c>src/assets/productionAdjustments.ts</c> and <c>src/assets/recipes.ts</c>.
/// </summary>
public static class CraftingData
{
    public static class Adjustments
    {
        /// <summary>
        /// Set <see cref="Requirement.Count"/> on every requirement that has one
        /// </summary>
        private static void SetAllRequirementCounts(HideoutProduction craft, int count)
        {
            if (craft.Requirements == null) return;
            foreach (var requirement in craft.Requirements)
            {
                if (requirement.Count.HasValue)
                {
                    requirement.Count = count;
                }
            }
        }

        private static Requirement? FindItem(HideoutProduction craft, MongoId templateId) =>
            craft.Requirements?.FirstOrDefault(r => r.TemplateId == templateId);

        private static Requirement? FindArea(HideoutProduction craft) =>
            craft.Requirements?.FirstOrDefault(r => r.Type == "Area");

        public static readonly List<CraftingAdjustment> All = new()
        {
            new(ItemTpl.BARTER_TOILET_PAPER, craft => craft.Count = 1),
            new(ItemTpl.BARTER_CLIN_WINDOW_CLEANER, craft => craft.Count = 4),
            new(ItemTpl.BARTER_PARACORD, craft => craft.Count = 2),

            new(ItemTpl.BARTER_CORRUGATED_HOSE, craft =>
            {
                SetAllRequirementCounts(craft, 1);
                craft.Count = 1;
            }),

            new(ItemTpl.BARTER_WATER_FILTER, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_GAS_MASK_AIR_FILTER);
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new(ItemTpl.DRINK_EMERGENCY_WATER_RATION, craft => craft.Count = 3),
            new(ItemTpl.BARTER_CAN_OF_MAJAICA_COFFEE_BEANS, craft => craft.Count = 3),
            new(ItemTpl.DRINK_BOTTLE_OF_WATER_06L, craft => craft.Count = 16),
            new(ItemTpl.STIM_MULE_STIMULANT_INJECTOR, craft => craft.Count = 2),

            new(ItemTpl.STIM_ETGCHANGE_REGENERATIVE_STIMULANT_INJECTOR, craft =>
            {
                craft.Count = 2;
                var requirement = FindItem(craft, ItemTpl.MEDICAL_CALOKB_HEMOSTATIC_APPLICATOR);
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new(ItemTpl.MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.MEDKIT_IFAK_INDIVIDUAL_FIRST_AID_KIT);
                if (requirement == null) return;
                requirement.Count = 1;
                requirement = FindItem(craft, ItemTpl.MEDICAL_ARMY_BANDAGE);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.MEDICAL_CALOKB_HEMOSTATIC_APPLICATOR;
            }),

            new(ItemTpl.MEDICAL_SURV12_FIELD_SURGICAL_KIT, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.MEDICAL_SURV12_FIELD_SURGICAL_KIT);
                if (requirement == null) return;
                requirement.Count = 2;
                requirement.TemplateId = ItemTpl.MEDICAL_CMS_SURGICAL_KIT;
            }),

            new(ItemTpl.BARTER_PORTABLE_DEFIBRILLATOR, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_PORTABLE_POWERBANK);
                if (requirement == null) return;
                requirement.Count = 4;
            }),

            new(ItemTpl.BARTER_LEDX_SKIN_TRANSILLUMINATOR, craft => SetAllRequirementCounts(craft, 1)),

            new(ItemTpl.MEDICAL_CMS_SURGICAL_KIT, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_MEDICAL_TOOLS);
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new(ItemTpl.MEDKIT_GRIZZLY_MEDICAL_KIT, craft => craft.Count = 1),
            new(ItemTpl.STIM_SJ6_TGLABS_COMBAT_STIMULANT_INJECTOR, craft => craft.Count = 3),
            new(ItemTpl.INFO_TOPOGRAPHIC_SURVEY_MAPS, craft => craft.Count = 2),

            new(ItemTpl.INFO_MILITARY_FLASH_DRIVE, craft =>
            {
                craft.Count = 1;
                var requirement = FindItem(craft, ItemTpl.INFO_SECURE_FLASH_DRIVE);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.BARTER_VPX_FLASH_STORAGE_MODULE;
                requirement = FindArea(craft);
                if (requirement == null) return;
                requirement.RequiredLevel = 2;
                SetAllRequirementCounts(craft, 1);
            }),

            new(ItemTpl.INFO_INTELLIGENCE_FOLDER, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.INFO_MILITARY_FLASH_DRIVE);
                if (requirement == null) return;
                requirement.Count = 1;
            }),

            new(ItemTpl.BARTER_VPX_FLASH_STORAGE_MODULE, craft => SetAllRequirementCounts(craft, 2)),

            new(ItemTpl.BARTER_VIRTEX_PROGRAMMABLE_PROCESSOR, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_MILITARY_CIRCUIT_BOARD);
                if (requirement == null) return;
                requirement.Count = 1;
            }),

            new(ItemTpl.BARTER_GRAPHICS_CARD, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_VPX_FLASH_STORAGE_MODULE);
                if (requirement == null) return;
                requirement.Count = 1;
                requirement.TemplateId = ItemTpl.BARTER_VIRTEX_PROGRAMMABLE_PROCESSOR;
                requirement = FindItem(craft, ItemTpl.BARTER_PC_CPU);
                if (requirement == null) return;
                requirement.Count = 1;
                requirement = FindItem(craft, ItemTpl.BARTER_PRINTED_CIRCUIT_BOARD);
                if (requirement == null) return;
                requirement.Count = 1;
            }),

            new(ItemTpl.BARTER_MILITARY_CIRCUIT_BOARD, craft => craft.Count = 2),

            new(ItemTpl.SPECIALSCOPE_FLIR_RS32_2259X_35MM_60HZ_THERMAL_RIFLESCOPE, craft =>
            {
                if (craft.Requirements == null) return;
                foreach (var requirement in craft.Requirements)
                {
                    if (requirement.Count.HasValue)
                    {
                        requirement.Count = 1;
                    }
                    if (requirement.TemplateId == ItemTpl.INFO_SAS_DRIVE)
                    {
                        requirement.TemplateId = ItemTpl.SPECIALSCOPE_ARMASIGHT_VULCAN_MG_35X_BRAVO_NIGHT_VISION_SCOPE;
                    }
                }
            }),

            new(ItemTpl.BARTER_UHF_RFID_READER, craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 11, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.BARTER_BROKEN_GPHONE_X_SMARTPHONE, Count = 1, IsFunctional = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.SPECITEM_SIGNAL_JAMMER, Count = 1, IsFunctional = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_FLAT_SCREWDRIVER_LONG, Type = "Tool" },
                    new() { TemplateId = ItemTpl.BARTER_FLAT_SCREWDRIVER, Type = "Tool" },
                    new() { Type = "QuestComplete", QuestId = QuestTpl.SNATCH },
                };
            }),

            new(ItemTpl.BARTER_GAS_ANALYZER, craft => SetAllRequirementCounts(craft, 1)),

            new(ItemTpl.BARTER_GUNPOWDER_HAWK, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_CLASSIC_MATCHES);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.BARTER_CAN_OF_THERMITE;
                requirement = FindArea(craft);
                if (requirement == null) return;
                requirement.RequiredLevel = 2;
            }),

            new(ItemTpl.BARTER_SPARK_PLUG, craft => craft.Count = 4),

            // Not in the TS mod: 6 tools -> 1 toolset is worse than Mechanic's barter
            new(ItemTpl.BARTER_TOOLSET, craft => craft.Count = 2),

            // TS source notes "this will break" on this one
            new(ItemTpl.BARTER_PRINTED_CIRCUIT_BOARD, craft =>
            {
                craft.Count = 3;
                var requirement = FindItem(craft, ItemTpl.BARTER_GAS_ANALYZER);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.BARTER_GEIGERMULLER_COUNTER;
            }),

            new(ItemTpl.BARTER_GEIGERMULLER_COUNTER, craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
                    new() { TemplateId = ItemTpl.BARTER_GAS_ANALYZER, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_TOOLSET, Type = "Tool" },
                };
            }),

            new(ItemTpl.BARTER_GREENBAT_LITHIUM_BATTERY, craft =>
            {
                craft.Count = 2;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.BARTER_PORTABLE_POWERBANK, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_ROUND_PLIERS, Type = "Tool" },
                };
            }),

            new(ItemTpl.GRENADE_VOG25_KHATTABKA_IMPROVISED_HAND, craft => SetAllRequirementCounts(craft, 2)),

            new(ItemTpl.BARTER_BROKEN_LCD, craft =>
            {
                craft.Count = 1;
                SetAllRequirementCounts(craft, 1);
            }),

            new(ItemTpl.AMMO_23X75_ZVEZDA, craft =>
            {
                craft.Count = 20;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.BARTER_GUNPOWDER_EAGLE, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.AMMO_23X75_SHRAP10, Count = 20, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.GRENADE_ZARYA_STUN, Count = 2, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_TOOLSET, Type = "Tool" },
                    new() { TemplateId = ItemTpl.MULTITOOLS_LEATHERMAN_MULTITOOL, Type = "Tool" },
                };
            }),

            new(ItemTpl.BARTER_RECHARGEABLE_BATTERY, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.BARTER_PORTABLE_POWERBANK);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.BARTER_ELECTRIC_DRILL;
            }),

            new(ItemTpl.BARTER_CAN_OF_THERMITE, craft =>
            {
                var requirement = FindItem(craft, ItemTpl.KEY_DORM_ROOM_308);
                if (requirement == null) return;
                requirement.TemplateId = ItemTpl.KNIFE_BARS_A2607_DAMASCUS;
            }),

            new(ItemTpl.AMMO_45ACP_AP, craft =>
            {
                craft.Count = 120;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.AMMO_45ACP_LASERMATCH, Count = 120, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.MULTITOOLS_LEATHERMAN_MULTITOOL, Type = "Tool" },
                    new() { TemplateId = ItemTpl.BARTER_GUNPOWDER_EAGLE, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_PACK_OF_NAILS, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_SET_OF_FILES_MASTER, Type = "Tool" },
                };
            }),

            // Disabled in the TS source as well
            // new(ItemTpl.AMMO_57X28_SS190, craft =>
            // {
            //     craft.Requirements = new List<Requirement>
            //     {
            //         new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
            //         new() { TemplateId = ItemTpl.BARTER_HAND_DRILL, Type = "Tool" },
            //         new() { TemplateId = ItemTpl.BARTER_PLIERS_ELITE, Type = "Tool" },
            //         new() { TemplateId = ItemTpl.AMMO_57X28_SS197SR, Count = 180, IsFunctional = false, IsEncoded = false, Type = "Item" },
            //         new() { TemplateId = ItemTpl.BARTER_GUNPOWDER_HAWK, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
            //         new() { TemplateId = ItemTpl.BARTER_PACK_OF_NAILS, Count = 2, IsFunctional = false, IsEncoded = false, Type = "Item" },
            //     };
            // }),

            new(ItemTpl.AMMO_556X45_SOST, craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.AMMO_556X45_HP, Count = 150, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_GUNPOWDER_EAGLE, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_PLIERS_ELITE, Type = "Tool" },
                };
            }),

            new(ItemTpl.AMMO_9X18PM_PSTM, craft =>
            {
                craft.Requirements ??= new List<Requirement>();
                craft.Requirements.Add(new Requirement
                {
                    TemplateId = ItemTpl.AMMO_9X18PM_PST,
                    Count = 140,
                    IsFunctional = false,
                    IsEncoded = false,
                    Type = "Item"
                });
            }),

            new(ItemTpl.AMMO_12G_AP20, craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = ItemTpl.AMMO_12G_MAGNUM, Count = 80, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.AMMO_9X19_AP_63, Count = 80, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = ItemTpl.BARTER_NIPPERS, Type = "Tool" },
                    new() { TemplateId = ItemTpl.BARTER_FLAT_SCREWDRIVER_LONG, Type = "Tool" },
                    new() { Type = "QuestComplete", QuestId = QuestTpl.THE_HUNTSMAN_PATH_OUTCASTS },
                };
            }),

            // Disabled in the TS source as well
            // new(ItemTpl.AMMO_366TKM_APM, craft =>
            // {
            //     craft.Requirements = new List<Requirement>
            //     {
            //         new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
            //         new() { TemplateId = ItemTpl.AMMO_9X39_SPP, Count = 100, IsFunctional = false, IsEncoded = false, Type = "Item" },
            //         new() { TemplateId = ItemTpl.AMMO_762X39_HP, Count = 100, IsFunctional = false, IsEncoded = false, Type = "Item" },
            //         new() { TemplateId = ItemTpl.BARTER_PLIERS, Type = "Tool" },
            //         new() { Type = "QuestComplete", QuestId = QuestTpl.THE_TARKOV_SHOOTER_PART_3 },
            //     };
            // }),

            new(ItemTpl.BARTER_OFZ_30X165MM_SHELL, craft => SetAllRequirementCounts(craft, 1)),
            new(ItemTpl.GRENADE_RGD5_HAND, craft => SetAllRequirementCounts(craft, 1)),
            new(ItemTpl.GRENADE_ZARYA_STUN, craft => SetAllRequirementCounts(craft, 1)),
            new(ItemTpl.AMMO_12G_PIRANHA, craft => craft.Count = 150),
            new(ItemTpl.AMMO_545X39_BP, craft => craft.Count = 180),
            new(ItemTpl.AMMO_556X45_M855A1, craft => craft.Count = 180),
        };
    }

    public static class NewRecipes
    {
        #region Container Recipes

        private static readonly HideoutProduction Alpha = new()
        {
            Id = "63da4dbee8fa73e22500001a",
            AreaType = HideoutAreas.Workbench,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
                new() { TemplateId = ItemTpl.LOCKABLECONTAINER_PISTOL_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_SIMPLE_WALLET, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_DOGTAG_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.INFO_SECURE_FLASH_DRIVE, Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 5600,
            EndProduct = ItemTpl.SECURE_CONTAINER_ALPHA,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = true,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Beta = new()
        {
            Id = "63da4dbee8fa73e22500001b",
            AreaType = HideoutAreas.Workbench,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
                new() { TemplateId = ItemTpl.SECURE_CONTAINER_ALPHA, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_AMMUNITION_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_DOCUMENTS_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.INFO_MILITARY_FLASH_DRIVE, Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 10800,
            EndProduct = ItemTpl.SECURE_CONTAINER_BETA,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = true,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Epsilon = new()
        {
            Id = "63da4dbee8fa73e22500001c",
            AreaType = HideoutAreas.Workbench,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                new() { TemplateId = ItemTpl.SECURE_CONTAINER_BETA, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_MAGAZINE_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_KEY_TOOL, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_KEYCARD_HOLDER_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.INFO_SECURE_MAGNETIC_TAPE_CASSETTE, Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 35000,
            EndProduct = ItemTpl.SECURE_CONTAINER_EPSILON,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = true,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Gamma = new()
        {
            Id = "63da4dbee8fa73e22500001d",
            AreaType = HideoutAreas.Workbench,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 10, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.SECURE_CONTAINER_EPSILON, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_GRENADE_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_MONEY_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_SICC, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.CONTAINER_INJECTOR_CASE, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_MICROCONTROLLER_BOARD, Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 61200,
            EndProduct = ItemTpl.SECURE_CONTAINER_GAMMA,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = true,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        #endregion

        #region Additional Recipes

        private static readonly HideoutProduction Ophthalmoscope = new()
        {
            Id = "63da4dbee8fa73e225000001",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.BARTER_GREENBAT_LITHIUM_BATTERY, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_MEDICAL_TOOLS, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.FLASHLIGHT_ULTRAFIRE_WF501B, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.SPECITEM_WIFI_CAMERA, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_DUCT_TAPE, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 105,
            EndProduct = ItemTpl.BARTER_OPHTHALMOSCOPE,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Zagustin = new()
        {
            Id = "63da4dbee8fa73e225000002",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_PROPITAL_REGENERATIVE_STIMULANT_INJECTOR, Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.MEDICAL_CALOKB_HEMOSTATIC_APPLICATOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_AHF1M_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 105,
            EndProduct = ItemTpl.STIM_ZAGUSTIN_HEMOSTATIC_DRUG_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 3,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Obdolbos = new()
        {
            Id = "63da4dbee8fa73e225000003",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_SJ1_TGLABS_COMBAT_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_FUEL_CONDITIONER, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_SMOKED_CHIMNEY_DRAIN_CLEANER, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_BOTTLE_OF_PEVKO_LIGHT_BEER, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_BOTTLE_OF_TARKOVSKAYA_VODKA, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_BOTTLE_OF_DAN_JACKIEL_WHISKEY, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_FP100_FILTER_ABSORBER, Type = "Tool" },
            },
            ProductionTime = 564,
            EndProduct = ItemTpl.STIM_OBDOLBOS_COCKTAIL_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 8,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction CALOK = new()
        {
            Id = "63da4dbee8fa73e225000004",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 2, Type = "Area" },
                new() { TemplateId = ItemTpl.BARTER_PACK_OF_SODIUM_BICARBONATE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRUGS_VASELINE_BALM, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 48,
            EndProduct = ItemTpl.MEDICAL_CALOKB_HEMOSTATIC_APPLICATOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Adrenaline = new()
        {
            Id = "63da4dbee8fa73e225000005",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 2, Type = "Area" },
                new() { TemplateId = ItemTpl.DRINK_CAN_OF_HOT_ROD_ENERGY, Count = 3, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.MEDKIT_AI2, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 23,
            EndProduct = ItemTpl.STIM_ADRENALINE_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction ThreebTG = new()
        {
            Id = "63da4dbee8fa73e225000006",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_ADRENALINE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_BOTTLE_OF_HYDROGEN_PEROXIDE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.FOOD_ALYONKA_CHOCOLATE_BAR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 31,
            EndProduct = ItemTpl.STIM_3BTG_STIMULANT_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction AHF1 = new()
        {
            Id = "63da4dbee8fa73e225000007",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 2, Type = "Area" },
                new() { TemplateId = ItemTpl.DRUGS_AUGMENTIN_ANTIBIOTIC_PILLS, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRUGS_MORPHINE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 47,
            EndProduct = ItemTpl.STIM_AHF1M_STIMULANT_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction OLOLO = new()
        {
            Id = "63da4dbee8fa73e225000008",
            AreaType = HideoutAreas.Kitchen,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 8, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.DRINK_PACK_OF_GRAND_JUICE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_PACK_OF_VITA_JUICE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_PACK_OF_APPLE_JUICE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_CAN_OF_ICE_GREEN_TEA, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRINK_PACK_OF_RUSSIAN_ARMY_PINEAPPLE_JUICE, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.DRUGS_ANALGIN_PAINKILLERS, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.BARTER_WATER_FILTER, Type = "Tool" },
                new() { TemplateId = ItemTpl.BARTER_ANTIQUE_TEAPOT, Type = "Tool" },
            },
            ProductionTime = 71,
            EndProduct = ItemTpl.BARTER_BOTTLE_OF_OLOLO_MULTIVITAMINS,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 3,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction L1 = new()
        {
            Id = "63da4dbee8fa73e225000009",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_ADRENALINE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_SJ6_TGLABS_COMBAT_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 71,
            EndProduct = ItemTpl.STIM_L1_NOREPINEPHRINE_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 1,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Trimadol = new()
        {
            Id = "63da4dbee8fa73e225000011",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_3BTG_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_L1_NOREPINEPHRINE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 52,
            EndProduct = ItemTpl.STIM_TRIMADOL_STIMULANT_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Meldonin = new()
        {
            Id = "63da4dbee8fa73e225000012",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_L1_NOREPINEPHRINE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_MULE_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 39,
            EndProduct = ItemTpl.STIM_MELDONIN_INJECTOR,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Continuous = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        private static readonly HideoutProduction Perfotran = new()
        {
            Id = "63da4dbee8fa73e225000014",
            AreaType = HideoutAreas.MedStation,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 7, RequiredLevel = 3, Type = "Area" },
                new() { TemplateId = ItemTpl.STIM_ZAGUSTIN_HEMOSTATIC_DRUG_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_XTG12_ANTIDOTE_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = ItemTpl.STIM_PROPITAL_REGENERATIVE_STIMULANT_INJECTOR, Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 45,
            EndProduct = ItemTpl.STIM_PERFOTORAN_BLUE_BLOOD_STIMULANT_INJECTOR,
            Continuous = false,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        #endregion

        // Declared after the recipe fields: static initialisers run in textual order,
        // so listing them first would populate this with nulls.
        public static readonly List<HideoutProduction> All = new()
        {
            // Container progression recipes (alpha -> beta -> epsilon -> gamma)
            Alpha,
            Beta,
            Epsilon,
            Gamma,

            // Additional stim/medical recipes
            ThreebTG,
            Adrenaline,
            L1,
            AHF1,
            CALOK,
            Ophthalmoscope,
            Zagustin,
            Obdolbos,
            OLOLO,
            Perfotran,
            Trimadol,
            Meldonin,
        };
    }
}
