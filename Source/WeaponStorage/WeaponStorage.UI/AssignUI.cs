using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WeaponStorage.UI;

[StaticConstructorOnStartup]
public class AssignUI : Window
{
    public static Texture2D DropTexture;

    public static Texture2D UnknownWeaponIcon;

    public static Texture2D assignweaponsTexture;

    public static Texture2D emptyTexture;

    public static Texture2D collectTexture;

    public static Texture2D yesSellTexture;

    public static Texture2D noSellTexture;

    public static Texture2D meleeTexture;

    public static Texture2D rangedTexture;

    public static Texture2D ammoTexture;

    public static Texture2D nextTexture;

    public static Texture2D previousTexture;

    public static Texture2D weaponStorageTexture;

    private readonly List<SelectablePawns> selectablePawns;

    private AssignedWeaponContainer assignedWeapons;

    private int pawnIndex = -1;

    private List<ThingWithComps> PossibleWeapons;

    private float PreviousY;

    private Vector2 scrollPosition = new Vector2(0f, 0f);

    private string textBuffer = "";

    private Building_WeaponStorage weaponStorage;

    static AssignUI()
    {
        DropTexture = ContentFinder<Texture2D>.Get("UI/drop");
        UnknownWeaponIcon = ContentFinder<Texture2D>.Get("UI/UnknownWeapon");
        assignweaponsTexture = ContentFinder<Texture2D>.Get("UI/assignweapons");
        emptyTexture = ContentFinder<Texture2D>.Get("UI/empty");
        collectTexture = ContentFinder<Texture2D>.Get("UI/collect");
        yesSellTexture = ContentFinder<Texture2D>.Get("UI/yessell");
        noSellTexture = ContentFinder<Texture2D>.Get("UI/nosell");
        meleeTexture = ContentFinder<Texture2D>.Get("UI/melee");
        rangedTexture = ContentFinder<Texture2D>.Get("UI/ranged");
        ammoTexture = ContentFinder<Texture2D>.Get("UI/ammo");
        nextTexture = ContentFinder<Texture2D>.Get("UI/next");
        previousTexture = ContentFinder<Texture2D>.Get("UI/previous");
        weaponStorageTexture = ContentFinder<Texture2D>.Get("UI/weaponstorage");
    }

    public AssignUI(Building_WeaponStorage weaponStorage, Pawn pawn = null)
    {
        this.weaponStorage = weaponStorage;
        PossibleWeapons = null;
        closeOnClickedOutside = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        selectablePawns = Util.GetPawns(true);
        UpdatePawnIndex(pawn);
    }

    public override Vector2 InitialSize => new Vector2(650f, 600f);

    private void RebuildPossibleWeapons()
    {
        if (PossibleWeapons != null)
        {
            PossibleWeapons.Clear();
            PossibleWeapons = null;
        }

        var num = weaponStorage?.Count ?? 0;
        if (assignedWeapons != null)
        {
            num += assignedWeapons.Count;
        }

        PossibleWeapons = new List<ThingWithComps>(num);
        if (assignedWeapons != null)
        {
            foreach (var weapon in assignedWeapons.Weapons)
            {
                PossibleWeapons.Add(weapon);
            }
        }

        if (weaponStorage == null)
        {
            return;
        }

        foreach (var weapon2 in weaponStorage.GetWeapons(false))
        {
            PossibleWeapons.Add(weapon2);
        }

        foreach (var bioEncodedWeapon in weaponStorage.GetBioEncodedWeapons())
        {
            PossibleWeapons.Add(bioEncodedWeapon);
        }
    }

    private bool IsAssignedWeapon(int i)
    {
        if (assignedWeapons != null)
        {
            return i < assignedWeapons.Count;
        }

        return false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        try
        {
            var num = 0f;
            var num2 = 0f;
            string text;
            if (Settings.EnableAssignWeapons)
            {
                Widgets.Label(new Rect(num, num2 + 4f, 100f, 30f), "WeaponStorage.AssignTo".Translate());
                num += 80f;
                if (selectablePawns.Count > 0 && GUI.Button(new Rect(num, num2, 30f, 30f), previousTexture))
                {
                    pawnIndex--;
                    if (pawnIndex < 0 || assignedWeapons == null)
                    {
                        pawnIndex = selectablePawns.Count - 1;
                    }

                    LoadAssignedWeapons();
                }

                num += 30f;
                text = assignedWeapons != null ? SelectablePawns.GetLabelAndStatsFor(assignedWeapons.Pawn) : "";
                if (Widgets.ButtonText(new Rect(num, num2, 400f, 30f), text))
                {
                    var list = new List<FloatMenuOption>();
                    foreach (var p in selectablePawns)
                    {
                        list.Add(new FloatMenuOption(p.LabelAndStats, delegate { UpdatePawnIndex(p.Pawn); }));
                    }

                    Find.WindowStack.Add(new FloatMenu(list));
                }

                num += 400f;
                if (selectablePawns.Count > 0 && GUI.Button(new Rect(num, num2, 30f, 30f), nextTexture))
                {
                    pawnIndex++;
                    if (pawnIndex >= selectablePawns.Count || assignedWeapons == null)
                    {
                        pawnIndex = 0;
                    }

                    LoadAssignedWeapons();
                }

                num2 += 40f;
            }

            num = 0f;
            Widgets.Label(new Rect(num, num2 - 4f, 100f, 60f), "WeaponStorage".Translate());
            num += 80f;
            if (WorldComp.HasStorages() && GUI.Button(new Rect(num, num2, 30f, 30f), previousTexture))
            {
                NextWeaponStorage(-1);
            }

            num += 30f;
            text = weaponStorage != null ? weaponStorage.Label : "";
            if (Widgets.ButtonText(new Rect(num, num2, 400f, 30f), text))
            {
                var list2 = new List<FloatMenuOption>();
                foreach (var ws in WorldComp.GetWeaponStorages())
                {
                    list2.Add(new FloatMenuOption(ws.Label, delegate
                    {
                        weaponStorage = ws;
                        RebuildPossibleWeapons();
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(list2));
            }

            num += 400f;
            if (WorldComp.HasStorages() && GUI.Button(new Rect(num, num2, 30f, 30f), nextTexture))
            {
                NextWeaponStorage(1);
            }

            num2 += 40f;
            Widgets.Label(new Rect(0f, num2 + 4f, 70f, 30f), "WeaponStorage.Search".Translate());
            textBuffer = Widgets.TextField(new Rect(80f, num2, 200f, 30f), textBuffer);
            num2 += 40f;
            var num3 = inRect.width - 100f;
            scrollPosition = GUI.BeginScrollView(new Rect(40f, num2, num3, inRect.height - num2 - 121f), scrollPosition,
                new Rect(0f, 0f, num3 - 16f, PreviousY));
            num2 = 0f;
            if (PossibleWeapons != null)
            {
                for (var i = 0; i < PossibleWeapons.Count; i++)
                {
                    num = 0f;
                    var thingWithComps = PossibleWeapons[i];
                    var checkOn = false;
                    if (assignedWeapons != null && weaponStorage != null)
                    {
                        checkOn = IsAssignedWeapon(i);
                        if (!checkOn && !IncludeWeapon(thingWithComps))
                        {
                            continue;
                        }

                        var checkWasOn = checkOn;
                        Widgets.Checkbox(num, num2, ref checkOn, 20f);
                        num += 22f;
                        if (checkOn != checkWasOn)
                        {
                            if (IsAssignedWeapon(i))
                            {
                                if (assignedWeapons.Pawn.equipment.Primary == thingWithComps)
                                {
                                    assignedWeapons.Pawn.equipment.Remove(thingWithComps);
                                    if (assignedWeapons.Pawn.jobs.curJob.def == JobDefOf.Hunt)
                                    {
                                        assignedWeapons.Pawn.jobs.StopAll();
                                    }
                                }

                                if (assignedWeapons.Remove(thingWithComps))
                                {
                                    if (weaponStorage == null || !weaponStorage.AddWeapon(thingWithComps) &&
                                        !WorldComp.Add(thingWithComps))
                                    {
                                        BuildingUtil.DropSingleThing(thingWithComps, assignedWeapons.Pawn.Position,
                                            assignedWeapons.Pawn.Map);
                                    }
                                }
                                else
                                {
                                    Log.Error($"Unable to remove weapon {thingWithComps}");
                                }
                            }
                            else if (weaponStorage != null && weaponStorage.RemoveNoDrop(thingWithComps))
                            {
                                assignedWeapons.Add(thingWithComps);
                            }
                            else
                            {
                                Log.Error($"Unable to remove weapon {thingWithComps}");
                            }

                            RebuildPossibleWeapons();
                            break;
                        }
                    }

                    if (!checkOn && !IncludeWeapon(thingWithComps))
                    {
                        continue;
                    }

                    Widgets.ThingIcon(new Rect(num, num2, 30f, 30f), thingWithComps);
                    num += 32f;
                    if (Widgets.InfoCardButton(num, num2, thingWithComps))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(thingWithComps));
                    }

                    num += 32f;
                    Widgets.Label(new Rect(num, num2, 250f, 30f), thingWithComps.Label);
                    num += 252f;
                    if (weaponStorage != null &&
                        Widgets.ButtonImage(new Rect(num3 - 16f - 30f, num2, 20f, 20f), DropTexture))
                    {
                        if (IsAssignedWeapon(i))
                        {
                            if (assignedWeapons != null && !assignedWeapons.Remove(thingWithComps))
                            {
                                Log.Error("Unable to drop assigned weapon");
                            }
                        }
                        else if (weaponStorage == null || !weaponStorage.Remove(thingWithComps))
                        {
                            Log.Error($"Unable to remove weapon {thingWithComps}");
                        }

                        RebuildPossibleWeapons();
                        break;
                    }

                    var comp = thingWithComps.GetComp<CompBiocodable>();
                    if (comp?.CodedPawn != null)
                    {
                        num2 += 26f;
                        Widgets.Label(new Rect(num - 250f - 2f, num2, 250f, 20f), comp.CompInspectStringExtra());
                        num2 += 4f;
                    }

                    PossibleWeapons[i] = thingWithComps;
                    num2 += 32f;
                }
            }
            else if (weaponStorage != null)
            {
                foreach (var weapon in weaponStorage.GetWeapons(false))
                {
                    if (!IncludeWeapon(weapon))
                    {
                        continue;
                    }

                    num = 34f;
                    Widgets.ThingIcon(new Rect(num, num2, 30f, 30f), weapon);
                    num += 32f;
                    if (Widgets.InfoCardButton(num, num2, weapon))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(weapon));
                    }

                    num += 32f;
                    Widgets.Label(new Rect(num, num2, 250f, 30f), weapon.Label);
                    num += 252f;
                    if (Widgets.ButtonImage(new Rect(num + 100f, num2, 20f, 20f), DropTexture))
                    {
                        weaponStorage.Remove(weapon);
                        break;
                    }

                    num2 += 32f;
                }

                foreach (var bioEncodedWeapon in weaponStorage.GetBioEncodedWeapons())
                {
                    num = 34f;
                    Widgets.ThingIcon(new Rect(num, num2, 30f, 30f), bioEncodedWeapon);
                    num += 32f;
                    if (Widgets.InfoCardButton(num, num2, bioEncodedWeapon))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(bioEncodedWeapon));
                    }

                    num += 32f;
                    Widgets.Label(new Rect(num, num2, 250f, 30f), bioEncodedWeapon.Label);
                    num += 252f;
                    if (Widgets.ButtonImage(new Rect(num + 100f, num2, 20f, 20f), DropTexture))
                    {
                        weaponStorage.Remove(bioEncodedWeapon);
                        break;
                    }

                    var comp2 = bioEncodedWeapon.GetComp<CompBiocodable>();
                    if (comp2?.CodedPawn != null)
                    {
                        num2 += 26f;
                        Widgets.Label(new Rect(num - 250f - 2f, num2, 250f, 20f), comp2.CompInspectStringExtra());
                        num2 += 4f;
                    }

                    num2 += 32f;
                }
            }

            GUI.EndScrollView();
            PreviousY = num2;
        }
        catch (Exception ex)
        {
            var text2 = $"{GetType().Name} closed due to: {ex.GetType().Name} {ex.Message}";
            Log.Error(text2);
            Messages.Message(text2, MessageTypeDefOf.NegativeEvent);
            base.Close();
        }
        finally
        {
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    private void NextWeaponStorage(int increment)
    {
        var weaponStorages = WorldComp.GetWeaponStorages();
        if (weaponStorage == null)
        {
            weaponStorage = weaponStorages[increment < 0 ? weaponStorages.Count - 1 : 0];
        }
        else
        {
            for (var i = 0; i < weaponStorages.Count; i++)
            {
                if (weaponStorage != weaponStorages[i])
                {
                    continue;
                }

                i += increment;
                if (i < 0)
                {
                    weaponStorage = weaponStorages[weaponStorages.Count - 1];
                }
                else if (i >= weaponStorages.Count)
                {
                    weaponStorage = weaponStorages[0];
                }
                else
                {
                    weaponStorage = weaponStorages[i];
                }

                break;
            }
        }

        RebuildPossibleWeapons();
    }

    private void UpdatePawnIndex(Pawn p)
    {
        pawnIndex = -1;
        if (p == null)
        {
            return;
        }

        for (pawnIndex = 0; pawnIndex < selectablePawns.Count; pawnIndex++)
        {
            if (!selectablePawns[pawnIndex].Pawn.Equals(p))
            {
                continue;
            }

            LoadAssignedWeapons();
            break;
        }
    }

    private void LoadAssignedWeapons()
    {
        var selectablePawn = selectablePawns[pawnIndex];
        if (!WorldComp.TryGetAssignedWeapons(selectablePawn.Pawn, out assignedWeapons))
        {
            assignedWeapons = new AssignedWeaponContainer
            {
                Pawn = selectablePawn.Pawn
            };
            if (selectablePawn.Pawn.equipment.Primary != null)
            {
                assignedWeapons.Add(selectablePawn.Pawn.equipment.Primary);
            }

            WorldComp.AddAssignedWeapons(selectablePawn.Pawn, assignedWeapons);
        }

        RebuildPossibleWeapons();
    }

    private bool IncludeWeapon(ThingWithComps weapon)
    {
        if (textBuffer.Length <= 0)
        {
            return true;
        }

        var text = textBuffer.ToLower();
        if ((text.StartsWith("mel") || text.StartsWith("mee")) && weapon.def.IsMeleeWeapon)
        {
            return true;
        }

        if (text.StartsWith("ran") && weapon.def.IsRangedWeapon)
        {
            return true;
        }

        return weapon.Label.ToLower().Contains(text);
    }
}