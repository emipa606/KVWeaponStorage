using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using WeaponStorage.UI;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.GetGizmos))]
public static class Pawn_DraftController_GetGizmos
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
                list =
                [
                    new Command_Action
                    {
                        icon = AssignUI.weaponStorageTexture,
                        defaultLabel = "WeaponStorage.UseWeaponStorage".Translate(),
                        activateSound = SoundDef.Named("Click"),
                        action = delegate { Find.WindowStack.Add(new AssignUI(null, pawn)); }
                    }
                ];
                list.AddRange(__result);
            }

            if (WorldComp.TryGetAssignedWeapons(__instance.pawn, out var weapons))
            {
                if (list == null)
                {
                    list = [];
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
                    list = [];
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