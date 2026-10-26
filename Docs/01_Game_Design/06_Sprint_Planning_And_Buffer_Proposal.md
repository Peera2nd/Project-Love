# Sprint planning, forecasts and buffer — discussion proposal

Session 035 refines this discussion with [candidate formulas and a one-week paper trial](07_Work_Planning_Math_And_Paper_Trial.md), plus a [Thai romance/reaction draft](../02_Narrative_And_World/03_Planning_Reactions_And_Romance_Draft.md). Owner requests design only; no runtime/UI implementation or numeric approval. Earlier open questions below remain historical discussion unless refined there.

Session 034, 2026-10-25. Owner endorses the coherent first-week slice but requests gameplay discussion first and explicitly defers UX implementation. This document distinguishes new direction from proposed resolution; **no runtime change or new numeric balance is approved by this discussion**. Full campaign remains 30 configurable days.

## Owner direction and implemented baseline

| Topic | New direction | Current runtime, unchanged |
| --- | --- | --- |
| Core activity | Plan work; events challenge the plan; revisit the board mainly at sprint start or a meaningful disruption | Task priorities/dependencies/review exist; no task schedule/timeline/buffer model |
| Player assistance | Builds affection without accelerating specialist work | Help adds up to 6 effort and recovers Stress; Night affection/history currently require positive helper effort |
| Acceleration | Special events/items may improve speed; owner also explores compressed schedules, Stress and OT | Help, coordination and Producer team bonuses currently add output; Stress bands alter rate |
| Scheduling | Kanban/Timeline views; adjustable duration, due dates, forecasts, waiting dependencies and buffer | Fixed effort, release rules and priorities; no draggable plan or forecast range |
| Team response | Replanning may produce individual relief/objection dialogue | Shared scheduler/conditional VN available; no committed-plan reaction rules |
| Producer | No early S2 unlock; follow authored sprint pacing | Producer completion currently opens subsequent sprint tasks immediately |

The early-unlock direction supersedes Session 027 as a future requirement. Do not report that it or the social-help change is already implemented. Final Producer duties/rewards and exact sprint-release rules still need definition.

## Proposed central loop

Plan a small sprint -> let team work while the player lives/socializes -> a legible event consumes buffer or changes an assumption -> inspect affected dependencies and choose a response -> review outcomes and carry work forward. Routine production should follow the committed plan without requiring per-phase board visits.

Retain Kanban and Timeline as views of one task/state model. No second scheduler, separate task copies or contradictory progress. Calendar release of the next sprint follows authored pacing, not early Producer output. For discussion: unfinished Producer duties create an explained review/administrative issue with recovery rather than silently restoring the old early-unlock gate.

## Separate plan, estimate and commitment

- **Work scope:** remaining effort/definition of done. Dragging a date never deletes work.
- **Planned start / due:** the player's intended window. Future-only edits preserve completed effort and past schedule history.
- **Forecast:** projected active work and finish window given remaining effort, actual staff availability, dependencies, current pace/Stress and already-agreed OT. It can extend beyond the planned bar.
- **Buffer:** usable slack between forecast completion and the relevant internal due/external milestone. Waiting for a predecessor is waiting, not automatically productive time or safety margin.
- **External commitment:** authored sprint/demo deadline stays visible. Extending an internal date does not move that deadline or mint unlimited safe time.

Use a simple baseline first (e.g. roughly 2–3 days, not a false 93% confidence). Show the reason for uncertainty: unresolved brief, known unstable integration or worker strain. Forecast ranges need a defined method and later calibration; do not present arbitrary ranges as validated probabilities. A forecast is never a guarantee.

## Compression and speed: unresolved rule

Owner mentions both special-event/item-only speed gains and earlier completion by shortening task time. These need one consistent interpretation.

**Recommended starting interpretation:** tightening a due date increases pressure and may prompt a compensated OT request; it does not directly increase normal output. Earlier completion must be supported by extra agreed work hours, changed sequencing/removed waiting, or an authored special effect. A tight impossible date remains visibly at risk. OT increases hours, not per-hour efficiency.

**Alternative requiring explicit selection:** an authored rush pace can increase output at a Stress/rework cost, with hard limits. This is an additional acceleration rule beyond special events/items; it must not happen merely because the UI bar was shortened. Do not silently combine both interpretations in implementation.

Extra allowance reduces deadline pressure; it must not automatically slow already-completed/active work until the whole allowance is consumed. For a gentle pace, assign actual work/rest capacity explicitly. Count only active available slots: a six-day calendar bar with three days waiting does not provide six days of effort or erase three days of rushing.

## Dependencies and everyday operation

Preview links and downstream consequences when moving a task. Waiting on an accepted prerequisite must be included in the forecast. Current explicit-priority stalling and manual acceptance can demand frequent board visits; discuss a committed queue that can select planned ready alternatives, and a low-friction handoff/review path. Neither is approved to bypass existing acceptance semantics yet. Preserve meaningful independent work and clear responsibility; do not auto-reorder behind the player's back.

Example only: Design ready Day 2 -> Art forecast Days 3–5 -> Code forecast Days 6–7, leaving little integration slack before a Day 7 demo. A one-day brief delay can consume downstream slack. Independently preparable Code work can be scheduled earlier, while an internal Art extension may be safe if its successor is not ready anyway. These are paper dates, not validated workload values or new story content.

## Social assistance, Producer and Stress audit

Work alongside a person should spend meaningful shared time/energy and build affection without direct task units. Eligibility/completion must use actual participation and a daily/scene reward policy rather than the current positive-helper-effort check. Still allow friendship when work is blocked, without an unlimited reward loop. Keep ordinary talk separate.

Audit indirect acceleration too: helper Stress recovery can cross a rate band, and Producer +1/team coordination currently add effort. Do not claim special-effect-only acceleration while retaining these unnoticed. Producer could instead handle budget/client communication/brief clarification/review and personal duties on the authored sprint schedule; retain or replace current money rewards only after discussing the economy. Final duties and whether social support alters Stress remain open.

Stress should reflect actual sustained workload/deadline pressure and recovery, not just the number of days drawn on a bar. Repeated compression can cause fatigue/rework/OT reluctance; these are proposed, communicated consequences, not a hidden daily failure roll. Preserve a recovery margin and a feasible non-OT path. Existing Stress-output bands need joint evaluation with any rush rule to avoid contradictory 'more pressure always faster' and 'more Stress always slower' behavior.

## Replanning and event reactions

Dragging previews projected workload/deadline/Stress changes. Commit a batch of edits before applying mechanics or queuing dialogue; do not emit a scene for every mouse movement. Compare against the last committed plan, aggregate per worker, use meaningful thresholds/cooldowns, and prevent affection farming by shortening/extending repeatedly.

Reactions should consider remaining work, how much notice was given, whether the original date was promised, individual voice, current Stress/affection, and the cause. A compressed plan can generate a negotiated OT request, not guaranteed automatic agreement. Relief or objection need not award/penalize affection automatically. Existing forced/bubble arbitration resolves simultaneous worker reactions after the planning modal closes.

Events should reveal which assumption changed, the tasks/people affected, forecast movement, consumed buffer and available responses. Start with legible authored disruptions and avoid frequent invisible random throughput rolls that invalidate planning. Reuse unchanged scope/history and completion-once rules.

## Proposed next work, after discussion

1. Paper-model one dependency chain and an independent task with comfortable/rushed/disrupted schedules; settle compression, Stress, assistance rewards, handoffs and Producer role.
2. Implement the agreed planning/forecast model with data-owned parameters and save validation, preserving the once-only phase resolver.
3. Redesign the UI against the agreed information hierarchy; [owner backlog](../04_Technical_And_Tools/06_Studio_UX_Redesign_Backlog.md) is explicitly held for now.
4. Build the first-week scenario/contextual review and test recovery, non-OT feasibility and clarity before a minigame.

Current 145 Edit Mode /24 Play Mode results cover the old prototype only. Removing help output/early release invalidates prior capacity assumptions; rerun 30-day policies for the new model. No runtime, Unity assets, new scenes, narrative scripts or balances are changed by this proposal.
