using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.AddHumanlikeOrders))]
internal static class Patch_FloatMenuMakerMap_AddHumanlikeOrders
{
    private static void Postfix(Pawn pawn, List<FloatMenuOption> opts)
    {
        if (Settings.AllowPawnsToDropWeapon || !pawn.Faction.IsPlayer || !pawn.RaceProps.Humanlike ||
            !WorldComp.CanAdd(pawn.equipment?.Primary))
        {
            return;
        }

        var taggedString = "Drop".Translate(pawn.equipment?.Primary.Label, pawn.equipment?.Primary);
        for (var i = 0; i < opts.Count; i++)
        {
            if (opts[i].Label != taggedString)
            {
                continue;
            }

            opts.RemoveAt(i);
            break;
        }
    }
}