# .github/copilot-instructions.md

## Mod Overview and Purpose

This RimWorld mod extends the functionality of weapon storage and mending mechanics. It aims to provide players with enhanced control over managing weapon inventories, repairing damaged weapons, and integrating seamlessly with other game systems like colonist activities and caravan formations. The mod enhances gameplay by introducing nuanced systems for weapon storage and handling, complementing the base game’s mechanics with robust, user-controllable options for weapon management.

## Key Features and Systems

- **Weapon Storage**: Allows the storage of weapons in designated storage buildings, with sorting and retrieval mechanisms.
- **Weapon Mending**: Introduces a system for repairing weapons, either automatically or through player-driven interactions.
- **Enhanced Integration**: Works within existing game systems to facilitate improved caravan loading, pawn equipment management, and trade interactions.
- **UI Enhancements**: Adds user interface components for weapon assignment, storage management, and weapon filtering.
- **Compatibility with Mods**: Designed to be compatible with other mods, especially those altering or extending equipment and storage.

## Coding Patterns and Conventions

- **C# Access Modifiers**: Internal and public classes are utilized for clear separation between public interfaces and internal logic.
- **Method Naming**: Methods are named verbosely to indicate their functionality clearly, e.g., `TryGetLastThingUsed`, `RebuildPossibleWeapons`.
- **Interfaces**: Implements `IExposable` for classes that need to integrate with RimWorld's data saving/loading mechanics.
- **Static Util Classes**: Utility functions are organized into static classes (e.g., `Util`, `TradeUtil`) for code reuse.

## XML Integration

Although XML specifics are not detailed, the mod likely integrates XML for defining game configurations such as:

- **Defs for Weapons and Buildings**: Configurations to introduce new items or modify existing ones.
- **Patch XMLs**: Utilized for altering game behavior and introducing new functionality via assembly patches.
- **UI Layouts**: XML could define layouts or data that feed into UI components for modifications.

## Harmony Patching

The mod makes extensive use of Harmony to patch various game behaviors. Important classes and systems patched include:

- **WeaponHandling Patches**: Modify how weapons are handled during pawn actions or building interactions (e.g. `Patch_Pawn_EquipmentTracker_TryDropEquipment`).
- **Caravan Formation Patches**: Adjust the mechanics of how weapons are included in caravans (`Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan`).
- **Game UI Patches**: Alter UI elements or add new functionalities (`Patch_Dialog_FormCaravan_PostOpen`).

## Suggestions for Copilot

When using GitHub Copilot in this mod development, consider utilizing it for:

- **Boilerplate Code**: Generating basic structures for new classes and methods, especially when creating additional storage systems or UI elements.
- **Method Stubbing**: Quickly drafting new methods when expanding functionality, e.g., new ways to filter or handle weapons.
- **XML Definitions**: Suggesting initial templates for new XML defs that could be refined into working content configurations.
- **Harmony Patches**: Assisting with the initial setup of Harmony patches for newly identified needs or game features.

By adhering to these guidelines and leveraging Copilot effectively, contributors can enhance productivity and maintain a high standard of code quality in extending RimWorld's modding capabilities.
