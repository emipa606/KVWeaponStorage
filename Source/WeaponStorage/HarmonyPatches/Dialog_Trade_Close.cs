using HarmonyLib;
using RimWorld;

namespace WeaponStorage;

[HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.Close))]
internal static class Dialog_Trade_Close
{
    private static void Postfix()
    {
        TradeUtil.ReclaimWeapons();
    }
}