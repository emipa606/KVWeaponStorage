using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using WeaponStorage.UI;

namespace WeaponStorage;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    static HarmonyPatches()
    {
        new Harmony("com.weaponstorage.rimworld.mod").PatchAll(Assembly.GetExecutingAssembly());
    }

    private struct StoredWeapons
    {
        public readonly Building_WeaponStorage Storage;

        public readonly ThingWithComps Weapon;

        public StoredWeapons(Building_WeaponStorage storage, ThingWithComps weapon)
        {
            Storage = storage;
            Weapon = weapon;
        }
    }

    private struct WeaponsToUse
    {
        public readonly List<StoredWeapons> Weapons;

        public readonly int Count;

        public WeaponsToUse(List<StoredWeapons> weapons, int count)
        {
            Weapons = weapons;
            Count = count;
        }
    }

    private class NeededIngrediants
    {
        public readonly int Count;
        public readonly ThingFilter Filter;

        public readonly Dictionary<Def, List<StoredWeapons>> FoundThings;

        public NeededIngrediants(ThingFilter filter, int count)
        {
            Filter = filter;
            Count = count;
            FoundThings = new Dictionary<Def, List<StoredWeapons>>();
        }

        public void Add(StoredWeapons things)
        {
            if (!FoundThings.TryGetValue(things.Weapon.def, out var value))
            {
                value = new List<StoredWeapons>();
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

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    private static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        private static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
        {
            if (bill.Map == null)
            {
                Log.Error("Bill's map is null");
            }
            else
            {
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

                var linkedList = new LinkedList<NeededIngrediants>();
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
                        linkedList.AddLast(new NeededIngrediants(ingredient.filter, (int)ingredient.GetBaseCount()));
                    }
                }

                var list = new List<WeaponsToUse>();
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
                                value2.Add(new StoredWeapons(weaponStorage, item3));
                            }

                            if (value2.CountReached())
                            {
                                list.Add(new WeaponsToUse(value2.GetFoundThings(), value2.Count));
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
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    private static class Patch_Pawn_DraftController_GetGizmos
    {
        [HarmonyPriority(600)]
        private static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            try
            {
                var pawn = __instance.pawn;
                if (pawn.Faction != Faction.OfPlayer)
                {
                    return;
                }

                List<Gizmo> list = null;
                if (Settings.ShowWeaponStorageButtonForPawns && WorldComp.HasStorages())
                {
                    list = new List<Gizmo>
                    {
                        new Command_Action
                        {
                            icon = AssignUI.weaponStorageTexture,
                            defaultLabel = "WeaponStorage.UseWeaponStorage".Translate(),
                            activateSound = SoundDef.Named("Click"),
                            action = delegate { Find.WindowStack.Add(new AssignUI(null, pawn)); }
                        }
                    };
                    list.AddRange(__result);
                }

                if (WorldComp.TryGetAssignedWeapons(__instance.pawn, out var weapons))
                {
                    if (list == null)
                    {
                        list = new List<Gizmo>();
                        list.AddRange(__result);
                    }

                    foreach (var weapon in weapons.Weapons)
                    {
                        var isTool = Settings.IsTool(weapon);
                        var showWeapon = false;
                        if (pawn.Drafted)
                        {
                            showWeapon = true;
                        }
                        else if (isTool || Settings.ShowWeaponsWhenNotDrafted)
                        {
                            showWeapon = true;
                        }

                        if (showWeapon)
                        {
                            showWeapon = pawn.equipment.Primary != weapon;
                        }

                        if (showWeapon)
                        {
                            list.Add(CreateEquipWeaponGizmo(weapon.def, delegate
                            {
                                HarmonyPatchUtil.EquipWeapon(weapon, pawn, weapons);
                                weapons.SetLastThingUsed(pawn, weapon, false);
                            }));
                        }
                    }
                }

                foreach (var f in WorldComp.SharedWeaponFilter)
                {
                    if (list == null)
                    {
                        list = new List<Gizmo>();
                        list.AddRange(__result);
                    }

                    f.UpdateFoundDefCache();
                    if (!f.AssignedPawns.Contains(pawn))
                    {
                        continue;
                    }

                    foreach (var d in f.AllowedDefs)
                    {
                        if (d == pawn.equipment.Primary?.def || !f.FoundDefCacheContains(d))
                        {
                            continue;
                        }

                        list.Add(CreateEquipWeaponGizmo(d, delegate
                        {
                            if (!WorldComp.TryRemoveWeapon(d, f, false, out var weapon2))
                            {
                                return;
                            }

                            HarmonyPatchUtil.EquipWeapon(weapon2, pawn);
                            f.UpdateDefCache(d);
                        }, "WeaponStorage.EquipShared"));
                    }
                }

                if (list != null)
                {
                    __result = list;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(
                    $"Exception while getting gizmos for pawn {__instance.pawn.Name.ToStringShort}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}",
                    (__instance.pawn.Name.ToStringFull + "WSGIZMO").GetHashCode());
            }
        }

        private static Command_Action CreateEquipWeaponGizmo(ThingDef def, Action equipWeaponAction,
            string label = "WeaponStorage.Equip")
        {
            var command_Action = new Command_Action();
            if (def.uiIcon != null)
            {
                command_Action.icon = def.uiIcon;
            }
            else if (def.graphicData.texPath != null)
            {
                command_Action.icon = ContentFinder<Texture2D>.Get(def.graphicData.texPath);
            }
            else
            {
                command_Action.icon = null;
            }

            var stringBuilder = new StringBuilder(label.Translate());
            stringBuilder.Append(" ");
            stringBuilder.Append(def.label);
            command_Action.defaultLabel = stringBuilder.ToString();
            command_Action.defaultDesc = "WeaponStorage.EquipDesc".Translate();
            command_Action.activateSound = SoundDef.Named("Click");
            command_Action.groupKey = (label + def).GetHashCode();
            command_Action.action = equipWeaponAction;
            return command_Action;
        }
    }

    [HarmonyPatch(typeof(WealthWatcher), "ForceRecount")]
    private static class Patch_WealthWatcher_ForceRecount
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
}