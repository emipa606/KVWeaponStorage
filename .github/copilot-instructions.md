# GitHub Copilot Instructions for [KV] Weapon Storage (Continued)

## Mod Overview and Purpose

**Mod Name:** [KV] Weapon Storage (Continued)  
**Author:** Kiame Vivacity  
**Description:** An update to Kiame Vivacity's original mod, this mod provides advanced weapon storage and repair functionalities in RimWorld. It allows weapons to be assigned to pawns for use in the field and includes a repair attachment to automatically repair stored weapons.

## Key Features and Systems

### Weapon Storage
- **Build Menu:** Misc
- **Size:** 1x1
- **Cost:** 40 steel + 40 wood/stone/steel
- **Stores:** Ranged and melee weapons

### Repair Attachment
- **Build Menu:** Misc
- **Size:** 1x1
- **Cost:** 250 steel + 6 components + 20 wood/stone/steel
- **Energy:** 10 W (not in use) or 1000 W (in use)
- **Functionality:** Automatically repairs weapons within attached Weapon Storage containers
- **Research Required:** Mending & Multi-Analyzer
- **Compatibility:** Works without the Mending mod, but highly recommended

## Coding Patterns and Conventions

### General Conventions
- **Mod Structure:** The mod is organized with various subdirectories for different functionalities, such as `WeaponStorage` and `MendingWeaponStoragePatch`.
- **File Naming:** C# files are named to reflect their functionality (e.g., `HarmonyPatches_Mending.cs`, `WorkGiver_DoBill_TryFindBestBillIngredients.cs`).
- **Namespace:** All C# files are under the `KVWeaponStorage` namespace.

### XML Integration
- **Def Files:** XML files are used to define game elements such as `ThingDef` and `ResearchProjectDef`.
- **Location:** Def files are located in the `Defs` directory within the mod's folder structure.
- **Naming:** XML files are named to reflect their content (e.g., `ResearchProjects.xml`, `Furniture.xml`).

### Harmony Patching
- **Harmony Library:** The mod uses the Harmony library for patching vanilla RimWorld code.
- **Patch Files:** Patch files are located in the `HarmonyPatches` directory within each subdirectory.
- **Patch Structure:** Each patch file contains a class with methods that are prefixed or postfixed to the target methods in the vanilla game.

## Harmony Patching Examples

### Example 1: Prefix Patch
csharp
public class CaravanExitMapUtility_ExitMapAndCreateCaravan
{
    public static void Prefix(Caravan caravan, ref bool ___canExitMap)
    {
        // Custom logic to modify the behavior of the ExitMapAndCreateCaravan method
        ___canExitMap = true;
    }
}


### Example 2: Postfix Patch
csharp
public class CaravanFormingUtility_StopFormingCaravan
{
    public static void Postfix(Caravan caravan)
    {
        // Custom logic to execute after the StopFormingCaravan method
        caravan.AddMessage("Caravan formation cancelled.");
    }
}


## Suggestions for Copilot

### XML File Assistance
- **Def File Creation:** Request Copilot to generate new `ThingDef` or `ResearchProjectDef` XML files based on specific requirements.
- **Def File Modifications:** Ask Copilot to help modify existing def files to include additional attributes or change existing ones.

### C# File Assistance
- **Harmony Patch Generation:** Request Copilot to generate new harmony patches for specific methods in the vanilla game.
- **Utility Method Creation:** Ask Copilot to help create utility methods for common functionalities within the mod.

### General Assistance
- **Mod Integration:** Request Copilot to help integrate new features or systems within the existing mod structure.
- **Debugging and Error Handling:** Ask Copilot to provide suggestions for debugging and handling potential errors within the mod.

By following these guidelines, you can effectively use GitHub Copilot to enhance your modding experience for [KV] Weapon Storage (Continued).

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Temporary files

- Do not create temporary files, build output, test artifacts, or Copilot session
  folders inside the mod root.
- Store temporary artifacts under `%USERPROFILE%\CopilotTemp\[modname]`.
- Direct build and test output there as well, without changing project settings.
- Clean up task-specific temporary artifacts when finished.

## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

