using System.Reflection;
using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
internal static class Patch_Pawn_HealthTracker_MakeDowned
{
    private static readonly FieldInfo pawnFI =
        typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPriority(800)]
    private static void Prefix(Pawn_HealthTracker __instance)
    {
        if (Settings.EnableAssignWeapons && pawnFI.GetValue(__instance) is Pawn pawn && !__instance.Downed &&
            pawn.IsColonist && pawn.equipment?.Primary != null && WorldComp.TryGetAssignedWeapons(pawn, out var aw))
        {
            HarmonyPatchUtil.UnequipPrimaryWeapon(pawn, aw);
        }
    }
}