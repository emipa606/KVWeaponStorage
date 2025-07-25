using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(WealthWatcher), nameof(WealthWatcher.ForceRecount))]
public static class WealthWatcher_ForceRecount
{
    private static void Postfix(WealthWatcher __instance)
    {
        var map = (Map)__instance.GetType().GetField("map", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(__instance);
        var field = __instance.GetType().GetField("wealthItems", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        var num = TallyWealth(wealthItems: (float)field.GetValue(__instance),
            storages: WorldComp.GetWeaponStorages(map));
        field.SetValue(__instance, num);
    }

    private static float TallyWealth(IEnumerable<Building_WeaponStorage> storages, float wealthItems)
    {
        foreach (var storage in storages)
        {
            foreach (var weapon in storage.GetWeapons(true))
            {
                wealthItems += weapon.stackCount + weapon.MarketValue;
            }
        }

        return wealthItems;
    }
}