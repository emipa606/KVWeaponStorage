using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace WeaponStorage;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    static HarmonyPatches()
    {
        new Harmony("com.weaponstorage.rimworld.mod").PatchAll(Assembly.GetExecutingAssembly());
    }

    public struct StoredWeapons(Building_WeaponStorage storage, ThingWithComps weapon)
    {
        public readonly Building_WeaponStorage Storage = storage;

        public readonly ThingWithComps Weapon = weapon;
    }

    public struct WeaponsToUse(List<StoredWeapons> weapons, int count)
    {
        public readonly List<StoredWeapons> Weapons = weapons;

        public readonly int Count = count;
    }

    public class NeededIngrediants(ThingFilter filter, int count)
    {
        public readonly int Count = count;
        public readonly ThingFilter Filter = filter;

        public readonly Dictionary<Def, List<StoredWeapons>> FoundThings = new();

        public void Add(StoredWeapons things)
        {
            if (!FoundThings.TryGetValue(things.Weapon.def, out var value))
            {
                value = [];
                FoundThings.Add(things.Weapon.def, value);
            }

            value.Add(things);
        }

        public void Clear()
        {
            FoundThings.Clear();
        }

        public bool CountReached()
        {
            foreach (var value in FoundThings.Values)
            {
                if (CountReached(value))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CountReached(List<StoredWeapons> l)
        {
            var num = Count;
            foreach (var item in l)
            {
                num -= item.Weapon.stackCount;
            }

            return num <= 0;
        }

        public List<StoredWeapons> GetFoundThings()
        {
            foreach (var value in FoundThings.Values)
            {
                if (CountReached(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}