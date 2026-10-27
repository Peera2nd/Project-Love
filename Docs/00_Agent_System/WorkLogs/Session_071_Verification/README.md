# Session 071 verification

## Focused automated checks

- [Initial.xml](Initial.xml): 9/9 passed (six retained sandbox cases + three new unified-authoring/runtime cases).
- [Affected.xml](Affected.xml): 3/3 passed (new-scene persistence/gate recheck, real Work Together completion callback, affected window).
- [Window.xml](Window.xml): 1/1 passed after deferred startup/searchable-reference/tooltip refinements, asserting the actual loaded workspace and game before repaint.
- [Titles.xml](Titles.xml): 2/2 passed after routing scene-title edits through the shared localization catalog; checks Thai/English title resolution in the actual runtime and the loaded window.
- [Panels.xml](Panels.xml): 1/1 affected-window recheck after replacing default raw debug-state paths with readable values/history labels and optional JSON/raw trace.

Ten unique cases, sixteen executions. Compilation confirmed by isolated Unity 6000.3.24f1 test completion; no full-game suites/player build. New source edits are outside production Assets and copied only to `.verify`/`.sandbox` projects. Existing baseline menu/licensing warnings are not new compile failures.

Coverage includes: all opening branches/shared join/all caller gates; exact localized search; original catalog/definitions preservation; moved stable line/key with English lookup through runtime-only aliases; actual saved-prefab rendering; Undo/Save/reload; explicit disabled/new scene and actual condition gate; choice IDs/flags versus completion history; actual rollback; callback-owned activity affection.

## Live UI

The returned StoryWorkspaceProject window was selected with computer-use. Confirmed inline speaker/expression/text cards, compact English tool chrome, Run Caller gate check results and actual game view. A line drag moved narration below Grace; Ctrl+Z restored it on refreshed observation. Save produced the identical Workspace hash before and after the trial. Hover help for insertion was visible. [Drag insertion capture](Drag_Insert.png) records the temporary moved order, not an authored narrative change; final captures show the restored order.

[Unified writer and caller test](Unified_Write_And_Test.png), [actual runner trace](Runtime_Trace.png). The initial Trace capture includes raw field paths; the final UI adds readable labels with Raw/JSON as optional detail.

[Final readable State view](Unified_State.png) was inspected after the final live recompilation and Run Caller. Day/phase/room/balance/progress/character affection and mood are primary; internal hashes and raw paths are in optional JSON/Raw detail. Test, Next Line, Trace and State tabs were exercised in the actual window.

Rejected stale-user-input and tooltip-bound calls did not execute confirmed actions. They are excluded from interaction evidence (ERR-110). Actual initial-copy persistence was checked from YAML; explicit Save wrote the full subasset graph. Startup was deferred until the Editor is no longer compiling/updating (ERR-111), and the final affected window test passed. Do not claim every cold first-open environment has been tested.

## Preservation / remaining UX work

Live Workspace hash before/after drag → Undo → Save: `842896B1E4E14FF09E36BE94C3668B38D38A279130E080BE674CA6BEE79DA937`.

Pre-existing production Sarabun font hash unchanged: `B422193E3776607D9DEADE8434C3432EA0BE505C4CD072FA172503405977F205`. No new production diff beyond that owner modification. Original text overlay preserved; original player saves/preferences not touched. Generated test data remains in ignored projects.

Owner acceptance of the new writing/conditions/Save/trace workflow remains pending. Native card typing across all cases, full condition-reference usability, monitor/font-size coverage and large-corpus UX are not established by these passes. No production migration, audio or arbitrary saved-state import is implemented.
