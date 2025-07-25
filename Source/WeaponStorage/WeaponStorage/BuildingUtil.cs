using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponStorage;

internal class BuildingUtil
{
    public static List<T> FindThingsOfTypeNextTo<T>(Map map, IntVec3 position, int distance) where T : Thing
    {
        var num = Math.Max(0, position.x - distance);
        var num2 = Math.Min(map.info.Size.x, position.x + distance);
        var num3 = Math.Max(0, position.z - distance);
        var num4 = Math.Min(map.info.Size.z, position.z + distance);
        var reservationManager = map.reservationManager;
        var list = new List<T>();
        for (var i = num - 1; i <= num2; i++)
        {
            for (var j = num3 - 1; j <= num4; j++)
            {
                foreach (var item in map.thingGrid.ThingsAt(new IntVec3(i, position.y, j)))
                {
                    if (item is T thing && (reservationManager == null ||
                                            !reservationManager.IsReservedByAnyoneOf(
                                                new LocalTargetInfo(thing),
                                                Faction.OfPlayer)))
                    {
                        list.Add(thing);
                    }
                }
            }
        }

        return list;
    }

    public static bool DropThing(Thing toDrop, Building_WeaponStorage from, Map map, bool makeForbidden = false)
    {
        if (toDrop == null)
        {
            return false;
        }

        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        try
        {
            from.AllowAdds = false;
            return toDrop.stackCount <= toDrop.def.stackLimit
                ? DropSingleThing(toDrop, from, map, makeForbidden)
                : DropThing(toDrop, toDrop.stackCount, from, map, null, makeForbidden);
        }
        catch (Exception ex)
        {
            Log.Warning($"failed to drop {toDrop.def?.defName}\n{ex.Message}");
            return true;
        }
        finally
        {
            from.AllowAdds = true;
        }
    }

    public static bool DropThing(Thing toDrop, IntVec3 from, Map map, bool makeForbidden = false)
    {
        if (toDrop.stackCount != 0)
        {
            return toDrop.stackCount <= toDrop.def.stackLimit
                ? DropSingleThing(toDrop, from, map, makeForbidden)
                : DropThing(toDrop, toDrop.stackCount, from, map, null, makeForbidden);
        }

        Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
        return false;
    }

    public static bool DropThing(Thing toDrop, int amountToDrop, Building_WeaponStorage from, Map map,
        List<Thing> droppedThings = null, bool makeForbidden = false)
    {
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        var result = false;
        try
        {
            from.AllowAdds = false;
            var thingsLeft = false;
            while (!thingsLeft)
            {
                var num = toDrop.def.stackLimit;
                if (num > amountToDrop)
                {
                    num = amountToDrop;
                    thingsLeft = true;
                }

                if (num >= toDrop.stackCount)
                {
                    if (amountToDrop > num)
                    {
                        Log.Error($"        ThingStorage: Unable to drop {amountToDrop - num} of {toDrop.def.label}");
                    }

                    num = toDrop.stackCount;
                    thingsLeft = true;
                }

                if (num <= 0)
                {
                    continue;
                }

                amountToDrop -= num;
                var thing = toDrop.SplitOff(num);
                droppedThings?.Add(thing);
                if (DropSingleThing(thing, from, map, makeForbidden))
                {
                    result = true;
                }
            }

            return result;
        }
        finally
        {
            from.AllowAdds = true;
        }
    }

    public static bool DropThing(Thing toDrop, int amountToDrop, IntVec3 from, Map map,
        List<Thing> droppedThings = null, bool makeForbidden = false)
    {
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        var result = false;
        var thingsLeft = false;
        while (!thingsLeft)
        {
            var num = toDrop.def.stackLimit;
            if (num > amountToDrop)
            {
                num = amountToDrop;
                thingsLeft = true;
            }

            if (num >= toDrop.stackCount)
            {
                if (amountToDrop > num)
                {
                    Log.Error($"        ThingStorage: Unable to drop {amountToDrop - num} of {toDrop.def.label}");
                }

                num = toDrop.stackCount;
                thingsLeft = true;
            }

            if (num <= 0)
            {
                continue;
            }

            amountToDrop -= num;
            var thing = toDrop.SplitOff(num);
            droppedThings?.Add(thing);
            if (DropSingleThing(thing, from, map, makeForbidden))
            {
                result = true;
            }
        }

        return result;
    }

    public static bool DropSingleThing(Thing toDrop, Building_WeaponStorage from, Map map, bool makeForbidden = false)
    {
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        try
        {
            from.AllowAdds = false;
            return DropSingleThing(toDrop, from.InteractionCell, map, makeForbidden);
        }
        finally
        {
            from.AllowAdds = true;
        }
    }

    public static bool DropSingleThing(Thing toDrop, IntVec3 from, Map map, bool makeForbidden = false)
    {
        if (toDrop == null)
        {
            return false;
        }

        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        try
        {
            if (!toDrop.Spawned)
            {
                GenThing.TryDropAndSetForbidden(toDrop, from, map, ThingPlaceMode.Near, out _, makeForbidden);
                if (!toDrop.Spawned && !GenPlace.TryPlaceThing(toDrop, from, map, ThingPlaceMode.Near))
                {
                    Log.Error($"Failed to spawn {toDrop.Label} x{toDrop.stackCount}");
                    return false;
                }
            }

            toDrop.Position = from;
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(BuildingUtil)}.DropApparel\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }

        return toDrop.Spawned;
    }
}