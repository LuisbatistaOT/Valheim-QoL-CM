# Issues — spec 002

Defects found while implementing or testing spec 002. Each row cites a requirement in `spec.md`.

| Id | Spec | Found during | What happened | Resolution |
| --- | --- | --- | --- | --- |
| | | | | |
| ISS-001 | REQ-4 | implementation | The version string `0.002` does not survive BepInEx. `System.Version` drops the extra zero and the plugin list shows `0.2`. | The published string is `0.2` in the plugin, the panel, the usage doc, and the git tag. The constitution now says spec `N` ships as `0.N`. |
