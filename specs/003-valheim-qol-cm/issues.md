# Issues — spec 003

| ID | Spec | When | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-001 | REQ-2, spec 001 REQ-5 | play | Spawn reported "Player is not connected." Choosing an item did not choose a player, and the local player was missing from the list whenever anyone else was connected. | An empty selection spawns at your feet. The connected-player list always includes you. |
| ISS-002 | REQ-2 | play | Filtering for Stone missed it. The list kept only the first eight prefab names, so the in-game name never appeared. | The list reads every `ObjectDB` item and shows `Localization` names. The filter matches that name. |
| ISS-003 | REQ-2, spec 001 REQ-5 | play | Clicking an item set the quantity to that item's stack size. The console sometimes named a different item than the prefab that spawned. | Deferred to spec 004. Selecting a row sets quantity to 1, and the console uses that row's name. |
| ISS-004 | REQ-2 | play | Searching for bow listed many rows named Bow. Spawning some of them created a drop that could not be picked up. | Deferred to spec 004. The list keeps prefabs whose `m_icons` list has at least one entry. |
