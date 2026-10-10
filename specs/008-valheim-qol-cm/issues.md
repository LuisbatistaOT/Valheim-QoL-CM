# Issues — spec 008

World tab work is not frozen and is not on `master`. This file replaces the issues list that was lost with the working tree on 2026-10-09. Play notes below are from the local world only. `dotnet test` does not open Valheim.

| Id | Area | What happened | Resolution |
| --- | --- | --- | --- |
| ISS-001 | Version | The in-game header stayed on 1.7 after a local build. BepInEx loads the plugin folder, not the repository output. | Copy `ValheimQoLCM.dll`, `ValheimQoLCM.pdb`, `ValheimQoLCM.Core.dll`, and `ValheimQoLCM.Core.pdb` into the game plugin folder after a successful build. The panel then showed 1.8. |
| ISS-002 | Tabs | Clicking any tab other than Cheats did nothing. `PaintWorld` threw `IndexOutOfRangeException` while the panel was built. | Tab bodies are built before the first paint. The five tabs switch. |
| ISS-003 | Skill overwrite | With overwrite applied, a death still dropped a skill from 95 to 90. That is the vanilla 5% step (`level * 0.05`). | `DeathPenaltyManager` zeroes the death factor while overwrite is on, including Hardcore, and restores the saved level if vanilla code already changed it. A later death logged `Skill overwrite death kept.` |
| ISS-004 | Apply | After overwrite was on, choosing another preset or moving a slider and pressing Apply left the live death penalty unchanged. `ZoneSystem.SetGlobalKey(string)` only sends an RPC. | The host writes with the private global-key add and remove methods, then calls `UpdateWorldRates` and `SendGlobalKeys` for the local peer. Later logins still showed the written keys. |
| ISS-005 | Slider value | Preset clicks and Apply wrote the right step names in `qol-cm.log`, but the marks and the words beside them did not follow. | The draft and the world save were already updating. The remaining bug is the visible mark and the readout. See ISS-008 and ISS-009. |
| ISS-006 | Slider position | Marks sat at the far left, including after logout and login. A zero-width slider makes every fraction land on the left, and `normalizedValue` is 0 when min and max are equal. | Do not place the mark from `rect.width` sampled before layout. The current handle is the Unity slider handle. Reload position is still wrong in play. See ISS-010. |
| ISS-007 | Step names | Assigning `Text.text` on the existing readout did not change the glyphs. A `Canvas.willRenderCanvases` hook froze text meshes and does not compile without `UnityEngine.UIModule`. | That hook is gone. The readout is replaced with a new text object whose constructor string is the step name. Play on 2026-10-10 still showed "Normal" for every row. See ISS-009. |
| ISS-008 | Slider mark | The visible orange piece was a separate knob created at anchor 0.5. The real handle was invisible, so a drag could change the draft while the mark stayed centered. A later build made the handle itself the mark. It moves, and it is tall enough to cover the row above or below. Unity stretches the handle vertically and adds `sizeDelta.y`, so a positive height overflows the track. | Open. The branch now sets a negative `sizeDelta.y` so the drawn height stays about 12px, and tints a `UISprite` orange. This build is copied and has not been checked in game. |
| ISS-009 | Step names | On 2026-10-10 the handles sat at different steps and every readout still said "Normal". The world read for that login was Combat Hard, Death penalty Normal, Resources Normal, Raids Normal, Portals Very hard. Apply stayed gray, so the drag was not stored. Replacing readouts by searching for a child named `Readout` did not change the words on screen. | Open. The branch now replaces the readout stored when the row was built, and a drag no longer has to pass the old "is the World page open" check before it updates the draft. Not checked in game. |
| ISS-010 | Reload | Apply, quit, and login again left the handles at the leftmost stop. The same login's debug line had already read the saved steps correctly, so the keys survived and the panel did not show them. | Open. While the World tab is open, the branch now sets each slider from the staged step whenever the slider value differs. Not checked in game. |

## Confirmed in play

- Header reads version 1.8. The Cheats tab no longer has the skill-loss slider.
- Cheats, World, Spawn, Players, and Log all open.
- Preset clicks and Apply write the five step names. The action log is the proof.
- World keys survive logout. A later login's debug line names the same steps.
- Skill overwrite at 0% kept the skill level on a death after ISS-003.

## Not confirmed

The build copied after the 2026-10-10 00:12 report (large handle, names stuck on Normal, reload stuck at the left) has not been opened in Valheim. Do not treat `dotnet test` as that check.
