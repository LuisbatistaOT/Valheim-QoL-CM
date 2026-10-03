# Technical plan — spec 001

Version 0.001. The spec in this folder is frozen.

## Technology stack

- Language: C#
- Plugin runtime: BepInEx 5.4.23.5, `net472`
- Library: Jötunn 2.30.1 (`com.jotunn.jotunn`) for the wood panel, the rebindable hotkey, admin status, and admin-only config sync
- Domain library: `netstandard2.0`, tested with xUnit
- Game: `F:\SteamLibrary\steamapps\common\Valheim`
- Packages: restored under `.nuget/packages` in this repository

## Runtime authority

The world host is the authority. Jötunn reports `SynchronizationManager.PlayerIsAdmin`, which is true for the local host and for server admins. The skill-loss percent is a BepInEx config entry marked admin-only so Jötunn copies the server value onto clients. Clients send teleport, spawn, and grant-admin requests to the host. Mode toggles change only the admin's own character.

## Projects and files

- `src/ValheimQoLCM.Core/ValheimQoLCM.Core.csproj`
- `src/ValheimQoLCM.Core/SkillLoss.cs`
- `src/ValheimQoLCM.Core/AdminGate.cs`
- `src/ValheimQoLCM.Core/ModeToggles.cs`
- `src/ValheimQoLCM.Core/SpawnValidation.cs`
- `src/ValheimQoLCM.Core/TeleportSelection.cs`
- `src/ValheimQoLCM.Core/SteamId.cs`
- `src/ValheimQoLCM.Core/ActionResult.cs`
- `src/ValheimQoLCM/ValheimQoLCM.csproj`
- `src/ValheimQoLCM/Plugin.cs`
- `src/ValheimQoLCM/ConsoleManager.cs`
- `src/ValheimQoLCM/ConsoleView.cs`
- `src/ValheimQoLCM/AdminCommands.cs`
- `src/ValheimQoLCM/GameplayModifiers.cs`
- `src/ValheimQoLCM/ItemSpawner.cs`
- `src/ValheimQoLCM/DeathPenaltyManager.cs`
- `tests/ValheimQoLCM.Core.Tests/ValheimQoLCM.Core.Tests.csproj`
- `tests/ValheimQoLCM.Core.Tests/CoreRulesTests.cs`
- `docs/USAGE.md`
- `ValheimQoLCM.sln`
- `nuget.config`

## Behavior mapping

- God, fly, creative, and free cam call the same player and camera flags the vanilla console uses. Each flag is independent.
- Teleport fills its list from connected peers plus the local player. Actions call `Player.TeleportTo` on the host.
- Spawn instantiates the vanilla prefab at the selected player's feet and sets stack and quality. Quantity outside 1 through 100 spawns nothing.
- Death penalty prefixes the vanilla skill-loss call while a player death is in progress and substitutes `percent / 100`.
- Grant admin reads the Steam ID from the peer connection. The typed field appears only when that ID is missing. The host adds a well-formed ID to the vanilla admin list.

## Deploy

Build output stays in this repository. Copy `ValheimQoLCM.dll`, `ValheimQoLCM.Core.dll`, and their PDB files to `F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\ValheimQoLCM\`.
