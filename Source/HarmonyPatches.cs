﻿using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WeaponStorage
{
    [StaticConstructorOnStartup]
    partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.weaponstorage.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            Log.Message(
                "WeaponStorage Harmony Patches:" + Environment.NewLine +
                "  Prefix:" + Environment.NewLine +
                "    Pawn_HealthTracker.MakeDowned - not blocking" + Environment.NewLine +
                "    Dialog_FormCaravan.PostOpen" + Environment.NewLine +
                "    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int)" + Environment.NewLine +
                "    CaravanExitMapUtility.ExitMapAndCreateCaravan(IEnumerable<Pawn>, Faction, int, int)" + Environment.NewLine +
                "  Postfix:" + Environment.NewLine +
                "    Pawn_DraftController.GetGizmos" + Environment.NewLine +
                "    Pawn_TraderTracker.ColonyThingsWillingToBuy" + Environment.NewLine +
                "    TradeShip.ColonyThingsWillingToBuy" + Environment.NewLine +
                "    Window.PreClose" + Environment.NewLine +
                "    ReservationManager.CanReserve" + Environment.NewLine +
                "    CaravanFormingUtility.StopFormingCaravan");
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    static class Patch_Pawn_DraftController_GetGizmos
    {
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.pawn.Drafted || Settings.ShowWeaponsWhenNotDrafted)
            {
                AssignedWeaponContainer weapons;
                if (WorldComp.TryGetAssignedWeapons(__instance.pawn.ThingID, out weapons))
                {
                    List<Gizmo> l;
                    if (__result != null)
                        l = new List<Gizmo>(__result);
                    else
                        l = new List<Gizmo>(weapons.Weapons.Count);

                    try
                    {
                        for (int i = 0; i < weapons.Weapons.Count; ++i)
                        {
                            ThingWithComps t = weapons.Weapons[i];

                            Command_Action a = new Command_Action();
                            if (t.def.uiIcon != null)
                            {
                                a.icon = t.def.uiIcon;
                            }
                            else if (t.def.graphicData.texPath != null)
                            {
                                a.icon = ContentFinder<UnityEngine.Texture2D>.Get(t.def.graphicData.texPath, true);
                            }
                            else
                            {
                                a.icon = null;
                            }
                            StringBuilder sb = new StringBuilder("WeaponStorage.Equip".Translate());
                            sb.Append(" ");
                            sb.Append(t.def.label);
                            a.defaultLabel = sb.ToString();
                            a.defaultDesc = "Equip this item.";
                            a.activateSound = SoundDef.Named("Click");
                            a.groupKey = t.def.GetHashCode();
                            a.action = delegate
                            {
                                ThingWithComps p = __instance.pawn.equipment.Primary;
                                if (p != null)
                                {
                                    /*if (!p.def.IsRangedWeapon && !p.def.IsMeleeWeapon)
                                    {
                                        ThingWithComps temp;
                                        __instance.pawn.equipment.TryDropEquipment(p, out temp, __instance.pawn.Position, true);
                                    }
                                    else
                                    {*/
                                    __instance.pawn.equipment.Remove(p);
                                    //}
                                }

                                bool added = false;
                                for (int j = 0; j < weapons.Weapons.Count; ++j)
                                {
                                    if (weapons.Weapons[j].thingIDNumber == t.thingIDNumber)
                                    {
                                        if (p == null)
                                        {
                                            weapons.Weapons.RemoveAt(j);
                                        }
                                        else
                                        {
                                            weapons.Weapons[j] = p;
                                        }
                                            added = true;
                                        break;

                                    }
                                }

                                if (!added)
                                {
                                    weapons.Weapons.Add(p);
                                }

                                __instance.pawn.equipment.AddEquipment(t);
                            };
                            l.Add(a);
                        }
                    }
                    catch
                    {

                    }

                    __result = l;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    static class Patch_Pawn_HealthTracker_MakeDowned
    {
        static void Prefix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = (Pawn)__instance.GetType().GetField(
                "pawn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (pawn != null &&
                !__instance.Downed &&
                pawn.Faction == Faction.OfPlayer && 
                pawn.def.race.Humanlike)
            {
                ThingWithComps primary = pawn.equipment.Primary;
                if (primary != null &&
                    (primary.def.IsMeleeWeapon || primary.def.IsRangedWeapon))
                {
                    AssignedWeaponContainer c;
                    if (WorldComp.TryGetAssignedWeapons(pawn.ThingID, out c))
                    {
                        c.Weapons.Add(primary);
                        c.WeaponUsedBeforeDowned = primary;
                        pawn.equipment.Remove(primary);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeUndowned")]
    static class Patch_Pawn_HealthTracker_MakeUndowned
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = (Pawn)__instance.GetType().GetField(
                "pawn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (pawn != null &&
                pawn.Faction == Faction.OfPlayer &&
                pawn.def.race.Humanlike)
            {
                AssignedWeaponContainer c;
                if (WorldComp.TryGetAssignedWeapons(pawn.ThingID, out c) && 
                    c.WeaponUsedBeforeDowned != null)
                {
                    if (c.Weapons.Remove(c.WeaponUsedBeforeDowned))
                    {
                        pawn.equipment.AddEquipment(c.WeaponUsedBeforeDowned);
                        c.WeaponUsedBeforeDowned = null;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ReservationManager), "CanReserve")]
    static class Patch_ReservationManager_CanReserve
    {
        private static FieldInfo mapFI = null;
        static void Postfix(ref bool __result, ReservationManager __instance, Pawn claimant, LocalTargetInfo target, int maxPawns, int stackCount, ReservationLayerDef layer, bool ignoreOtherReservations)
        {
            if (mapFI == null)
            {
                mapFI = typeof(ReservationManager).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
            }

#if DEBUG_RESERVE
            Log.Warning("\nCanReserve original result: " + __result);
#endif
            if (!__result && (target.Thing == null || target.GetType() == typeof(Building_WeaponStorage)))
            {
                IEnumerable<Thing> things = ((Map)mapFI.GetValue(__instance))?.thingGrid.ThingsAt(target.Cell);
                if (things != null)
                {
#if DEBUG_RESERVE
                    Log.Warning("CanReserve - Found things");
#endif
                    foreach (Thing t in things)
                    {
#if DEBUG_RESERVE
                        Log.Warning("CanReserve - def " + t.def.defName);
#endif
                        if (t.GetType() == typeof(Building_WeaponStorage))
                        {
#if DEBUG_RESERVE
                            Log.Warning("CanReserve is now true\n");
#endif
                            __result = true;
                        }
                    }
                }
            }
        }
    }

    static class TradeUtil
    {
        public static IEnumerable<Thing> EmptyWeaponStorages(Map map)
        {
            List<Thing> l = new List<Thing>();
            foreach (Building_WeaponStorage ws in WorldComp.WeaponStoragesToUse)
            {
                if (ws.Map == map && ws.Spawned && ws.IncludeInTradeDeals)
                {
                    foreach (ThingWithComps t in ws.StoredWeapons)
                    {
                        l.Add(t);
                    }
                    ws.Empty();
                }
            }
            return l;
        }

        public static void ReclaimWeapons()
        {
            foreach (Building_WeaponStorage ws in WorldComp.WeaponStoragesToUse)
            {
                if (ws.Map != null && ws.Spawned)
                {
                    ws.ReclaimWeapons();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
    static class Patch_TradeShip_ColonyThingsWillingToBuy
    {
        // Before a caravan trade
        static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            if (playerNegotiator != null && playerNegotiator.Map != null)
            {
                List<Thing> result = new List<Thing>(__result);
                result.AddRange(TradeUtil.EmptyWeaponStorages(playerNegotiator.Map));
                __result = result;
            }
        }
    }

    [HarmonyPatch(typeof(TradeShip), "ColonyThingsWillingToBuy")]
    static class Patch_PassingShip_TryOpenComms
    {
        // Before an orbital trade
        static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            if (playerNegotiator != null && playerNegotiator.Map != null)
            {
                List<Thing> result = new List<Thing>(__result);
                result.AddRange(TradeUtil.EmptyWeaponStorages(playerNegotiator.Map));
                __result = result;
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), "Reset")]
    static class Patch_TradeDeal_Reset
    {
        // On Reset from Trade Dialog
        static void Prefix()
        {
            TradeUtil.ReclaimWeapons();
        }
    }

    [HarmonyPatch(typeof(Dialog_Trade), "Close")]
    static class Patch_Window_PreClose
    {
        static void Postfix(bool doCloseSound)
        {
            TradeUtil.ReclaimWeapons();
        }
    }

    #region Caravan Forming
    [HarmonyPatch(typeof(Dialog_FormCaravan), "PostOpen")]
    static class Patch_Dialog_FormCaravan_PostOpen
    {
        static void Prefix(Window __instance)
        {
            Type type = __instance.GetType();
            if (type == typeof(Dialog_FormCaravan))
            {
                Map map = __instance.GetType().GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Map;

                foreach (Building_WeaponStorage storage in WorldComp.GetWeaponStorages(map))
                {
                    storage.Empty();
                }
            }
        }
    }

    [HarmonyPatch(typeof(CaravanFormingUtility), "StopFormingCaravan")]
    static class Patch_CaravanFormingUtility_StopFormingCaravan
    {
        [HarmonyPriority(Priority.First)]
        static void Postfix(Lord lord)
        {
            foreach (Building_WeaponStorage storage in WorldComp.GetWeaponStorages(lord.Map))
            {
                storage.ReclaimWeapons();
            }
        }
    }

    [HarmonyPatch(
        typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan",
        new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int) })]
    static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan_1
    {
        [HarmonyPriority(Priority.First)]
        static void Prefix(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile)
        {
            if (faction == Faction.OfPlayer)
            {
                List<Pawn> p = new List<Pawn>(pawns);
                if (p.Count > 0)
                {
                    foreach (Building_WeaponStorage storage in WorldComp.GetWeaponStorages(p[0].Map))
                    {
                        storage.ReclaimWeapons();
                    }
                }
            }
        }
    }

    [HarmonyPatch(
        typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan",
        new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int) })]
    static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan_2
    {
        static void Prefix(IEnumerable<Pawn> pawns, Faction faction, int startingTile)
        {
            if (faction == Faction.OfPlayer)
            {
                List<Pawn> p = new List<Pawn>(pawns);
                if (p.Count > 0)
                {
                    foreach (Building_WeaponStorage storage in WorldComp.GetWeaponStorages(p[0].Map))
                    {
                        storage.ReclaimWeapons();
                    }
                }
            }
        }
    }
    #endregion

    #region Handle "Do until X" for stored weapons
    [HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
    static class Patch_RecipeWorkerCounter_CountProducts
    {
        static void Postfix(ref int __result, RecipeWorkerCounter __instance, Bill_Production bill)
        {
            List<ThingCountClass> products = __instance.recipe.products;
            if (WorldComp.WeaponStoragesToUse.Count > 0 && products != null)
            {
                foreach(ThingCountClass product in products)
                {
                    ThingDef def = product.thingDef;
                    if (def.IsWeapon)
                    {
                        foreach (Building_WeaponStorage ws in WorldComp.WeaponStoragesToUse)
                        {
                            if (bill.Map == ws.Map)
                            {
                                __result += ws.GetWeaponCount(def);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}