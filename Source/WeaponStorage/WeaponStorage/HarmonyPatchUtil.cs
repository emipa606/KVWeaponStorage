using Verse;

namespace WeaponStorage;

public static class HarmonyPatchUtil
{
    public static void UnequipPrimaryWeapon(Pawn pawn, AssignedWeaponContainer c)
    {
        var thingWithComps = pawn?.equipment?.Primary;
        if (thingWithComps == null)
        {
            return;
        }

        pawn.equipment.Remove(thingWithComps);
        if (c != null && c.Contains(thingWithComps))
        {
            c.Add(thingWithComps);
        }
        else if (!WorldComp.Add(thingWithComps) &&
                 !BuildingUtil.DropSingleThing(thingWithComps, pawn.Position, pawn.Map))
        {
            Log.Warning(
                $"Failed to drop {pawn.Name.ToStringShort}'s primary weapon [{pawn.equipment.Primary.Label}].");
        }
    }

    public static void EquipWeapon(ThingWithComps weapon, Pawn pawn)
    {
        if (WorldComp.TryGetAssignedWeapons(pawn, out var aw))
        {
            EquipWeapon(weapon, pawn, aw);
        }
    }

    public static void EquipWeapon(ThingWithComps weapon, Pawn pawn, AssignedWeaponContainer c)
    {
        if (pawn.equipment?.Primary == weapon)
        {
            return;
        }

        UnequipPrimaryWeapon(pawn, c);
        pawn.equipment?.AddEquipment(weapon);
    }

    internal static bool EquipRanged(AssignedWeaponContainer c)
    {
        foreach (var weapon in c.Weapons)
        {
            if (!weapon.def.IsRangedWeapon)
            {
                continue;
            }

            EquipWeapon(weapon, c.Pawn, c);
            return true;
        }

        return false;
    }

    internal static bool EquipMelee(AssignedWeaponContainer c)
    {
        foreach (var weapon in c.Weapons)
        {
            if (!weapon.def.IsMeleeWeapon)
            {
                continue;
            }

            EquipWeapon(weapon, c.Pawn, c);
            return true;
        }

        return false;
    }
}