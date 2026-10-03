# Technical plan — spec 002

Version 0.2. The spec in this folder is frozen. Spec 001 gameplay stays unchanged. The version string is `0.2` because BepInEx rejects the padded form `0.002`.

## Change

Rearrange the wood panel in `src/ValheimQoLCM/ConsoleView.cs`.

- Left column: connected-player rows, then the item filter, then item rows.
- A clicked player row and a clicked item row use a highlight color. Other rows stay plain.
- Right column: mode buttons, teleport, quantity, quality, spawn, skill loss, and grant admin.
- Bottom: action lines append in a console. `ConsoleManager.Show` still calls `SetStatus`. Closing the panel does not clear those lines.

## Files

- `src/ValheimQoLCM/ConsoleView.cs`
- `src/ValheimQoLCM/Plugin.cs` for the version string `0.2`
- `docs/USAGE.md`
- `test/features/panel-layout.feature`

No new Core rules. `dotnet test` still covers spec 001.
