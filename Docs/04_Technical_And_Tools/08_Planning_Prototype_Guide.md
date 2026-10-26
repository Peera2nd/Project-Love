# Planning prototype — quick start and authoring

**Historical prototype:** Session 041 replaces this as the primary gameplay path. Use [Romance-first prototype](10_Romance_First_Prototype_Guide.md) for current play/authoring; retain this guide for regression/reference only.

Session 037 baseline, updated Session 038. Planning domain, compact queue board and Timeline are delivered in the separate prototype. See the [Timeline/UI guide](09_Timeline_And_Conversation_UI_Guide.md) for current gestures, icon effect rows and separate Rest/End Day controls. Contextual replanning reactions and balance remain later milestones.

## Open and try

Unity menu **Tools > Project Love > Open Planning Prototype**, or Dashboard button **เปิดแผนงาน Sprint ใหม่**, then Play. Direct scene: `Assets/_Project/Scenes/Gameplay/StudioPlanningPrototype.unity`. Start a fresh run in this scene; the original Studio scene remains the legacy comparison fixture. No new images are needed.

- Top-left board icon opens three worker queues. Cards show title/portrait, discipline stripe, real progress, readiness and internal due. A blocked card stays blocked even if moved first; the next ready alternative can run.
- Drag a card above/below another in the same worker queue. The card lifts with a placeholder/insertion line; edges scroll. Other-owner/outside drops restore it. Escape cancels a drag; ordinary Back discards the draft and exits.
- Click a card for its forecast range and blocking predecessor/decision. Clock/cup icons shift its internal due by one day in Queue or one phase in Timeline; the priority icon provides a non-drag move-to-first fallback. Shortening due changes pressure, never remaining effort or a direct speed multiplier.
- Top-right clipboard check applies the complete draft; nearby return icon reverts edits. Bottom-right Back matches VN. Applying is free and does not advance time. Saving while a board/modal is open remains blocked; drafts are not saved.
- Producer icon at board bottom completes one duty action. Its meter shows the earliest released unfinished duty. Costs/rewards are in its hover hint. A dirty draft disables administrative actions until applied/discarded.
- Three right-hand status cards remain visible in room, board, inventory and conversation: affection, Stress and current-sprint discipline effort/required quota. Quota progress is not a second copy of the selected task. Room navigation uses supplied icons and hover hints; auxiliary menus have their own shared corner Back.
- Character Work now means shared time: one phase/player energy, first per-person/day affection, zero extra task effort and zero direct Stress recovery. Normal team work can still advance during that phase. Daily talk/gifts keep their own independent rules.

## Implemented domain

`PlanningConfigSO` on `ProductionConfigSO.planning` opts into the new resolver. Static content remains ScriptableObjects; `PlanningState` stores only seed, plan/dates, fractional effort and participation/day/OT ledgers. The original production mode uses its prior resolver. All paths retain the same phase boundary/event scheduler; no parallel clock exists.

- Team works Morning/Daytime/Evening, Night only with an existing paid individual OT order. Rest orders suppress that person's output for the phase. Planned release/start, actual prerequisites and explicit Producer decisions constrain work.
- Routine completion auto-accepts at the boundary. Dependents begin no earlier than the next phase. `WorkTaskSO.requiresManualReview` retains an explicit review checkpoint, including at sprint deadlines; selected completed checkpoints expose acceptance.
- Sprints release on authored start days. Producer completion never opens a future sprint, and unfinished Producer duties do not freeze the team. Work credit stays with its task's authored sprint, including late carryover.
- Producer earns only own duty effort/Money. The old team +1/coordination buff is excluded in planning mode. Shared participation/reward no longer requires positive helper units; actual shared OT can still drive existing next-day callbacks.
- Candidate C: stable bounded task factor and worker/day factor, Stress-dependent mean and daily spread. Actual keyed draws use a persisted campaign seed; preview/load cannot consume/reroll a random cursor. Float residue is stored separately from legacy integer effort.
- Pressure compares pending released work's estimated required productive slots with allowance before internal due. Overdue work can accumulate pressure until replanned. Normal/pressure/waiting/recovery coefficients are editable; no final balance claim.

## Editable data and save boundary

`Data/Planning/PlanningCampaign.asset`, `PlanningProduction.asset`, `PlanningRules.asset`, `PlanningPresentation.asset`. The production clone references existing tasks/sprints/workers/events rather than duplicating story content. Changing those shared definitions affects both prototypes; duplicate content before experimenting with separate catalogs. `Data/Settings/PlanningSeed.json` creates defaults only on first build; subsequent menu opens preserve authored assets.

Rules contain task spread, Stress bands, work/wait/recovery/affection coefficients, forecast sample count and drag threshold/scroll speed. Existing production config contains base rates, cost/reward and rest/OT economics. Thai game text remains presentation data. `useProducerStages` still enables Producer duty tracking; non-null planning rules replace its former early-unlock meaning with calendar release.

Planning saves use envelope **v7** and an additional `_planning_v7` filename identity. Prior Studio v6/v5 and legacy slots are preserved separately; no automatic content migration is claimed. Catalog mismatch or invalid ledgers/dates/fractions reject safely. Save/reload retains actual future outcomes.

## Forecast and trial limits

The compact forecast uses 24 independent reference paths and displays p10–p90, without reading the actual campaign seed. It holds current Stress means and known rules/absence/rest/OT assumptions; future decisions, hidden events and evolving Stress are not fully simulated. This is an estimate, not a calibrated confidence interval or promise. Unresolved manual/decision work can show unknown.

Four fixed-seed all-event 30-day diagnostic policies complete 120 phases/five reviews/five Producer duties. Fixed dues reach 88% scope; a daily carryover-buffer replan reaches 97%, with lower final Code Stress (77 vs 100). All decline voluntary OT. These are reproducible diagnostics, not statistical balance/optional-OT feasibility approval; the current Code catalog still needs capacity/recovery tuning. Do not transfer the old helper-based 100% results to this mode.

Timeline move/resize is delivered in Session 038. Plan-change conversation batching, final first-week narrative, complete romance routes, full keyboard/gamepad navigation and measured live-frame performance remain pending. Real pointer/render checks establish technical interaction; the owner still needs to assess readability and feel. See [Session 037 evidence](../00_Agent_System/WorkLogs/Session_037_Verification/README.md).
