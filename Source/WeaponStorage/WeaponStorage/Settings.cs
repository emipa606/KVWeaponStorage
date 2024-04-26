using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WeaponStorage;

public class Settings : ModSettings
{
    private const float FIRST_COLUMN_WIDTH = 250f;

    private const float SECOND_COLUMN_X = 260f;

    private const int DEFAULT_REPAIR_SPEED = 1;

    private const float DEFAULT_REPAIR_UPDATE_INTERVAL = 5f;

    public static bool ShowWeaponsWhenNotDrafted;

    public static bool AutoSwitchMelee = true;

    public static readonly int RepairAttachmentDistance = 6;

    public static PreferredDamageTypeEnum PreferredDamageType = PreferredDamageTypeEnum.ArmorSharp;

    public static int RepairAttachmentMendingSpeed = 1;

    private static string RepairAttachmentMendingSpeedBuffer = 1.ToString();

    public static float RepairAttachmentUpdateInterval = 5f;

    private static string repairAttachmentUpdateIntervalBuffer = 5f.ToString();

    public static bool AllowPawnsToDropWeapon = true;

    public static bool PlaceDroppedWeaponsInStorage = true;

    public static bool ShowWeaponStorageButtonForPawns = true;

    public static bool EnableAssignWeapons = true;

    public static long RepairAttachmentUpdateIntervalTicks => (long)(RepairAttachmentUpdateInterval * 10000000f);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ShowWeaponsWhenNotDrafted, "WeaponStorage.ShowWeaponsWhenNotDrafted");
        Scribe_Values.Look(ref RepairAttachmentMendingSpeed, "WeaponStorage.RepairAttachmentHpPerTick", 1);
        RepairAttachmentMendingSpeedBuffer = RepairAttachmentMendingSpeed.ToString();
        Scribe_Values.Look(ref RepairAttachmentUpdateInterval, "WeaponStorage.RepairAttachmentUpdateInterval", 5f);
        repairAttachmentUpdateIntervalBuffer = $"{RepairAttachmentUpdateInterval:0.0###}";
        Scribe_Values.Look(ref AutoSwitchMelee, "WeaponStorage.AutoSwitchMelee", true);
        Scribe_Values.Look(ref PreferredDamageType, "WeaponStorage.PreferredDamageType",
            PreferredDamageTypeEnum.ArmorSharp);
        Scribe_Values.Look(ref AllowPawnsToDropWeapon, "WeaponStorage.AllowPawnsToDropWeapon", true);
        Scribe_Values.Look(ref PlaceDroppedWeaponsInStorage, "WeaponStorage.PlaceDroppedWeaponsInStorage", true);
        Scribe_Values.Look(ref ShowWeaponStorageButtonForPawns, "WeaponStorage.ShowButtonForPawns", true);
        Scribe_Values.Look(ref EnableAssignWeapons, "EnableAssignWeapons", true);
    }

    public static void DoSettingsWindowContents(Rect rect)
    {
        var num = 40f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.ShowButtonForPawns".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref ShowWeaponStorageButtonForPawns);
        num += 32f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.ShowWeaponsWhenNotDrafted".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref ShowWeaponsWhenNotDrafted);
        num += 32f;
        num += 20f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.RepairAttachmentSettings".Translate());
        num += 32f;
        NumberInput(ref num, "WeaponStorage.SecondsBetweenTicks", ref RepairAttachmentUpdateInterval,
            ref repairAttachmentUpdateIntervalBuffer, 5f, 0.25f, 120f);
        NumberInput(ref num, "WeaponStorage.HPPerTick", ref RepairAttachmentMendingSpeed,
            ref RepairAttachmentMendingSpeedBuffer, 1, 1, 60);
        num += 20f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.AllowPawnsToDropWeapon".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref AllowPawnsToDropWeapon);
        num += 32f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.PlaceDroppedWeaponsInStorage".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref PlaceDroppedWeaponsInStorage);
        num += 32f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.EnableAssignWeapons".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref EnableAssignWeapons);
        Widgets.Label(new Rect(310f, num, 200f, 30f), "WeaponStorage.UnassignAllWeaponsFirst".Translate());
        num += 32f;
        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.AutoSwitchMeleeForTarget".Translate());
        Widgets.Checkbox(new Vector2(260f, num + 4f), ref AutoSwitchMelee);
        num += 32f;
        if (!AutoSwitchMelee)
        {
            return;
        }

        Widgets.Label(new Rect(0f, num, 250f, 30f), "WeaponStorage.PreferredDamageType".Translate());
        if (!Widgets.ButtonText(new Rect(260f, num, 100f, 30f), PreferredDamageType.ToString().Translate()))
        {
            return;
        }

        var list = new List<FloatMenuOption>();
        if (PreferredDamageType != 0)
        {
            list.Add(new FloatMenuOption(PreferredDamageTypeEnum.WeaponStorage_None.ToString().Translate(),
                delegate { PreferredDamageType = PreferredDamageTypeEnum.WeaponStorage_None; }));
        }

        if (PreferredDamageType != PreferredDamageTypeEnum.ArmorBlunt)
        {
            list.Add(new FloatMenuOption(PreferredDamageTypeEnum.ArmorBlunt.ToString().Translate(),
                delegate { PreferredDamageType = PreferredDamageTypeEnum.ArmorBlunt; }));
        }

        if (PreferredDamageType != PreferredDamageTypeEnum.ArmorSharp)
        {
            list.Add(new FloatMenuOption(PreferredDamageTypeEnum.ArmorSharp.ToString().Translate(),
                delegate { PreferredDamageType = PreferredDamageTypeEnum.ArmorSharp; }));
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    public static bool IsTool(Thing t)
    {
        return t != null && IsTool(t.def);
    }

    public static bool IsTool(ThingDef def)
    {
        return def?.defName.StartsWith("RTFTJ") ?? false;
    }

    private static void NumberInput(ref float y, string label, ref float val, ref string buffer, float defaultVal,
        float min, float max)
    {
        try
        {
            Widgets.Label(new Rect(20f, y, 250f, 30f), label.Translate());
            Widgets.TextFieldNumeric(new Rect(280f, y, 50f, 30f), ref val, ref buffer, min, max);
            if (Widgets.ButtonText(new Rect(340f, y, 100f, 30f), "ResetButton".Translate()))
            {
                val = defaultVal;
                buffer = $"{defaultVal:0.0###}";
            }
        }
        catch
        {
            val = min;
            buffer = $"{min:0.0###}";
        }

        y += 32f;
    }

    private static void NumberInput(ref float y, string label, ref int val, ref string buffer, int defaultVal, int min,
        int max)
    {
        try
        {
            Widgets.Label(new Rect(20f, y, 250f, 30f), label.Translate());
            Widgets.TextFieldNumeric(new Rect(280f, y, 50f, 30f), ref val, ref buffer, min, max);
            if (Widgets.ButtonText(new Rect(340f, y, 100f, 30f), "ResetButton".Translate()))
            {
                val = defaultVal;
                buffer = defaultVal.ToString();
            }
        }
        catch
        {
            val = min;
            buffer = min.ToString();
        }

        y += 32f;
    }
}