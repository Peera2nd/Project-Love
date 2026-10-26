# Session 030 verification

Unity 6 `6000.3.24f1`, isolated `.verify`. **118/118 Edit Mode + 18/18 Play Mode passes**. These include 22 new event Edit Mode cases and three new mouse-driven scene tests, plus preserved AP/room/Studio regressions. Original live Editor Console and standalone player build were not checked.

## Coverage

- Deterministic forced priority/deadline/preferred-date/ID ordering, one global VN, forced reservation of normal actions, same-character top bubble and simultaneous different-character bubbles.
- Completion-based exclusive groups, daily gates, changing conditions, room/phase visibility, expiration, chained events and no duplicate effects.
- Whole-event rollback and same-phase forced snooze/manual retry, save/load, catalog incompatibility, invalid stamps, invalid workload outcomes, dialogue and prerequisite cycle authoring checks.
- End Day stops at newly eligible forced phases; authored story duration ticks production once. Load uses actual modal state rather than pending-event reservations.
- Four temporary scenes connected to the real bug workload, Code quota 104, separate character affection and later callback. Worker status is visible during worker-owned story VN.
- Real mouse bubble/forced/back/choice/board flows, render bounds and Thai glyph checks. Five final previews were visually inspected.

Two new complete 30-day policies use the actual event catalog, accept all four scenes and add the bug workload once; one talks to all three characters daily, the other does not. Both finish main progress 100, all five Producer stages and 120 phases. Daily affection is Grace 60 / Plaifah 61 / Ploy 60 (Plaifah receives the once-only event +1). See [summary](schedule_summary.json). Baseline four capacity policies remain isolated no-story regressions. This is recovery/feasibility evidence for specific trial schedules, not final difficulty, route, volunteer-OT or economy validation.

## Evidence

- [Edit Mode XML](Session030_EditMode_Delivery.xml), [Play Mode XML](Session030_PlayMode_Delivery.xml)
- [First sprint](Session030_FirstSprint.json), [30 days without ordinary talk](Session030_30Day_Events_TalkFalse.json), [30 days with daily talk](Session030_30Day_Events_TalkTrue.json)
- [Forced VN](Session030_Forced.png), [bubbles](Session030_Bubbles.png), [bug choice/status](Session030_BugChoice.png), [changed board](Session030_BugBoard.png), [after separate event](Session030_AfterEvent.png)
- [Delivery manifest](delivery_manifest.json): 357 delivered source/data files match the tested clone, 39 runtime/Editor sources, 11 tests, 98 Studio assets. 25 asset files are new/changed. All Studio GUID targets resolve.

Pre-edit archive `.backups/session-030-before.zip` passed integrity checking (821 files). 405 protected baseline files are byte-identical. One out-of-delivery UI file, `Art/UI/Mockup/Icons/ui_art.png`, changed concurrently and was preserved; its baseline/current hashes are recorded. The event delivery never writes Art. Original scene/package/project settings, legacy data and source font caches are preserved. No claim that every original PNG remained unchanged despite concurrent work.

ERR-045–047 document restricted process/Unity licensing launch issues (host-access hidden batch launch succeeded). ERR-048 records the initial pending-event Load guard failure (109/111 before fix). ERR-049 records same-frame mouse checks against freshly rebuilt graphics (15/18 before test waits). Final actual mouse/raycast/render tests pass. ERR-050 records detection/preservation of the concurrent UI PNG change.
