using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace Softcore.Assets;

/// <summary>
/// Represents an adjustment to an existing crafting recipe
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
/// Static data for crafting recipe adjustments and new recipes
/// </summary>
public static class CraftingData
{
    public static class Adjustments
    {
        public static readonly List<CraftingAdjustment> All = new()
        {
            // Simple count adjustments
            new("5bc9b355d4351e6d1509862a", craft => craft.Count = 1), // BARTER_TOILET_PAPER
            new("5c0111ab0db834001966914d", craft => craft.Count = 4), // BARTER_CLIN_WINDOW_CLEANER
            new("5c12613b86f7743bbe2c3f76", craft => craft.Count = 2), // BARTER_PARACORD
            new("5d1b385e86f774252167b98a", craft => craft.Count = 3), // DRINK_EMERGENCY_WATER_RATION
            new("5d40407c86f774318526545a", craft => craft.Count = 3), // BARTER_CAN_OF_MAJAICA_COFFEE_BEANS
            new("5448fee04bdc2dbc018b4567", craft => craft.Count = 16), // DRINK_BOTTLE_OF_WATER_06L
            new("5ed515ece452db0eb56fc028", craft => craft.Count = 2), // STIM_MULE_STIMULANT_INJECTOR
            new("60098ad7c2240c0fe85c570a", craft => craft.Count = 1), // MEDKIT_GRIZZLY_MEDICAL_KIT
            new("5c10c8fd86f7743d7d706df3", craft => craft.Count = 3), // STIM_SJ6_TGLABS_COMBAT_STIMULANT_INJECTOR
            new("5c052e6986f7746b207bc3c9", craft => craft.Count = 2), // INFO_TOPOGRAPHIC_SURVEY_MAPS
            new("5bc9b720d4351e450201234b", craft => craft.Count = 2), // BARTER_MILITARY_CIRCUIT_BOARD
            new("590a3c0a86f774385a33c450", craft => craft.Count = 4), // BARTER_SPARK_PLUG
            new("62a09f32621468534a797acb", craft => craft.Count = 150), // AMMO_12G_PIRANHA
            new("56dff421d2720b5f5a8b4567", craft => craft.Count = 180), // AMMO_545X39_BP
            new("54527ac44bdc2d36668b4567", craft => craft.Count = 180), // AMMO_556X45_M855A1

            // Find and modify specific requirements
            new("590c60fc86f77412b13fddcf", craft => // BARTER_WATER_FILTER
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5c06779c86f77426e00dd782");
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new("5ed5160a87bb8443d10680b5", craft => // STIM_ETGCHANGE_REGENERATIVE_STIMULANT_INJECTOR
            {
                craft.Count = 2;
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5c0e530286f7747fa1419862");
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new("60098ad7c2240c0fe85c570a", craft => // MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT
            {
                var req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "590c657e86f77412b013051d");
                if (req == null) return;
                req.Count = 1;
                req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5755356824597772cb798962");
                if (req == null) return;
                req.TemplateId = "5c0e530286f7747fa1419862";
            }),

            new("5d02797c86f774203f38e30a", craft => // MEDICAL_SURV12_FIELD_SURGICAL_KIT
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5d02797c86f774203f38e30a");
                if (requirement == null) return;
                requirement.Count = 2;
                requirement.TemplateId = "5d02778e86f774203e7dedbe";
            }),

            new("5c052fb986f7746b2101e909", craft => // BARTER_PORTABLE_DEFIBRILLATOR
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5733279d245977289b77ec24");
                if (requirement == null) return;
                requirement.Count = 4;
            }),

            new("5d02778e86f774203e7dedbe", craft => // MEDICAL_CMS_SURGICAL_KIT
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5755356824597772cb798962");
                if (requirement == null) return;
                requirement.Count = 2;
            }),

            new("60b0f6c058e0b0481a09ad11", craft => // INFO_MILITARY_FLASH_DRIVE
            {
                craft.Count = 1;
                var req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "590c2d4786f77470e92f38fd");
                if (req == null) return;
                req.TemplateId = "5c052e6986f7746b207bc3c9";
                req = craft.Requirements?.FirstOrDefault(r => r.Type == "Area");
                if (req == null) return;
                req.RequiredLevel = 2;
                if (craft.Requirements != null)
                {
                    foreach (var x in craft.Requirements)
                    {
                        if (x.Count.HasValue)
                        {
                            x.Count = 1;
                        }
                    }
                }
            }),

            new("5c052e6986f7746b207bc3c9", craft => // INFO_INTELLIGENCE_FOLDER
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "60b0f6c058e0b0481a09ad11");
                if (requirement == null) return;
                requirement.Count = 1;
            }),

            new("5c052fb986f7746b2101e909", craft => // BARTER_VIRTEX_PROGRAMMABLE_PROCESSOR
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5bc9bc53d4351e00367fbcee");
                if (requirement == null) return;
                requirement.Count = 1;
            }),

            new("57347c93245977448d35f6e4", craft => // BARTER_GRAPHICS_CARD
            {
                var req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5c052fb986f7746b2101e909");
                if (req == null) return;
                req.Count = 1;
                req.TemplateId = "5c052f6886f7746b1e3db148";
                req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5c052f6886f7746b1e3db148");
                if (req == null) return;
                req.Count = 1;
                req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5c06779c86f77426e00dd782");
                if (req == null) return;
                req.Count = 1;
            }),

            new("590a358486f77429692b2790", craft => // BARTER_RECHARGEABLE_BATTERY
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5733279d245977289b77ec24");
                if (requirement == null) return;
                requirement.TemplateId = "57347c1124597737fb1379e3";
            }),

            new("5d70e500a4b9364de70d38ce", craft => // BARTER_CAN_OF_THERMITE
            {
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "5780d0532459777a5108b9a2");
                if (requirement == null) return;
                requirement.TemplateId = "5c0126f40db834002a125382";
            }),

            new("5e340dcdcb6d5863cc5e5efb", craft => // BARTER_GUNPOWDER_HAWK
            {
                var req = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "590c311186f77424d1667482");
                if (req == null) return;
                req.TemplateId = "5d70e500a4b9364de70d38ce";
                req = craft.Requirements?.FirstOrDefault(r => r.Type == "Area");
                if (req == null) return;
                req.RequiredLevel = 2;
            }),

            // Complex adjustments - loops
            new("5d1b371186f774253763a656", craft => // BARTER_CORRUGATED_HOSE
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
                craft.Count = 1;
            }),

            new("5bc9bdb8d4351e003562b8a1", craft => // BARTER_LEDX_SKIN_TRANSILLUMINATOR
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("5c052fb986f7746b2101e909", craft => // BARTER_VPX_FLASH_STORAGE_MODULE
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 2;
                        }
                    }
                }
            }),

            new("590a3efd86f77437d351a25b", craft => // BARTER_GAS_ANALYZER
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("5e340dcdcb6d5863cc5e5efb", craft => // GRENADE_VOG25_KHATTABKA_IMPROVISED_HAND
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 2;
                        }
                    }
                }
            }),

            new("5f2a9575926fd9352339381f", craft => // BARTER_BROKEN_LCD
            {
                craft.Count = 1;
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("62178c4d4ecf221597654e3d", craft => // BARTER_OFZ_30X165MM_SHELL
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("5e32f56fcb6d5863cc5e5ee4", craft => // GRENADE_RGD5_HAND
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("5b0bfa0f5acfc432ff4dcbaf", craft => // GRENADE_ZARYA_STUN
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                    }
                }
            }),

            new("62a0a16d0b9d3c46de5b6e97", craft => // SPECIALSCOPE_FLIR_RS32_2259X_35MM_60HZ_THERMAL_RIFLESCOPE
            {
                if (craft.Requirements != null)
                {
                    foreach (var requirement in craft.Requirements)
                    {
                        if (requirement.Count.HasValue)
                        {
                            requirement.Count = 1;
                        }
                        if (requirement.TemplateId == "5a154d5cfcdbcb001a3b00da")
                        {
                            requirement.TemplateId = "558022b54bdc2dac148b458d";
                        }
                    }
                }
            }),

            // Complex adjustments - complete requirement replacements
            new("5c052f6886f7746b1e3db148", craft => // BARTER_UHF_RFID_READER
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 11, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5c052e6986f7746b207bc3c9", Count = 1, IsFunctional = false, Type = "Item" },
                    new() { TemplateId = "5c13cd2486f774072c757944", Count = 1, IsFunctional = false, Type = "Item" },
                    new() { TemplateId = "59ccfdba86f7747f2109a587", Type = "Tool" },
                    new() { TemplateId = "590c31c586f774245e3141b2", Type = "Tool" },
                    new() { Type = "QuestComplete", QuestId = "63966fccac6f8f3c677b9d89" },
                };
            }),

            new("5c06779c86f77426e00dd782", craft => // BARTER_PRINTED_CIRCUIT_BOARD
            {
                craft.Count = 3;
                var requirement = craft.Requirements?.FirstOrDefault(r => r.TemplateId == "590a3efd86f77437d351a25b");
                if (requirement == null) return;
                requirement.TemplateId = "5d1b2fa286f77425227d1674";
            }),

            new("5d1b2fa286f77425227d1674", craft => // BARTER_GEIGERMULLER_COUNTER
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
                    new() { TemplateId = "590a3efd86f77437d351a25b", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b376e86f774252519444e", Type = "Tool" },
                };
            }),

            new("5d1b2f3f86f774252167a52c", craft => // BARTER_GREENBAT_LITHIUM_BATTERY
            {
                craft.Count = 2;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5733279d245977289b77ec24", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b33e486f7742523398394", Type = "Tool" },
                };
            }),

            new("5ede4739e0350d05467f73e8", craft => // AMMO_23X75_ZVEZDA
            {
                craft.Count = 20;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5d6fc78386f77449d825f9dc", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5ede47405b097655935d7d16", Count = 20, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5b0bfa0f5acfc432ff4dcbaf", Count = 2, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b376e86f774252519444e", Type = "Tool" },
                    new() { TemplateId = "5d1b309586f77425227d1676", Type = "Tool" },
                };
            }),

            new("5e81c3cbac2bb513793cdc75", craft => // AMMO_45ACP_AP
            {
                craft.Count = 120;
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5e81c519cb2b95385c177551", Count = 120, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b309586f77425227d1676", Type = "Tool" },
                    new() { TemplateId = "5d6fc78386f77449d825f9dc", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "590a373286f774287540368b", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b327086f7742525194449", Type = "Tool" },
                };
            }),

            // COMMENTED OUT: AMMO_57X28_SS190 (lines 495-535 in TS)
            /*
            new("5cc80f53e4a949000e1ea4f8", craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5d1b2f3f86f774252167a52c", Type = "Tool" },
                    new() { TemplateId = "5d1b33a686f7742523398398", Type = "Tool" },
                    new() { TemplateId = "5cc80f79e4a949033c7343b2", Count = 180, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5e340dcdcb6d5863cc5e5efb", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "590a373286f774287540368b", Count = 2, IsFunctional = false, IsEncoded = false, Type = "Item" },
                };
            }),
            */

            new("59e690b686f7746c9f75e848", craft => // AMMO_556X45_SOST
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "59e6906286f7746c9f75e847", Count = 150, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d6fc78386f77449d825f9dc", Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b33a686f7742523398398", Type = "Tool" },
                };
            }),

            new("573719df2459775a626ccbc2", craft => // AMMO_9X18PM_PSTM
            {
                craft.Requirements?.Add(new Requirement
                {
                    TemplateId = "573719762459775a626ccbc1",
                    Count = 140,
                    IsFunctional = false,
                    IsEncoded = false,
                    Type = "Item"
                });
            }),

            new("5d6e68a8a4b9360b6c0d54e2", craft => // AMMO_12G_AP20
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "5d6e6806a4b936088465b17e", Count = 80, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5c925fa22e221601da359b7b", Count = 80, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b36a186f7742523398433", Type = "Tool" },
                    new() { TemplateId = "59ccfdba86f7747f2109a587", Type = "Tool" },
                    new() { Type = "QuestComplete", QuestId = "6179ad0a6e9dd54ac275e3f2" },
                };
            }),

            // COMMENTED OUT: AMMO_366TKM_APM (lines 616-649 in TS)
            /*
            new("5f0596629e22f464da6bbdd9", craft =>
            {
                craft.Requirements = new List<Requirement>
                {
                    new() { AreaType = 10, RequiredLevel = 2, Type = "Area" },
                    new() { TemplateId = "57a0dfb82459774d3078b56c", Count = 100, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "59e0d99486f7744a32234762", Count = 100, IsFunctional = false, IsEncoded = false, Type = "Item" },
                    new() { TemplateId = "5d1b371186f774253763a656", Type = "Tool" },
                    new() { Type = "QuestComplete", QuestId = "5bc47dbf86f7741ee74e93b9" },
                };
            }),
            */
        };
    }

    public static class NewRecipes
    {
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

        #region Container Recipes

        private static readonly HideoutProduction Alpha = new()
        {
            Id = "63da4dbee8fa73e22500001a",
            AreaType = HideoutAreas.Workbench,
            Requirements = new List<Requirement>
            {
                new() { AreaType = 10, RequiredLevel = 1, Type = "Area" },
                new() { TemplateId = "567143bf4bdc2d1a0f8b4567", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5783c43d2459774bbe137486", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5c093e3486f77430cb02e593", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "590c621186f774138d11ea29", Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 5600,
            EndProduct = "544a11ac4bdc2d470e8b456a",
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
                new() { TemplateId = "544a11ac4bdc2d470e8b456a", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5aafbde786f774389d0cbc0f", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "590c60fc86f77412b13fddcf", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "62a0a16d0b9d3c46de5b6e97", Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 10800,
            EndProduct = "5857a8b324597729ab0a0e7d",
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
                new() { TemplateId = "5857a8b324597729ab0a0e7d", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5c127c4486f7745625356c13", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "59fafd4b86f7745ca07e1232", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "619cbf9e0a7c3a1a2731940a", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "61bf7c024770ee6f9c6b8b53", Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 35000,
            EndProduct = "59db794186f77448bc595262",
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
                new() { TemplateId = "59db794186f77448bc595262", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5e2af55f86f7746d4159f07c", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "59fb016586f7746d0d4b423a", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d235bb686f77443f4331278", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "619cbf7d23893217ec30b689", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "6389c7750ef44505c87f5996", Count = 2, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 61200,
            EndProduct = "5857a8bc2459772bad15db29",
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
                new() { TemplateId = "5e2aedd986f7746d404f3aa4", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "619cc01e0a7c3a1a2731940c", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "57d17c5e2459775a5c57d17d", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5b4391a586f7745321235ab2", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "57347c1124597737fb1379e3", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 105,
            EndProduct = "5af0534a86f7743b6f354284",
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
                new() { TemplateId = "5c0e530286f7747fa1419862", Count = 2, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5e8488fa988a8701445df1e4", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5ed515f6915ec335206e4152", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 105,
            EndProduct = "5c0e533786f7747fa23f4d47",
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
                new() { TemplateId = "5c0e531286f7747fa54205c2", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5b43575a86f77424f443fe62", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5e2af00086f7746d3f3c33f7", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "62a09f32621468534a797acb", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d40407c86f774318526545a", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d403f9186f7743cac3f229b", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d1b376e86f774252519444e", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d1b2f3f86f774252167a52c", Type = "Tool" },
            },
            ProductionTime = 564,
            EndProduct = "5ed5166ad380ab312177c100",
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
                new() { TemplateId = "59e35abd86f7741778269d82", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5755383e24597772cb798966", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 48,
            EndProduct = "5e8488fa988a8701445df1e4",
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
                new() { TemplateId = "5751496424597720a27126da", Count = 3, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5755356824597772cb798962", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 23,
            EndProduct = "5c10c8fd86f7743d7d706df3",
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
                new() { TemplateId = "5c10c8fd86f7743d7d706df3", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "59e361e886f774176c10a2a5", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "57505f6224597709a92585a9", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 31,
            EndProduct = "5ed515c8d380ab312177c0fa",
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
                new() { TemplateId = "590c695186f7741e566b64a2", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "544fb3f34bdc2d03748b456a", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 47,
            EndProduct = "5ed515f6915ec335206e4152",
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
                new() { TemplateId = "57513f9324597720a7128161", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "57513fcc24597720a31c09a6", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "57513f07245977207e26a311", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "575062b524597720a31c09a1", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "544fb62a4bdc2dfb738b4568", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "544fb37f4bdc2dee738b4567", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5d1b385e86f774252167b98a", Type = "Tool" },
                new() { TemplateId = "590de71386f774347051a052", Type = "Tool" },
            },
            ProductionTime = 71,
            EndProduct = "62a0a043cf4a99369e2624a5",
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
                new() { TemplateId = "5c10c8fd86f7743d7d706df3", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5c0e531d86f7747fa23f4d42", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 71,
            EndProduct = "5ed515e03a40a50460332579",
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
                new() { TemplateId = "5ed515c8d380ab312177c0fa", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5ed515e03a40a50460332579", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 52,
            EndProduct = "637b620db7afa97bfc3d7009",
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
                new() { TemplateId = "5ed515e03a40a50460332579", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5ed51652f6c34d2cc26336a1", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 39,
            EndProduct = "5ed5160a87bb8443d10680b5",
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
                new() { TemplateId = "5c0e533786f7747fa23f4d47", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5fca138c2a7b221b2852a5c6", Count = 1, IsFunctional = false, Type = "Item" },
                new() { TemplateId = "5c0e530286f7747fa1419862", Count = 1, IsFunctional = false, Type = "Item" },
            },
            ProductionTime = 45,
            EndProduct = "637b6251104668754b72f8f9",
            Continuous = false,
            IsEncoded = false,
            Locked = false,
            NeedFuelForAllProductionTime = false,
            Count = 2,
            ProductionLimitCount = 0,
            IsCodeProduction = false
        };

        #endregion
    }
}
