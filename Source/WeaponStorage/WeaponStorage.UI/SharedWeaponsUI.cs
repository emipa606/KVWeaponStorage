using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WeaponStorage.UI;

internal class SharedWeaponsUI : Window
{
    private readonly IEnumerable<SelectablePawns> pawns;
    private float lastX;

    private float lastY;
    private Vector2 scrollPosition = new Vector2(0f, 0f);

    private SharedWeaponFilter selectedFilter;

    public SharedWeaponsUI()
    {
        closeOnClickedOutside = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        pawns = Util.GetPawns(true);
    }

    public override Vector2 InitialSize => new Vector2(800f, 600f);

    public override void DoWindowContents(Rect inRect)
    {
        if (!Find.WindowStack.CurrentWindowGetsInput)
        {
            return;
        }

        Text.Font = GameFont.Small;
        var num = 0f;
        if (Widgets.ButtonText(new Rect(0f, num, 250f, 32f),
                selectedFilter == null
                    ? "WeaponStorage.SharedWeaponsFilter".Translate().ToString()
                    : selectedFilter.Label) && WorldComp.SharedWeaponFilter.Count > 0)
        {
            var list = new List<FloatMenuOption>();
            foreach (var f in WorldComp.SharedWeaponFilter)
            {
                list.Add(new FloatMenuOption(f.Label, delegate { selectedFilter = f; }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        if (Widgets.ButtonText(new Rect(275f, num, 100f, 32f), "WeaponStorage.New".Translate()))
        {
            var sharedWeaponFilter = new SharedWeaponFilter
            {
                Label = ""
            };
            WorldComp.SharedWeaponFilter.Add(sharedWeaponFilter);
            Find.WindowStack.Add(new SharedWeaponFilterUI(sharedWeaponFilter));
        }

        if (selectedFilter != null &&
            Widgets.ButtonText(new Rect(400f, num, 100f, 32f), "WeaponStorage.Edit".Translate()))
        {
            Find.WindowStack.Add(new SharedWeaponFilterUI(selectedFilter));
        }

        if (selectedFilter != null && Widgets.ButtonText(new Rect(525f, num, 100f, 32f), "Delete".Translate()))
        {
            WorldComp.SharedWeaponFilter.Remove(selectedFilter);
            selectedFilter = null;
        }

        num += 60f;
        if (WorldComp.SharedWeaponFilter.Count == 0)
        {
            return;
        }

        var num2 = 0f;
        Widgets.Label(new Rect(num2, num, 100f, 30f), "MedGroupColonists".Translate());
        num2 += 120f;
        Widgets.DrawTextureFitted(new Rect(num2, num, 30f, 30f), AssignUI.meleeTexture, 1f);
        num2 += 40f;
        Widgets.DrawTextureFitted(new Rect(num2, num, 30f, 30f), AssignUI.rangedTexture, 1f);
        num2 += 50f;
        var num3 = num2;
        var vector = new Vector2(scrollPosition.x, 0f);
        Widgets.BeginScrollView(new Rect(num2, num, inRect.width - num3 - 36f, 32f), ref vector,
            new Rect(0f, 0f, lastX, 32f), false);
        num2 = 0f;
        foreach (var item in WorldComp.SharedWeaponFilter)
        {
            Widgets.Label(new Rect(num2, 0f, 200f, 30f), item.Label);
            num2 += 150f;
        }

        Widgets.EndScrollView();
        num += 32f;
        vector = new Vector2(0f, scrollPosition.y);
        Widgets.BeginScrollView(new Rect(20f, num, 200f, inRect.height - num - 76f), ref vector,
            new Rect(0f, 0f, 100f, lastY), false);
        var num4 = 0f;
        foreach (var pawn in pawns)
        {
            num2 = 0f;
            Widgets.Label(new Rect(0f, num4 + 2f, 100f, 30f), pawn.Pawn.Name.ToStringShort);
            num2 += 110f;
            Widgets.Label(new Rect(num2, num4 + 2f, 30f, 30f), pawn.Melee);
            num2 += 40f;
            Widgets.Label(new Rect(num2, num4 + 2f, 30f, 30f), pawn.Ranged);
            num2 += 50f;
            num4 += 32f;
            Widgets.DrawLineHorizontal(0f, num4, 200f);
        }

        Widgets.EndScrollView();
        Widgets.BeginScrollView(new Rect(num2, num, inRect.width - num3 - 20f, inRect.height - num - 60f),
            ref scrollPosition, new Rect(0f, 0f, lastX, lastY));
        num4 = 0f;
        foreach (var pawn2 in pawns)
        {
            num2 = 0f;
            foreach (var item2 in WorldComp.SharedWeaponFilter)
            {
                var contains = item2.AssignedPawns.Contains(pawn2.Pawn);
                var checkOn = contains;
                Widgets.Checkbox(new Vector2(num2, num4 + 2f), ref checkOn, 26f);
                if (checkOn != contains)
                {
                    if (checkOn)
                    {
                        item2.AssignedPawns.Add(pawn2.Pawn);
                    }
                    else
                    {
                        item2.AssignedPawns.Remove(pawn2.Pawn);
                    }
                }

                num2 += 150f;
            }

            num4 += 32f;
            Widgets.DrawLineHorizontal(0f, num4, lastX);
        }

        Widgets.EndScrollView();
        lastX = num2;
        lastY = num4;
    }
}