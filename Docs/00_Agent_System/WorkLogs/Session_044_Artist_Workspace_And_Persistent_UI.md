# Session 044 — 2026-10-25 — Artist Workspace and Persistent UI

## Objective

Owner approves the Session 043 tools audit: make content readable/discoverable/editable and UI tangible without routine C# edits. Record it as a permanent working rule. Latest instruction is to continue implementation. Scope T1.5–T1.7 in the current romance campaign; no new gameplay rates, narrative or route expansion.

## Changes

- Replaced the global Dashboard with a Thai campaign-scoped writing/events, character, scenes/UI and settings workspace. Search actual Thai lines/choices with character/purpose filters; ordered speaker/expression/text rows, stable IDs, source links, explicit Save/Undo and Play edit guard. Source navigation selects the actual played/greeting node and marks the current line.
- Added readable scene names independent of IDs; separate blank event drafts, candidate validation and explicit Undoable campaign publication. Selected node/choice and event tests run on detached state; event eligibility/arbitration remains real, with contextual diagnostics. Day/phase/affection/selected mood controls retain the event under test. Screen/node previews bypass unrelated forced scenes solely for direct inspection.
- Migrated the primary scene to nine real UI Prefabs. HUD, roster, dialogue, support and recap are fixed serialized structures; reusable actions/cards/hotspots/bubbles are authored assets. Dynamic row/target pools replace refresh-time reconstruction in this campaign. Runtime updates content/visibility and respects artist layout/typography/colors. Legacy generated fixture paths remain available for old regression scenes.
- Added a visual phase-aware room placement editor: drag position/resize handle, pose/background/visibility fields, Undo/Save to the actual RoomDataSO. Room target positions remain data-owned; overall room frame/UI layout is Prefab-owned.
- Added safe Edit Mode rendering of the same main Prefab via isolated PreviewRenderUtility scene, detached GameSession and game-type camera. No live Start/subscriptions/SaveStore. Existing authored Prefabs/migrated scene survive ordinary builder reopen. Linked nested screen Prefabs open active; main screen keeps initial visibility overrides.
- Durable owner decision in AGENTS rule 8, GR-012, onboarding and Master; reconciled architecture/roadmap/current guide/historical audit. [Canonical workflow and ownership](../../04_Technical_And_Tools/12_Artist_Workspace_And_UI_Authoring_Guide.md) avoids repeated editing instructions here.

## Verification

- Pre-edit `.backups/session-044-before.zip`: 1,293 entries, integrity checked; baseline hashes recorded before editing. Kept the owner's foreground Editor/session untouched; Unity runs used the isolated `.verify` copy with hidden windows.
- Builder/compilation succeeded. Full **189/189 Edit Mode** and **35/35 Play Mode** tests passed. After final Editor-only label/source-navigation/repaint cleanup, **12/12 authoring tests** passed again. Runtime code did not change after the full Play run.
- Authoring checks: Thai full-body search/current graph (55 definitions/38 nodes); text save/reimport/runtime use with unchanged GUID/IDs; add/reorder/Undo; invalid draft rejection and publish/Undo; per-phase room placement save/reload/Undo; Prefab portrait edit/save/builder-reopen preservation; selected-event date/forced-priority/mood/choice behavior; actual source ownership after switching nodes; window open/repaint without unexpected errors.
- All four Edit Mode screen previews produced nonblack pixels and left source definitions, a separate live-state fixture and the active scene unchanged. Play tests use real Input System mouse/raycast paths for activities, cancel, gifts, bubble choices, focus, recap and saves. Fixed UI instance identity, authored portrait offset and fixed typography survive refresh/day changes. Thai glyph/bounds and 1920x1080/1280x720 captures inspected.
- Guarded delivery: 69 runtime/Editor sources, 21 test files, nine new Prefabs and one migrated main scene; 243 delivered code/test/UI/scene files match the tested copy. Existing source meta/GUIDs, authored data, PNGs, packages/settings and historical scenes preserved; detailed counts/hashes in [evidence](Session_044_Verification/README.md).
- Documentation/link/GUID/hash checks are recorded in the verification report. No standalone build, foreground Editor inspection, long-session performance or hands-on owner authoring approval is claimed. The prior gameplay/fun/complete-story limits remain.

## Issues

- **ERR-074:** initial isolated previews were black. WorldSpace alone did not fix it; diagnostics found active unculled UI meshes. Changing the isolated camera from Preview to Game produced rendered UI in the same URP setup. Kept pixel assertions/captures.
- **ERR-075:** test handle-type equality and TMP auto-sizing assumptions corrected without weakening behavior checks; read-path/wildcard probes corrected. Delivery guard initially matched Scripts/Data as content; explicit directory ownership fixed it before copying.
- **ERR-076:** review caught stale source/event context and preview selection resets. Source navigation now resolves its event and clears filters; changing test day/mood retains the selected event. Workspace repaints edited room placement. Regression checks added to the existing authoring suite.

## Handoff

Open **Tools > Project Love > Workspace**, stop Play for authoring, and use its first Open Game button once to load the migrated scene. The normal gameplay entry remains Open Romance Prototype. Suggested owner trial: search/change/save/reopen a sentence; move/save/preview a portrait; move one room phase and Undo it. These are owner usability checks, not remaining implementation permission.

Next after authoring feedback: discuss the bounded first-week invitation/tradeoff/memory/callback chain from Session 042. Keep romance first and 30 configurable days; do not restore professional scheduling or infer approval for complete routes/new systems.
