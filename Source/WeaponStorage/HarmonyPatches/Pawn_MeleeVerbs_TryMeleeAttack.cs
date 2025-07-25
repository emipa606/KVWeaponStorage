using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WeaponStorage;

[HarmonyPatch(typeof(Pawn_MeleeVerbs), nameof(Pawn_MeleeVerbs.TryMeleeAttack))]
internal static class Pawn_MeleeVerbs_TryMeleeAttack
{
    private static readonly Dictionary<Def, DT> weaponDamageTypes = new();

    [HarmonyPriority(800)]
    private static void Postfix(Pawn_MeleeVerbs __instance, Thing target)
    {
        if (!Settings.AutoSwitchMelee)
        {
            return;
        }

        var pawn = __instance.Pawn;
        if (pawn == null || !WorldComp.TryGetAssignedWeapons(pawn, out var aw))
        {
            return;
        }

        var verb = pawn.TryGetAttackVerb(target, !pawn.IsColonist);
        if (verb == null || !verb.verbProps.IsMeleeAttack)
        {
            return;
        }

        var statValue = target.GetStatValue(StatDefOf.ArmorRating_Blunt);
        var statValue2 = target.GetStatValue(StatDefOf.ArmorRating_Sharp);
        var dt = !(statValue > statValue2) ? DamageType.Blunt : DamageType.Sharp;
        if (statValue == statValue2)
        {
            if (Settings.PreferredDamageType == PreferredDamageTypeEnum.WeaponStorage_None)
            {
                return;
            }

            dt = Settings.PreferredDamageType == PreferredDamageTypeEnum.ArmorBlunt
                ? DamageType.Blunt
                : DamageType.Sharp;
        }

        if (TryGetBestWeapon(dt, pawn.equipment.Primary, aw, out var bestWeapon))
        {
            HarmonyPatchUtil.EquipWeapon(bestWeapon, pawn, aw);
        }
    }

    private static bool TryGetBestWeapon(DamageType dt, Thing equiped, AssignedWeaponContainer c,
        out ThingWithComps bestWeapon)
    {
        bestWeapon = null;
        var dT = equiped == null ? new DT(DamageType.Blunt, -1f) : GetWeaponDamage(equiped.def);
        if (dT.DamageType == dt && dT.Power != -1f)
        {
            return false;
        }

        dT.Power = -1f;
        foreach (var weapon in c.Weapons)
        {
            if (!weapon.def.IsMeleeWeapon)
            {
                continue;
            }

            var weaponDamage = GetWeaponDamage(weapon.def);
            if (weaponDamage.DamageType != dt || !(weaponDamage.Power > dT.Power))
            {
                continue;
            }

            dT.Power = weaponDamage.Power;
            bestWeapon = weapon;
        }

        return dT.Power > 0f;
    }

    private static DT GetWeaponDamage(ThingDef def)
    {
        if (weaponDamageTypes.TryGetValue(def, out var value))
        {
            return value;
        }

        value = new DT(DamageType.Blunt, -1f);
        foreach (var tool in def.tools)
        {
            if (!(tool.power > value.Power))
            {
                continue;
            }

            value.Power = tool.power;
            using var enumerator2 = tool.VerbsProperties.GetEnumerator();
            if (!enumerator2.MoveNext())
            {
                continue;
            }

            value.DamageType = enumerator2.Current?.meleeDamageDef == DamageDefOf.Blunt
                ? DamageType.Blunt
                : DamageType.Sharp;
        }

        weaponDamageTypes.Add(def, value);

        return value;
    }

    private enum DamageType
    {
        Sharp,
        Blunt
    }

    private struct DT(DamageType dt, float p)
    {
        public DamageType DamageType = dt;

        public float Power = p;
    }
}