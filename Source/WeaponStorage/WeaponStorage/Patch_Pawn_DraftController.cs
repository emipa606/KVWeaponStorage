using HarmonyLib;
using RimWorld;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
internal static class Patch_Pawn_DraftController
{
    private static void Postfix(Pawn_DraftController __instance)
    {
        var pawn = __instance.pawn;
        if (WorldComp.TryGetAssignedWeapons(pawn, out var aw) && aw.TryGetLastThingUsed(pawn, out var t))
        {
            HarmonyPatchUtil.EquipWeapon(t, pawn, aw);
        }
    }
}