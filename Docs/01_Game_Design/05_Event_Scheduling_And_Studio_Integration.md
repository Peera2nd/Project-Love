# Event Scheduling and Studio Integration

Updated Session 032, 2026-10-25. Owner authorizes event implementation first; Session 031 owner readiness supersedes the earlier UI hold, and the supplied skin/S1 dependency presentation is integrated. Temporary Studio content exercises the system; it is not final narrative or balance approval.

## Authoring contract

`GameConfigSO.events` owns a campaign-local catalog of `EventDataSO` assets. Do not scan all project events into a live game. Existing legacy manual catalogs remain supported and are not auto-started.

Each event defines stable ID, delivery, owner character, optional room, day window/preferred date, phase or any-phase, priority, AND conditions, independent Stamina/duration, entry node and optional exclusive group. Prefer multi-day recovery windows. The preferred date never narrows availability. Character/room references must belong to the selected campaign.

- **Forced:** starts automatically at the next safe room point. Does not interrupt dialogue, gifts, character menus, board commands or an in-flight action.
- **CharacterBubble:** player starts it through a temporary `!` button anchored above a visible character's phase placement. Clicking the character body retains ordinary interaction.
- **Manual:** legacy explicit start; no automatic popup or bubble.

Conditions support existing stats/flags/completed events, plus workload activated/not activated, task accepted/not accepted, current-character OT, ready pressured volunteer work and actual shared-Night history. All conditions are AND. Use separate event variants and priorities for alternatives; use an exclusive group when completion must permanently suppress other variants. A daily event cannot use a permanent exclusive group. No built-in character favoritism, random selection or hardcoded plot thresholds.

## Arbitration

1. Forced events reserve ordinary action input and precede bubbles, even if a bubble has a larger priority.
2. Within a delivery queue: descending priority, ascending end day, ascending preferred day, then ordinal stable ID. Asset-list order has no effect.
3. Only one VN runs. Per-character bubbles show the highest eligible event; different characters may show bubbles simultaneously. Lower entries are retained by their definitions, not discarded.
4. Re-evaluate current eligibility after completion, cancellation, time/room/resource changes and load. The pending list is derived; there is no stale persisted queue.
5. An exclusive-group winner is established only on completion using the existing completed-event ledger. Opening/canceling an event does not select a winner.

Overlapping eligibility is expected, not automatically an error. Priority decides forced order and a character's visible bubble. Separate characters' optional events remain player choices. Validation rejects invalid references/windows, duplicate IDs, invalid transactions, dialogue cycles, circular positive event prerequisites and prerequisites excluded by their own permanent group.

## Safe points, deferral and time

The room pumps at `LateUpdate`, after command commits/notifications. Scheduler evaluation itself has no effects. Gameplay commands cannot bypass pending forced events. A real open modal blocks another event. Load is allowed before a pending event starts but blocked during a real modal.

Back always remains a recovery exit. Deferring restores the complete event-start snapshot: resources, flags, choice/read history, production tasks/ledger/orders/cursor and event completion. For a scheduled forced event only, record a deferral stamp after rollback. It stops automatic reopening through the current day/phase and exposes a manual bubble retry, or an overview retry button if its actor is not visible. Another forced event may then run. The deferred event re-arms automatically in the next eligible phase; it still obeys its window and conditions.

Completing an event records completion once and consumes its authored duration through the existing production resolver. Ordinary talk rewards remain separate. End Day advances one phase at a time and stops if a forced event becomes eligible before the day ends; the player decides what to do after the event rather than silently resuming skipped phases. Other multi-phase actions and active VN scenes are atomic: events are evaluated at their completion, not inserted into the middle of them. Author recovery windows accordingly.

Autosaves requested at a day boundary wait until dialogue/board/event reservations clear. They do not report a failed save just because a forced scene started at that boundary.

## Production bridge

`DialogueChoice.activateWorkloads` references existing manual `ProductionConfigSO.events` IDs. Validate the complete choice before spending resources or applying effects. Activating a workload adds its existing task/quota/absence rules without creating duplicate work. The active VN snapshot owns rollback if deferred after an earlier choice. Invalid/duplicate/already-active workload outcomes reject the choice without a partial commit.

Do not award aggregate main progress from story choices in Studio; accepted task weights remain canonical. Dialogue text can explain the tradeoff; the board exposes actual changed scope. Session 032 adds structured `DialogueChoice.workAction` OT/shared-work transactions through the same resolver, plus per-character voice cooldown groups. See the [Night-work contract](../04_Technical_And_Tools/05_Studio_Night_Work_Guide.md). Arbitrary script execution, scope cuts and graded minigame rewards remain unsupported.

## Persistence and compatibility

Campaigns with a non-empty event catalog use save envelope v6 and an additional `_events_v6` filename-key suffix. `EventScheduleState` stores the catalog fingerprint and one latest forced-deferral stamp per event. Completion/group winners use existing completion state. Reject mismatched schedule/choice/condition/outcome catalogs or invalid/future stamps. No guessed conversion of previous Studio v5 saves.

Catalog fingerprint covers schedule fields, target IDs, conditions, linked node/line/choice IDs, durations/costs, recurrence groups/delays, stat/flag outcomes, structured work commands and workload references. Display-copy edits do not invalidate saves; branching/mechanical changes do. Source definitions are not serialized into mutable state.

Configs without scheduled catalogs retain Studio v5 or legacy room/AP v1–v3 paths and behavior. Unity inline-null normalization explicitly preserves both production and event-state schema boundaries.

## Temporary delivered content

`Data/Settings/StudioEventSeed.json` supplies Thai game copy; `StudioEventUpdate` imports once using `GameConfigSO.eventContentVersion` and preserves subsequent author edits. Assets live in `Data/Studio/Events/`.

- Day 2–4: Ploy forced sprint reminder, zero phase/energy cost.
- Day 2–5: Grace optional bug conversation, only while the manual bug workload is inactive. Completing its acceptance adds Code 24/raises the current trial Code quota to 104. A details branch and rollback after the choice are available.
- Day 2–5: Plaifah optional art-direction conversation, one-time trial affection +1, zero phases.
- Day 3–7: Grace optional follow-up requires completion of her bug event; it makes no claim that the bug is already solved.

These scenes demonstrate queues, two actors, branches, outcomes and callbacks. They are not a full authored first-sprint story or final route content. The existing board debug workload button remains once-only; using it before the story removes the now-inapplicable bug invitation.

## Session 032 Night integration and remaining content

Volunteer bubbles, paid acceptance, genuine shared Night output and one-time relationship callbacks are implemented. The [Night guide](../04_Technical_And_Tools/05_Studio_Night_Work_Guide.md) owns precise eligibility/economics/recurrence/transaction rules. Eighteen event variants and 36 Thai nodes are provisional; current single-VN/per-character priority arbitration remains unchanged. The higher-priority bug invitation can intentionally precede a volunteer offer.

Session 031 delivered readable S1 Design -> Art -> Code dependencies and independent alternatives. More sprint-specific chains, a coherent first-sprint story/contextual review, minigames and final balance remain authoring/iteration work. No additional UI art or illness event is introduced by the Night prototype.
