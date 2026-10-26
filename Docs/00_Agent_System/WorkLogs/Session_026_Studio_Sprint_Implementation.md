# Session 026 — Studio Sprint Implementation

Date: 2026-10-24. Owner instruction: begin work after the Session 025 work-rate/Stress/Producer proposal. Implementation authorization supersedes the earlier planning hold. Prototype values remain tunable; no production plot, illness timing or final balance is approved.

## Delivered behavior

New `StudioSprintPrototype.unity` and `Data/Studio/StudioCampaign.asset` isolate the management loop from the existing AP/Isometric fixtures. Three independent cast definitions bind Grace/Code, Plaifah/Art and Ploy/Design to supplied phase sprites and Normal/Smile portraits, temporary Thai greetings/talk/close-affection/gift reactions, current social reward policy and work forecasts.

WorkTaskSO/SprintDataSO/ProductionConfigSO define 39 base cards, an optional zero-main-weight Code bug, four seven-day sprint fixtures and a two-day final integration fixture. The board has four scrolling columns, named blockers/prerequisites, free priority/compatible assignment and batch ready review. Deadline acceptance precedes reporting; partial cards carry forward without routine game over. Accepted base weight alone drives main progress; per-stream accepted effort drives sprint credit once.

ProductionSystem applies pre-output Stress rates, shared helper/baseline remaining effort, no card spill within a tick and three productive phases/Night off. Coordinate has two finite productive ticks/no stacking; clarify clears explicit Producer decisions; personal break lowers target Stress and skips its output while colleagues continue. Player rest recovers player energy only. Generic configurable absence expires independently; no Grace illness story was authored.

All elapsed Studio paths use the same resolver, including completed activities/story durations, travel costs, pass-phase and early end-day. Talk, gifts and board operations produce no work. Action preview runs the same cloned-state transaction as commit, and commits revalidate availability/resources. VN status distinguishes affection, worker Stress, card/sprint, effective helper/normal output, cost and Stress before/after; room/board show actual results. Direct MainProgress story-choice effects are rejected in task-production mode to prevent double attribution.

Studio save v4 persists the task ledger, assignments/priorities, explicit-block clears, coordination, event origins/quota sprint, absence expiry, immutable review records and exact resolved-phase cursor. Checks cover resource/config identity, checksum, catalog fingerprint and structural/range/ledger consistency. Slots use a distinct config ID. Legacy v1–v3 remain their original schemas; inline-null normalization prevents empty production objects from invalidating old snapshots.

## Authoring and preservation

35 runtime/Editor C# files and seven tests now exist. Added seven source files, two test files, 75 SO assets, one scene and one JSON seed. Dashboard opens Studio and exposes the new SO types. Existing authoring values are preserved on rerun. The new package reuses shared streams, old room PNGs/font/button prefab; old scenes/data/build settings are preserved.

Eight Grace/Ploy image importer metas are intentionally normalized to Single/readable/uncompressed FullRect, retaining their GUIDs and PNG bytes. No image generation/editing occurred. Seating, style consistency and foreground occlusion remain mockups. Source fonts are preserved; disposable verification TMP feature-cache growth is excluded from delivery.

Pre-edit backup `.backups/session-026-before.zip` was read back successfully (479 entries); `.backups/` is ignored. No repository/remote was initialized. Verification stayed in `.verify`; the user's open Editor/scene was not operated, closed or saved.

## Verification and limits

Final Unity results: **85 Edit Mode + 11 Play Mode passes**. Includes legacy regressions, actual mouse flows, all required art present, active Thai text bounds/glyph coverage, explicit URP captures, black surround and arrow visibility. Four full 30-day policies finish all base tasks and fixture quotas, with/without Code +24 and with/without first-daily talk to all three. Daily-talk policies rest at Night and reach affection 60 each. Intentional missed review/carryover and generic absence are separately tested.

The [verification folder](Session_026_Verification/README.md) contains actual XML, schedules, screenshots and delivery manifest. Feasible scripted policies do not establish optimality, final difficulty, authored route/event/economy viability or owner acceptance. No standalone player build or original live Editor Console validation was performed.

ERR-033–040 immediately record path-read/patch errors, OTL namespace compilation, legacy inline-null serialization, absent scene controller, unsupported escaped arrow and overwritten normalized sprite metas. Fixes were rerun through final suites. The first controller omission's root cause is not established; builder and startup now enforce the requirement. Missing-art tests explicitly require non-null references rather than comparing two null values.

## Next

Owner playtest Studio action feel and board clarity. Then author the first real sprint, conversation topics and a clearly notified workload event/recovery decision; vary weekly discipline emphasis with capacity checks. Fine visual placement/occlusion, minigames, skills, broader tools and final story/ending remain pending. No implicit illness implementation.

Closeout checks: 293 delivered source/generated files match the tested clone; 318 protected baseline files remain byte-identical. All new serialized GUIDs resolve in delivered Assets/resolved Unity packages; 101 local references across 13 changed/new Markdown documents resolve after URL decoding. Verification-link false positives are recorded as ERR-040.
