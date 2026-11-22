using HarmonyLib;
using RimWorld;

namespace WeaponStorage;

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.Reset))]
internal static class TradeDeal_Reset
{
    public static void Prefix()
    {
        TradeUtil.ReclaimWeapons();
    }
}