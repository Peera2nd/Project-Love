# Planning prototype delivery and drag contract

Session 036 specification, updated Session 037, 2026-10-25. The owner now explicitly says to start; the previous design/UI hold is superseded. Planning domain and compact draggable queue board are implemented in the separate Planning prototype. Session 038 adds Timeline and the owner-requested conversation/rest corrections; context/balance remain pending. See the [implemented guide and limits](08_Planning_Prototype_Guide.md).

**Readiness:** enough to begin a bounded prototype, with the working defaults below. Final economy/romance/content balance and actual usability require iteration. These defaults make the next implementation concrete; they are proposals, not claims of owner selection or completed features.

Canonical [work math/paper trial](../01_Game_Design/07_Work_Planning_Math_And_Paper_Trial.md), [UX backlog](06_Studio_UX_Redesign_Backlog.md), [Thai romantic direction](../02_Narrative_And_World/03_Planning_Reactions_And_Romance_Draft.md).

## Owner interaction requirement

Cards must feel clean and directly manipulable. Front face: short task name, owner portrait/discipline stripe, a work meter and a compact due/risk or blocked marker. No paragraphs, formulas, dependency lists, per-action reports or permanent button cluster on every card. Clicking/focusing selects detail in the central workspace; the right three-worker roster is permanent, including affection/hearts/Stress/work. Date/progress numerals are concise and not repeated in prose. Familiar toolbar actions use icons and focus/hover names. Back remains the same bottom-right arrow as VN.

## Drag meaning and authority

| Gesture | Intended change | Must not do |
| --- | --- | --- |
| Drag a queued Kanban card before/after another legal card | Reorder that worker's committed work queue in the draft | Rewrite effort/completion or silently reassign disciplines |
| Drop a ready card into the current-work queue | Request it as next eligible work; preview affected sequence | Produce work immediately or start a blocked task |
| Drag a Timeline body | Move future planned start/due together, snapped to phase grid | Move actual history, external milestone or hidden random identity |
| Resize a Timeline edge | Adjust future allowance/internal due; update pressure/buffer forecast | Delete effort or guarantee earlier completion |
| Drop on an illegal target | Explain briefly and restore the card | Charge resources, change Stress/affection or partially mutate state |
| Click/tap without crossing drag threshold | Open/close selected detail | Accidentally change priority |

Kanban status still derives from real production. A card cannot be dragged into Done to bypass required work/review. A legitimate manual-review checkpoint may expose acceptance only after completion; routine auto-handoff behavior is defined below. Finished cards/actual-history portions are read-only. Reassignment between disciplines is outside the first drag prototype.

Timeline uses one underlying plan, not separate copied cards/state. Dependencies constrain actual readiness; projected conflicts are shown with a compact link/risk marker and a focused explanation. No invisible auto-shift of every successor. Deliberately late carryover dates may remain possible with an explicit risk preview; external deadlines are immutable through ordinary card gestures.

## Fluid interaction requirements

- Use a small configurable movement threshold to separate selection from dragging.
- Lift a card/drag ghost above the board, retain its original placeholder and show an insertion gap. Highlight legal drop areas; invalid areas show a concise reason.
- Follow the pointer each rendered frame. Do not destroy/rebuild the board, recompute the full forecast ensemble or apply domain commands every pointer move. The current BuildBoard/inspection rebuild pattern needs replacement for active drag.
- Support bounded edge auto-scroll in board/Timeline viewports, predictable scroll-vs-drag handling and coordinate scaling at 1920x1080/1280x720.
- Snap valid drops; return invalid/cancelled drops to the original position. Pointer release outside, Escape/Back during drag, loss of focus and disabled/destroyed UI must release capture/restore safely.
- During drag reserve input; do not activate a character, background action, event bubble or click-through button. Domain eligibility is revalidated on drop/apply, not trusted from an old highlight.
- Cache card views by stable task ID and update only affected areas after drop. Keep the permanent roster outside changing detail/board content.
- Provide a non-drag fallback for reorder/date editing and focus labels. Department color and deadline risk use different visual cues; meaning is not conveyed by color alone.

Target responsive one-frame pointer feedback; actual frame cost and feel need profiling and owner mouse testing, not a promise inferred from passing text/raycast checks.

## Draft, apply and cancel

Board edits accumulate in a plan draft. Dragging previews position immediately; forecast refresh is coalesced after a meaningful snapped change/drop. No phase, Money/Stamina/Stress, relationship or random sample changes during preview.

A single Apply command validates the whole draft, commits it atomically and returns to the room; then existing event arbitration can present aggregated worker reactions. Revert restores the current committed plan. Back first cancels an active drag; ordinary board Back discards remaining uncommitted edits and exits, matching the existing VN cancellation convention. Clear compact Apply/Revert controls distinguish uncommitted changes; no separate confirmation dialog for every card/drop. Zero-diff Apply produces no reaction/reward.

Canonical state can change only at a safe transaction boundary. An invalid/stale draft rejects without losing unrelated committed state. No save of a halfway drag or draft; save/load behavior at safe points must be explicit.

## Proposed working defaults to start implementation

These fill prototype gaps without treating them as final design approval:

- Use candidate C task/day variation and the current paper coefficients as initial editable data; retain deterministic comparison for tests/debug. No direct speed from due-date compression.
- Routine tasks accept at their completion boundary; dependent tasks start at the next phase. Only explicitly authored review/decision checkpoints wait for Producer attention. Planned ready-alternative order must be visible and agreed in the queue; no hidden priority changes.
- Sprints release at authored calendar gates. Producer completion cannot open them early. Producer work targets the earliest released unfinished duty/stage, allowing carryover; an unfinished duty does not freeze team sprint release.
- Preserve existing Producer own effort/Money/energy figures only as initial trial data, remove team +1 and ordinary coordination output buffs. No direct worker Stress heal. Finishing released duties early leaves discretionary time, not access to future stages. Replace/tune administrative activities after the basic loop is playable.
- Shared time spends one phase and trial player energy, grants first completed per-person/day affection/memory, and adds zero effort/direct worker Stress recovery. Ordinary talk/gifts retain their independent policies. Update participation-ledger/reward eligibility instead of relying on positive helper units. Night shared reward must not exceed daytime merely because OT occurred.
- Use already supplied art and reusable UI sprites; no new asset handoff required for the first implementation.
- Introduce a separately versioned planning save namespace/schema, preserve prior Studio v6 files and legacy fixtures, and reject incompatible state honestly. Exact schema is finalized with implemented fields, not invented migration history.

The original Studio fixture retains Session 032 production/save/UI behavior; the new Planning scene opts into the replacement rules. Existing tests for deliberately replaced helper/early-unlock semantics must be updated; preserve clock/cap/dependency/rollback/legacy invariants rather than claiming all old expectations stay applicable.

## Delivery sequence and acceptance gates

| Stage | Concrete output | Required evidence |
| --- | --- | --- |
| 1. Planning domain | Plan/queue/dates, ready handoffs, Stress/random identity, calendar release and social/Producer separation | Actual domain tests: deterministic replay, forecast purity, caps/clock, dependencies, no helper/heal buff, carryover and save compatibility |
| 2. Clean Kanban shell | Permanent roster/hearts, compact cards/details, shared Back, reorder drag and draft Apply/Revert | Real pointer drag/cancel/invalid drop/auto-scroll, no click-through, owner can read ready/blocked/at-risk work, two-resolution layout/profile checks |
| 3. Timeline | Shared plan view, snapped move/resize, forecast/risk/buffer and dependency consequences | Same data across views; no scope/history/reroll changes; affected forecast is correct; real mouse move/resize |
| 4. Context and week slice | Batched replanning reactions, shared-time callbacks, coherent Days 1–7 and Sprint Review | Safe event order, no duplicate reactions/affection farming, outcomes reflect actual work/history |
| 5. Actual-catalog tuning | Readable first sprint and recoverable 30-day schedules | Combined player time/energy/Money/Producer/romance/non-OT/recovery trials; explicitly separate feasibility from final fun/approval |

Do not make the owner wait until every stage is finished to inspect interaction. After Stage 2, provide a playable compact board; Stage 3 expands it with Timeline. Final stories/routes, extra art, a minigame and production balance are later iterations. Session 037 delivers the Stage 2 feedback point; Session 038 delivers the Stage 3 Timeline candidate. Remaining stages retain their separate acceptance gates.
