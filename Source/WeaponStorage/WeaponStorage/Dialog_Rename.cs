namespace WeaponStorage;

internal class Dialog_Rename : Verse.Dialog_Rename
{
    private readonly Building_WeaponStorage WeaponStorage;

    public Dialog_Rename(Building_WeaponStorage weaponStorage)
    {
        WeaponStorage = weaponStorage;
        curName = weaponStorage.Name;
    }

    public override void SetName(string name)
    {
        WeaponStorage.Name = name;
    }
}