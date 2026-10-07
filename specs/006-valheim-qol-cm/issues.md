# Issues — spec 006

Defects found while implementing or testing spec 006. Each row cites a requirement in `spec.md`.

| Id | Spec | Found during | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-001 | REQ-2 | implementation | Jötunn sets an admin-only config entry back to its default when a client joins. The default is 5. An admin client can write that 5 back to the host, so a saved 0 disappears on relog. | The host stores the applied percent beside the plugin. The next death and the slider read that value. A client join does not replace a saved 0. |
