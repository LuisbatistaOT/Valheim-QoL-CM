# Issues — spec 008

Spec 008 is frozen at `1.8`. This file replaces the issues list that was lost with the working tree on 2026-10-09. Play notes are from the local world. `dotnet test` does not open Valheim.

| Id | Spec | Area | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-001 | REQ-7 | Version | The in-game header stayed on 1.7 after a local build. BepInEx loads the plugin folder, not the repository output. | Copy `ValheimQoLCM.dll`, `ValheimQoLCM.pdb`, `ValheimQoLCM.Core.dll`, and `ValheimQoLCM.Core.pdb` into the game plugin folder after a successful build. The panel then showed 1.8. |
| ISS-002 | REQ-1 | Tabs | Clicking any tab other than Cheats did nothing. `PaintWorld` threw `IndexOutOfRangeException` while the panel was built. | Tab bodies are built before the first paint. The five tabs switch. |
| ISS-003 | REQ-5 | Skill overwrite | With overwrite applied, a death still dropped a skill from 95 to 90. That is the vanilla 5% step (`level * 0.05`). | `DeathPenaltyManager` zeroes the death factor while overwrite is on, including Hardcore, and restores the saved level if vanilla code already changed it. Deaths on 2026-10-10 logged `Skill overwrite death kept.` |
| ISS-004 | REQ-4 | Apply | After overwrite was on, choosing another preset or moving a slider and pressing Apply left the live death penalty unchanged. `ZoneSystem.SetGlobalKey(string)` only sends an RPC. | The host writes with the private global-key add and remove methods, then calls `UpdateWorldRates` and `SendGlobalKeys` for the local peer. Later logins still showed the written keys. |
| ISS-005 | REQ-2, REQ-3 | Slider value | Preset clicks and Apply wrote the right step names in `qol-cm.log`, but the marks and the words beside them did not follow. | Symptom of ISS-011. The draft and the save were right; paint never reached the live rows. |
| ISS-006 | REQ-2 | Slider position | Marks sat at the far left, including after logout and login. | Symptom of ISS-011. The zero-width and `normalizedValue` theories were real Unity facts but not this cause. |
| ISS-007 | REQ-2 | Step names | Assigning `Text.text` on the readout did not change the glyphs. A `Canvas.willRenderCanvases` hook froze text meshes and does not compile without `UnityEngine.UIModule`. | Symptom of ISS-011. The text that was written belonged to a destroyed object. The hook is gone. |
| ISS-008 | REQ-2 | Slider mark | The moving mark covered the row above or below. Unity stretches the handle across the handle area and adds `sizeDelta.y`; Jötunn sets that to `(40, 10)`. | The handle area is a fixed 12px strip, centered, and the handle delta is `(14, 0)`. Checked 2026-10-10 22:23: the mark stays inside its row. |
| ISS-009 | REQ-2, REQ-3 | Step names | Every readout said "Normal" while the save held other steps. | Symptom of ISS-011. The readout is now written in place and its mesh rebuilt, which is correct but was not the cause. Checked 2026-10-10 22:23: `World readout Easy, Casual, Normal, Less, Normal`. |
| ISS-010 | REQ-4 | Reload | Apply, quit, and login again left the handles at the leftmost stop while the debug line already read the saved steps. | Symptom of ISS-011. A late update also places the handle again when the value or anchor is off the step, because disabling the page releases the driven anchors. Checked 2026-10-10 22:23: marks `1@0.25, 0@0.00, 2@0.40, 2@0.40, 1@0.33`. |
| ISS-011 | REQ-2, REQ-3, REQ-4 | Panel rebuild | The 2026-10-10 22:10 login logged `World readout , , , , , Normal, Normal, Normal, Normal, Normal.` Ten readouts for five rows. Jötunn raises `OnCustomGUIAvailable` in the menu scene and again in the world scene. The scene change destroyed the first panel before the second build, so `Build` skipped clearing the row lists and appended the new rows behind the dead ones. Every paint loop stops at five and painted the dead rows. Drags still worked because the listener sits on the live slider. Root cause of ISS-005 through ISS-010. | `Build` clears every row list on each call. The `World readout` line prints the row count and each mark as `value@anchor`. Checked 2026-10-10 22:23: `Rows 5`. |

## Confirmed in play, 2026-10-10

- Header reads version 1.8. The Cheats tab has no percent skill-loss slider.
- Cheats, World, Spawn, Players, and Log all open.
- Login shows the saved steps on the sliders and readouts. `Rows 5`, marks on their fractions.
- Preset clicks move the five marks and readouts. Apply writes the keys and the action log line.
- Quit and login again keeps the applied steps on the World tab.
- A death with overwrite on logged `Skill overwrite death kept.`

## Not confirmed

Hosted-server play: a remote admin applying, clients receiving keys and the overwrite flag, and a client without the plugin. This machine has no dedicated-server executable.
