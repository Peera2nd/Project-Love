# Session 012 — AP Safety and Three-Day Prototype

- **Date:** 2026-10-23, Asia/Bangkok.
- **Authorization:** owner reported an AP-exhaustion dialogue deadlock and requested its correction plus the Session 011 implementation plan.
- **Backup:** durable `project-love-session-012-before.zip` in the permitted visualization directory; 282 baseline files, read back before edits.

## Delivered behavior

- Dialogue choices retain their authored AP/condition rules. Continue becomes a free defer control at every choice screen, even when no answer is available. A failed default-node advance also defers safely.
- DialogueRunner snapshots runtime state before entry. Deferral restores all costs/effects/flags/history to that point and leaves the event incomplete. Previous completed activities/events are retained; repeated partial replay cannot farm rewards.
- EventScheduler recalculates day/phase/condition availability after state changes. Order: descending priority, ascending preferred day, ordinal ID. Completed events are excluded; preferred dates do not shorten windows. The player selects available events rather than receiving forced interruptions.
- SaveStore provides separate manual/automatic files per config ID at non-dialogue safe points. Version/config identity, checksum, day/AP/roster/stat bounds and nonblank unique IDs are validated before state application. A flushed temporary write replaces an existing file with a prior-file backup. Missing/corrupt/incompatible files and I/O errors leave the live state intact.
- Automatic slot updates at day end and next-day start. Restart resets runtime only; existing saves remain. No mid-dialogue save/load or silent corrupt-file recovery.
- ThreeDayPrototype is a new scene, with separate config/character/presentation, four nodes/events and two activities. It exercises listening-gated follow-up, a failure-gated recovery event, a wrap-up, missed preferred dates and a daily progress delta.
- Both campaign configs remain 30 days. The new view's scene-local prototypeDays=3 stops the short trial after its third day. No final production story, heroine or game ending is approved by this fixture.

## Files and authoring

- Added `Scripts/Simulation/EventScheduler.cs` and `Scripts/Core/SaveStore.cs`; extended session/state, dialogue, day summary, event priority, UI and presentation data.
- Extended the existing builder with a separate three-day command/menu and non-overwriting fixture generation. Existing button prefab is reused. Dashboard includes a dedicated Thai prototype button.
- Added `Data/Settings/ThreeDaySeed.json` and 14 generated scene/SO assets with their metas. Updated only new UI fields in the old presentation asset; original scene and other original test content remain intact.
- Thai long labels use bounded 18–22 point autosizing with no wrapping. UI content stays data-authored; runtime comments/backend docs remain English.
- Refreshed Task Board, Handoff, architecture/master/core/data status summaries and quick start; no Thai narrative documents altered.

## Verification

- Unity `6000.3.24f1` compiled and generated the prototype in the existing short `.verify` copy. Unity connector did not respond; original open Editor and user work were left intact.
- **27 Edit Mode passes:** foundation behavior plus zero-AP/later-choice/condition-blocked deferral rollback, deterministic scheduling/catch-up, disk roundtrip, completed-event reward protection, active-dialogue rejection, slot isolation/replacement backup, missing/empty/corrupt/incompatible/out-of-bounds saves and I/O failure handling.
- **4 Play Mode passes:** original dialogue/activity/day loop, actual Input System mouse-click zero-AP exit, three-day failed-work/recovery/catch-up flow, manual/automatic loading without duplicated completion/rewards, unchanged source definitions and Thai glyph/layout checks.
- Expanded screen-state checks initially exposed long Thai button-label overflow; fixed and all four Play Mode tests passed on rerun. Reviewed rendered 1920x1080 initial, zero-AP and final summary previews.
- Reports: [Edit Mode](Session_012_Verification/EditModeResults.xml), [Play Mode](Session_012_Verification/PlayModeResults.xml).
- Delivered source/tests and 14 new generated assets match the verified copy. Asset/meta/GUID and baseline-preservation audit performed at closeout.
- No standalone player build or original live Console verification. No claim of complete 30-day balancing, final typography for future content/resolutions or production save-format migration.

## Incidents and boundaries

- ERR-017: original all-paid-choice UI had no fallback exit; fixed with full-event deferral and meaningful regression tests.
- ERR-018: expanded UI checks found long Thai labels overflowing; bounded autosizing fixed initial/blocked/choice states.
- ERR-019: Windows case-insensitive globs duplicated delivery selections and stopped a new-file assertion after partial copying. Resumed through a deduplicated manifest, accepting only byte-identical destinations/metas; existing authoring was preserved.
- Basic save envelope is prototype version 1. Same-ID authored content changes are not fully fingerprinted/migrated; global history, multi-slot/quick/pre-branch saves, full validators and advanced presentation remain planned.

## Handoff

Technical increment complete. T3.7 is complete; T4.1 retains advanced save scope; T5.2 retains production-story/art/presentation scope. Open Dashboard, choose the three-day prototype button, then Play. Trial failure/recovery, missed Day 2 events on Day 3 and safe save/load before selecting a concrete small minigame/activity mechanic. Full 30-day balancing and version-control/remote setup remain pending.
