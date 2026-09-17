using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace Softcore.Changers;

/// <summary>Trader assort edits shared by the changers.</summary>
internal static class AssortHelper
{
    /// <summary>
    /// Adds a barter in the same shape as the vanilla case barters (unlimited stack, per-restock buy
    /// limit). The id is minted on every server start; that is fine because the live table is what
    /// trader resets clone from and TraderPurchasePersisterService drops purchases whose assort id no
    /// longer exists.
    /// </summary>
    public static void CreateBarter(TraderAssort assort, MongoId tpl, int loyaltyLevel, int buyLimit, List<BarterScheme> requirements)
    {
        var root = new Item
        {
            Id = new MongoId(),
            Template = tpl,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                UnlimitedCount = true,
                StackObjectsCount = 9999999,
                BuyRestrictionMax = buyLimit,
                BuyRestrictionCurrent = 0,
            },
        };

        assort.Items.Add(root);
        assort.BarterScheme[root.Id] = [requirements];
        assort.LoyalLevelItems[root.Id] = loyaltyLevel;
    }
}
