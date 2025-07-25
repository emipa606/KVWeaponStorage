using HarmonyLib;
using RimWorld.Planet;
using Verse.AI.Group;

namespace WeaponStorage;

[HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.StopFormingCaravan))]
internal static class CaravanFormingUtility_StopFormingCaravan
{
    [HarmonyPriority(800)]
    private static void Postfix(Lord lord)
    {
        foreach (var weaponStorage in WorldComp.GetWeaponStorages(lord.Map))
        {
            weaponStorage.ReclaimWeapons();
        }
    }
}