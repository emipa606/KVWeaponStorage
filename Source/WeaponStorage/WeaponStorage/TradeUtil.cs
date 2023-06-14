using System.Collections.Generic;
using Verse;

namespace WeaponStorage;

internal static class TradeUtil
{
    public static IEnumerable<Thing> EmptyWeaponStorages(Map map)
    {
        var list = new List<Thing>();
        foreach (var weaponStorage in WorldComp.GetWeaponStorages(map))
        {
            if (weaponStorage.Map != map || !weaponStorage.Spawned || !weaponStorage.IncludeInTradeDeals)
            {
                continue;
            }

            foreach (var weapon in weaponStorage.GetWeapons(true))
            {
                list.Add(weapon);
            }

            weaponStorage.Empty();
        }

        return list;
    }

    public static void ReclaimWeapons()
    {
        foreach (var weaponStorage in WorldComp.GetWeaponStorages(null))
        {
            if (weaponStorage.Map != null && weaponStorage.Spawned)
            {
                weaponStorage.ReclaimWeapons();
            }
        }
    }
}