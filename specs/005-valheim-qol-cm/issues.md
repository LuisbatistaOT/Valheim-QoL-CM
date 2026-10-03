# Issues — spec 005

Defects found while implementing or testing spec 005. Each row cites a requirement in `spec.md`.

| Id | Spec | Found during | What happened | Resolution |
| --- | --- | --- | --- | --- |
| ISS-001 | REQ-4 | implementation | `TryAdmin` returned before assigning `position` when the sender had no name, so the plugin did not compile. | Assign `Vector3.zero` on that path and still reply that the admin was not found. The world is unchanged. |
