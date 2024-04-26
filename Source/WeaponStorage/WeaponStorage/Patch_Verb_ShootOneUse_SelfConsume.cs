using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Verb_ShootOneUse), nameof(Verb_ShootOneUse.SelfConsume))]
internal static class Patch_Verb_ShootOneUse_SelfConsume
{
    private static void Prefix(Verb_ShootOneUse __instance)
    {
        if (__instance.caster is Pawn pawn && WorldComp.TryGetAssignedWeapons(pawn, out var aw))
        {
            aw.Remove(__instance.EquipmentSource);
        }
    }
}