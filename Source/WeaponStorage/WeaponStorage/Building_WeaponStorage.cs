using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using WeaponStorage.UI;

namespace WeaponStorage;

public class Building_WeaponStorage : Building_Storage
{
    private readonly FieldInfo AllowedDefsFI =
        typeof(ThingFilter).GetField("allowedDefs", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly ThingFilter previousStorageFilters = new ThingFilter();

    public readonly Dictionary<ThingDef, LinkedList<ThingWithComps>> StoredBioEncodedWeapons =
        new Dictionary<ThingDef, LinkedList<ThingWithComps>>();

    public readonly Dictionary<ThingDef, LinkedList<ThingWithComps>> StoredWeapons =
        new Dictionary<ThingDef, LinkedList<ThingWithComps>>();

    public bool AllowAdds;

    private List<Thing> forceAddedWeapons;

    public bool IncludeInSharedWeapons = true;

    private bool includeInTradeDeals = true;

    public string Name = "";

    public List<ThingWithComps> temp;

    public Building_WeaponStorage()
    {
        AllowAdds = true;
    }

    private Map CurrentMap { get; set; }

    public bool IncludeInTradeDeals => includeInTradeDeals;

    public override string Label => Name != "" ? Name : base.Label;

    public int Count => StoredWeapons.Count + StoredBioEncodedWeapons.Count;

    public bool HasWeapon(SharedWeaponFilter filter, ThingDef thingDef)
    {
        if (!StoredWeapons.TryGetValue(thingDef, out var value))
        {
            return false;
        }

        if (value == null)
        {
            StoredWeapons.Remove(thingDef);
        }
        else
        {
            foreach (var item in value)
            {
                if (filter.Allows(item))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        CurrentMap = map;
        if (settings == null)
        {
            settings = new StorageSettings(this);
            settings.CopyFrom(def.building.defaultStorageSettings);
            settings.filter.SetDisallowAll();
        }

        UpdatePreviousStorageFilter();
        WorldComp.Add(this);
        foreach (var item in BuildingUtil.FindThingsOfTypeNextTo<Building_RepairWeaponStorage>(Map, Position,
                     Settings.RepairAttachmentDistance))
        {
            item.Add(this);
        }

        WorldComp.InitializeAssignedWeapons();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        try
        {
            Dispose();
            base.Destroy(mode);
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.Destroy\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        try
        {
            Dispose();
            base.DeSpawn(mode);
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.DeSpawn\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanAdd(ThingWithComps t)
    {
        return t != null && settings.AllowedToAccept(t);
    }

    private void Dispose()
    {
        try
        {
            if (StoredWeapons != null)
            {
                foreach (var value in StoredWeapons.Values)
                {
                    foreach (var item in value)
                    {
                        DropThing(item);
                    }
                }

                StoredWeapons.Clear();
            }

            if (StoredBioEncodedWeapons != null)
            {
                foreach (var value2 in StoredBioEncodedWeapons.Values)
                {
                    foreach (var item2 in value2)
                    {
                        DropThing(item2);
                    }
                }

                StoredBioEncodedWeapons.Clear();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.Dispose\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }

        WorldComp.Remove(this);
        foreach (var item3 in BuildingUtil.FindThingsOfTypeNextTo<Building_RepairWeaponStorage>(Map, Position,
                     Settings.RepairAttachmentDistance))
        {
            item3?.Remove(this);
        }
    }

    public bool TryRemoveWeapon(ThingDef thingDef, SharedWeaponFilter filter, bool includeBioencoded,
        out ThingWithComps weapon)
    {
        Log.Message($"Trying to remove {thingDef}");
        if (StoredWeapons.TryGetValue(thingDef, out var value))
        {
            for (var linkedListNode = value.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
            {
                if (!filter.Allows(linkedListNode.Value))
                {
                    continue;
                }

                weapon = linkedListNode.Value;
                value.Remove(linkedListNode);
                return true;
            }
        }

        if (includeBioencoded && StoredBioEncodedWeapons.TryGetValue(thingDef, out value))
        {
            for (var linkedListNode2 = value.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
            {
                if (!filter.Allows(linkedListNode2.Value))
                {
                    continue;
                }

                weapon = linkedListNode2.Value;
                value.Remove(linkedListNode2);
                return true;
            }
        }

        weapon = null;
        return false;
    }

    private bool DropThing(Thing t)
    {
        return BuildingUtil.DropThing(t, this, CurrentMap);
    }

    private void DropWeapons<T>(IEnumerable<T> things) where T : Thing
    {
        try
        {
            if (things == null)
            {
                return;
            }

            foreach (var thing in things)
            {
                DropThing(thing);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"ChangeDresser:Building_Dresser.DropApparel\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void Empty()
    {
        try
        {
            AllowAdds = false;
            foreach (var value in StoredWeapons.Values)
            {
                DropWeapons(value);
            }

            foreach (var value2 in StoredBioEncodedWeapons.Values)
            {
                DropWeapons(value2);
            }

            CombatExtendedUtil.EmptyAmmo(this);
            StoredWeapons.Clear();
            StoredBioEncodedWeapons.Clear();
        }
        finally
        {
            AllowAdds = true;
        }
    }

    public override void Notify_ReceivedThing(Thing newItem)
    {
        if (!AllowAdds)
        {
            if (!newItem.Spawned)
            {
                DropThing(newItem);
            }

            return;
        }

        if ((newItem is not ThingWithComps withComps || !withComps.def.IsWeapon) &&
            !CombatExtendedUtil.IsAmmo(newItem))
        {
            if (!newItem.Spawned)
            {
                DropThing(newItem);
            }

            return;
        }

        base.Notify_ReceivedThing(newItem);
        if (CombatExtendedUtil.AddAmmo(newItem) || newItem is not ThingWithComps item || Contains(item))
        {
            return;
        }

        if (item.Spawned)
        {
            item.DeSpawn();
        }

        if (!AddWeapon(item) && !WorldComp.Add(item))
        {
            BuildingUtil.DropThing(item, this, CurrentMap, true);
        }
    }

    private bool Contains(ThingWithComps t)
    {
        return Contains(t, CompBiocodable.IsBiocoded(t) ? StoredBioEncodedWeapons : StoredWeapons);
    }

    private bool Contains(ThingWithComps t, Dictionary<ThingDef, LinkedList<ThingWithComps>> storage)
    {
        if (t != null && storage.TryGetValue(t.def, out var value))
        {
            return value.Contains(t);
        }

        return false;
    }

    internal bool AddWeapon(ThingWithComps weapon)
    {
        if (!CanAdd(weapon))
        {
            return false;
        }

        if (weapon.Spawned)
        {
            weapon.DeSpawn();
        }

        if (CombatExtendedUtil.AddAmmo(weapon))
        {
            return true;
        }

        AddToSortedList(weapon, CompBiocodable.IsBiocoded(weapon) ? StoredBioEncodedWeapons : StoredWeapons);
        return true;
    }

    private void AddToSortedList(ThingWithComps weapon, Dictionary<ThingDef, LinkedList<ThingWithComps>> storage)
    {
        _ = weapon.def.label;
        if (!storage.TryGetValue(weapon.def, out var value))
        {
            value = [];
            value.AddFirst(weapon);
            storage[weapon.def] = value;
            return;
        }

        if (weapon.TryGetQuality(out var qc))
        {
            for (var linkedListNode = value.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
            {
                if (!linkedListNode.Value.TryGetQuality(out var qc2) || (int)qc <= (int)qc2 &&
                    (qc != qc2 || weapon.HitPoints <
                        linkedListNode.Value.HitPoints))
                {
                    continue;
                }

                value.AddBefore(linkedListNode, weapon);
                return;
            }
        }

        value.AddLast(weapon);
    }

    internal int GetWeaponCount(ThingDef expectedDef, QualityRange qualityRange, FloatRange hpRange,
        ThingFilter ingredientFilter)
    {
        var num = 0;
        foreach (var value in StoredWeapons.Values)
        {
            foreach (var item in (IEnumerable<ThingWithComps>)value)
            {
                if (Allows(item, expectedDef, qualityRange, hpRange, ingredientFilter))
                {
                    num++;
                }
            }
        }

        foreach (var value2 in StoredBioEncodedWeapons.Values)
        {
            foreach (var item2 in (IEnumerable<ThingWithComps>)value2)
            {
                if (Allows(item2, expectedDef, qualityRange, hpRange, ingredientFilter))
                {
                    num++;
                }
            }
        }

        return num;
    }

    private bool Allows(Thing t, ThingDef expectedDef, QualityRange qualityRange, FloatRange hpRange,
        ThingFilter filter)
    {
        if (t.def != expectedDef)
        {
            return false;
        }

        if (expectedDef.useHitPoints && hpRange.min != 0f && hpRange.max != 100f)
        {
            var f = t.HitPoints / (float)t.MaxHitPoints;
            f = GenMath.RoundedHundredth(f);
            if (!hpRange.IncludesEpsilon(Mathf.Clamp01(f)))
            {
                return false;
            }
        }

        if (qualityRange == QualityRange.All || !t.def.FollowQualityThingFilter())
        {
            return filter == null || filter.Allows(t.Stuff);
        }

        if (!t.TryGetQuality(out var qc))
        {
            qc = QualityCategory.Normal;
        }

        if (!qualityRange.Includes(qc))
        {
            return false;
        }

        return filter == null || filter.Allows(t.Stuff);
    }

    internal bool TryGetFilteredWeapons(Bill bill, ThingFilter filter, out List<ThingWithComps> gotten)
    {
        var list = new List<ThingWithComps>();
        foreach (var storedWeapon in StoredWeapons)
        {
            GetFilteredWeaponsFromStorage(bill, filter, list, storedWeapon);
        }

        foreach (var storedBioEncodedWeapon in StoredBioEncodedWeapons)
        {
            GetFilteredWeaponsFromStorage(bill, filter, list, storedBioEncodedWeapon);
        }

        gotten = list.Count > 0 ? list : null;

        return gotten != null;
    }

    private void GetFilteredWeaponsFromStorage(Bill bill, ThingFilter filter, List<ThingWithComps> gotten,
        KeyValuePair<ThingDef, LinkedList<ThingWithComps>> kv)
    {
        if (!filter.Allows(kv.Key))
        {
            return;
        }

        foreach (var item in kv.Value)
        {
            if (bill.IsFixedOrAllowedIngredient(item) && filter.Allows(item))
            {
                gotten.Add(item);
            }
        }
    }

    internal void ReclaimWeapons(bool force = false)
    {
        if (Map == null)
        {
            return;
        }

        var list = BuildingUtil.FindThingsOfTypeNextTo<ThingWithComps>(Map, Position, 1);
        if (list.Count <= 0)
        {
            return;
        }

        foreach (var item in list)
        {
            if (AddWeapon(item) || !force || !item.Spawned ||
                !item.def.IsWeapon && !CombatExtendedUtil.IsAmmo(item) ||
                item.def.IsStuff)
            {
                continue;
            }

            item.DeSpawn();
            if (forceAddedWeapons == null)
            {
                forceAddedWeapons = new List<Thing>(list.Count);
            }

            forceAddedWeapons.Add(item);
        }

        list.Clear();
    }

    public void HandleThingsOnTop()
    {
        if (!Spawned)
        {
            return;
        }

        foreach (var item in Map.thingGrid.ThingsAt(Position))
        {
            if (item == null || item == this || item is Blueprint || item is Building ||
                item is ThingWithComps withComps && AddWeapon(withComps) || !item.Spawned)
            {
                continue;
            }

            var position = item.Position;
            position.x++;
            item.Position = position;
            Log.Warning($"Moving {item.Label}");
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            temp = [];
            foreach (var value in StoredWeapons.Values)
            {
                temp.AddRange(value);
            }

            foreach (var value2 in StoredBioEncodedWeapons.Values)
            {
                temp.AddRange(value2);
            }

            if (forceAddedWeapons == null)
            {
                forceAddedWeapons = [];
            }
        }

        Scribe_Collections.Look(ref temp, false, "storedWeapons", LookMode.Deep);
        Scribe_Values.Look(ref includeInTradeDeals, "includeInTradeDeals", true);
        Scribe_Values.Look(ref IncludeInSharedWeapons, "includeInSharedWeapons", true);
        Scribe_Collections.Look(ref forceAddedWeapons, false, "forceAddedWeapons", LookMode.Deep);
        Scribe_Values.Look(ref Name, "name", "");
        if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
        {
            StoredWeapons.Clear();
            StoredBioEncodedWeapons.Clear();
            if (temp != null)
            {
                foreach (var item in temp)
                {
                    AddToSortedList(item, CompBiocodable.IsBiocoded(item) ? StoredBioEncodedWeapons : StoredWeapons);
                }
            }
        }

        if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
        {
            return;
        }

        if (temp != null)
        {
            temp.Clear();
            temp = null;
        }

        if (forceAddedWeapons is { Count: 0 })
        {
            forceAddedWeapons = null;
        }
    }

    public override string GetInspectString()
    {
        Tick();
        var stringBuilder = new StringBuilder(base.GetInspectString());
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Append(Environment.NewLine);
        }

        stringBuilder.Append("WeaponStorage.StoragePriority".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(("StoragePriority" + settings.Priority).Translate());
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("WeaponStorage.Count".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(Count);
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("WeaponStorage.IncludeInTradeDeals".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(includeInTradeDeals.ToString());
        return stringBuilder.ToString();
    }

    public IEnumerable<ThingWithComps> GetWeapons(bool includeBioencoded)
    {
        foreach (var value in StoredWeapons.Values)
        {
            foreach (var item in value)
            {
                yield return item;
            }
        }

        if (!includeBioencoded)
        {
            yield break;
        }

        foreach (var value2 in StoredBioEncodedWeapons.Values)
        {
            foreach (var item2 in value2)
            {
                yield return item2;
            }
        }
    }

    public IEnumerable<ThingWithComps> GetBioEncodedWeapons()
    {
        foreach (var value in StoredBioEncodedWeapons.Values)
        {
            foreach (var item in value)
            {
                yield return item;
            }
        }
    }

    public bool Remove(ThingWithComps weapon)
    {
        return weapon == null || RemoveFrom(weapon,
            !CompBiocodable.IsBiocoded(weapon) ? StoredWeapons : StoredBioEncodedWeapons);
    }

    private bool RemoveFrom(ThingWithComps weapon, Dictionary<ThingDef, LinkedList<ThingWithComps>> storage)
    {
        if (weapon == null)
        {
            return true;
        }

        if (!storage.TryGetValue(weapon.def, out var value) || !value.Remove(weapon) || weapon.Spawned ||
            DropThing(weapon))
        {
            return weapon.Spawned;
        }

        Log.Warning($"failed to drop {weapon.Label} from storage {Label}");
        return false;
    }

    public bool RemoveNoDrop(ThingWithComps thing)
    {
        if (!StoredWeapons.TryGetValue(thing.def, out var value) &&
            !StoredBioEncodedWeapons.TryGetValue(thing.def, out value))
        {
            return false;
        }

        return value.Remove(thing);
    }

    public override void TickLong()
    {
        if (Spawned && Map != null)
        {
            HandleThingsOnTop();
        }

        while (WorldComp.WeaponsToDrop.Count > 0)
        {
            DropThing(WorldComp.WeaponsToDrop.Pop());
        }

        if (forceAddedWeapons is not { Count: > 0 })
        {
            return;
        }

        foreach (var forceAddedWeapon in forceAddedWeapons)
        {
            try
            {
                DropThing(forceAddedWeapon);
            }
            catch
            {
                // ignored
            }
        }

        forceAddedWeapons.Clear();
        forceAddedWeapons = null;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var gizmos = base.GetGizmos();
        var list = gizmos == null ? new List<Gizmo>(1) : [..gizmos];
        var hashCode = "WeaponStorage".GetHashCode();
        if (Settings.EnableAssignWeapons)
        {
            list.Add(new Command_Action
            {
                icon = AssignUI.assignweaponsTexture,
                defaultDesc = "WeaponStorage.AssignWeaponsDesc".Translate(),
                defaultLabel = "WeaponStorage.AssignWeapons".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate { Find.WindowStack.Add(new AssignUI(this)); },
                groupKey = hashCode
            });
        }
        else
        {
            list.Add(new Command_Action
            {
                icon = AssignUI.assignweaponsTexture,
                defaultDesc = "WeaponStorage".Translate(),
                defaultLabel = "WeaponStorage".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate { Find.WindowStack.Add(new AssignUI(this)); },
                groupKey = hashCode
            });
        }

        hashCode++;
        if (Settings.EnableAssignWeapons)
        {
            list.Add(new Command_Action
            {
                icon = AssignUI.assignweaponsTexture,
                defaultDesc = "WeaponStorage.SharedWeaponsDesc".Translate(),
                defaultLabel = "WeaponStorage.SharedWeapons".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate { Find.WindowStack.Add(new SharedWeaponsUI()); },
                groupKey = hashCode
            });
            hashCode++;
            if (CombatExtendedUtil.HasCombatExtended)
            {
                list.Add(new Command_Action
                {
                    icon = AssignUI.ammoTexture,
                    defaultDesc = "WeaponStorage.ManageAmmoDesc".Translate(),
                    defaultLabel = "WeaponStorage.ManageAmmo".Translate(),
                    activateSound = SoundDef.Named("Click"),
                    action = delegate { Find.WindowStack.Add(new AmmoUI(this)); },
                    groupKey = hashCode
                });
                hashCode++;
            }
        }

        list.Add(new Command_Action
        {
            icon = AssignUI.emptyTexture,
            defaultDesc = "WeaponStorage.EmptyDesc".Translate(),
            defaultLabel = "WeaponStorage.Empty".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = Empty,
            groupKey = hashCode
        });
        hashCode++;
        list.Add(new Command_Action
        {
            icon = AssignUI.collectTexture,
            defaultDesc = "WeaponStorage.CollectDesc".Translate(),
            defaultLabel = "WeaponStorage.Collect".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = delegate { ReclaimWeapons(); },
            groupKey = hashCode
        });
        hashCode++;
        list.Add(new Command_Action
        {
            icon = includeInTradeDeals ? AssignUI.yesSellTexture : AssignUI.noSellTexture,
            defaultDesc = "WeaponStorage.IncludeInTradeDealsDesc".Translate(),
            defaultLabel = "WeaponStorage.IncludeInTradeDeals".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = delegate { includeInTradeDeals = !includeInTradeDeals; },
            groupKey = hashCode
        });
        return list;
    }

    protected bool AreStorageSettingsEqual()
    {
        var filter = settings.filter;
        if (filter.AllowedDefCount != previousStorageFilters.AllowedDefCount ||
            filter.AllowedQualityLevels != previousStorageFilters.AllowedQualityLevels ||
            filter.AllowedHitPointsPercents != previousStorageFilters.AllowedHitPointsPercents)
        {
            return false;
        }

        var hashSet = AllowedDefsFI.GetValue(filter) as HashSet<ThingDef>;
        foreach (var item in (HashSet<ThingDef>)AllowedDefsFI.GetValue(previousStorageFilters))
        {
            if (hashSet != null && !hashSet.Contains(item))
            {
                return false;
            }
        }

        return true;
    }

    private void UpdatePreviousStorageFilter()
    {
        var filter = settings.filter;
        previousStorageFilters.AllowedHitPointsPercents = filter.AllowedHitPointsPercents;
        previousStorageFilters.AllowedQualityLevels = filter.AllowedQualityLevels;
        var obj = AllowedDefsFI.GetValue(previousStorageFilters) as HashSet<ThingDef>;
        obj?.Clear();
        obj.AddRange(AllowedDefsFI.GetValue(filter) as HashSet<ThingDef>);
    }
}