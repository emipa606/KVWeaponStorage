using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using WeaponStorage.UI;

namespace WeaponStorage;

public class WorldComp : WorldComponent
{
    public static List<Building_WeaponStorage> WeaponStoragesToUse;

    private static readonly Dictionary<Pawn, AssignedWeaponContainer> AssignedWeapons;

    public static List<SharedWeaponFilter> SharedWeaponFilter;

    public static readonly Stack<ThingWithComps> WeaponsToDrop;

    private List<AssignedWeaponContainer> tmp;

    static WorldComp()
    {
        WeaponStoragesToUse = new List<Building_WeaponStorage>();
        AssignedWeapons = new Dictionary<Pawn, AssignedWeaponContainer>();
        SharedWeaponFilter = new List<SharedWeaponFilter>();
        WeaponsToDrop = new Stack<ThingWithComps>();
        WeaponStorageDef = null;
    }

    public WorldComp(World world)
        : base(world)
    {
        if (WeaponStorageDef == null)
        {
            ThingDef thingDef = null;
            var list = new List<ThingDef>();
            foreach (var allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.defName.Equals("WeaponStorage"))
                {
                    thingDef = allDef;
                    WeaponStorageDef = allDef;
                }
                else if (allDef.IsWeapon)
                {
                    list.Add(allDef);
                }
            }

            foreach (var item in list)
            {
                thingDef?.building.fixedStorageSettings.filter.SetAllow(item, true);
                var allow = !(item.defName.Equals("Beer") || item.IsStuff);

                thingDef?.building.defaultStorageSettings.filter.SetAllow(item, allow);
            }

            thingDef?.building.fixedStorageSettings.filter.RecalculateDisplayRootCategory();
            thingDef?.building.defaultStorageSettings.filter.RecalculateDisplayRootCategory();
            if (WeaponStorageDef == null)
            {
                Log.Error("Unabled to find WeaponStorageDef");
            }
        }

        foreach (var value in AssignedWeapons.Values)
        {
            value.Clear();
        }

        AssignedWeapons.Clear();
        SharedWeaponFilter.Clear();
        if (WeaponStoragesToUse == null)
        {
            WeaponStoragesToUse = new List<Building_WeaponStorage>();
        }

        WeaponStoragesToUse.Clear();
    }

    public static ThingDef WeaponStorageDef { get; private set; }

    public static IEnumerable<AssignedWeaponContainer> AssignedWeaponContainers
    {
        get
        {
            if (!Settings.EnableAssignWeapons)
            {
                return new List<AssignedWeaponContainer>(0);
            }

            return AssignedWeapons.Values;
        }
    }

    public static void Add(Building_WeaponStorage ws)
    {
        if (ws == null || ws.Map == null)
        {
            Log.Error("Cannot add WeaponStorage that is either null or has a null map.");
        }
        else if (!WeaponStoragesToUse.Contains(ws))
        {
            WeaponStoragesToUse.Add(ws);
        }
    }

    public static bool Add(ThingWithComps t)
    {
        if (t == null)
        {
            return false;
        }

        foreach (var item in WeaponStoragesToUse)
        {
            if (item.AddWeapon(t))
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanAdd(ThingWithComps t)
    {
        if (t == null)
        {
            return false;
        }

        foreach (var item in WeaponStoragesToUse)
        {
            if (item.CanAdd(t))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryRemoveWeapon(ThingDef def, SharedWeaponFilter filter, bool includeBioencoded,
        out ThingWithComps weapon)
    {
        if (def != null)
        {
            if (CombatExtendedUtil.TryRemoveAmmo(def, 1, out var ammo))
            {
                weapon = ammo as ThingWithComps;
                if (weapon != null)
                {
                    return true;
                }
            }

            foreach (var item in WeaponStoragesToUse)
            {
                if (item.TryRemoveWeapon(def, filter, includeBioencoded, out weapon))
                {
                    return true;
                }
            }
        }

        weapon = null;
        return false;
    }

    public static void Drop(ThingWithComps w)
    {
        foreach (var item in WeaponStoragesToUse)
        {
            if (BuildingUtil.DropThing(w, item, item.Map))
            {
                return;
            }
        }
    }

    public static List<Building_WeaponStorage> GetWeaponStorages()
    {
        var list = new List<Building_WeaponStorage>(WeaponStoragesToUse.Count);
        foreach (var item in WeaponStoragesToUse)
        {
            if (item.Spawned)
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static IEnumerable<Building_WeaponStorage> GetWeaponStorages(Map map)
    {
        if (WeaponStoragesToUse == null)
        {
            yield break;
        }

        foreach (var item in WeaponStoragesToUse)
        {
            if (map == null || item.Spawned && item.Map == map)
            {
                yield return item;
            }
        }
    }

    public static bool HasStorages()
    {
        foreach (var item in WeaponStoragesToUse)
        {
            if (item.Spawned)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasStorages(Map map)
    {
        foreach (var item in WeaponStoragesToUse)
        {
            if (item.Spawned && item.Map == map)
            {
                return true;
            }
        }

        return false;
    }

    public static void Remove(Building_WeaponStorage ws)
    {
        WeaponStoragesToUse.Remove(ws);
    }

    public static void Remove(Map map)
    {
        for (var num = WeaponStoragesToUse.Count - 1; num >= 0; num--)
        {
            if (WeaponStoragesToUse[num].Map == map)
            {
                WeaponStoragesToUse.RemoveAt(num);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            tmp = new List<AssignedWeaponContainer>(AssignedWeapons.Values);
        }

        Scribe_Collections.Look(ref tmp, "assignedWeapons", LookMode.Deep);
        Scribe_Collections.Look(ref SharedWeaponFilter, "sharedWeaponFilter", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            foreach (var item in tmp)
            {
                if (!Settings.EnableAssignWeapons)
                {
                    if (item.Weapons == null)
                    {
                        continue;
                    }

                    foreach (var weapon in item.Weapons)
                    {
                        if (!Add(weapon))
                        {
                            WeaponsToDrop.Push(weapon);
                        }
                    }
                }
                else if (item.Pawn == null || item.Pawn.Dead)
                {
                    Log.Warning($"Unable to load pawn [{item.Pawn}]. Re-storing assigned weapons");
                    if (item.Weapons == null)
                    {
                        continue;
                    }

                    foreach (var weapon2 in item.Weapons)
                    {
                        if (!Add(weapon2))
                        {
                            WeaponsToDrop.Push(weapon2);
                        }
                    }
                }
                else
                {
                    AssignedWeapons.Add(item.Pawn, item);
                }
            }
        }

        if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        if (tmp != null)
        {
            tmp.Clear();
            tmp = null;
        }

        if (SharedWeaponFilter == null)
        {
            SharedWeaponFilter = new List<SharedWeaponFilter>();
        }
    }

    public static void SortWeaponStoragesToUse()
    {
        WeaponStoragesToUse.Sort((l, r) => l.settings.Priority.CompareTo(r.settings.Priority));
    }

    public static bool CreateOrGetAssignedWeapons(Pawn pawn, out AssignedWeaponContainer aw)
    {
        if (!Settings.EnableAssignWeapons)
        {
            aw = null;
            return false;
        }

        if (AssignedWeapons.TryGetValue(pawn, out aw))
        {
            return true;
        }

        aw = new AssignedWeaponContainer();
        AssignedWeapons.Add(pawn, aw);

        return true;
    }

    public static void AddAssignedWeapons(Pawn pawn, AssignedWeaponContainer aw)
    {
        if (Settings.EnableAssignWeapons)
        {
            AssignedWeapons[pawn] = aw;
        }
    }

    public static bool TryGetAssignedWeapons(Pawn pawn, out AssignedWeaponContainer aw)
    {
        if (Settings.EnableAssignWeapons)
        {
            return AssignedWeapons.TryGetValue(pawn, out aw);
        }

        aw = null;
        return false;
    }

    public static void RemoveAssignedWeapons(Pawn pawn)
    {
        AssignedWeapons.Remove(pawn);
    }

    public static void InitializeAssignedWeapons()
    {
        if (!Settings.EnableAssignWeapons || AssignedWeapons.Count != 0)
        {
            return;
        }

        foreach (var pawn in Util.GetPawns(true))
        {
            var assignedWeaponContainer = new AssignedWeaponContainer
            {
                Pawn = pawn.Pawn
            };
            if (pawn.Pawn.equipment.Primary != null)
            {
                assignedWeaponContainer.Add(pawn.Pawn.equipment.Primary);
            }

            AssignedWeapons.Add(pawn.Pawn, assignedWeaponContainer);
        }
    }

    public static void ClearAll()
    {
        WeaponStoragesToUse.Clear();
        AssignedWeapons.Clear();
        SharedWeaponFilter.Clear();
        WeaponsToDrop.Clear();
    }
}