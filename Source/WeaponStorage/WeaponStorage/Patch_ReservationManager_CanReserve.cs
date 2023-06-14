using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace WeaponStorage;

[HarmonyPatch(typeof(ReservationManager), "CanReserve")]
internal static class Patch_ReservationManager_CanReserve
{
    private static FieldInfo mapFI;

    private static void Postfix(ref bool __result, ReservationManager __instance, LocalTargetInfo target)
    {
        if (mapFI == null)
        {
            mapFI = typeof(ReservationManager).GetField("map", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if (__result || target.Thing != null && !(target.GetType() == typeof(Building_WeaponStorage)))
        {
            return;
        }

        var enumerable = ((Map)mapFI?.GetValue(__instance))?.thingGrid.ThingsAt(target.Cell);
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