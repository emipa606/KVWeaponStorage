using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WeaponStorage.UI;

public static class Util
{
    public static List<SelectablePawns> GetPawns(bool excludeNonViolent)
    {
        if (!Settings.EnableAssignWeapons)
        {
            return new List<SelectablePawns>(0);
        }

        var sortedDictionary = new SortedDictionary<string, List<Pawn>>();
        foreach (var allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder
                     .AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
        {
            if (allMapsCaravansAndTravelingTransportPods_Alive_Colonist == null ||
                allMapsCaravansAndTravelingTransportPods_Alive_Colonist.Faction != Faction.OfPlayer ||
                !allMapsCaravansAndTravelingTransportPods_Alive_Colonist.def.race.Humanlike ||
                allMapsCaravansAndTravelingTransportPods_Alive_Colonist.Dead)
            {
                continue;
            }

            var apparel = allMapsCaravansAndTravelingTransportPods_Alive_Colonist.apparel;
            if (apparel == null || apparel.LockedApparel?.Count != 0 || excludeNonViolent &&
                allMapsCaravansAndTravelingTransportPods_Alive_Colonist
                    .WorkTagIsDisabled(WorkTags.Violent))
            {
                continue;
            }

            var toStringShort = allMapsCaravansAndTravelingTransportPods_Alive_Colonist.Name.ToStringShort;
            if (!sortedDictionary.TryGetValue(toStringShort, out var value))
            {
                value = sortedDictionary[toStringShort] = new List<Pawn>();
            }

            value.Add(allMapsCaravansAndTravelingTransportPods_Alive_Colonist);
        }

        var list2 = new List<SelectablePawns>(sortedDictionary.Count);
        foreach (var value2 in sortedDictionary.Values)
        {
            foreach (var item in value2)
            {
                list2.Add(new SelectablePawns(item));
            }
        }

        return list2;
    }
}