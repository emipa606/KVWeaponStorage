using HarmonyLib;
using RimWorld;

namespace WeaponStorage;

[HarmonyPatch(typeof(TradeDeal), "Reset")]
internal static class Patch_TradeDeal_Reset
{
    private static void Prefix()
    {
        TradeUtil.ReclaimWeapons();
    }
}