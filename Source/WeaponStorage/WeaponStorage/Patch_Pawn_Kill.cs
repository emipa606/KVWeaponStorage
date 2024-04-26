using HarmonyLib;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
internal static class Patch_Pawn_Kill
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn __instance, ref State __state)
    {
        __state = new State(__instance);
    }

    [HarmonyPriority(800)]
    private static void Postfix(Pawn __instance, ref State __state)
    {
        if (!__instance.Dead || !__instance.IsColonist)
        {
            return;
        }

        var apparel = __instance.apparel;
        if (apparel == null || apparel.LockedApparel?.Count != 0 || __state.Weapon == null)
        {
            return;
        }

        if (WorldComp.Add(__state.Weapon))
        {
            __instance.equipment?.Remove(__state.Weapon);
        }

        if (!WorldComp.TryGetAssignedWeapons(__instance, out var aw))
        {
            return;
        }

        WorldComp.RemoveAssignedWeapons(__instance);
        foreach (var weapon in aw.Weapons)
        {
            if (!WorldComp.Add(weapon))
            {
                BuildingUtil.DropSingleThing(weapon, __instance.Position, __state.Map);
            }
        }
    }

    private struct State
    {
        internal readonly Map Map;

        internal readonly ThingWithComps Weapon;

        internal State(Pawn pawn)
        {
            Map = pawn.Map;
            Weapon = pawn.equipment?.Primary;
        }
    }
}