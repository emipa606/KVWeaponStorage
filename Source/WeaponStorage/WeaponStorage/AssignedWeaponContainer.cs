using System.Collections.Generic;
using Verse;

namespace WeaponStorage;

public class AssignedWeaponContainer : IExposable
{
    public readonly List<ThingWithComps> weapons = [];
    private ThingWithComps LastToolUsed;

    private ThingWithComps LastWeaponUsed;

    public Pawn Pawn;

    private List<AssignedWeapon> tmp;

    public HashSet<int> weaponIds = [];

    public IEnumerable<ThingWithComps> Weapons => weapons;

    public int Count => weaponIds.Count;

    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            tmp = new List<AssignedWeapon>(weapons.Count);
            var primary = Pawn.equipment.Primary;
            foreach (var weapon in Weapons)
            {
                var item = new AssignedWeapon
                {
                    IsEquipped = primary == weapon,
                    Weapon = weapon
                };
                tmp.Add(item);
            }
        }

        Scribe_References.Look(ref Pawn, "pawn");
        Scribe_Collections.Look(ref tmp, "weapons", LookMode.Deep);
        Scribe_References.Look(ref LastWeaponUsed, "lastWeaponUsed");
        Scribe_References.Look(ref LastToolUsed, "lastToolUsed");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            weaponIds ??= [];

            weapons.Clear();
            weaponIds.Clear();
            foreach (var item2 in tmp)
            {
                if (item2.Weapon == null)
                {
                    Log.Error($"failed to load weapon assigned to {Pawn.Name.ToStringShort}");
                    continue;
                }

                weapons.Add(item2.Weapon);
                weaponIds.Add(item2.Weapon.thingIDNumber);
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Pawn != null)
            {
                foreach (var weapon2 in weapons)
                {
                    var allVerbs = weapon2.GetComp<CompEquippable>()?.AllVerbs;
                    if (allVerbs == null)
                    {
                        continue;
                    }

                    foreach (var item3 in allVerbs)
                    {
                        item3.caster = Pawn;
                    }
                }
            }
        }

        if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.PostLoadInit || tmp == null)
        {
            return;
        }

        tmp.Clear();
        tmp = null;
    }

    public void Add(ThingWithComps weapon)
    {
        if (Contains(weapon))
        {
            return;
        }

        weapons.Add(weapon);
        weaponIds.Add(weapon.thingIDNumber);
    }

    public bool Contains(ThingWithComps weapon)
    {
        return weapon != null && weaponIds.Contains(weapon.thingIDNumber);
    }

    public void Clear()
    {
        weaponIds.Clear();
        weapons.Clear();
    }

    public bool TryGetLastThingUsed(Pawn pawn, out ThingWithComps t)
    {
        var lastThingUsed = false;
        t = pawn.Drafted ? LastWeaponUsed : LastToolUsed;

        if (t != null)
        {
            if (weaponIds.Contains(t.thingIDNumber))
            {
                lastThingUsed = true;
            }
            else
            {
                SetLastThingUsed(pawn, null, false);
            }
        }

        if (!lastThingUsed)
        {
            t = null;
        }

        return lastThingUsed;
    }

    public void SetLastThingUsed(Pawn pawn, ThingWithComps t, bool isForceMelee)
    {
        if (pawn.Drafted || !Settings.IsTool(t))
        {
            LastWeaponUsed = t;
        }

        if (!pawn.Drafted)
        {
            LastToolUsed = t;
        }
    }

    public bool Remove(ThingWithComps weapon)
    {
        if (LastToolUsed == weapon)
        {
            LastToolUsed = null;
        }

        if (LastWeaponUsed == weapon)
        {
            LastWeaponUsed = null;
        }

        weaponIds.Remove(weapon.thingIDNumber);
        return weapons.Remove(weapon);
    }

    private class AssignedWeapon : IExposable
    {
        public bool IsEquipped;

        public ThingWithComps Weapon;

        public void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref IsEquipped, "isEquipped");
                if (IsEquipped)
                {
                    Scribe_References.Look(ref Weapon, "weapon");
                }
                else
                {
                    Scribe_Deep.Look(ref Weapon, "weapon", null);
                }
            }
            catch
            {
                Weapon = null;
                IsEquipped = false;
            }
        }
    }
}