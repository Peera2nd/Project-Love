# Studio volunteer and shared Night work

Session 034 proposes relationship-only assistance; this guide still describes the implemented effort-based Night reward/history. Participation/reward eligibility must be redesigned before removing helper output. No runtime change yet. See [planning discussion](../01_Game_Design/06_Sprint_Planning_And_Buffer_Proposal.md).

Session 032, 2026-10-25. Owner authorizes the next gameplay milestone. Rates, eligibility and Thai scenes are prototype content, not final balance or personal-arc approval. The existing [event contract](../01_Game_Design/05_Event_Scheduling_And_Studio_Integration.md) and [task/dependency rules](04_Studio_UI_And_Dependency_Guide.md) remain canonical.

## Player flow

Open Tools > Project Love > Dashboard > Studio Sprint, then Play. Start a fresh Studio run: mechanical fingerprints changed; previous Studio snapshots are rejected, not converted or overwritten with a guessed migration.

At Night, a visible worker can offer OT through a character-head bubble. The top eligible event per character remains authoritative: the earlier Grace bug invitation has priority 50, volunteer invitations 40 and her bug callback 20. Finish or resolve the higher-priority invitation first. The character body still opens ordinary Work/Talk/Gift/request-OT.

The invitation offers:

- **Work together:** compensate the worker, add OT Stress if no order exists, pay player Stamina, then resolve exactly one Night through the normal production resolver. Show a short shared-work epilogue; finishing commits the event and returning with Back restores the entire event-start snapshot.
- **Allow OT:** pay/add Stress once, keep Night open, finish the response and return to the room. The player may order other people, talk, or use ordinary Work on an ordered worker to join them. There is no second fee for joining an existing order.
- **Rest tonight:** no new order, no cost, no time, no relationship penalty. This does not award mechanical Stress recovery; ordinary overnight recovery still applies.

Choice labels show actual capped helper/team output, compensation, Stamina, phase consumption, Stress and affection gain. Accepted-only output is a forecast for Night close. Work together closes Night; other already-ordered workers tick once too, and un-ordered workers remain off. Only one worker receives player help. There is no second team-wide tick or same-phase card spill. Completed effort still needs normal free review; a story does not bypass accepted-prerequisite gates.

After real shared work, a later available Morning bubble recalls the experience. Callback variants depend on current affection. Completing one suppresses the character's other callback variant; Back does not. Callbacks grant no extra affection.

## Trial data

| Rule | Prototype value / location |
| --- | --- |
| Ready current work, no absence, Night only, affordable fee, no existing order | `CharacterCanVolunteer`; checks the actual current task, including accepted dependencies and Producer decisions |
| Pressure window | Current task's own sprint deadline is at most 2 days away, or overdue; `Condition.value` |
| Offer window | Days 2–30, Night; visible workspace hotspot |
| Offer cooldown | 3 days after completed accept/join/decline, shared across all four voice variants through `repeatGroup` |
| Voice bands | Affection 0–19 / 20–100; Stress 0–39 / 40–65 (Plaifah max 60) |
| Fee / OT Stress | Existing configured 5 Money / +10 Stress; voluntary work is compensated |
| Shared help | Existing 20 Stamina, up to 6 helper effort, Stress recovery 3, plus stress-adjusted OT output |
| Relationship | Up to +3 affection for positive actual player help; independent of ordinary daily-talk reward |
| Normal Night | No worker output without an OT order |

Offer eligibility is deliberately conservative. No ready current work, blocked/absent worker, excessive Stress, no fee, an existing order or cooldown means no volunteer bubble. Independent alternatives remain a player prioritization decision. Shared work through already-requested paid OT does not require a volunteer invitation or its Stress ceiling.

A reproducible domain/mouse fixture resolves earlier first-week invitations, keeps Grace's control-test alternative until Day 5 Evening, then prioritizes that ready card before Night. This creates real pressure without falsifying effort, skipping prerequisites or introducing a shipping debug control. Ordinary gameplay need not reproduce that exact schedule.

## Authoring and transactions

`Data/Settings/StudioNightSeed.json` is the initial import source. `StudioNightUpdate` appends 18 events/36 Thai nodes once, controlled by `ProductionConfigSO.nightContentVersion`. Opening the prototype again preserves later Inspector edits. Edit the generated `Data/Studio/Events/` SOs for ongoing authoring; do not lower the version to overwrite an authored catalog.

- `DialogueChoice.workAction`: None, Overtime, WorkTogether; `workCharacter` is a configured worker reference. New command kinds require explicit domain support, not arbitrary scripts.
- Commands and ordinary choice costs/effects are evaluated on a clone and committed atomically. Command choices cannot also activate workloads. Their owning event has zero authored elapsed phases; the command owns any Night boundary.
- After a work-command choice only a linear, choice-free epilogue is valid. Validation rejects a later interactive branch or extra event duration that could charge/tick again after Night ends.
- `EventDataSO.minimumRepeatDays` and `repeatGroup` share recurrence across voice variants using the existing completion-day ledger. Cooldown begins only on completion. Back restores it along with costs/orders/tasks/affection/history/deadline reports.
- `CharacterSharedNight` requires a real production ledger entry. It cannot be satisfied by accepting OT, ordinary talk, a decorative flag or a declined invitation.
- `ProductionState.sharedNights` records person/day/task/capped helper and team effort; one player-assisted Night per day. Save validation checks catalog, worker/stream/task references, completed Night, effort limits and duplicate days.

No new save envelope is necessary: existing event-enabled v6/baseline v5 state carries an added ledger, while changed production/event fingerprints reject older incompatible snapshots. Legacy AP/room saves remain separate. Display-copy edits do not change mechanical fingerprints.

## Verification

See the [Session 032 evidence](../00_Agent_System/WorkLogs/Session_032_Verification/README.md) for actual results and limits. Original live Editor Console, standalone builds, final economy/route balance and final story approval are separate checks.
