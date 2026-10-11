# Issues — spec 009

Every defect cites a requirement in `spec.md`. Play notes are from the local world. `dotnet test` does not open Valheim.

| Id | Spec | Area | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-012 | REQ-5 | Pick search | Typing `Stone` after removing it from the Mining list showed Bloodstone, Brimstone, the gemstones (`Gemstone…` prefabs), Rock, and Sharpening Stone, and never Stone. The eight matches were taken alphabetically, so the exact match fell past the cut-off. | `ItemCatalog.Search` ranks an exact label or prefab match first, then labels or prefabs that start with the text, then the rest, and sorts alphabetically within each rank before trimming to eight. Fixed 2026-10-10 during the play check. |
