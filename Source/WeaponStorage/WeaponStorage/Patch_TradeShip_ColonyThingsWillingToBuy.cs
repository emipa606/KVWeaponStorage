using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
internal static class Patch_TradeShip_ColonyThingsWillingToBuy
{
    private static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
    {
        if (playerNegotiator == null || playerNegotiator.Map == null)
        {
            return;
        }

        var list = new List<Thing>(__result);
        list.AddRange(TradeUtil.EmptyWeaponStorages(playerNegotiator.Map));
        __result = list;
    }
}