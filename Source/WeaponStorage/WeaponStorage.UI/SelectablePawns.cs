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
            labelAndStats ??= GetLabelAndStatsFor(Pawn);

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

    public override bool Equals(object obj)
    {
        if (obj is SelectablePawns selectablePawns)
        {
            return Pawn?.thingIDNumber == selectablePawns.Pawn?.thingIDNumber;
        }

        if (obj is Pawn pawnObject)
        {
            return Pawn?.thingIDNumber == pawnObject.thingIDNumber;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Pawn.GetHashCode() * labelAndStats.GetHashCode();
    }
}