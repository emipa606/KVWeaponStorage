using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WeaponStorage.UI;

internal class SharedWeaponFilterUI : Window
{
    private readonly SharedWeaponFilter filter;

    private readonly SortedDictionary<string, ThingDef> meleeWeapons = new SortedDictionary<string, ThingDef>();

    private readonly SortedDictionary<string, ThingDef> rangedWeapons = new SortedDictionary<string, ThingDef>();
    private Vector2 scrollPosition = new Vector2(0f, 0f);

    private float y;

    public SharedWeaponFilterUI(SharedWeaponFilter filter)
    {
        this.filter = filter;
        closeOnClickedOutside = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        foreach (var allDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (allDef.IsRangedWeapon)
            {
                rangedWeapons[allDef.label] = allDef;
            }
            else if (allDef.IsMeleeWeapon)
            {
                meleeWeapons[allDef.label] = allDef;
            }
        }
    }

    public override Vector2 InitialSize => new Vector2(300f, 600f);

    public override void DoWindowContents(Rect inRect)
    {
        var num = 3f;
        Widgets.Label(new Rect(0f, num, 70f, 32f), "WeaponStorage.Name".Translate());
        filter.Label = Widgets.TextArea(new Rect(80f, num - 3f, inRect.width - 100f, 32f), filter.Label);
        num += 36f;
        DrawHitPointsFilterConfig(0f, ref num, inRect.width, filter);
        DrawQualityFilterConfig(0f, ref num, inRect.width, filter);
        Widgets.Label(new Rect(0f, num, 200f, 30f), "WeaponStorage.AllowedWeapons".Translate());
        num += 32f;
        Widgets.BeginScrollView(new Rect(10f, num, inRect.width - 10f, inRect.height - 40f - num), ref scrollPosition,
            new Rect(0f, 0f, inRect.width - 26f, y));
        y = 0f;
        foreach (var value in meleeWeapons.Values)
        {
            DrawThingDefCheckbox(value);
        }

        foreach (var value2 in rangedWeapons.Values)
        {
            DrawThingDefCheckbox(value2);
        }

        Widgets.EndScrollView();
    }

    private void DrawThingDefCheckbox(ThingDef d)
    {
        Widgets.Label(new Rect(0f, y, 190f, 30f), d.label);
        bool checkOn;
        var num = checkOn = filter.AllowedDefs.Contains(d);
        Widgets.Checkbox(new Vector2(200f, y), ref checkOn, 28f);
        if (num != checkOn)
        {
            if (checkOn)
            {
                filter.AllowedDefs.Add(d);
            }
            else
            {
                filter.AllowedDefs.Remove(d);
            }
        }

        y += 32f;
    }

    private void DrawHitPointsFilterConfig(float x, ref float internalY, float width, SharedWeaponFilter internalFilter)
    {
        var rect = new Rect(x + 20f, internalY, width - 40f, 28f);
        var hpRange = internalFilter.HpRange;
        Widgets.FloatRange(rect, 1, ref hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
        internalFilter.HpRange = hpRange;
        internalY += 28f;
        internalY += 5f;
        Text.Font = GameFont.Small;
    }

    private void DrawQualityFilterConfig(float x, ref float internalY, float width, SharedWeaponFilter internalFilter)
    {
        var rect = new Rect(x + 20f, internalY, width - 40f, 28f);
        var range = internalFilter.QualityRange;
        Widgets.QualityRange(rect, "WeaponStorageQualityRange".GetHashCode(), ref range);
        internalFilter.QualityRange = range;
        internalY += 28f;
        internalY += 5f;
        Text.Font = GameFont.Small;
    }
}