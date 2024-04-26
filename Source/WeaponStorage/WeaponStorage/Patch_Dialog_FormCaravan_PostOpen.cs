using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Dialog_FormCaravan), nameof(Dialog_FormCaravan.PostOpen))]
internal static class Patch_Dialog_FormCaravan_PostOpen
{
    private static void Prefix(Window __instance, Map ___map)
    {
        if (!(__instance.GetType() == typeof(Dialog_FormCaravan)))
        {
            return;
        }

        foreach (var weaponStorage in WorldComp.GetWeaponStorages(___map))
        {
            weaponStorage.Empty();
        }
    }
}