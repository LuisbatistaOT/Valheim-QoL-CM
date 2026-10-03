# Technical plan — spec 004

Version 0.4. The spec in this folder is frozen. Spec 001 gameplay and the spec 003 module layout stay unchanged.

## Change

- Selecting an item sets quantity and quality to 1. Max Stack and `x10` keep their current jobs.
- `ItemListing` decides the row name and whether an icon count is enough to pick the drop up. `Humanoid.Pickup` rejects an empty `m_icons` list, so those prefabs never enter the catalog.
- The catalog keeps the `ObjectDB` prefab Spawn instantiates for that name. Shared in-game names get the prefab appended. The console repeats the row name.
- The panel, plugin, and usage doc show `0.4`.

## Files

- `src/ValheimQoLCM.Core/ItemListing.cs`
- `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- `src/ValheimQoLCM/ItemSpawner.cs`
- `src/ValheimQoLCM/ConsoleView.cs`
- `src/ValheimQoLCM/Plugin.cs`
- `src/ValheimQoLCM/ValheimQoLCM.csproj`
- `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`
- `docs/USAGE.md`
- `README.md`
- `test/features/item-list.feature`
