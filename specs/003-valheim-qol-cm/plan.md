# Technical plan — spec 003

Version 0.3. The spec in this folder is frozen. Spec 001 gameplay stays unchanged.

## Change

Rebuild the wood panel in `src/ValheimQoLCM/ConsoleView.cs` as a two-by-two grid of wood modules.

- Player Management: connected-player rows, then bring-me, bring-player, and grant admin. Those three stay gray with no selection and when the highlighted row is the local player.
- Global Cheats: God, Fly, Creative, and Free cam in a two-by-two grid. They stay clickable.
- Item Spawner: the filter on the top edge of a fixed-height scrolling list, then quantity and quality as fixed number boxes between minus and plus, then Spawn. Those controls stay gray until an item row is highlighted.
- Server skill loss: slider, fixed-width percent, and Apply skill loss. The percent remains the server-wide death penalty.
- The action console stays along the bottom.

## Files

- `src/ValheimQoLCM/ConsoleView.cs`
- `src/ValheimQoLCM/Plugin.cs` for the version string `0.3`
- `src/ValheimQoLCM/ValheimQoLCM.csproj`
- `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- `docs/USAGE.md`
- `README.md`
- `test/features/panel-layout.feature`

No new Core rules. `dotnet test` still covers spec 001.
