# Spec 007 — version 1.7

The spec, plan, tasks, and issues markdown for spec 007 were lost when the working tree was emptied on 2026-10-09. They were not in the published repository and were not recovered. This note is the record that replaces them. The Obsidian `lessons-learned.md` keeps the two lessons from that cycle.

## What 1.7 did

- Fly is saved by the plugin. A later session starts with the saved choice, and a missing value is off. Core rule: `SavedFly.Choose`. ISS-001, GitHub issue 1, REQ-5.
- The panel hides even when a tab switch throws, and the hotkey closes it whenever it is visible. ISS-002, REQ-1.

Spec 008 inherits both. Its tabbed layout is where the tab switch from ISS-002 now lives.
