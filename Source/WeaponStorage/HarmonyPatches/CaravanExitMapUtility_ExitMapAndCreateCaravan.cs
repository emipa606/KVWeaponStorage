using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan),
    typeof(IEnumerable<Pawn>), typeof(Faction),
    typeof(PlanetTile), typeof(PlanetTile), typeof(PlanetTile), typeof(bool))]
internal static class CaravanExitMapUtility_ExitMapAndCreateCaravan
{
    [HarmonyPriority(800)]
    private static void Prefix(IEnumerable<Pawn> pawns, Faction faction)
    {
        if (faction != Faction.OfPlayer || new List<Pawn>(pawns).Count <= 0)
        {
            return;
        }

        foreach (var weaponStorage in WorldComp.GetWeaponStorages(null))
        {
            weaponStorage.ReclaimWeapons();
        }
    }
}