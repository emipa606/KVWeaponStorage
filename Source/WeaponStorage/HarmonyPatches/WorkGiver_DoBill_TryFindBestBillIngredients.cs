using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.TryFindBestBillIngredients))]
public static class WorkGiver_DoBill_TryFindBestBillIngredients
{
    private static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
    {
        if (bill.Map == null)
        {
            Log.Error("Bill's map is null");
            return;
        }

        if (__result || !WorldComp.HasStorages(bill.Map) || bill.Map != pawn.Map)
        {
            return;
        }

        var dictionary = new Dictionary<ThingDef, int>();
        foreach (var item in chosen)
        {
            int value = !dictionary.TryGetValue(item.Thing.def, out value) ? item.Count : value + item.Count;
            dictionary[item.Thing.def] = value;
        }

        var linkedList = new LinkedList<HarmonyPatches.NeededIngrediants>();
        foreach (var ingredient in bill.recipe.ingredients)
        {
            var foundIngredient = false;
            foreach (var item2 in dictionary)
            {
                if ((int)ingredient.GetBaseCount() != item2.Value || !ingredient.filter.Allows(item2.Key))
                {
                    continue;
                }

                foundIngredient = true;
                break;
            }

            if (!foundIngredient)
            {
                linkedList.AddLast(
                    new HarmonyPatches.NeededIngrediants(ingredient.filter, (int)ingredient.GetBaseCount()));
            }
        }

        var list = new List<HarmonyPatches.WeaponsToUse>();
        foreach (var weaponStorage in WorldComp.GetWeaponStorages(bill.Map))
        {
            if (!((float)(weaponStorage.Position - billGiver.Position).LengthHorizontalSquared <
                  Math.Pow(bill.ingredientSearchRadius, 2.0)))
            {
                continue;
            }

            var linkedListNode = linkedList.First;
            while (linkedListNode != null)
            {
                var next = linkedListNode.Next;
                var value2 = linkedListNode.Value;
                if (weaponStorage.TryGetFilteredWeapons(bill, value2.Filter, out var gotten))
                {
                    foreach (var item3 in gotten)
                    {
                        value2.Add(new HarmonyPatches.StoredWeapons(weaponStorage, item3));
                    }

                    if (value2.CountReached())
                    {
                        list.Add(new HarmonyPatches.WeaponsToUse(value2.GetFoundThings(), value2.Count));
                        value2.Clear();
                        linkedList.Remove(linkedListNode);
                    }
                }

                linkedListNode = next;
            }
        }

        if (linkedList.Count == 0)
        {
            __result = true;
            foreach (var item4 in list)
            {
                var num = item4.Count;
                foreach (var weapon in item4.Weapons)
                {
                    if (num <= 0)
                    {
                        break;
                    }

                    if (!weapon.Storage.Remove(weapon.Weapon))
                    {
                        continue;
                    }

                    num -= weapon.Weapon.stackCount;
                    chosen.Add(new ThingCount(weapon.Weapon, weapon.Weapon.stackCount));
                }
            }
        }

        list.Clear();
        foreach (var item5 in linkedList)
        {
            item5.Clear();
        }

        linkedList.Clear();
        dictionary.Clear();
    }
}