using RimWorld;
using Verse;

namespace WeaponStorage.UI;

public struct SelectablePawns(Pawn pawn)
{
    public readonly Pawn Pawn = pawn;

    private string labelAndStats = null;

    public string LabelAndStats
    {
        get
        {
            if (labelAndStats == null)
            {
                labelAndStats = GetLabelAndStatsFor(Pawn);
            }

            return labelAndStats;
        }
    }

    public string Melee => !Pawn.WorkTagIsDisabled(WorkTags.Violent)
        ? Pawn.skills.GetSkill(SkillDefOf.Melee).levelInt.ToString()
        : "-";

    public string Ranged => !Pawn.WorkTagIsDisabled(WorkTags.Violent)
        ? Pawn.skills.GetSkill(SkillDefOf.Shooting).levelInt.ToString()
        : "-";

    public static string GetLabelAndStatsFor(Pawn pawn)
    {
        var text = pawn.WorkTagIsDisabled(WorkTags.Violent)
            ? "-"
            : pawn.skills.GetSkill(SkillDefOf.Melee).levelInt.ToString();
        var text2 = pawn.WorkTagIsDisabled(WorkTags.Violent)
            ? "-"
            : pawn.skills.GetSkill(SkillDefOf.Shooting).levelInt.ToString();
        return $"{pawn.Name.ToStringShort} -- {SkillDefOf.Melee.label}: {text} -- {SkillDefOf.Shooting.label}: {text2}";
    }

    public override bool Equals(object o)
    {
        if (o is SelectablePawns selectablePawns)
        {
            return Pawn?.thingIDNumber == selectablePawns.Pawn?.thingIDNumber;
        }

        if (o is Pawn pawn)
        {
            return Pawn?.thingIDNumber == pawn.thingIDNumber;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Pawn.GetHashCode() * labelAndStats.GetHashCode();
    }
}