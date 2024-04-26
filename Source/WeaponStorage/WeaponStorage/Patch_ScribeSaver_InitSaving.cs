using System;
using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
internal static class Patch_ScribeSaver_InitSaving
{
    private static void Prefix()
    {
        try
        {
            foreach (var weaponStorage in WorldComp.GetWeaponStorages(null))
            {
                try
                {
                    weaponStorage.ReclaimWeapons(true);
                }
                catch (Exception ex)
                {
                    Log.Warning($"Error while reclaiming weapon for storage\n{ex.Message}");
                }
            }
        }
        catch (Exception ex2)
        {
            Log.Warning($"Error while reclaiming weapons\n{ex2.Message}");
        }
    }
}