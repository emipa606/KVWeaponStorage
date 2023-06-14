using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Dialog_FormCaravan), "PostOpen")]
internal static class Patch_Dialog_FormCaravan_PostOpen
{
    private static void Prefix(Window __instance)
    {
        if (!(__instance.GetType() == typeof(Dialog_FormCaravan)))
        {
            return;
        }

        foreach (var weaponStorage in WorldComp.GetWeaponStorages(__instance.GetType()
                     .GetField("map", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?.GetValue(__instance) as Map))
        {
            weaponStorage.Empty();
        }
    }
}