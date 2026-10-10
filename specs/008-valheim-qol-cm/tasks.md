# Tasks — spec 008

- [x] Core rules for stops, presets, key bundles, parsing keys back to steps, the overwrite flag, and slider placement.
- [x] Host writes global keys with the private add and remove methods, updates world rates, and sends keys to peers.
- [x] Skill overwrite keeps the skill level on death, including Hardcore, and the host stores and sends the flag.
- [x] Panel on five tabs. World tab with presets, five stepped sliders, readouts, Custom label, overwrite, and Apply.
- [x] Panel rebuild clears every row list, so the world-scene panel paints its own rows.
- [x] Version `1.8` in the plugin, both project files, the panel, README, and usage doc.
- [x] Local play, 2026-10-10 22:23: login showed `Easy, Casual, Normal, Less, Normal` on sliders and readouts, `Rows 5`, marks on their fractions, Apply wrote the keys, and a death with overwrite on logged `kept`.
- [ ] Hosted-server play, after that server has this DLL: a remote admin applies a preset, every client gets the keys, a client without the plugin follows the world's death penalty, and the overwrite flag reaches plugin clients.
- [ ] `dotnet build ValheimQoLCM.sln -warnaserror` clean before this branch lands on `master`. Open on the backlog as DEF-001.
