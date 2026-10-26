# Session 026 verification

Unity 6 `6000.3.24f1`, isolated workspace `.verify`. Final source: 35 runtime/Editor C# files, seven test files. Final results: **85/85 Edit Mode, 11/11 Play Mode**. XML files in this folder are the actual Unity Test Framework results; earlier failed attempts are documented in ERR-033–040 and ignored engine logs, not counted as passes.

## Covered behavior

- Existing AP/Sandbox/room/social/gift/save regressions remain passing.
- Stress boundaries 0/39/40/69/70/89/90/100, helper/worker shared card cap, no same-tick spill, idle/blocked/break/absence Stress behavior, overnight recovery, three productive ticks, Night rest, early-end-day once-only phases.
- Pure forecast/atomic rejection, Producer non-stacking/tick expiry/Night preparation, decision versus prerequisite blocks, free priorities/review, accepted ledger/main weights, missed sprint/carryover and immutable old reports.
- One-time workload/quota application, generic absence expiry, whole-dialogue cancellation, v4 save roundtrip/catalog rejection/legacy incompatibility, invalid state/cursor rejection, source-definition immutability and cycle/window validation. Legacy v1/v2/v3 stay supported in their original configs; Unity inline-null behavior has a regression fixture.
- Actual Input System mouse clicks: three worker hotspots/portraits/status/help, board blocking, coordinate/clarify/priority/review/bug, Night disabled work with talk still available, daily/repeat talk, liked gift/cancellation, manual/auto save flow and week rollover after a miss.
- Active TMP bounds and glyph coverage; required art non-null in every worker phase/expression; explicit URP PNG captures, black surround and rendered back arrow.

## Full-campaign schedules

Four deterministic 30-day engine schedules finish all 39 base cards at 100%, resolve exactly 120 phases and produce five sprint reports: with/without a first-week Code +24 bug, each with/without first-daily talk to all three characters. The daily-talk policy rests at Night when useful; it reaches affection 60 with each character using actual interaction completions, not direct stat edits. Every phase state passes restore validation. All four policies meet their fixture quotas, including the raised first-week Code 104 target in bug runs. See `schedule_summary.json` and the four runtime JSON snapshots.

These prove feasible examples, not optimal play, final difficulty, full narrative/route viability or recovery from every mistake. No mandatory story interruptions, gifting economy, skills, minigame grades, illness trigger or final ending logic is simulated. Separate unit/mouse tests cover an intentional missed sprint and carryover.

## Rendered previews

- [Room](Session026_Room.png)
- [Grace work forecast](Session026_VN_studio_grace.png), [Plaifah](Session026_VN_studio_plaifah.png), [Ploy](Session026_VN_studio_ploy.png)
- [Immediate help result](Session026_After_Help.png)
- [Sprint board](Session026_Board.png), [bug workload](Session026_Board_Bug.png), [Day 8 missed review](Session026_Review_Day8.png)
- [Night interaction](Session026_Night.png), [new-day conversation](Session026_Day2.png)

All image files were visually inspected. Seating/depth/style remain mockup quality. Sources, generated assets and importer metas are compared against the tested clone in `delivery_manifest.json`; dynamic Sarabun SDF font-cache growth in the clone is deliberately excluded. PNG bytes/GUIDs, legacy scene/data/build settings and source font assets are preserved. No standalone build, original live Editor Console check or human-play balance approval is claimed.
