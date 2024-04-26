using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.MakeRoomFor))]
internal static class Patch_Pawn_EquipmentTracker_MakeRoomFor
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
    {
        if (eq.def.equipmentType == EquipmentType.Primary && __instance.Primary != null &&
            WorldComp.TryGetAssignedWeapons(__instance.pawn, out var aw) && aw.Contains(eq))
        {
            __instance.Remove(eq);
        }
    }
}