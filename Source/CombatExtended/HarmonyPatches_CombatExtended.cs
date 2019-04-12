﻿using CombatExtended;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using WeaponStorage;

namespace CombatExtendedWeaponStoragePatch
{
    [StaticConstructorOnStartup]
    class HarmonyPatches_CombatExtended
    {
        public static Assembly CombatExtendedAssembly { get; private set; }
        public static Type AmmoDef { get; private set; }

        static HarmonyPatches_CombatExtended()
        {
            CombatExtendedAssembly = null;
            bool wsFound = false, ceFound = false;
            CombatExtendedUtil.SetHasCombatExtended(false);
            var mods = new List<ModMetaData>(ModsConfig.ActiveModsInLoadOrder);
            for (int i = 0; i < mods.Count; ++i)
            {
                if (mods[i].Name.StartsWith("[KV] Weapon Storage"))
                {
                    wsFound = true;
                    if (ceFound)
                        break;
                }
                else if (mods[i].Name.Equals("Combat Extended"))
                {
                    ceFound = true;
                    if (wsFound)
                    {
                        Log.Error("Weapon Storage must be loaded after Combat Extended.");
                        return;
                    }
                }
            }

            if (ceFound)
            {
                try
                {
                    bool found = false;
                    foreach (ModContentPack pack in LoadedModManager.RunningMods)
                    {
                        foreach (Assembly assembly in pack.assemblies.loadedAssemblies)
                        {
                            if (assembly.GetName().Name.Equals("CombatExtended"))
                            {
                                AmmoDef = assembly.GetType("CombatExtended.AmmoDef");
                                if (AmmoDef == null)
                                    throw new Exception("Unable to find CombatExtended.AmmoDef");

                                /*compAmmoUser = assembly.GetType("CombatExtended.CompAmmoUser");
                                if (compAmmoUser == null)
                                    throw new Exception("Unable to find CombatExtended.CompAmmoUser");*/

                                CombatExtendedAssembly = assembly;
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }

                    var harmony = HarmonyInstance.Create("com.combatextendedweaponstoragepatch.rimworld.mod");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());

                    CombatExtendedUtil.SetHasCombatExtended(true);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to patch \"Combat Extended\"." + Environment.NewLine + e.Message);
                }
            }
            else
            {
                Log.Message("Weapon Storage \"Combat Extended\" Patch did not find \"Combat Extended\". Will not load patch.");
            }
        }
    }

    [HarmonyPatch(typeof(CompAmmoUser), "TryStartReload")]
    static class Patch_CompAmmoUser_TryStartReload
    {
        static void Prefix(CompAmmoUser __instance)
        {
            AmmoDef ammoDef = __instance.CurrentAmmo;//__instance.GetType().GetProperty("CurrentAmmo", BindingFlags.Instance | BindingFlags.Public).GetValue(__instance, null) as Def;
            if (ammoDef != null &&
                CombatExtendedUtil.HasAmmo(ammoDef))
            {
                if (!__instance.HasMagazine)
                {
                    if (__instance.turret != null)
                    {
                        // TODO
                    }
                    else if (__instance.Wielder != null)
                    {
                        // TODO
                    }
                }
                else
                {
                    if (!__instance.TryFindAmmoInInventory(out Thing ammo))
                    {
                        int magazineSize = __instance.Props.magazineSize;
                        if (CombatExtendedUtil.TryRemoveAmmo(ammoDef, magazineSize, out ammo))
                        {
                            __instance.CompInventory.ammoList.Add(ammo as ThingWithComps);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingOwner<Thing>))]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch("TryAdd", new Type[] { typeof(Thing), typeof(int), typeof(bool) })]
    static class Patch_ThingOwner_TryAdd
    {
        static bool Prefix(ThingOwner<Thing> __instance, ref int __result, Thing item, int count, bool canMergeWithExistingStacks)
        {
            var inv = __instance.Owner as Pawn_InventoryTracker;
            if (inv?.pawn.IsColonist == true &&
                item?.def is AmmoDef && 
                WorldComp.HasStorages())
            {
                Log.Warning("Ammo: " + item.Label);
                if (CombatExtendedUtil.AddAmmo(item.def, count))
                {
                    __result = count;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Command_Reload), "MakeAmmoMenu")]
    static class Patch_Command_Reload_MakeAmmoMenu
    {
        static void Postfix(Command_Reload __instance, ref FloatMenu __result)
        {
            if (__instance.compAmmo.turret == null)
            {
                List<FloatMenuOption> options = __result.GetType().GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__result) as List<FloatMenuOption>;

                List<AmmoDef> list = new List<AmmoDef>();
                foreach (AmmoLink curLink in __instance.compAmmo.Props.ammoSet.ammoTypes)
                {
                    if (CombatExtendedUtil.HasAmmo(curLink.ammo))
                    {
                        bool contains = false;
                        foreach (var o in options)
                        {
                            if (o.Label.Equals(curLink.ammo.ammoClass.LabelCap))
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains)
                        {
                            options.Insert(0,
                                new FloatMenuOption(
                                    curLink.ammo.ammoClass.LabelCap, delegate
                                    {
                                        if (__instance.compAmmo.SelectedAmmo != curLink.ammo ||
                                            __instance.compAmmo.CurMagCount < __instance.compAmmo.Props.magazineSize)
                                        {
                                            __instance.compAmmo.SelectedAmmo = curLink.ammo;

                                            Building_TurretGunCE turret = __instance.compAmmo.turret;
                                            if (turret == null || turret.MannableComp == null)
                                            {
                                                if (CombatExtendedUtil.TryRemoveAmmo(curLink.ammo, __instance.compAmmo.Props.magazineSize, out Thing ammo))
                                                {
                                                    __instance.compAmmo.TryUnload();
                                                    
                                                    __instance.compAmmo.CompInventory.UpdateInventory();
                                                    __instance.compAmmo.CompInventory.ammoList.Add(ammo as ThingWithComps);

                                                    if (turret != null)
                                                    {
                                                        __instance.compAmmo.turret.TryOrderReload();
                                                    }
                                                    else
                                                    {
                                                        __instance.compAmmo.TryStartReload();
                                                    }
                                                }
                                            }
                                        }
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                    }
                }
            }
        }
    }
}