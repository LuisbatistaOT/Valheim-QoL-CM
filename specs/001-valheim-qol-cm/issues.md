# Issues — spec 001

Defects found while implementing or testing spec 001. Each row cites a requirement in `spec.md`.

| Id | Spec | Found during | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-001 | REQ-5, REQ-8 | implementation | `record` result types failed to compile for the netstandard2.0 Core library because `IsExternalInit` is missing. Spawn and action results could not be tested. | Replaced those records with classes that use constructors. The `{ data, error }` envelope is unchanged. |
| ISS-002 | REQ-1, REQ-3, REQ-4, REQ-5, REQ-6, REQ-7 | test | The first `dotnet test` run did not compile because the test file did not import `Xunit`, so `[Fact]` was unknown. | Added `using Xunit`. |
| ISS-003 | REQ-6 | test | A unit test expected a 0 percent loss to clear a skill level of 40. The spec says 0 removes nothing, so the level stays 40. The test failed with actual 40. | The test now expects 40 at 0 percent and 0 at 100 percent. |
| ISS-004 | REQ-2 | test | BepInEx logged `Loading [Valheim QoL CM 0.1]` while the plugin logged `0.001 loaded`. `System.Version` treats `0.001` as minor version 1. | The panel, the plugin log line, the usage doc, and the git tag keep the literal `0.001`. The chainloader line stays `0.1`. |
| ISS-005 | REQ-2 | closeout | GitHub CLI had no login when the first tag was created, so the release was not published then. | Published as [0.001](https://github.com/LuisbatistaOT/Valheim-QoL-CM/releases/tag/0.001). |
| ISS-006 | REQ-1 | test | The panel did not appear in a running game. The saved hotkey was still Left Ctrl+Tab, and the wood panel was created only if a later GUI event arrived. | The open and close key is now backtick. The panel is built as soon as the game GUI exists. Closing hides the panel and leaves gameplay changes in place. |
| ISS-007 | REQ-5 | test | The item filter stays open, and its match list draws under the quantity and quality controls. | Deferred to spec 002. Spec 001 stays frozen. Spec 002 places the filter and a match table on the left, under the connected players, with the filter kept above that table. |
| ISS-008 | REQ-4 | test | The connected-player list does not show which player is selected. | Deferred to spec 002. Spec 002 shows connected players as a clickable table and highlights the selected row. |
| ISS-009 | REQ-8 | test | Action results use a single status line near the top of the panel. | Deferred to spec 002. Spec 002 shows that log as a console along the bottom of the panel. |
