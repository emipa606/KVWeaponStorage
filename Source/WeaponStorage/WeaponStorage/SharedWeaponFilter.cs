using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponStorage;

public class SharedWeaponFilter : IExposable
{
    private readonly HashSet<ThingDef> foundDefCache = new HashSet<ThingDef>();
    public HashSet<ThingDef> AllowedDefs = new HashSet<ThingDef>();

    public HashSet<Pawn> AssignedPawns = new HashSet<Pawn>();

    public FloatRange HpRange = new FloatRange(0f, 1f);
    public string Label;

    private long lastCacheUpdate;

    public QualityRange QualityRange = QualityRange.All;

    public void ExposeData()
    {
        Scribe_Values.Look(ref Label, "label", "");
        Scribe_Collections.Look(ref AllowedDefs, "allowedDefs", LookMode.Def);
        Scribe_Values.Look(ref HpRange, "hpRange");
        Scribe_Values.Look(ref QualityRange, "qalityRange");
        Scribe_Collections.Look(ref AssignedPawns, false, "assignedPawns", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && AssignedPawns == null)
        {
            AssignedPawns = new HashSet<Pawn>();
        }
    }

    public bool Allows(ThingWithComps t)
    {
        if (!t.TryGetQuality(out var qc))
        {
            return t.HitPoints >= HpRange.min * t.def.BaseMaxHitPoints;
        }

        if ((int)qc >= (int)QualityRange.min && (int)qc <= (int)QualityRange.max)
        {
            return t.HitPoints >= HpRange.min * t.def.BaseMaxHitPoints;
        }

        return false;
    }

    public bool FoundDefCacheContains(ThingDef d)
    {
        return foundDefCache.Contains(d);
    }

    public void UpdateFoundDefCache()
    {
        var ticks = DateTime.Now.Ticks;
        if (ticks - lastCacheUpdate <= 10000000)
        {
            return;
        }

        lastCacheUpdate = ticks;
        foundDefCache.Clear();
        foreach (var allowedDef in AllowedDefs)
        {
            if (CombatExtendedUtil.GetAmmoCount(allowedDef) > 0)
            {
                foundDefCache.Add(allowedDef);
                continue;
            }

            foreach (var weaponStorage in WorldComp.GetWeaponStorages(null))
            {
                if (!weaponStorage.HasWeapon(this, allowedDef))
                {
                    continue;
                }

                foundDefCache.Add(allowedDef);
                break;
            }
        }
    }

    public void UpdateDefCache(ThingDef def)
    {
        if (AllowedDefs.Contains(def))
        {
            if (CombatExtendedUtil.GetAmmoCount(def) > 0)
            {
                foundDefCache.Add(def);
                return;
            }

            foreach (var weaponStorage in WorldComp.GetWeaponStorages(null))
            {
                if (!weaponStorage.HasWeapon(this, def))
                {
                    continue;
                }

                foundDefCache.Add(def);
                return;
            }
        }

        foundDefCache.Remove(def);
    }
}