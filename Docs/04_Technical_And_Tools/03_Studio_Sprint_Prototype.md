# Studio Sprint Prototype

Session 034 direction changes the intended help/Producer/planning model, but runtime remains unchanged. Existing values below are current implementation, not approval of the future model. See [planning discussion](../01_Game_Design/06_Sprint_Planning_And_Buffer_Proposal.md).

Updated Session 032. Owner-approved rules are implemented with tunable trial numbers. No final narrative, minigame or balance approval is implied. [Verification](../00_Agent_System/WorkLogs/Session_031_Verification/README.md) · [UI asset request](../03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md).

## Open and controls

Open `Tools > Project Love > Dashboard`, use the Studio Sprint button, then Play. Scene: `Assets/_Project/Scenes/Gameplay/StudioSprintPrototype.unity`. Start a new Studio run for the event catalog; old Studio v4/v5 files remain in their own slot namespaces and are not converted. Legacy AP/Isometric scenes retain their rules.

Click Grace/Code, Plaifah/Art or Ploy/Design for Work/Talk/Gift and a fourth context action. The four standard actions fit the expanded Studio menu; longer gift/choice lists still scroll. View affection, worker Stress, task/goal, sprint delivery and exact capped help/normal output before work. Existing daily talk/gift/cancellation rules remain unchanged.

The fourth action during Morning/Daytime/Evening is **rest for this phase**. It is a free order, keeps the menu open and does not advance time. Order several people if desired. At the phase boundary each ordered worker recovers Stress and does no work; others continue. Direct help is disabled for the resting worker; their Producer bonus is also zero. The order expires at that boundary. No worker-break command remains on Kanban. The sofa remains the separate player-Stamina recovery action.

At Night the fourth action is **request OT**. Everyone normally rests. A valid request pays that worker's fee and adds Stress immediately, without advancing time or generating effort. One order per worker that Night; multiple workers can agree. Normal talk then selects that person's OT variant using affection and current Stress, independently from the first-daily reward gate. Finish/cancel talk and pass Night, or choose Work to help one ordered worker, to apply OT output once. The fee is not paid again; there is no extra normal-work Stress increment on top of the OT request penalty. Orders are committed decisions, with no undo toggle in this prototype.

Open the Sprint button/desk for Kanban. Inspect, prioritize/compatible-assign and review ready cards for free. Cards now use supplied owner portraits, role icons/colors, shared tinted progress bars and an interactive dependency inspector. See [UI/dependency controls](04_Studio_UI_And_Dependency_Guide.md). Producer's right panel includes work, own progress, coordinate and clarify; worker status is informational. The optional first-week bug demo remains available. Board/active conversation block background actions and saving.

## Event trial

Start a new run, End Day, then Next Day. On Day 2 Ploy opens a short forced scene. Tap to finish; Grace and Plaifah then have independent temporary `!` bubbles above their phase poses. Click the bubble for the event or the character body for ordinary Work/Talk/Gift/rest/OT.

Grace's bug choice adds the once-only Code +24 workload and changes the sprint target to 104. Worker affection/Stress/current task/quota remain visible during a worker-owned story scene. Back after the choice rolls the added work and flags back; finish the result line to commit. A later callback opens Days 3–7 after completion. Plaifah's separate scene gives one trial affection point without time passing. These are temporary test scenes, not locked narrative.

Back from a forced scene suppresses automatic reopening for the current phase and exposes a retry bubble. Other forced events still take their turns. The next eligible phase re-arms it. End Day pauses at newly eligible forced scenes; it does not silently skip their phase. Autosaves wait for a safe non-modal point. See [event contract](../01_Game_Design/05_Event_Scheduling_And_Studio_Integration.md) for editable assets, priorities, exclusions and exact limits.

## Volunteer and shared Night work

Near a ready task’s own sprint deadline, a worker may offer compensated OT through the normal bubble queue. Choose to join, allow OT without time passing, or decline without penalty. Joining consumes one Night, produces capped real task work and adds trial affection; a later Morning callback remembers actual shared work. Back rolls the whole event back. See [Night controls and editable trial data](05_Studio_Night_Work_Guide.md). Start a fresh run for the updated mechanical catalogs.

## Producer work and stage gates

Producer work consumes one phase and Stamina, earns money and own progress, and contributes up to +1 to one current card per discipline. Bonuses share the card cap with normal work, do not spill into another card and are zero for resting/absent/blocked/no-task workers. Night normal output still requires OT; the Producer's own support contribution is distinct from worker output. The action preview shows actual bonuses, rather than promising +1 where none can apply.

S1 worker tasks begin unlocked. Completing the Producer goal for S1 unlocks S2 tasks immediately; completing S2's Producer goal unlocks S3, etc. **The calendar never bypasses this gate.** Normal sprint review dates remain authored separately. A gate can open before normal output in the completion action, letting an idle worker start the new stage that tick. Producer effort does not spill across its own stage boundary either. The final Producer stage completes its own work without another unlock.

Each task references its owning `SprintDataSO`. Early S2 work credits S2, not the current calendar sprint. Late S1 work still belongs to S1 and contributes main progress, while its already-recorded deadline result remains unchanged. Ready cards are accepted before each calendar deadline report. Work-in-progress stays visible; accepted base weight alone feeds main progress. A missed weekly target is recoverable, with no routine instant game over.

Trial defaults:

| Action / rule | Value |
| --- | --- |
| Worker baseline | 4 each Morning/Daytime/Evening; Stress tiers 4/3/2/1, actual normal work Stress +2, overnight −4 |
| Help | 20 Stamina / one productive phase; up to 6 effort, target Stress −3 before baseline |
| Worker rest order | Zero time/energy/money; Stress −5 at current phase close; zero target output |
| OT request | Night only, money −5/person and Stress +10 immediately; zero time/energy; stress-adjusted normal rate at Night close |
| Producer work | 20 Stamina / one phase; money +10, own effort +10, up to +1 per available discipline |
| Producer goals | S1–S4: 40 each; final stage: 20 |
| Coordinate / clarify | Prior finite +1 plan over two productive ticks for 15; explicit decision clear for 15; neither bypasses specialist prerequisites |
| Player rest / overnight | One phase +30 Stamina; overnight +40, cap 100; no unrelated worker Stress reward |
| Ordinary talk | Zero phases; first completion/person/day −3 Stamina/+2 affection, repeats free/no further reward |

Money/progress rewards are fixed data in this prototype. A future minigame result should feed the same preview/transaction with authored rewards/grades; no minigame is implemented here. Actual quotas and full-campaign balance remain trial content.

## Authoring and state

Assets: `Assets/_Project/Data/Studio/`. `StudioProduction.asset` holds costs, Stress/OT/rest/Producer settings and catalogs; sprint assets hold Producer goals and weekly quotas; task assets hold owning sprint/effort/weight/prerequisites; characters hold `ConversationVariant` affection/Stress ranges, Night/OT condition and dialogue refs. OT lines are temporary Thai content. WorkStreamSO holds editable role tint/icon; character taskAvatar and presentation icon slots accept future art. Session 031 binds the supplied task-owner heads and icons.

`StudioManagementSeed.json` initializes the one-time management upgrade. Version-stamped Editor migration preserves authored settings on rerun. Required UI export folders are prepared at `Art/UI/Mockup/{Icons,Portraits,Bars,Panels}/`. The full brief specifies 24 core icons, three owner portraits, a shared grayscale bar sheet and four panel/frame assets. Session 031 owner readiness supersedes that hold: supplied UI is integrated, including the canonical sliced bar sheet. PNG bytes and existing GUIDs are preserved.

`ProductionState.orders` stores current-phase per-worker rest/OT decisions; `producerStages` stores own effort and completion day/phase. The shared resolver consumes orders exactly once. Forecasts use clones, costs are revalidated at commit, and full dialogue deferral snapshots preserve already-committed orders correctly. Main progress cannot be arbitrarily incremented by Studio story choices.

Studio with scheduled events uses **v6** and an additional event key suffix; configs without an event catalog retain **v5**, preserving config IDs and old v4/v5 files. Production catalog/identity/resource/checksum/ledger/cursor/order/stage validation rejects incompatible state instead of inventing task or Producer history. v1–v3 AP/room migrations remain supported in their original configs. No mid-dialogue/board saves. Definitions are never mutated by play; verification font-cache growth is excluded from delivery.

## Limits and next work

See [Session 031 verification](../00_Agent_System/WorkLogs/Session_031_Verification/README.md) for current tests, mouse flows, previews and six 30-day policies with the S1 chain, including two with all four events enabled. Prior four capacity policies are preserved as isolated no-story regressions. These show feasible/recoverable schedules, not final narrative/route/OT economy balance. No original live Console/standalone build claim.

Session 031 implements the S1 cross-discipline chain and readable predecessor/successor/ready-alternative inspection. Session 032 delivers volunteer OT/joint Night scenes through the same event foundation. Next: coherent first-sprint narrative/contextual review, further dependency content and balance. Final illustrations/foreground depth, richer topics/events, minigames, skills and ending logic remain pending. No illness trigger is authored.
