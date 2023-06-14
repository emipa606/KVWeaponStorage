using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MendingWeaponStoragePatch;

[StaticConstructorOnStartup]
internal class HarmonyPatches_Mending
{
    static HarmonyPatches_Mending()
    {
        if (ModsConfig.ActiveModsInLoadOrder.Any(m => "MendAndRecycle".Equals(m.Name)))
        {
            try
            {
                new Harmony("com.mendingweaponstoragepatch.rimworld.mod").PatchAll(Assembly.GetExecutingAssembly());
                Log.Message(
                    $"MendingWeaponStoragePatch Harmony Patches:{Environment.NewLine}  Postfix:{Environment.NewLine}    WorkGiver_DoBill.TryFindBestBillIngredients - Priority Last");
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to patch Mending & Recycling.{Environment.NewLine}{ex.Message}");
                return;
            }
        }

        Log.Message("MendingWeaponStoragePatch did not find MendAndRecycle. Will not load patch.");
    }
}