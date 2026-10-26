# Work planning math and one-week paper trial

Session 035, 2026-10-25. **Design proposal + offline arithmetic only.** Owner requests design/planning before implementation. New direction: no direct team effort or Stress reduction from Producer assistance; dating/romantic buildup remains a core axis. Comfortable allowance may finish early, compressed commitments may still miss and create strain. UI redesign remains explicitly held. Full game stays 30 configurable days.

This refines the [planning discussion](06_Sprint_Planning_And_Buffer_Proposal.md), not the unchanged Session 032 runtime. Values below are a complete candidate for testing, not approved production balance or psychological/scientific claims. [Reproducible paper evidence](../00_Agent_System/WorkLogs/Session_035_Paper_Trials/README.md).

## 1. Design recommendation and comparison

| Candidate | Benefit | Main cost | Recommendation |
| --- | --- | --- | --- |
| A. Deterministic output from current Stress | Most legible; event uncertainty alone can challenge plans | Quiet work becomes exactly predictable once rules are known | Keep as reference/debug comparison |
| B. Bounded random output every productive phase | Simple stress-dependent output range | Short visible fluctuations can feel like noise; player cannot meaningfully answer each roll | Compare, do not start here |
| C. Small task factor sampled once + one bounded daily factor, with Stress affecting mean/spread | A task can be consistently easier/harder than estimated; a day's flow is coherent | Needs saved/random-key discipline and completion-window forecasting | Preferred prototype candidate, pending owner review |

C is not proven more fun or statistically better. Its appeal is legible uncertainty at the same cadence as planning/story. No affection-to-output multiplier; relationship and professional capacity remain separate. No normal-output bonus merely from a tighter date.

## 2. State and units

Use phase work units, not a second AP budget. Each day retains Morning/Daytime/Evening normal work and Night rest unless OT is agreed. Static task effort is separate from the player's planned start/internal due, the fixed external sprint milestone and the forecast. All Timeline/Kanban views read one plan.

Conceptual mutable data for a future implementation: remaining effort/completed effort, committed plan version and dates, actual start/finish/acceptance, worker Stress, known scope modifiers, work schedule/rest/OT commitments, random identity, shared-experience history and recent plan-change context. Definitions, coefficients, bounds and dialogue conditions stay designer-editable. These are schema requirements, not new C# types or assets.

Moving a future date never erases work, moves a past finish, changes the external milestone or rerolls a sample. Extending an internal date reduces future pressure if it creates real usable slack; it applies no flat Stress heal or work bonus. An early completion does not wait artificially for its due date.

## 3. Candidate output formula

For an eligible worker/task phase:

`output = min(remaining, baseRate * stressMean(S) * taskFactor * dayFactor * knownEffect * availableFraction)`

- `baseRate = 4` units/normal phase in the paper fixture, equal for the three disciplines to isolate planning effects. Individual rates remain data in a future game. Test near band boundaries before locking this stepped table; smoothing/hysteresis is a later tuning option, not part of this trial.
- `S` is Stress at phase start (after an agreed OT surcharge, if any). Stress is updated after normal output. `availableFraction=1` in this trial; blocked/rest/absent means zero, without a random roll.
- `taskFactor ~ Uniform(0.90,1.10)`, keyed once per worker/task for the run. It represents estimation/implementation variation, not hidden changing scope.
- `dayFactor ~ Uniform(1-w,1+w)`, keyed once per worker/day. Width is selected from Stress at the start of the day. Current Stress still selects the mean multiplier each phase. In normal play future samples are unknown; do not let forecasts inspect them.
- `knownEffect=1` here. Authored temporary event/item multipliers may change it with a visible duration/source; ordinary help, Producer work, coordination and affection do not.
- Preserve fractional effort internally and clamp only the visible display/completion comparison. Do not floor small output to zero or spill unused capacity into another card in the same phase.

| Stress | Mean multiplier | Daily half-width w | Base mean output | Combined possible range at that day's fixed band |
| --- | ---: | ---: | ---: | --- |
| 0–39 | 1.00 | 0.05 | 4.00 | 3.42–4.62 |
| 40–69 | 0.90 | 0.10 | 3.60 | 2.916–4.356 |
| 70–89 | 0.75 | 0.15 | 3.00 | 2.295–3.795 |
| 90–100 | 0.55 | 0.20 | 2.20 | 1.584–2.904 |

Ranges assume current/day-start Stress stay in the same band. If Stress changes bands within the day, combine the current mean with the already-fixed day's width. This is a tuning table, not a claim about human productivity. No separate random failure/automatic rework roll is stacked onto it in the first prototype; authored rework events remain explicit.

For comparison, A uses factor 1; B uses one phase factor `Uniform(1-w(S),1+w(S))` without task/day factors. Same nominal mean does not imply the same task finish time because phases and dependency handoffs round completion upward.

## 4. Pressure and Stress formula

At phase start, compute a cheap mean forecast using visible remaining work, current Stress mean, normal productive phases, dependency acceptance and the committed same-owner queue. It excludes future secret randomness, unrevealed events and unapproved OT. The paper forecast freezes the current mean for projection; it is an approximation, not the final displayed completion window.

For each pending task:

`requiredSlots = ceil(remaining / (baseRate * stressMean(currentStress)))`

`availableSlots = normal work slots from projected ready/start to its committed due, after predecessors and earlier owner work`

`loadRatio = requiredSlots / max(1, availableSlots)`

Pressure category is 0 when ratio <=0.8; 1 when 0.8<ratio<=1; 2 when ratio>1 or no slot remains. A worker's paper pressure is the maximum over their pending sprint tasks. Do not count waiting calendar days as work capacity or Buffer. For a larger backlog, limit this calculation to released/committed tasks in the current planning horizon.

| Phase state | Candidate Stress change |
| --- | --- |
| Positive normal work | +1 ordinary workload + pressure (0/1/2) |
| Idle while waiting, a pending task is pressured | +1 concern; no fictitious work output |
| Idle without pressured pending work | -1 recovery |
| Night without OT | -5, clamp at 0 |
| Agreed Night OT | +10 before its output; no extra normal-work Stress and no ordinary Night recovery |
| Shared time/ordinary Producer work | No direct worker Stress modifier; only the normal passing-time schedule applies |
| Paper scope-change event | +4 once to affected Art/Code workers; this is a visible authored shock, not a hidden random penalty |

Clamp Stress to 0–100. All values are provisional. A tight due changes workload pressure, not units/hour. More available time need not consume more active work. A high-Stress worker still makes positive eligible progress but has lower mean and more uncertainty; the first candidate avoids catastrophic random zero days.

For future rest/absence and consecutive OT, consider fatigue/limits only after this model is readable. Do not add several overlapping punishment systems at once. Do not allow social affection to override a health/availability limit.

## 5. Randomness, saves and preview discipline

Proposed deterministic keys: campaign seed + model version + worker/task for task factors; campaign seed + worker/day for daily factors. Paper evidence uses stable SHA-256-derived uniform values. Unity implementation is intentionally absent. Store canonical draws or a versioned deterministic identity; never use UI evaluation order or wall-clock time. Loading, reopening the board, ordinary talk, date dragging and unrelated actions must not reroll outcomes. An explicit authored task revision can have its own revision identity only if designed; no automatic reroll on reassignment. A future task factor must retain its stored first-start identity/draw if the owner changes; the paper fixture has fixed assignments and does not exercise reassignment.

A preview has no mutation. Compare old/new plans with the same projected sample set, so forecast differences represent plan changes rather than new noise. Actual production, OT and side activity must ultimately use the existing once-only phase resolver.

## 6. Forecast, buffer and handoffs

For a future displayed forecast, run an independent ensemble from observed state under the known plan, sampling plausible future task/day factors rather than reading actual hidden future keys. Include observed completed effort/Stress, normal schedules, fixed blockers and agreed OT only. Unknown future events are not predicted as if already known.

Show a central/likely-late finish window (paper reporting uses nearest-rank P10/P50/P90 internally). Label it an estimate, not a validated 80%/90% guarantee. At first, show the window and main risk reason; keep derivation/quantiles in inspection/debug. Completion distributions must include late paths, not discard failures. Recalculate at phase boundaries, an event, accepted handoff or committed replan, not every UI frame.

A started task's prior factor should not be secretly revealed to the forecast. A later estimator may constrain plausible factors using observed uncapped output and the known daily range; capped finish-phase output is censored evidence, not proof of a slow rate. Any estimator approximation needs calibration. The paper trial below compares policies; it does not implement this interactive forecast estimator.

Calculate two different slacks: local usable buffer before an internal due, and downstream margin to the fixed milestone. For example, an Art extension can reduce Art pressure yet consume Code's margin. When the conservative finish is after the milestone, show a shortfall rather than a green long bar. No extra player-entered Buffer field is required.

**Paper handoff assumption, not owner-approved implementation:** routine work is accepted at its completion boundary, successors start no earlier than the next phase, and explicitly planned ready alternatives can run without reopening the board. The trial uses a small ordered per-worker queue with independent work first. High-risk/manual review checkpoints would be separately authored and included in estimates. Current runtime still uses explicit review/priority; do not silently bypass it during implementation.

## 7. One-week fixture and policies

All tasks below start released; no S2/Producer early release. All planned start dates are Day 1. A shared owner is sequential, and missing prerequisites wait. Same-phase spill is forbidden.

| Task | Worker | Effort | Dependency | Owner sequence | Comfortable internal due |
| --- | --- | ---: | --- | --- | --- |
| Brief | Design | 12 | None | First | Day 2 Evening |
| Rules | Design | 12 | None | After Brief | Day 4 Evening |
| Scene | Art | 12 | None | First | Day 2 Evening |
| Character | Art | 20 | Accepted Brief | After Scene | Day 4 Evening |
| Setup | Code | 12 | None | First | Day 2 Evening |
| Integration | Code | 20 | Accepted Character | After Setup | Day 6 Evening |

The external demo deadline is Day 7 Night. A routine miss carries work forward; it is not a run-ending fail. This is an illustrative small deliverable, not existing Studio asset effort/quotas.

1. **Comfortable:** dates above, no disruption/OT. Normal production may finish before due.
2. **Compressed:** Brief due Day 1 Daytime, Character Day 2 Evening, Integration Day 4 Morning; scope/starts unchanged. The dates are intentionally infeasible under the mean. No extra work capacity is granted.
3. **Disruption, hold old plan:** before Day 3 Morning, visibly add 12 effort each to Character and Integration (+4 Stress each affected worker). Keep old internal dates.
4. **Disruption, replan:** same event/draws, extend Character to Day 5 Evening and Integration to Day 7 Evening. External deadline unchanged; no direct Stress subtraction.
5. **Replan + optional bounded OT policy:** same as 4, with compensated Night requests only when the normal-hours forecast threatens Day 7, the worker has ready work, Stress<70 and fewer than two OT slots this week. Fee 5 per slot; this is an automated paper policy, not a player-behavior prediction. Only actual agreement would reserve OT in a game.

Simulate seeds 0–999 across A/B/C and all five policies (15,000 paths; fixed repeats are identical). Late work continues to Day 14 for uncensored completion times. Peak Stress is the most stressed worker in the first seven days.

### Candidate C results

| Policy | Deliver by Day 7 Night | P10 / median / P90 completion | Mean peak Stress | Mean late internal tasks |
| --- | --- | --- | ---: | ---: |
| Comfortable | 1,000 /1,000 | D5 Daytime /D5 Evening /D6 Morning | 33.00 | 0 |
| Compressed | 1,000 /1,000 | D5 Daytime /D5 Evening /D6 Morning | 46.28 | 3 |
| Disruption, hold | 797 /1,000 | D7 Daytime /D7 Evening /D8 Morning | 39.07 | 1.38 |
| Disruption, replan | 801 /1,000 | D7 Daytime /D7 Evening /D8 Morning | 33.33 | 0.20 |
| Replan + bounded OT | 998 /1,000 | D7 Daytime /D7 Evening /D7 Evening | 35.91 | 0.03 |

Read honestly: compressed commitments miss three internal promises and increase strain, without an earlier median or guaranteed speed. The loose external date still allows recovery in these samples. Extension mainly reduces pressure; the 797 vs 801 difference is too small here to claim a meaningful delivery-speed improvement. OT adds actual slots at a cost; it does not guarantee success. All 1,000 comfortable samples meeting a deadline is not a proof over every seed or final game balance.

Phase-random B vs task/day C external delivery differences are small in this fixture (B disruption hold/replan: 808/811 vs C 797/801). Choose task/day coherence for player comprehension, not an invented superiority claim. Both stochastic models often finish after the fixed-rate point because dependency/phase rounding matters. [Full results/traces](../00_Agent_System/WorkLogs/Session_035_Paper_Trials/README.md).

## 8. Replan reactions: derive burden, then choose a voice

Do not calculate one universal happiness roll. First classify the mechanical change with paired before/after forecasts: new shortfall/usable slack, remaining work, time of notice, affected dependencies, accepted OT and whether a previous commitment changed. Then select response intent, character voice and private/public context.

Optional concern-score sketch for testing, not a psychology model:

`concern = max(0, wB*addedShortfall + wN*shortNotice + wP*recentBrokenPromises + wS*existingStrain - wM*concreteMitigation)`

Normalize features to 0–1 (cap added shortfall at 3 normal slots, notice window at 6 normal slots, recent broken commitments at 2, strain relative to the worker's comfort threshold). Starter weights 3/1/1/1/2 are placeholders. Mitigation means a real accepted schedule/scope/dependency response, not selecting a flattering line. Mood/acceptance cannot be inferred from affection alone.

Apply semantic gates before score: impossible/unsafe request -> negotiate/refuse; extra burden -> concern or conditional agreement; repeated changed promises -> uncertainty/disappointment; genuine slack improvement -> relief (possibly mixed with concern for a successor); negligible change -> brief acknowledgement. Affection influences familiarity, trust in explanation and willingness to show vulnerability, not guaranteed compliance. Character-specific thresholds/traits/response templates remain data.

Commit a batch, compare against its last committed version, aggregate once per worker, and use thresholds/cooldowns. Pure dragging causes no effects. Repeated extend/compress cannot farm affection. Use the existing forced/bubble scheduler after the board closes. [Thai voice and romance notes](../02_Narrative_And_World/03_Planning_Reactions_And_Romance_Draft.md) are working drafts, not final scenes.

## 9. Dating and Producer roles remain meaningful

- Ordinary talk retains its existing zero-phase/first-person-per-day policy unless separately retuned. It does not become a production tool.
- Shared work/time is a social action taking one phase/player energy; worker output equals the same normal phase spent elsewhere, and no direct Stress recovery is attached. Proposed starting reward: affection up to +3 on the first completed shared-time scene per person/day (same base reward Day or Night), plus remembered topics/experience flags. Costs and shared reward gates require 30-day tuning.
- Shared-time rewards depend on completing time/interaction, not positive helper output or whether a task was ready. Repeats may replay without rewards or offer an explicitly different authored scene; no infinite score farm. Cancelling before commitment restores costs/effects under the existing transaction approach.
- Shared time should expose personal interests, small vulnerabilities and future callbacks. Romantic advancement requires authored mutual moments/arc conditions, not filling an affection meter or manufacturing OT. A character may accept help professionally without romantic interest, or be romantically close while declining an unreasonable work request.
- Routine Producer duties can concern budget, client communication, brief decisions or review preparation and keep their own meaningful deadlines. No +1 to every worker, no direct Stress heal, no ordinary coordination-rate buff and no early S2 unlock. Income/own-work rewards are an economy question, not set by this paper model. Finishing duties early creates discretionary social time within the authored sprint cadence.
- Night OT remains optional, compensated and physically scheduled. Staying together at Night need not award more affection than daytime sharing. High Stress/availability may lead to refusal; no unique romantic route should require deliberate overwork.

## 10. Next review and later implementation boundaries

Review candidate C vs simpler event-only uncertainty, pressure/recovery values, routine handoffs, meaningful Producer duties and the shared-time reward/cost policy with the owner. Add gentle comfort/unsafe-load sensitivity cases before locking numbers. Then, only when implementation is requested, update data/state/save validation and the common resolver, followed by the deferred UX redesign and first-week scenario/contextual review.

Future checks: paired forecasts are side-effect free; seed/save/load/reordering stability; no scope deletion on resize; dependent waiting is not work/buffer; zero helper/Producer direct speed or heal; no automatic relationship compliance; one reaction per committed change; unchanged historical deadlines; readable forecast calibration; non-OT recovery; combined 30-day time/energy/Money/romance feasibility. Existing Session 032 engine tests validate only its old mechanics. No Unity test, new assets or UI implementation is claimed here.
