using HarmonyLib;
using Verse;
using Verse.AI;

namespace WeaponStorage;

[HarmonyPatch(typeof(ReservationManager), nameof(ReservationManager.CanReserve))]
internal static class Patch_ReservationManager_CanReserve
{
    private static void Postfix(ref bool __result, LocalTargetInfo target, Map ___map)
    {
        if (__result || target.Thing != null && !(target.GetType() == typeof(Building_WeaponStorage)))
        {
            return;
        }

        var enumerable = ___map?.thingGrid.ThingsAt(target.Cell);
        if (enumerable == null)
        {
            return;
        }

        foreach (var item in enumerable)
        {
            if (item.GetType() == typeof(Building_WeaponStorage))
            {
                __result = true;
            }
        }
    }
}