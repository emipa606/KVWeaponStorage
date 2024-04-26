using Verse;

namespace WeaponStorage.UI;

internal struct WeaponSelected(ThingWithComps thing, bool isChecked)
{
    public ThingWithComps thing = thing;

    public bool isChecked = isChecked;
}