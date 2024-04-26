using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using WeaponStorage;

namespace MendingWeaponStoragePatch;

[HarmonyPriority(0)]
[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
internal static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
{
    private static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
    {
        if (__result || pawn == null || bill?.recipe == null || bill.Map != pawn.Map ||
            bill.recipe.defName.Contains("Weapon"))
        {
            return;
        }

        var weaponStorages = WorldComp.GetWeaponStorages(bill.Map);
        if (weaponStorages == null)
        {
            Log.Message("MendingWeaponStoragePatch failed to retrieve WeaponStorages");
            return;
        }

        foreach (var item in weaponStorages)
        {
            if (!((item.Position - billGiver.Position).LengthHorizontalSquared <
                  bill.ingredientSearchRadius * bill.ingredientSearchRadius))
            {
                continue;
            }

            foreach (var storedWeapon in item.StoredWeapons)
            {
                FindMatches(ref __result, bill, chosen, item, storedWeapon);
            }

            foreach (var storedBioEncodedWeapon in item.StoredBioEncodedWeapons)
            {
                FindMatches(ref __result, bill, chosen, item, storedBioEncodedWeapon);
            }
        }
    }

    private static void FindMatches(ref bool __result, Bill bill, List<ThingCount> chosen, Building_WeaponStorage ws,
        KeyValuePair<ThingDef, LinkedList<ThingWithComps>> kv)
    {
        if (!bill.ingredientFilter.Allows(kv.Key))
        {
            return;
        }

        foreach (var item in kv.Value)
        {
            if (!bill.ingredientFilter.Allows(item) || item.HitPoints == item.MaxHitPoints)
            {
                continue;
            }

            if (ws.Remove(item))
            {
                if (!item.Spawned)
                {
                    Log.Error($"Failed to spawn weapon-to-mend [{item.Label}] from weapon storage [{ws.Label}].");
                    __result = false;
                }
                else
                {
                    __result = true;
                    chosen.Add(new ThingCount(item, 1));
                }
            }
            else
            {
                __result = false;
            }

            break;
        }
    }
}