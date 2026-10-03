# Spec 001 — Valheim QoL CM

- Version: 0.001
- Status: frozen
- Cycle: MVP, before V1

The plugin adds a clickable admin panel inside Valheim. Admins adjust casual-play rules without typing console commands. Regular players feel the shared death-penalty setting and cannot change it.

## Requirements

### REQ-1 Panel access

- WHEN an admin presses the bound hotkey during gameplay, the system SHALL toggle the admin panel.
- The default hotkey SHALL be Left Ctrl+Tab, and the player SHALL be able to rebind it.
- WHEN the panel is open, the system SHALL block gameplay input and close the panel when Esc is pressed.
- IF the local player is not an admin, THEN the system SHALL leave the panel closed when the hotkey is pressed.

### REQ-2 Version label

- WHEN the panel is open, the system SHALL show the version string `0.001`.

### REQ-3 Mode toggles

- WHEN an admin clicks God, Fly, Creative, or Free cam, the system SHALL toggle that mode on the admin's own character.
- Creative mode SHALL enable vanilla no-build-cost.
- Free cam SHALL enable vanilla free camera.
- WHEN one mode is turned off, the system SHALL leave the other three modes as they were.
- IF the admin's character is dead, THEN the system SHALL reject the toggle and leave every mode unchanged.

### REQ-4 Teleport

- WHEN the panel is open on a connected world, the system SHALL list the players connected at that moment.
- WHEN an admin selects a listed player and clicks a teleport action, the system SHALL either move the admin to that player or move that player to the admin.
- The teleport controls SHALL NOT offer a name field or a coordinate field.
- IF the selected player is no longer connected, THEN the system SHALL report the failure and move nobody.

### REQ-5 Item spawn

- WHEN an admin chooses an item, a quantity, and a quality, and clicks spawn, the system SHALL create that item at the selected player's feet.
- The quantity control SHALL default to 1.
- IF the quantity is below 1 or above 100, THEN the system SHALL spawn nothing and report the reason.
- IF the quality is below 1 or above that item's vanilla maximum, THEN the system SHALL spawn nothing.
- IF the item name is unknown, THEN the system SHALL spawn nothing.
- The spawned count SHALL equal the accepted quantity.

### REQ-6 Death penalty

- The skill-loss percent SHALL be a server-wide value from 0 through 100.
- WHEN a player dies, the system SHALL remove that percent of each current skill level.
- A value of 0 SHALL remove no skill level. A value of 100 SHALL reduce each skill level to 0.
- The system SHALL NOT change gear drop, tombstones, or skill gain while the player is alive.
- WHEN an admin applies a new percent, the system SHALL use it on the next death without a restart.
- IF a stored percent is outside 0 through 100, THEN the system SHALL clamp it before applying skill loss.
- Clients SHALL receive the server value. A client SHALL NOT be able to override the server value.

### REQ-7 Grant admin

- WHEN an admin confirms grant admin for a connected player, the system SHALL add that player's Steam ID from the live connection to the world admin list.
- IF the live connection does not expose a Steam ID, THEN the system SHALL show one Steam ID field and accept the grant only after the admin confirms a well-formed ID.
- IF the caller is not already an admin, THEN the system SHALL NOT grant admin.
- The system SHALL NOT remove an admin and SHALL NOT ban a player.

### REQ-8 Safe failures

- IF an action fails, THEN the system SHALL show a short panel message and SHALL keep the previous state.
- The system SHALL keep running after invalid input.

### REQ-9 Local and server play

- The same plugin DLL SHALL load for a local hosted world and for a Steam-hosted server world.
- Admin-only actions SHALL run with the world host as authority.

### REQ-10 Placement and usage

- The project SHALL include a usage document that states the required BepInEx and Jötunn versions, the plugin folder, how the first admin is recognized, and how each control works.
- The version string in that document SHALL be `0.001`.
