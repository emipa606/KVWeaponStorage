using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
internal static class Patch_RecipeWorkerCounter_CountProducts
{
    private static void Postfix(ref int __result, RecipeWorkerCounter __instance, Bill_Production bill)
    {
        var products = __instance.recipe.products;
        if (!WorldComp.GetWeaponStorages(bill?.Map).Any() || products == null)
        {
            return;
        }

        foreach (var item in products)
        {
            var thingDef = item.thingDef;
            if (!thingDef.IsWeapon)
            {
                continue;
            }

            if (bill == null)
            {
                continue;
            }

            foreach (var weaponStorage in WorldComp.GetWeaponStorages(bill.Map))
            {
                __result += weaponStorage.GetWeaponCount(thingDef, bill.qualityRange, bill.hpRange,
                    bill.limitToAllowedStuff ? bill.ingredientFilter : null);
            }
        }
    }
}