# Work Rates, Stress and Producer Actions — Prototype Proposal

Session 025 proposal; **authorized for prototype implementation in Session 026**, not locked final balance. The separate [Studio prototype](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md) implements these values, review/carryover and the character status forecast. Original AP/Isometric fixtures remain unchanged. Four 30-day engine policies demonstrate feasible examples; production story/route balance remains open.

**Session 027 supersedes the old personal-break/Producer-stage assumptions:** worker rest is a free character order (trial −5 Stress at phase close), Night OT is paid per person with no request time, Producer now earns money/own effort/team +1 and gates the next sprint. Task credit follows the task's own sprint, including early delivery. The tables below preserve the Session 025/026 iteration; use the [current Studio guide](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md) for implemented rules and trial values.

## Work units and sprint target

Use a common effort unit so output, costs and remaining work are readable. A card can require several units; four units are not four completed tasks. Start with equal base capacity and differentiate task type, dependencies and weekly workload before introducing character-specific productivity perks.

| Worker / discipline | Base output per productive phase | Healthy daily maximum | First-sprint accepted target | Three example card efforts |
| --- | --- | --- | --- | --- |
| Grace / Code | 4 | 12 | 80 | 28 / 28 / 24 |
| Plaifah / Art | 4 | 12 | 72 | 24 / 24 / 24 |
| Ploy / Design | 4 | 12 | 64 | 24 / 20 / 20 |

These are starter-week targets, not permanent discipline rankings or quotas multiplied across 30 days. Later sprints can emphasize a different discipline. Healthy theoretical capacity is 84 units/person/week; fatigue, card-end caps, blocks, absence and review/dependencies can reduce actual output.

- Code cards produce a feature or fix; events can add a visible bug card. Art cards produce an asset set or revision. Design cards produce rules/tutorial/balance decisions and can unlock a later Code/Art card.
- One active card per worker; an automatic worker follows the assigned priority. Work stops when blocked, unassigned, absent or ready for review, with a visible reason.
- Reaching the effort target enters Review. Free batch acceptance is proposed; accept ready cards before the weekly evaluation. Credit each completed card once to its sprint and the base-project weight, where applicable. Partial effort is visible but is not an accepted deliverable.
- Producer ordering/review is free. Specific Producer activities below consume time. Bug work does not erase unrelated accepted progress. Scope changes and incomplete carryover remain explicit decisions.

## Player help

**Help the selected worker:** one productive phase, player Stamina 20, up to **6 effort** on that worker's active card, worker Stress **-3** before the normal work tick. This is task help; it grants no implicit affection or bonus work on other disciplines.

The team's normal work also occurs while this phase passes. Both contributions share remaining effort; cap each contribution before committing. There is no spill into the next card in the same tick. Other workers still produce normally. After-hours character help/overtime remains undesigned; the approved Night rest is not permission to silently schedule overtime.

Example: task 8/20, worker Stress 50. Help adds 6 and lowers Stress to 47; the worker then adds 3 and gains 2 Stress. Result: task **17/20**, Stress **49**, player energy -20, one phase. Preview must show **player +6 / worker +3**, not label all nine units as player output. A nearly complete card advertises only the effective capped help.

## Stress

Stress is a game-management meter, separate from player energy and affection. Propose a shared transparent rule first:

| Stress before normal production | Worker output per productive phase |
| --- | --- |
| 0–39 | 4 |
| 40–69 | 3 |
| 70–89 | 2 |
| 90–100 | 1 |

- Paper-model starting Stress: **30 each**. This differs from the current Plaifah fixture's 35 and is not applied to it.
- A phase with actual normal output adds **2 Stress**, once. No normal-output gain when absent/blocked/idle, during a personal break, at Night or when player help already finished the card.
- Normal overnight recovery: **-4**, floored at zero. This is a new proposed character rule; the existing player's overnight energy +40 is separate.
- Player help lowers the target's Stress by 3 before normal output; calculate the new rate from that value. Finishing through help can avoid a normal-work stress gain in that phase.
- The 90+ tier remains slow but productive. Illness is not an automatic random consequence of reaching a meter threshold; the proposed Grace event remains independently authored and unapproved.
- Ordinary talk keeps its existing first-daily 3 energy / +2 affection and free repeats. Do not add unlimited Stress recovery to repeat dialogue. No new automatic gift effect is specified here.

## Producer actions

| Action | Player cost | Effect | Limit/tradeoff |
| --- | --- | --- | --- |
| Coordinate the team | 1 phase, 15 energy | Each eligible worker gets +1 output on the next two productive phase ticks; up to +6 total across three workers | One global active plan; no stacking/refresh before expiry. Tick duration advances even for blocked/absent workers; caps can waste potential output |
| Clarify a Producer decision | 1 phase, 15 energy | Remove one explicit `awaiting Producer decision` block, allowing its worker to resume normal work | Does not supply the missing Art/Code/Design effort or bypass an unfinished specialist prerequisite. Disabled when no such decision is pending |
| Workload check-in / personal break | 1 productive phase, 10 energy | Selected present worker Stress -15; they skip their own normal output in that phase | Other workers continue. No implicit affection/output reward; costs current production for later recovery |

Coordination adds its +1 after the stress-based rate, capped at remaining effort. A plan prepared at the desk at Night has no Night production and keeps its two charges for subsequent productive ticks. Clarifying a decision can also be desk work; taking a worker for a break is a daytime interaction. None of these creates an independent fourth weekly quota or arbitrary overall progress.

Free board operations: inspect, choose priority/assignment and batch review. Active support cannot remove real work simply because the player opens a menu. Multiple plans cannot store unlimited future buffs.

The player retains trial energy cap 100, normal overnight recovery +40 and a one-phase rest restoring 30. **Player rest does not also reduce an unrelated worker's Stress.** Session 026 implements this in Studio; the legacy fixture retains its placeholder targeted rest Stress -10. The team works while the Producer rests during productive phases. Coordinate/help/break effects supersede placeholder activity effects in the Studio scene.

## Sprint recovery and workload

Review dates 7/14/21/28 and final Days 29–30 are Session 026 prototype fixtures, configurable and not final balance. Missing a weekly target produces carryover and a recovery/scope decision, not routine instant game over. Existing partial effort is retained. A starter bug example is **Code +24 effort**, shown as an added task and new target 104; it is not an approved mandatory event.

An absence removes that worker's normal output and character-help availability for the affected phases. Other staff and desk actions continue. Buffer work before/after the absence can help; late events still need a feasible response window or explicit scope/deadline alternatives. The paper absence scenario is a capacity exercise, not approval to trigger Grace's story.

## Arithmetic evidence and limits

The reproducible [paper model/report](../00_Agent_System/WorkLogs/Session_025_Verification/README.md) runs ten seven-day schedules with nine concrete cards, effort caps, no same-tick card switching, immediate free boundary acceptance, energy, stress and normal work timing. It checks legal costs, bounded values, unique ticks and zero normal Night output. It does **not** run Unity or demonstrate final balance.

Illustrative outcomes: no help leaves Code six effort short; a late focused help finishes the starter week. Five mixed help actions also finish and leave more stress buffer. A schedule missing two intended help days still finishes with four help actions. A +24 Code bug is handled by nine total helps; a three-day Code absence also has a nine-help example. High Stress 75 across the whole team is expensive: one recovery plan needs nine breaks plus eight helps. A lighter recovery plan misses targets. Four coordination actions alone leave Code one effort short, demonstrating a remaining focused decision.

All examples include first-daily talk with present characters; no items or energy purchases are needed. These are hand-authored feasible/infeasible examples, not minimum-action proofs. Model omissions: specialist dependency chains (only a Producer-decision block is modeled), new story duration, economy/SP, gifts, arbitrary assignment changes, full 30-day carryover and final outcome scoring. Auto boundary acceptance assumes free reviews are performed. Actual priorities/dependencies/events can require more capacity.

Session 026 owner authorizes this implementation. Engine/mouse tests now cover card caps/blocks, Night preparation, early end-day, saved plans, generic absence and sprint rollover. Next, playtest action feel, ordinary mistakes and severe accumulated Stress with authored content. Keep resource/output/status previews tied to the actual transaction rather than duplicating these tables in UI code.
