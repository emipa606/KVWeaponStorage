using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeUndowned")]
internal static class Patch_Pawn_HealthTracker_MakeUndowned
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn_HealthTracker __instance)
    {
        if (!Settings.EnableAssignWeapons)
        {
            return;
        }

        var pawn = (Pawn)__instance.GetType().GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(__instance);
        if (pawn != null && pawn.Faction == Faction.OfPlayer && pawn.def.race.Humanlike &&
            WorldComp.TryGetAssignedWeapons(pawn, out var aw) && pawn.equipment?.Primary == null &&
            aw.TryGetLastThingUsed(pawn, out var t))
        {
            HarmonyPatchUtil.EquipWeapon(t, pawn, aw);
        }
    }
}