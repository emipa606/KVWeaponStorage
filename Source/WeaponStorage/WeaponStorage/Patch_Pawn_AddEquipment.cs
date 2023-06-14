using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
internal static class Patch_Pawn_AddEquipment
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn_EquipmentTracker __instance)
    {
        if (__instance.Primary != null)
        {
            __instance.Remove(__instance.Primary);
        }
    }

    private static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
    {
        if (__instance.pawn.Faction == Faction.OfPlayerSilentFail && WorldComp.HasStorages() &&
            WorldComp.CreateOrGetAssignedWeapons(__instance.pawn, out var aw))
        {
            aw.Add(newEq);
        }
    }
}