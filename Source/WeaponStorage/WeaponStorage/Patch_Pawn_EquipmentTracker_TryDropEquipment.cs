using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
internal static class Patch_Pawn_EquipmentTracker_TryDropEquipment
{
    [HarmonyPriority(800)]
    private static void Prefix(ref Pawn __state, ThingWithComps eq)
    {
        if (!eq.def.IsWeapon || eq.holdingOwner?.Owner is not Pawn_EquipmentTracker pawn_EquipmentTracker)
        {
            return;
        }

        var pawn = pawn_EquipmentTracker.pawn;
        if (pawn is { Faction.IsPlayer: true })
        {
            __state = pawn_EquipmentTracker.pawn;
        }
    }

    [HarmonyPriority(800)]
    private static void Postfix(ref Pawn __state, ThingWithComps eq)
    {
        if (__state == null)
        {
            return;
        }

        if (WorldComp.TryGetAssignedWeapons(__state, out var aw) && aw.Contains(eq))
        {
            if (!Settings.AllowPawnsToDropWeapon)
            {
                if (WorldComp.Add(eq))
                {
                    return;
                }

                Log.Warning($"unable to find weapon storage that can hold {eq.ThingID} so it will be dropped.");
                WorldComp.Drop(eq);

                return;
            }

            if (aw.Remove(eq))
            {
                if (eq.def.IsRangedWeapon)
                {
                    if (!HarmonyPatchUtil.EquipRanged(aw))
                    {
                        HarmonyPatchUtil.EquipMelee(aw);
                    }
                }
                else if (!HarmonyPatchUtil.EquipMelee(aw))
                {
                    HarmonyPatchUtil.EquipRanged(aw);
                }
            }

            if (!Settings.PlaceDroppedWeaponsInStorage || WorldComp.Add(eq))
            {
                return;
            }

            Log.Warning($"unable to find weapon storage that can hold {eq.ThingID} so it will be dropped.");
            WorldComp.Drop(eq);

            return;
        }

        foreach (var item in WorldComp.SharedWeaponFilter)
        {
            if (!item.Allows(eq) || WorldComp.Add(eq))
            {
                continue;
            }

            Log.Warning($"unable to find weapon storage that can hold {eq.ThingID} so it will be dropped.");
            WorldComp.Drop(eq);
            break;
        }
    }
}