# Issues - spec 010

Every defect cites a requirement in `spec.md`. Play notes are from the local world. `dotnet test` does not open Valheim.

| Id | Spec | Area | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-013 | REQ-3, REQ-4 | Log tab | The console showed only the first action. `qol-cm.log` and the `Log readout` count held every action from the same session (God, Fly, Ghost, Creative, Spawn, Clear log). `AddLayoutText` pins the text to one line (`preferredHeight` and `minHeight` of 14 + 8), the content group honors that height, and `Text` truncates the rest. | `BuildLog` clears those two heights so the text's own preferred height drives the box, and sets vertical overflow to Overflow. Play check passed 2026-10-11. |
| ISS-014 | REQ-8 | Cheats tab | The five mode buttons were a two-column grid, so Ghost sat in the left column. Tame was 140 wide and Kill enemies was 180, with a 6px gap, under columns that were 200 wide with an 8px gap. | Same buttons and actions. Each button is 200 by 34. Rows and the pair gap are 8, with 8px padding around the block. God/Fly and Creative/Free cam are centered pairs. Ghost is centered under them. Tame and Kill enemies match the columns. |
