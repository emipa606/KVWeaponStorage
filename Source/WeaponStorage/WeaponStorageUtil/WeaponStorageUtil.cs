using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace WeaponStorageUtil;

internal class WeaponStorageUtil
{
    private static Assembly wsAssembly;

    private static bool initialized;

    public static bool Exists
    {
        get
        {
            if (initialized)
            {
                return wsAssembly != null;
            }

            foreach (var runningMod in LoadedModManager.RunningMods)
            {
                foreach (var loadedAssembly in runningMod.assemblies.loadedAssemblies)
                {
                    if (!loadedAssembly.GetName().Name.Equals("WeaponStorage") ||
                        loadedAssembly.GetType("WeaponStorage.WorldComp") == null)
                    {
                        continue;
                    }

                    initialized = true;
                    wsAssembly = loadedAssembly;
                    break;
                }

                if (initialized)
                {
                    break;
                }
            }

            initialized = true;

            return wsAssembly != null;
        }
    }

    public static bool TryEquipType(Pawn p, ThingDef def)
    {
        if (!Exists)
        {
            return false;
        }

        try
        {
            if (wsAssembly.GetType("WeaponStorage.WorldComp")
                    .GetField("AssignedWeapons", BindingFlags.Static | BindingFlags.Public)
                    ?.GetValue(null) is IDictionary dictionary)
            {
                var obj = dictionary[p];
                if (obj?.GetType().GetField("Weapons", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj) is
                    List<ThingWithComps> list)
                {
                    foreach (var item in list)
                    {
                        if (item.def != def)
                        {
                            continue;
                        }

                        wsAssembly.GetType("WeaponStorage.HarmonyPatchUtil")
                            .GetMethod("EquipWeapon", BindingFlags.Static | BindingFlags.Public)
                            ?.Invoke(null, [item, p, obj]);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }

        return false;
    }
}