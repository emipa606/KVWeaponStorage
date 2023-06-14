using UnityEngine;
using Verse;

namespace WeaponStorage;

public class SettingsController : Mod
{
    public SettingsController(ModContentPack content)
        : base(content)
    {
        GetSettings<Settings>();
    }

    public override string SettingsCategory()
    {
        return "WeaponStorage".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}