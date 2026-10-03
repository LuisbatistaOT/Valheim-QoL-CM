# AI Agents and write boundaries

Read `lessons-learned.md` in `C:\Users\batis\OneDrive\Obsidian Vault\Fuente de Ingresos\Fuente de Ingresos\Projects\Valheim Plugins\QoL CM` before changing code or diagnosing a failure. If that file does not exist yet, record that the spec 001 cycle has not closed.

## Personas

- `@dev-lead` implements the C# in the technical plan and the unit tests that prove the Core rules.
- `@ui-architect` owns the in-game wood panel layout, labels, and click targets. The panel runs inside Valheim through Jötunn. Desktop UI frameworks are out of scope.
- `@code-sentinel` blocks cycle closeout until `dotnet test` passes, every new defect cites a spec id, public types have XML docs, and the panel stays a view over the gameplay services.

## Dev environment

- Run `dotnet test ValheimQoLCM.sln` from the repository root.
- Copy only `ValheimQoLCM.dll` and its PDB into `F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins\ValheimQoLCM`.
- Keep API results that cross a process boundary shaped as `{ data, error }` inside the Core action results.

## Forbidden write boundaries

- Do not edit Valheim game files outside `F:\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins`.
- Do not add features that are absent from the frozen spec.
- Do not add ads, bans, or online services.
- Implement only the files named in `specs/001-valheim-qol-cm/plan.md`.
