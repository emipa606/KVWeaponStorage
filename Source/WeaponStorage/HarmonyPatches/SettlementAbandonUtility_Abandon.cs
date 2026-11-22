using HarmonyLib;
using RimWorld.Planet;

namespace WeaponStorage;

[HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
internal static class SettlementAbandonUtility_Abandon
{
    [HarmonyPriority(800)]
    public static void Prefix(MapParent settlement)
    {
        WorldComp.Remove(settlement.Map);
    }
}