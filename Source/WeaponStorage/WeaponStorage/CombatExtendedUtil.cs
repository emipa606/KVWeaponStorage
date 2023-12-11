using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld.Planet;
using Verse;

namespace WeaponStorage;

public class CombatExtendedUtil : WorldComponent
{
    public static Dictionary<ThingDef, int> Ammo;

    private List<ThingDefCount> ac;

    static CombatExtendedUtil()
    {
        Ammo = new Dictionary<ThingDef, int>();
        HasCombatExtended = false;
        var hasCombatExtendedPatch = false;
        foreach (var item in ModsConfig.ActiveModsInLoadOrder)
        {
            if (item.Name.Equals("Combat Extended"))
            {
                HasCombatExtended = true;
            }

            if (item.Name.StartsWith("[KV] Weapon Storage Combat Extended Patch"))
            {
                hasCombatExtendedPatch = true;
            }
        }

        if (HasCombatExtended && !hasCombatExtendedPatch)
        {
            Log.Error("WeaponStorage.UseWSCEPatch".Translate());
        }
    }

    public CombatExtendedUtil(World world)
        : base(world)
    {
        Ammo?.Clear();
        if (Ammo == null)
        {
            Ammo = new Dictionary<ThingDef, int>();
        }
    }

    public static bool HasCombatExtended { get; private set; }

    public static void SetHasCombatExtended(bool enabled)
    {
        HasCombatExtended = enabled;
    }

    public static bool AddAmmo(ThingDef def, int count)
    {
        int value = !Ammo.TryGetValue(def, out value) ? count : value + count;
        Ammo[def] = value;
        return true;
    }

    public static bool AddAmmo(Thing ammo)
    {
        if (!IsAmmo(ammo))
        {
            return false;
        }

        if (ammo.Spawned)
        {
            ammo.DeSpawn();
        }

        int value = !Ammo.TryGetValue(ammo.def, out value) ? ammo.stackCount : value + ammo.stackCount;
        Ammo[ammo.def] = value;
        return true;
    }

    internal static void DropAmmo(ThingDef def, Building_WeaponStorage ws)
    {
        DropAllNoUpate(def, GetAmmoCount(def), ws);
        Ammo[def] = 0;
    }

    public static bool TryDropAmmo(ThingDef def, int count, IntVec3 position, Map map)
    {
        return TryDropAmmo(def, count, position, map, out _);
    }

    public static bool TryDropAmmo(ThingDef def, int count, IntVec3 position, Map map, out Thing droppedAmmo)
    {
        if (count > 0 && def != null && Ammo.TryGetValue(def, out var value) && value > 0)
        {
            if (value < count)
            {
                count = value;
            }

            value -= count;
            droppedAmmo = MakeAmmo(def, count);
            if (BuildingUtil.DropThing(droppedAmmo, position, map))
            {
                Ammo[def] = value;
                return true;
            }

            Log.Error("Failed to drop " + def.defName + " x" + count);
        }

        droppedAmmo = null;
        return false;
    }

    public static bool TryRemoveAmmo(ThingDef def, int count)
    {
        return TryRemoveAmmo(def, count, out _, false);
    }

    public static bool TryRemoveAmmo(ThingDef def, int count, out Thing ammo, bool spawn = true)
    {
        if (count > 0 && def != null && Ammo.TryGetValue(def, out var value) && value > 0)
        {
            if (value < count)
            {
                count = value;
            }

            value -= count;
            Ammo[def] = value;
            ammo = spawn ? MakeAmmo(def, count) : null;

            return true;
        }

        ammo = null;
        return false;
    }

    public static bool HasAmmo(ThingDef def)
    {
        return GetAmmoCount(def) > 0;
    }

    public static bool IsAmmo(Thing ammo)
    {
        if (HasCombatExtended)
        {
            return ammo?.GetType().GetProperty("AmmoDef", BindingFlags.Instance | BindingFlags.NonPublic) != null;
        }

        return false;
    }

    public static int GetAmmoCount(Thing ammo)
    {
        return GetAmmoCount(ammo.def);
    }

    public static int GetAmmoCount(ThingDef def)
    {
        if (HasCombatExtended && def != null && Ammo.TryGetValue(def, out var value))
        {
            return value;
        }

        return 0;
    }

    public static void EmptyAmmo(Building_WeaponStorage ws)
    {
        foreach (var item in Ammo)
        {
            DropAllNoUpate(item.Key, item.Value, ws);
        }

        Ammo.Clear();
    }

    private static void DropAllNoUpate(ThingDef def, int count, Building_WeaponStorage ws)
    {
        while (count > 0)
        {
            var num = Math.Max(def.stackLimit, 1);
            if (num > count)
            {
                num = count;
            }

            BuildingUtil.DropThing(MakeAmmo(def, num), ws, ws.Map);
            count -= num;
        }
    }

    private static Thing MakeAmmo(ThingDef def, int count)
    {
        var thing = ThingMaker.MakeThing(def);
        thing.stackCount = count;
        if (thing.stackCount == 0)
        {
            Log.Error(thing.Label + " has stack count of 0");
        }

        return thing;
    }

    internal static List<ThingDefCount> GetThingCounts()
    {
        var list = new List<ThingDefCount>(Ammo.Count);
        foreach (var item in Ammo)
        {
            list.Add(new ThingDefCount(item.Key, item.Value));
        }

        return list;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            ac = GetThingCounts();
        }
        else if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            Ammo.Clear();
            ac?.Clear();
            ac = [];
        }

        Scribe_Collections.Look(ref ac, "ammo", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Ammo.Clear();
            foreach (var item in ac)
            {
                if (item.Count > 0)
                {
                    Ammo.Add(item.ThingDef, item.Count);
                }
            }
        }

        if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        ac?.Clear();
        ac = null;
    }
}