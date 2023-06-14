using HarmonyLib;
using RimWorld.Planet;

namespace WeaponStorage;

[HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
internal static class Patch_SettlementAbandonUtility_Abandon
{
    [HarmonyPriority(800)]
    private static void Prefix(MapParent settlement)
    {
        WorldComp.Remove(settlement.Map);
    }
}