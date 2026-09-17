using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Softcore.Config;

namespace Softcore.Changers;

/// <summary>
/// Applies quest reward changes. Runs at Preload + 1, before anything reads quests; QuestHelper reads
/// <see cref="TemplateTable.Quests"/> live, so a single edit here is persistent.
/// </summary>
[Injectable]
public class QuestRewardsChanger(
    ISptLogger<QuestRewardsChanger> logger,
    TemplateTable templateTable)
{
    private const string StirrupQuestId = "596b455186f77457cb50eccb";

    // Fixed ids, shared with Geko's Better Progression so a profile that completed the quest under that
    // mod, or restarts mid-way, sees the same reward.
    private static readonly MongoId StirrupRewardId = new("67dfe1d6c86d01e21c962c36");
    private static readonly MongoId StirrupRewardItemId = new("67dfe1d2e5b3ffda5e44b529");

    private readonly ISptLogger<QuestRewardsChanger> _logger = logger;
    private readonly TemplateTable _templateTable = templateTable;

    public void Apply(QuestRewardsConfig config)
    {
        if (!config.Enabled)
        {
            _logger.Info("[Softcore] Quest reward changes disabled");
            return;
        }

        if (config.StirrupAmmunitionCase)
        {
            try
            {
                DoStirrupAmmunitionCase();
            }
            catch (Exception ex)
            {
                _logger.Warning($"[Softcore] Stirrup ammunition case failed: {ex.Message}");
            }
        }
    }

    /// <summary>Appends one Ammunition Case to Stirrup's Success rewards, shaped like the vanilla item rewards.</summary>
    private void DoStirrupAmmunitionCase()
    {
        if (!_templateTable.Quests.TryGetValue(StirrupQuestId, out var quest))
        {
            _logger.Warning("[Softcore] Quest Stirrup not found, skipping");
            return;
        }

        if (quest.Rewards == null || !quest.Rewards.TryGetValue("Success", out var rewards))
        {
            _logger.Warning("[Softcore] Quest Stirrup has no Success rewards, skipping");
            return;
        }

        if (rewards.Any(reward => reward.Id == StirrupRewardId))
        {
            _logger.Warning("[Softcore] Quest Stirrup already rewards an Ammunition Case, skipping");
            return;
        }

        rewards.Add(new Reward
        {
            Id = StirrupRewardId,
            Type = RewardType.Item,
            Index = rewards.Count,
            Target = StirrupRewardItemId,
            Value = 1,
            FindInRaid = false,
            Unknown = false,
            IsHidden = false,
            GameMode = ["regular", "pve"],
            AvailableInGameEditions = [],
            Items =
            [
                new Item
                {
                    Id = StirrupRewardItemId,
                    Template = ItemTpl.CONTAINER_AMMUNITION_CASE,
                    Upd = new Upd { StackObjectsCount = 1 },
                },
            ],
        });

        _logger.Success("[Softcore] Stirrup rewards an Ammunition Case");
    }
}
