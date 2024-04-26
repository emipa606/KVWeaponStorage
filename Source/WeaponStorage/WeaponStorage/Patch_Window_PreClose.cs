using HarmonyLib;
using RimWorld;

namespace WeaponStorage;

[HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.Close))]
internal static class Patch_Window_PreClose
{
    private static void Postfix()
    {
        TradeUtil.ReclaimWeapons();
    }
}