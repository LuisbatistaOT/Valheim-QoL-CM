# Future spec backlog

`@pm` owns this file. Items here are not in a frozen spec.

When a request, issue, or feature does not belong in the current spec, record it in the matching section. When a new spec is drafted, review every open item and move in only the ones that share that spec's application area. Leave the rest here.

## User stories

| ID | Area | Story | Why it waited | Status |
| --- | --- | --- | --- | --- |

## Defects

| ID | Area | Defect | Related spec | Why it waited | Status |
| --- | --- | --- | --- | --- | --- |
| DEF-001 | Build | `dotnet build ValheimQoLCM.sln -warnaserror` fails. `src/ValheimQoLCM` is a decompile with nullable warnings (`CS8600`, `CS8625`) and Core generates `CS1591` for public members without XML docs. The master push gate requires a clean build. | 008 | Spec 008 froze on World tab behavior. Cleaning the recovered source is its own change and must not alter behavior. | Open. Blocks landing `spec-008-world-modifiers` on `master`. |
| DEF-002 | Server | Hosted-server play for specs 006 through 008 is unproven: remote admin Apply, clients receiving keys and the overwrite flag, a client without the plugin, Kill enemies, and 0% skill loss after relog. | 006, 007, 008 | This machine has no dedicated-server executable. | Open. |
| DEF-003 | Storage | The old `qol-cm-skill.txt` percent file is ignored but never deleted. | 008 | Deleting a file the player may still want is a decision for a spec. | Open. |
| DEF-004 | Release | The committed `release/ValheimQoLCM/*.pdb` files (1.6) embed the build machine's profile path and a source-link map. The security check in the push gate blocks profile paths. They are already on `master`. | 006 | Found by the spec 008 freeze scan. Rewriting published artifacts is a release decision. Build 1.8 release files with `-p:DebugType=none` or `-p:PathMap`, or stop committing PDBs. | Open. |

## Features

| ID | Area | Feature | Why it waited | Status |
| --- | --- | --- | --- | --- |
