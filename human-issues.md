# Human issues

## 2026-10-02 — Spec 001 frozen

- Change: Replaced the typed-console draft with the frozen clickable-panel spec, version 0.001.
- Why: Admins need click controls, a live skill-loss percent, and a written trail from defects back to the spec.
- Spec: `specs/001-valheim-qol-cm/spec.md`
- Issues: none yet. New defects go in `specs/001-valheim-qol-cm/issues.md` and in this file.

## 2026-10-02 — ISS-001

- Spec: REQ-5 and REQ-8.
- What happened: the first Core build failed because `record` types need `IsExternalInit`, which netstandard2.0 does not ship.
- Resolution: action and spawn results are classes with constructors. Unit tests assert the same accept and reject outcomes.

## 2026-10-02 — ISS-002

- Spec: REQ-1, REQ-3, REQ-4, REQ-5, REQ-6, and REQ-7.
- What happened: `dotnet test` failed to compile because the test class used `[Fact]` without importing xUnit.
- Resolution: the test file imports `Xunit`.

## 2026-10-02 — ISS-003

- Spec: REQ-6.
- What happened: a unit test expected 0 percent skill loss to reduce a level of 40 to 0. The implementation left the level at 40, which matches the spec.
- Resolution: the test now expects the original level at 0 percent and an empty level at 100 percent.

## 2026-10-02 — ISS-004

- Spec: REQ-2.
- What happened: after the DLL loaded in Valheim, BepInEx printed version 0.1. The plugin's own log line printed 0.001. BepInEx parses the attribute with `System.Version`, which drops the extra zero.
- Resolution: the panel footer, usage doc, and release tag stay `0.001`. The chainloader line is the library's formatting.

## 2026-10-02 — ISS-005

- Spec: REQ-2.
- What happened: the local git tag `0.001` was created. GitHub CLI has no login, so the GitHub release was not published.
- Resolution: after `gh auth login`, push `master` and the tag, then create the GitHub release `0.001`.
