using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.MakeUndowned))]
internal static class Pawn_HealthTracker_MakeUndowned
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn ___pawn)
    {
        if (!Settings.EnableAssignWeapons)
        {
            return;
        }

        if (___pawn != null && ___pawn.Faction == Faction.OfPlayer && ___pawn.def.race.Humanlike &&
            WorldComp.TryGetAssignedWeapons(___pawn, out var aw) && ___pawn.equipment?.Primary == null &&
            aw.TryGetLastThingUsed(___pawn, out var t))
        {
            HarmonyPatchUtil.EquipWeapon(t, ___pawn, aw);
        }
    }
}