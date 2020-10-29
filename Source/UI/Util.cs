﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WeaponStorage.UI
{
	public struct SelectablePawns
	{
		public Pawn Pawn;
		private string labelAndStats;
		public SelectablePawns(Pawn pawn)
		{
			this.Pawn = pawn;
			this.labelAndStats = null;
		}
		public string LabelAndStats
		{
			get
			{
				if (this.labelAndStats == null)
					this.labelAndStats = Pawn.Name.ToStringShort + "-- " + SkillDefOf.Melee.label + ": " + Melee + " -- " + SkillDefOf.Shooting.label + ": " + Ranged;
				return this.labelAndStats;
			}
		}
		public string Melee => ((Pawn.WorkTagIsDisabled(WorkTags.Violent)) ? "-" : Pawn.skills.GetSkill(SkillDefOf.Melee).levelInt.ToString());
		public string Ranged => ((Pawn.WorkTagIsDisabled(WorkTags.Violent)) ? "-" : Pawn.skills.GetSkill(SkillDefOf.Shooting).levelInt.ToString());

        public override bool Equals(object o)
        {
			if (o is SelectablePawns sp)
				return this.Pawn?.thingIDNumber == sp.Pawn?.thingIDNumber;
			if (o is Pawn p)
				return this.Pawn?.thingIDNumber == p.thingIDNumber;
			return false;
        }

        public override int GetHashCode()
        {
			return this.Pawn.GetHashCode() * this.labelAndStats.GetHashCode();
        }
    }

	public static class Util
	{
		public static List<SelectablePawns> GetPawns(bool excludeNonViolent)
		{
			SortedDictionary<string, Pawn> pawns = new SortedDictionary<string, Pawn>();
			foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
				if (p != null && p.Faction == Faction.OfPlayer && p.def.race.Humanlike && !p.Dead && p.apparel?.LockedApparel?.Count == 0)
                {
                    if (!excludeNonViolent || !p.WorkTagIsDisabled(WorkTags.Violent))
                        pawns.Add(p.Name.ToStringShort, p);
                }

			List<SelectablePawns> result = new List<SelectablePawns>(pawns.Count);
			foreach (Pawn p in pawns.Values)
				result.Add(new SelectablePawns(p));

			return result;
		}
	}
}
