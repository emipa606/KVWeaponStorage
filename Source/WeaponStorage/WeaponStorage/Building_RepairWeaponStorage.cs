using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace WeaponStorage;

internal class Building_RepairWeaponStorage : Building
{
    private const int LOW_POWER_COST = 10;

    private const long THIRTY_SECONDS = 300000000L;

    private static readonly LinkedList<ThingWithComps> AllWeaponsBeingRepaired = [];

    private LinkedList<Building_WeaponStorage> AttachedWeaponStorages = [];

    private ThingWithComps beingRepaird;

    public CompPowerTrader compPowerTrader;

    private AssignedWeaponContainer container;

    private long lastSearch = DateTime.Now.Ticks;

    private long lastTick = DateTime.Now.Ticks;

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder(base.GetInspectString());
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Append(Environment.NewLine);
        }

        stringBuilder.Append("WeaponStorage.AttachedWeaponStorages".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(AttachedWeaponStorages.Count);
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("WeaponStorage.IsRepairing".Translate());
        stringBuilder.Append(": ");
        if (beingRepaird != null)
        {
            stringBuilder.Append(beingRepaird.Label);
            if (container != null)
            {
                stringBuilder.Append(" (");
                stringBuilder.Append(container.Pawn.Name.ToStringShort);
                stringBuilder.Append(")");
            }

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("    ");
            stringBuilder.Append(beingRepaird.HitPoints.ToString());
            stringBuilder.Append("/");
            stringBuilder.Append(beingRepaird.MaxHitPoints);
        }
        else
        {
            stringBuilder.Append(bool.FalseString);
        }

        return stringBuilder.ToString();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        compPowerTrader = GetComp<CompPowerTrader>();
        compPowerTrader.PowerOutput = -10f;
        foreach (var item in BuildingUtil.FindThingsOfTypeNextTo<Building_WeaponStorage>(Map, Position,
                     Settings.RepairAttachmentDistance))
        {
            Add(item);
        }

        compPowerTrader.powerStartedAction = delegate { compPowerTrader.PowerOutput = 10f; };
        compPowerTrader.powerStoppedAction = delegate
        {
            StopRepairing();
            compPowerTrader.PowerOutput = 0f;
        };
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
        StopRepairing();
        AttachedWeaponStorages.Clear();
    }

    public override void Discard(bool silentlyRemoveReferences = false)
    {
        base.Discard(silentlyRemoveReferences);
        StopRepairing();
        AttachedWeaponStorages.Clear();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        StopRepairing();
        AttachedWeaponStorages.Clear();
    }

    public override void Tick()
    {
        TickRare();
        var ticks = DateTime.Now.Ticks;
        if (ticks - lastTick <= Settings.RepairAttachmentUpdateIntervalTicks)
        {
            return;
        }

        lastTick = DateTime.Now.Ticks;
        if (!compPowerTrader.PowerOn)
        {
            if (beingRepaird != null)
            {
                StopRepairing();
            }
        }
        else if (beingRepaird == null)
        {
            if (ticks - lastSearch > 300000000)
            {
                lastSearch = ticks;
                StartRepairing();
            }
        }
        else if (beingRepaird != null && beingRepaird.HitPoints >= beingRepaird.MaxHitPoints)
        {
            beingRepaird.HitPoints = beingRepaird.MaxHitPoints;
            StopRepairing();
            lastSearch = ticks;
            StartRepairing();
        }

        if (beingRepaird != null)
        {
            beingRepaird.HitPoints += Settings.RepairAttachmentMendingSpeed;
            if (beingRepaird.HitPoints > beingRepaird.MaxHitPoints)
            {
                beingRepaird.HitPoints = beingRepaird.MaxHitPoints;
            }

            var num = GenTemperature.ControlTemperatureTempChange(Position, Map, 10f, float.MaxValue);
            this.GetRoom().Temperature += num;
            compPowerTrader.PowerOutput = 0f - compPowerTrader.Props.basePowerConsumption;
        }
        else
        {
            compPowerTrader.PowerOutput = 10f;
        }
    }

    private void OrderAttachedWeaponStorages()
    {
        var prioCheck = true;
        var linkedListNode = AttachedWeaponStorages.First;
        while (linkedListNode != null)
        {
            var next = linkedListNode.Next;
            if (!linkedListNode.Value.Spawned)
            {
                AttachedWeaponStorages.Remove(linkedListNode);
            }
            else if (linkedListNode.Next != null && (int)linkedListNode.Value.settings.Priority <
                     (int)linkedListNode.Next.Value.settings.Priority)
            {
                prioCheck = false;
            }

            linkedListNode = next;
        }

        if (prioCheck)
        {
            return;
        }

        var linkedList = new LinkedList<Building_WeaponStorage>();
        for (linkedListNode = AttachedWeaponStorages.First;
             linkedListNode != null;
             linkedListNode = linkedListNode.Next)
        {
            var value = linkedListNode.Value;
            var secondPrioCheck = false;
            for (var linkedListNode2 = linkedList.First;
                 linkedListNode2 != null;
                 linkedListNode2 = linkedListNode2.Next)
            {
                if ((int)value.settings.Priority <= (int)linkedListNode2.Value.settings.Priority)
                {
                    continue;
                }

                linkedList.AddBefore(linkedListNode2, value);
                secondPrioCheck = true;
                break;
            }

            if (!secondPrioCheck)
            {
                linkedList.AddLast(value);
            }
        }

        AttachedWeaponStorages.Clear();
        AttachedWeaponStorages = linkedList;
    }

    private void StartRepairing()
    {
        OrderAttachedWeaponStorages();
        foreach (var assignedWeaponContainer in WorldComp.AssignedWeaponContainers)
        {
            foreach (var weapon in assignedWeaponContainer.Weapons)
            {
                if (weapon.HitPoints >= weapon.MaxHitPoints || AllWeaponsBeingRepaired.Contains(weapon))
                {
                    continue;
                }

                beingRepaird = weapon;
                container = assignedWeaponContainer;
                AllWeaponsBeingRepaired.AddLast(weapon);
                return;
            }
        }

        for (var linkedListNode = AttachedWeaponStorages.First;
             linkedListNode != null;
             linkedListNode = linkedListNode.Next)
        {
            foreach (var weapon2 in linkedListNode.Value.GetWeapons(true))
            {
                if (weapon2.HitPoints >= weapon2.MaxHitPoints || AllWeaponsBeingRepaired.Contains(weapon2))
                {
                    continue;
                }

                beingRepaird = weapon2;
                container = null;
                AllWeaponsBeingRepaired.AddLast(weapon2);
                return;
            }
        }
    }

    private void StopRepairing()
    {
        if (beingRepaird == null)
        {
            return;
        }

        AllWeaponsBeingRepaired.Remove(beingRepaird);
        beingRepaird = null;
        container = null;
    }

    public void Add(Building_WeaponStorage s)
    {
        if (AttachedWeaponStorages == null)
        {
            return;
        }

        if (!AttachedWeaponStorages.Contains(s))
        {
            AttachedWeaponStorages.AddLast(s);
        }
    }

    public void Remove(Building_WeaponStorage s)
    {
        if (AttachedWeaponStorages == null || !AttachedWeaponStorages.Any() || !AttachedWeaponStorages.Contains(s))
        {
            return;
        }

        AttachedWeaponStorages.Remove(s);
    }
}