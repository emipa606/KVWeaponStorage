using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WeaponStorage.UI;

[StaticConstructorOnStartup]
public class AmmoUI : Window
{
    private readonly List<ThingDefCount> ammo = [];

    private readonly List<ThingDefCount> searchResults = [];
    private readonly Building_WeaponStorage weaponStorage;

    private bool performSearch;

    private float PreviousY;

    private Vector2 scrollPosition = new Vector2(0f, 0f);

    private string searchText = "";

    public AmmoUI(Building_WeaponStorage weaponStorage)
    {
        this.weaponStorage = weaponStorage;
        closeOnClickedOutside = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        RebuildItems();
    }

    public override Vector2 InitialSize => new Vector2(650f, 600f);

    private void RebuildItems()
    {
        ammo.Clear();
        foreach (var thingCount in CombatExtendedUtil.GetThingCounts())
        {
            ammo.Add(thingCount);
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        try
        {
            var num = 0f;
            Widgets.Label(new Rect(0f, num, 100f, 30f), "WeaponStorage.ManageAmmo".Translate());
            num += 32f;
            Widgets.Label(new Rect(0f, num + 4f, 70f, 30f), "WeaponStorage.Search".Translate());
            var value = searchText;
            searchText = Widgets.TextField(new Rect(75f, num, 150f, 30f), searchText);
            if (!searchText.Equals(value))
            {
                performSearch = true;
            }

            num += 44f;
            var thingsToShow = GetThingsToShow();
            if (thingsToShow == null || thingsToShow.Count == 0)
            {
                var label = "No ammo in storage";
                if (searchText.Length > 0)
                {
                    label = "No matches found";
                }

                Widgets.Label(new Rect(40f, num, 200f, 30f), label);
                return;
            }

            Widgets.BeginScrollView(new Rect(40f, num, inRect.width - 80f, inRect.height - num - 50f),
                ref scrollPosition, new Rect(0f, 0f, inRect.width - 96f, PreviousY));
            PreviousY = 0f;
            foreach (var item in thingsToShow)
            {
                var thingDef = item.ThingDef;
                Widgets.ThingIcon(new Rect(0f, PreviousY, 30f, 30f), thingDef);
                if (Widgets.InfoCardButton(40f, PreviousY, thingDef))
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(thingDef));
                }

                Widgets.Label(new Rect(70f, PreviousY, 250f, 30f), thingDef.label);
                Widgets.Label(new Rect(340f, PreviousY, 40f, 30f), item.Count.ToString());
                if (Widgets.ButtonImage(new Rect(inRect.width - 100f, PreviousY, 20f, 20f), AssignUI.DropTexture))
                {
                    CombatExtendedUtil.DropAmmo(thingDef, weaponStorage);
                    RebuildItems();
                    performSearch = true;
                    break;
                }

                PreviousY += 32f;
            }

            Widgets.EndScrollView();
        }
        catch (Exception ex)
        {
            var text = $"{GetType().Name} closed due to: {ex.GetType().Name} {ex.Message}";
            Log.Error(text);
            Messages.Message(text, MessageTypeDefOf.NegativeEvent);
            base.Close();
        }
        finally
        {
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    private List<ThingDefCount> GetThingsToShow()
    {
        if (performSearch)
        {
            performSearch = false;
            searchResults.Clear();
            if (!searchText.Trim().NullOrEmpty())
            {
                var value = searchText.ToLower().Trim();
                foreach (var item in ammo)
                {
                    if (item.ThingDef.label.ToLower().Contains(value) ||
                        item.ThingDef.defName.ToLower().Contains(value))
                    {
                        searchResults.Add(item);
                    }
                }
            }
        }

        if (searchResults.Count > 0)
        {
            return searchResults;
        }

        return searchText.Trim().NullOrEmpty() ? ammo : null;
    }

    private enum Tabs
    {
        Empty,
        WeaponStorage_General,
        WeaponStorage_Neolithic,
        WeaponStorage_Grenades,
        WeaponStorage_Rockets,
        WeaponStorage_Shotguns,
        WeaponStorage_Advanced
    }
}