# Studio Work, Sprints and Character Interaction

Session 034 records a new owner direction: relationship-only assistance, no early Producer sprint unlock, and planning/forecast/buffer gameplay. These are pending changes; the implemented/history text below describes the old prototype. See [current discussion](06_Sprint_Planning_And_Buffer_Proposal.md).

Updated Session 027, 2026-10-24. This file separates owner direction, proposed rules and the current prototype. Backend specification is English; story drafts remain [Thai](../02_Narrative_And_World/01_Studio_Event_Ideas.md).

The [numeric proposal](04_Work_Rates_Stress_And_Producer_Proposal.md) is now authorized and implemented as a tunable prototype. See the [Studio guide](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md) for the actual task/clock/Producer/save contract and [Session 026 verification](../00_Agent_System/WorkLogs/Session_026_Verification/README.md). Historical Session 023 proposal text below records its origin; it does not override the implemented contract or imply final balance approval.

## Owner direction

- The studio theme should affect decisions: Code, Art, Design and Producer work have separate progress, alongside the overall project.
- Weekly minimums across Code/Art/Design provide short-term goals. Weekly workloads can vary; events can add sudden work such as bugs. Specific quotas, schedules and consequences are not finalized.
- The team performs baseline work independently; the player helps bottlenecks or does Producer work. Confirmed Session 022. Session 023 confirms normal autonomous work in Morning/Daytime/Evening only, with Night off. Autonomous production is implemented in the separate Session 026 Studio scene.
- Clicking a character immediately opens their VN presentation with Work, Talk and Gift choices. Tap the screen to reveal/advance; use a bottom-right arrow to go back.
- Ordinary talk consumes no phase. First completed talk **per character per day** uses a little Stamina and grants a little affection; further talk that day is free and grants no further reward. This supersedes Session 015's provisional one-phase rule for ordinary talk. Story-event duration remains independently authored.
- Conversation varies with affection, day, phase and game events. Gifts consume owned items and give character/item-specific affection and reactions for liked, neutral and disliked items.
- A late-campaign character-absence event is an idea, not an approved scheduled scene. See the Thai event draft.

## Proposed work loop

At the beginning of a sprint, show the minimum deliverables by discipline and the available capacity. During each day the player identifies bottlenecks, helps a character, performs Producer tasks, rests or socializes. Tasks advance on a small Kanban board. The sprint review explains what shipped, what is blocked and what carries forward.

Recommended board: **Backlog -> In Progress -> Review -> Done**. Each card has stable ID, discipline, owner, remaining effort, completion criteria and optional dependencies/event origin. The Session 023 prototype proposal uses free batch acceptance of ready cards; weekly review resolves all ready cards before evaluating quotas, so completed work cannot be hidden to bank next week's quota. This review logic is implemented in Session 026; final consequences remain draft.

Use a small number of readable deliverables rather than recreating a real issue tracker. Examples: a playable input feature, a character-art set, or a level-rule specification. These are replaceable fixtures, not final project scope. Work selection should explain its energy, phase and expected output before commitment.

### Overall progress and weekly progress

Keep one source of accepted task completion. The same accepted work contributes to both its sprint quota and overall base-project progress; it is not two separate purchases of effort.

- **Overall:** weighted accepted base-scope work toward the 30-day release. Completed base work stays earned. Reaching 100% does not automatically resolve critical defects or complete character stories.
- **Weekly:** show each discipline's delivered/required units independently. Example only: Code 10/12, Art 14/10, Design 6/8. Excess Art does not hide missing Code/Design. Per-sprint quotas and weights belong in data.
- **Bugs/rework:** add visible tasks and, where authored, required stability gates. Do not silently erase unrelated accepted Art/Design or move the completion bar merely because the player is ahead. Show the event-caused quota change and its reason.
- **Producer:** prioritize, clarify scope, unblock dependencies and prepare reviews. Recommend that this improves team capacity/reduces avoidable rework rather than becoming an equal fourth mandatory weekly quota. Whether it also grants base-project progress is still open.
- **Windows:** propose reviews on Days 7/14/21/28 and final integration/release on Days 29–30. These dates are not approved constants. A short test sprint can run inside the 30-day campaign without shortening it.
- **Shortfall:** review with feedback, visible carryover and a recovery choice. A routine weekly miss should not cause immediate game over. Scope reduction, reassignment, catch-up or bounded extra effort can provide tradeoffs; specific penalties remain unbalanced.

### Autonomous capacity and help

The team's baseline work must be sufficient to make them feel like capable colleagues. Player help changes priorities or accelerates a bottleneck; the player should not have to personally manufacture every discipline's output. Session 023 owner chooses a phase-based normal work schedule: Morning, Daytime and Evening produce output when each phase passes; Night has no normal team output. Overtime would be a separate, currently undesigned mechanic.

Model base capacity, assignment, blocked work and temporary availability in data. Resolve passive output at one explicit clock boundary and record it once; menus, repeated talk, loads and repeated review openings must never duplicate a production tick. Player help and passive work must consume shared remaining task effort so they cannot both finish the same units twice. Define phase/day ordering and final-day handling before implementing automated output.

Do not make friendship a permanent productivity tax. Ordinary talk now uses energy without time, so evaluate energy scarcity against only four work/rest phases. At the current trial values (work 20, overnight 40), resource pressure may be weak in some schedules; a larger meter is not evidence of depth.

### Workload events

Use configurable event effects: add/change a task, change a quota with a visible reason, temporarily modify capacity/availability, or block a dependency. Store origin, application state and expiry so save/load cannot duplicate a bug spike or extend an absence. Provide a preview/notification and clear recovery options. Final-day events need adequate response windows.

Do not use high progress solely as an invisible difficulty trigger. The owner's late absence idea can create a story and management tradeoff, but its timing should have foreshadowing, a safe window, a one-time gate and feasible alternate staffing/scope recovery. Relationship opportunities should remain available to a less efficient player as well. Specific trigger, reward and route behavior remain proposals.

## Sprint prototype proposal — Session 023

Owner asks to settle gameplay before further runtime work. The following makes the earlier outline concrete for discussion; numeric rates, quotas, task lists, review cost and consequences are not approvals.

### What the player does

| Discipline | Task examples | Character/help entry | Role in the loop |
| --- | --- | --- | --- |
| Art | Character art set, environment pass, UI illustration | Plaifah | Player helps the currently assigned Art task; accepted work contributes Art delivery |
| Code | Input feature, dialogue integration, bug fix | Grace | Player helps a Code bottleneck; a bug task is visible work rather than arbitrary loss of Art/Design progress |
| Design | Tutorial flow, encounter/rule specification, balance pass | Ploy | Player helps the Design deliverable; dependencies can later connect a specification to Code/Art |
| Producer | Clarify a brief, coordinate a dependency, organize priorities | Player desk/board | Suggested support actions unblock or improve a named team's work; avoid a mandatory fourth quota for the first sprint |

Start with one week and about 6–9 small cards across the three disciplines, one active card per worker, and readable per-discipline minimums. The campaign remains 30 days. The team follows assigned/prioritized eligible work automatically; the player changes priorities or helps, rather than clicking every unit into existence. Start with one easily explained dependency, not a large project graph. Task names are temporary content.

Inspecting/reordering the board and selecting a task are proposed as free. Helping a worker remains a one-phase authored activity using player Stamina. The helper works on the same remaining effort as the worker, capped at that effort, so progress cannot overflow into unrelated work without explicit allocation. Recommend a simple fixed output for the first prototype before designing discipline-specific minigames.

Use **effort units** for card work and **accepted deliverables/weights** for overall completion; label both clearly. Work-in-progress remains visible without falsely claiming a deliverable has shipped. Ready cards enter Review; propose free batch acceptance. Weekly review accepts ready work first, checks Code/Art/Design separately, and shows unfinished carryover. Credit a task to one completion ledger and one sprint only; carried unfinished work is credited on eventual acceptance, not twice. The same accepted base-scope task feeds overall progress once. Final numeric targets require capacity/schedule trials.

Sprint shortfall should expose carryover/scope/recovery choices, with no routine instant game over. A bug event adds an identified Code card and an explicit quota/critical-fix change. Keep already accepted unrelated work earned. Session 026 implements carryover, the bug demo and bounded Producer effects from the numeric proposal; scope/recovery decision content remains open. Generic Producer +6 and direct aggregate effects remain legacy-fixture placeholders only.

### Proposed phase resolver and end-day handling

The normal three-work-phase schedule is owner-approved; the following transaction ordering is a proposal to implement/test:

1. Snapshot the current phase, eligible workers/tasks, availability, stress effects and existing dependencies. Explain blocked/unassigned/absent workers in the forecast.
2. Apply the completed player activity and its validated costs/effects. Add helper effort to its selected card, capped at remaining effort.
3. For a productive phase (Morning/Daytime/Evening), apply each eligible worker's baseline once to that card's remaining effort. Never apply normal output at Night. Reaching completion moves the card into Review. Newly unlocked work starts next phase rather than chaining free output inside one tick.
4. Record the resolved phase/worker output, publish the combined result and advance the clock. Boundary events apply explicitly for subsequent work, not retroactively to the output just previewed.

Resting still passes a phase while the team performs normal work. Zero-phase talk, gifts and menus do not generate team work. A worker's baseline and the player's help are displayed separately and share the same effort cap.

Propose that ending the day early resolves the remaining normal work phases through this same resolver with no extra player actions; it must not delete the team's workday or duplicate earlier ticks. Due events must still be processed in order. Night produces no normal tick. Save/load, cancellation, final-day evaluation, temporary absence and early end-day need explicit once-only regression coverage before delivery. No overtime or absence implementation is implied by this schedule decision.

### Deferred character status and work preview — owner request

**Required and recorded; intentionally not implemented in Session 023.** Before choosing Work, the character VN screen must show:

- Current affection and the character's Stress, clearly distinct from player Stamina.
- Selected task/discipline, current effort versus target, and the relevant sprint delivered/minimum. Keep overall project progress distinguishable from these figures.
- Exact player help output, resource/phase cost and character stress change before committing. Show resulting values and cap effective output to remaining effort/stat limits; do not advertise +6 when only +2 can apply.
- Separate normal team output forecast for the passing phase. Example only: task 8/20, help +4, normal worker +2 -> forecast 14/20; never label the whole +6 as player help. Explain a zero forecast for Night/block/absence/completed work.
- A clear unavailable reason when work is blocked, unaffordable or not assigned. After completion, show the actual deltas so the preview can be checked.

Derive the preview from the same authored data/transaction calculation used by completion, and revalidate at commit. A future uncertain minigame/event result needs an explicitly labeled estimate/range. Missing task/sprint systems should be represented honestly until implemented, not replaced with fabricated goals. The legacy IsometricPrototype menu keeps its old display; StudioSprintPrototype implements this complete forecast panel.

## Character interaction contract

Opening the character view is free and displays a contextual greeting plus the three actions. It does not complete a daily interaction. Work uses the selected character's authored activity/discipline and closes to the room when completed. The desk is the separate Producer-work entry point in the current fixture.

Talk selection and reward frequency are independent. Choose the highest-priority eligible variant, with stable ID as a deterministic tie-breaker, from day/phase/affection/flag/story conditions. Always provide a fallback. Choose the variant at entry and keep it stable while reading. Current authoring selects one variant; a topic browser, no-repeat rotation and a large content library remain future work.

The first talk reserves its authored energy and awards affection/daily completion only when the interaction finishes. Back/defer restores the full interaction-start state. Repeat talk applies no choice costs, stat effects or flags; its choices may branch and use conditions. Story events use the existing transactional runner with their authored costs/effects. Do not mix story progression into a repeatable small-talk asset.

Gift UI lists giftable inventory, not shop stock. The response is selected from the character/item preference, with neutral fallback. Completing it consumes one item and applies the authored affection change; cancellation restores everything. Gifts do not restore player energy as if consumed. Phase/energy cost is zero in this prototype. There is currently no per-day gift cap; repeated gifts consume finite stock/money. A cap/diminishing-return policy remains undecided and must not be silently inferred from the talk rule.

Screen taps first reveal all text, then advance. Tapping while choices are awaiting input never chooses one. The arrow returns Gift selection -> character menu -> room, or cancels the current unfinished interaction back to its menu. Legacy scene buttons remain unchanged. A pending interaction blocks background actions, saving and day transitions.

## Session 027 current rules

Owner explicitly moves worker rest from board to a zero-time character order for the current phase. Night OT is also a zero-time per-person request, paid/stressed immediately and resolved once at Night close. OT talk depends on individual personality, affection and Stress; daily reward policy is unchanged.

Producer work now has money/own-progress rewards and capped +1 support per available discipline. Completing a Producer stage is mandatory for the next sprint's task unlock; the calendar cannot bypass it. Early and late tasks credit their own authored sprint. Deadline reports remain immutable at their cutoff even when later work is accepted. See the [current Studio contract](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md) for exact ordering, exceptions, editable defaults and save v5. Old numeric/action descriptions below/above are historical prototype iterations when labeled by session.

Role-colored cards, owner portraits and simple bars are implemented; [requested UI art](../03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md) is pending. Four 30-day policies finish the base project with Producer gates; both bug policies miss the first deadline and recover later. Full OT-economy/narrative balance remains unvalidated.

## Session 026 historical boundary

Session 026 delivers the separately configured Studio scene: three workers, 39 base cards plus optional bug, editable sprints/quotas, four Kanban columns, priority/compatible assignment, ready/deadline acceptance, carryover, accepted-weight overall progress, three-phase production, capped help/Stress, finite Producer actions and truthful character status. Studio save v4 persists its ledger/buffs/events/absence/reviews/cursor; old room/AP fixtures and saves stay separate.

The owner authorizes the Session 025 numeric proposal for prototyping, superseding the earlier implementation hold. Four engine schedules finish 30 days with/without a bug and with/without daily talk to all three. Missed-review/carryover, generic absence and rendered mouse flows are tested. These are implementation/feasibility results, not final narrative or difficulty approval.

Next: owner playtest, readable workload notification/recovery choices, first authored sprint/dialogue/event content and varied weekly emphasis, with renewed capacity checks. Fine furniture occlusion, minigames, skills and ending evaluation remain pending. No Grace illness trigger is authored. See the [Studio guide](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md).
