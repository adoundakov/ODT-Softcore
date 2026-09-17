using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Models.Enums;

namespace Softcore.Patches;

/// <summary>
/// Adds GP to the currency courses the client receives with every trader's price list. The client
/// computes the sell price it displays from that dictionary, so a GP-currency trader shows nothing
/// sellable without a GP entry. The course is read from the handbook prices in the same response
/// (7500 on vanilla data) rather than hard-coded, so it tracks any handbook edit. Extra key for
/// every other trader; harmless. Enabled by <see cref="Plugin"/> when refChanges.buysInGpCoins is on.
/// </summary>
[Injectable]
public class GpCurrencyCoursePatch : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TraderController).GetMethod(nameof(TraderController.GetItemPrices))
            ?? throw new InvalidOperationException("TraderController.GetItemPrices not found");
    }

    [PatchPostfix]
    private static void Postfix(GetItemPricesResponse __result)
    {
        if (__result.CurrencyCourses == null || __result.Prices == null) return;

        if (__result.Prices.TryGetValue(Money.GP, out var course))
        {
            __result.CurrencyCourses.TryAdd(Money.GP, course);
        }
    }
}
