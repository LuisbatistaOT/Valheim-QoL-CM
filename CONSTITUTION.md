# Repository Constitution

Version rule: spec `N` ships as `1.N` now that V1 is confirmed. Do not pad the spec number with extra zeros. BepInEx reads the plugin version with `System.Version`, which turns `1.05` into `1.5`. Specs 001 through 004 keep the strings they already shipped: `0.001`, `0.2`, `0.3`, and `0.4`.

## Immutable architectural invariants

- WHEN the plugin is built, the system SHALL target BepInEx 5 on .NET Framework 4.7.2 and load beside the installed Jötunn plugin.
- WHEN a setting changes gameplay for other players, the system SHALL treat the world host or dedicated server as the authority.
- WHEN domain rules are tested, the system SHALL keep those rules free of Unity and Valheim types.
- IF a spec number is `N` and that spec was frozen before V1, THEN the system SHALL keep the version string that spec already published.
- IF a spec number is `N` and that spec is 005 or later, THEN the system SHALL publish the version string `1.N` in the plugin, the in-game panel, the usage document, and the Git tag.

## Absolute safety guardrails

- The system SHALL NOT include advertisements.
- The system SHALL NOT include automated or manual ban features.
- The system SHALL NOT take a dependency on a network service other than BepInEx, Jötunn, and the Valheim process itself.
- IF a person is not an admin for the current world, THEN the system SHALL NOT open the admin panel and SHALL NOT apply a gameplay mutation from that person.
- IF a skill-loss percent is outside 0 through 100, THEN the system SHALL clamp it to that range and SHALL NOT throw.
- IF a requested action is invalid, THEN the system SHALL leave the previous game state in place and SHALL NOT crash the process.
- The system SHALL NOT modify files in the Valheim install except the plugin files copied into `BepInEx\plugins`.
- WHEN a defect is found during implementation or testing, the system SHALL record an issue that cites the spec requirement it relates to.
- WHEN a development or troubleshooting session starts, the reader SHALL consult `lessons-learned.md` in the Obsidian QoL CM folder before changing behavior.
