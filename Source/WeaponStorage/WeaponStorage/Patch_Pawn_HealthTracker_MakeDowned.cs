using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.MakeDowned))]
internal static class Patch_Pawn_HealthTracker_MakeDowned
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn_HealthTracker __instance, Pawn ___pawn)
    {
        if (Settings.EnableAssignWeapons && !__instance.Downed &&
            ___pawn.IsColonist && ___pawn.equipment?.Primary != null &&
            WorldComp.TryGetAssignedWeapons(___pawn, out var aw))
        {
            HarmonyPatchUtil.UnequipPrimaryWeapon(___pawn, aw);
        }
    }
}