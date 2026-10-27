# 00 — Master Vision and Decisions

Canonical approval record. Implementation status belongs in the [Task Board](../00_Agent_System/02_Task_Board_And_Roadmap.md). A completed document is not implemented or approved content.

## Session 071 owner trial — one writing tool, concise English chrome and visible execution

Owner tried the sandbox and finds a wrapper around another authoring tool insufficient. Requires one surface for writing/adding/reordering/drag-inserting dialogue, speaker/expression and route context, plus readable caller logic, conditions, choice results and actual stored state. New/revised authoring UI uses concise English labels with hover help instead of long always-visible explanation; Thai narrative/game text and conversation remain. This supersedes older Thai-facing-tool wording for this workspace. [Unified sandbox/workflow](../04_Technical_And_Tools/17_Unified_Story_Workspace_And_Execution_Trace.md) implements the bounded revision; explicit production adoption/UX approval is still not given. Audio remains planned.

## Session 070 owner request — trial guidance and designer-first audio planning

Owner acknowledges the sandbox and intends to test, requests a practical test guide, then asks a comprehensive audio plan covering music, effects and optional partial speech. An empty voice slot must play no speech. Discovery must avoid a hundreds-item picker when assigning a character's voice; use the current speaker and sound intent/context. [Trial guide](../02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md), [audio proposal](../04_Technical_And_Tools/16_Audio_Authoring_And_Playback_Plan.md). This authorizes guidance/research/design, not audio implementation, exact UI/schema approval, production adoption, vendor selection or approval of the existing sandbox UX. The isolation gate remains.

## Session 069 owner authorization — repository and isolated authoring trial

Owner supplies the new [Project-Love repository](https://github.com/Peera2nd/Project-Love) and authorizes setup. Preserve the existing game baseline on `main`; develop the new Story Workspace in an isolated sandbox first, assess its real appearance/use and clarity, then adopt only after owner acceptance. Isolation and one-way dependencies are required to avoid tightly coupled experimental code. This authorizes the sandbox implementation, not a production migration, backend purchase or broad narrative rewrite. [Sandbox workflow](../../Sandbox/StoryWorkspace/README.md).

## Session 068 owner clarification — context navigation and discoverability

Owner clarifies that the concern about many SOs is designer authoring/search/overview UX, not game speed. Selecting a passage should reveal all upstream sources, every choice's destination/continuation, relevant conditions and all speaking characters; the designer must find a character's situation-specific dialogue without hunting assets. These are required outcomes for the [planned Story Workspace](../04_Technical_And_Tools/15_Scalable_Story_Authoring_And_Presentation_Plan.md), section 4.1/Stage A. Static possible paths and detached context-tested availability must be distinguished. This clarifies authoring requirements, not a runtime/backend selection or implementation-start instruction.

## Session 067 owner request — scalable authoring research and planning

Owner requests agent research into professional VN tools/engines and a designer-friendly plan for hundreds/thousands of lines, immediate overview/search, manageable source granularity and future shake/emote/entry-exit transitions. Research/design is authorized; no package/vendor, engine switch, runtime rewrite, asset migration or exact UI/schema is approved by this request. [Resulting plan](../04_Technical_And_Tools/15_Scalable_Story_Authoring_And_Presentation_Plan.md), [sources/comparison](../04_Technical_And_Tools/Research/01_VN_Authoring_Systems_Comparison.md). Continue preserving the existing story/gameplay/layout/localization; proposed Stage A is a complete-event authoring proof before broader migration.

## Session 064 owner authorization — implement localization now

After clarification that the translation tab was only designed, owner explicitly says to start. Implemented the first Thai/English adoption with the actual Workspace tab, catalog authoring/CSV/preview, current Romance opt-in and guarded separate v9 saves. Preserve story, gameplay values, source strings, layout and GUIDs. Initial English examples are drafts, not owner-approved full translations. [Adoption register](../04_Technical_And_Tools/13_Localization_Architecture_And_Adoption_Contract.md), [workflow](../04_Technical_And_Tools/14_Localization_Authoring_Guide.md). Continue reporting real adoption and gaps under Session 063; no Producer scope or narrative/balance approval changes.

## Session 063 owner direction — preserve baseline; adopt Thai/English localization in the next version

Owner instructs keeping the existing structure, designing the replacement text/localization structure now, adopting it during subsequent version development and informing continuing agents. Initial game languages are **Thai and English**. Record Thai as source/default/fallback and require the adopting agent to report actual migration to the owner. This is a standing next-version transition direction, superseding Session 062's unapproved-future-gap status; it does not request an immediate running-project rewrite or approve English scripts.

The [Localization Architecture and Adoption Contract](../04_Technical_And_Tools/13_Localization_Architecture_And_Adoption_Contract.md) is designed under this direction: a single game graph, separate text catalogs, stable identities, artist-facing tools, legacy support and checked save migration. AGENTS rule 10 makes it required intake for the next playable/content/system version adding/revising player text; it includes implementation of the foundation before new single-language content grows. Adoption/verification/remaining legacy and translation gaps must be recorded and reported explicitly. Current runtime remains legacy; backend documentation/user-conversation language policy, narrative approvals and Producer deferral are unchanged.

## Session 061 owner direction — everyday identity, sweet romance and quiet emotion

Owner endorses natural storytelling with sweet romance and emotionally moving drama that does not need crying scenes or major quarrels. Record the supplied character details as approved authoring inputs: **Plaifah** retains comically meticulous perfectionism and delicious snacks, and is dreamy with interests in space and underwater worlds; **Grace** retains romantic manga, occasional cute/deadpan moments, can be a mature Producer thinking partner on finance and people management, and genuinely values exercise/healthy food while sometimes concentrating on necessary project work; **Ploy** is authentically cheerful/energetic, can simply be tired of carrying the laughter while tired herself, likes fortune-telling/tarot and strange playful debates with close people more than large parties. Her ENTP label is an owner-provided writing cue, not a scientific claim or party-seeking rule.

Apply these through everyday conversation, reciprocal care and remembered details. Do not treat Ploy's joy as a false mask or require personality reversal/tears to establish depth; do not reduce Grace to code jokes or mandatory burnout, or Plaifah to self-doubt. [Thai current authoring guide](../02_Narrative_And_World/07_Character_Everyday_Interests_And_Story_Tone.md) distinguishes owner details from illustrative scene ideas. All three profiles and the story spine now reference this direction; conflicting older dramatic samples are historical, not default reactions. Session 060 Plaifah route direction remains; no new approval of every old plot line, scientific/market claim, mechanic or runtime change is implied. Producer systems remain deferred and the Session 059 opening is preserved.

## Session 060 owner decision — modern non-melodramatic storytelling & Plaifah 8-anime route direction

Owner explicitly endorses the modern storytelling direction synthesized in Session 058 from 8 anime/manga case studies (*Wotakoi*, *Gekkan Shoujo Nozaki-kun*, *Kimi ni Todoke*, *Dungeon Meshi*, *Campfire Cooking in Another World*, *Frieren*, *Bakuman*, *Blue Period*): **engaging, deeply memorable modern storytelling does not require crying breakdowns or melodramatic self-pity (`ไม่จำเป็นต้องร้องไห้ฟูมฟาย`); it comes from portraying a character's authentic identity, everyday craft, and quiet inner feelings so the audience feels they are truly alive.**

This supersedes the earlier *"When the Brief Ends"* / crying-breakdown drafts for Plaifah. Plaifah's endorsed 30-day route direction (*"The Unspoken Canvas / เงาสะท้อนในเฟรมสุดท้าย"*) combines: (1) *Nozaki-kun* deadpan romantic-reference experiments in the studio, (2) *Campfire Cooking / Dungeon Meshi* tactile art-process details and late-night snack comfort loops, (3) *Bakuman / Blue Period* strategic Producer-Artist partnership (challenging her past "formula-copying" contest win by championing her warm first-person observational POV sketches, and treating the final wordless Ending CG as emotional nakedness rather than burnout), and (4) *Frieren* delayed emotional resonance (on Day 30 Launch Day, the Producer inspects the full 30-day gallery and discovers she quietly painted their shared memories and his reflection into every background and final CG). Recorded in [Synthesis Section 7](../02_Narrative_And_World/Research_References/05_Synthesis_And_Project_Love_Story_Proposals.md) and [30-Day Story Spine Section 6.1](../02_Narrative_And_World/05_Thirty_Day_Story_Spine_And_Character_Routes.md). This approves the narrative principle and route blueprint for authoring while preserving the Session 059 playable Days 1–3 opening slice.

## Session 059 owner authorization — playable story opening

Owner authorizes the Session 056 Days 1–3 opening proposal and reiterates proportionate verification. Author the group confession-script opening, one chosen personal vignette, remembered response, invitation and Day 3 payoff with the existing room/VN/events. Producer discovery/Wishlist remain deferred. This authorizes playable draft content, not final dialogue approval, selection of Plaifah's main wound or the full Session 057 plot.

Implemented data uses one exclusive opening companion without locking ordinary relationships, equal rewards for personal response alternatives, explicit appointment phases/windows, persistent greeting reminders and missed/declined invitation recovery. [Thai play/authoring guide](../02_Narrative_And_World/06_Opening_Story_Play_And_Authoring_Guide.md). Existing casual activity/Producer rates, art/layout and runtime/save schema remain unchanged; changed content requires a fresh playthrough for this trial.

## Session 057 narrative drafting request — proposals delivered

Owner requests reading current progress/research and developing an adaptable beginning-to-end plot. Delivered a [Thai 30-day shared spine, three character routes, ending composition and six-scene Week 1](../02_Narrative_And_World/05_Thirty_Day_Story_Spine_And_Character_Routes.md), plus a [research/evidence review](../02_Narrative_And_World/Research_References/06_Research_Review_And_Story_Design_Summary.md). This authorizes narrative-document drafting, not runtime implementation or approval of exact story details. Sessions 050–054 remain the accepted broad direction. Current runtime remains Session 046.

Plaifah's latest human-conflict options are recorded in Session 055; her profile retains rejected brief-versus-personal-art material. The new outline uses **When the Brief Ends** as a **worked proposal**, keeps the alternatives open, and marks conflicting profile/application passages. Review her replacement wound before reconciling the full profile. The Producer's end-of-month assignment, game-within-game, specific contact terms/dates and Ploy's explicit project-pass criterion are also proposed, not locked. Preserve Session 056 priority: trial the short story opening first, keep Producer gameplay deferred. The longer external-offer chain is a future expansion, not a prerequisite.

The review verifies four bounded primary-source claims and labels broader legacy research interpretations/unsupported audience assertions. It does not validate the entire reference library, psychological diagnoses, market demand or story enjoyment. No code/assets/save/numeric changes or engine tests.

## Session 056 owner priority — story events first

Owner explicitly defers the proposed Producer gameplay work and asks for ideas that make the first playthrough engaging through story events. Preserve the Session 054 design for later; do not start its discovery/wishlist implementation by default. Recommend a short Days 1–3 story slice using the existing room/VN/event tools: a group opening, chosen personal scene, next-day callback and near-term invitation/payoff. [Thai opening ideas](../02_Narrative_And_World/05_First_Play_Story_Event_Ideas.md). Specific scenes/dialogue and Plaifah's main conflict remain proposals awaiting review; this request is ideation, not runtime implementation.

## Session 054 owner agreement — Producer discovery loop

Owner says to record the preceding recommendation: team game completion and Producer launch-support work run in parallel; Producer routine work stops directly granting team progress in the proposed revision. Use a modest routine benefit plus context-eligible encounters, discover contacts and allow deliberate follow-up, and keep critical story opportunities authored rather than luck-gated. Trial with **one phase per Producer action**, deferring Stamina reintroduction. Recalibrate team completion feasibility without relying on Producer bonuses. Exact routine yield, encounter weights, targets/rewards and final narrative remain provisional. This turn authorizes recording and asks for the next-work recommendation; it is not a request to implement code now. [Bounded prototype plan](10_Producer_Opportunities_And_Romance_Integration_Plan.md).

## Session 053 owner brainstorm — parallel Producer discovery work

Owner proposes team-controlled game completion and Producer work focused on launch support/wishlists/external encounters rather than direct team progress. Examples include quiet routine work, creator promotion, encouragement, fan contributions and discovering a sponsor contact; Stamina cost is also suggested. These are proposals for evaluation, not implementation authorization or approved numeric/resource changes. [Assessment and existing-rule conflicts](10_Producer_Opportunities_And_Romance_Integration_Plan.md). Current time-only rules and rewards remain implemented until a revision is agreed; prior outcome/story directions remain in effect.

## Session 051 owner direction — outcomes beyond completion

Owner endorses final game quality/launch results affecting story beyond reaching 100% progress, with wishlist readiness as an example. Declining an advantageous offer can leave a meaningful success gap and motivate alternate opportunities. Accept the broad multi-factor outcome direction; exact metrics/targets/formulas/endings and new systems remain proposals, not implemented or numerically locked. [Outcome model and recovery proposal](10_Producer_Opportunities_And_Romance_Integration_Plan.md). Preserve romance-first clarity; production, audience readiness and relationship outcomes should remain understandable.

## Session 050 owner direction — combined studio romance and external opportunities

Owner endorses combining the research premises: a team inexperienced in romance making a romance game, personal confidences shared with the protagonist, and a shared crisis requiring **release within 30 days**, with issues each week. Publisher/client/localization/marketing/partner/industry/investor/creator contacts are proposed as sources of team discussions and meaningful decisions. Record the combined broad direction as accepted; do not treat every research secret, line, date, reward, commercial detail or new mechanic as approved. The research's demo/competition-only final objective is superseded by the owner's release goal; weekly demos can be intermediate checkpoints.

This turn requests analysis/application planning, not code or campaign authoring. [System/application plan](10_Producer_Opportunities_And_Romance_Integration_Plan.md), [Thai story/application outline](../02_Narrative_And_World/04_Combined_Studio_RomCom_And_Producer_Event_Plan.md). Exact first-week flow and specific character confidences still need authoring review; current romance-first simple gameplay/mood, 30-day configuration and artist-first/proportionate-verification rules remain.

## Session 044 owner decision — tangible artist authoring

Owner approves the authoring-tools upgrade and explicitly requires readable, directly editable data/UI instead of routine C# editing. This becomes a durable development rule: Thai-facing current-campaign search/context tools; persistent serialized content and real UI prefabs/scenes; Undo/Save; safe same-view Edit Mode previews; runtime respects authored layout. Initial generators may create missing assets but never overwrite later artist edits. Include the authoring workflow in feature acceptance. See AGENTS rule 8, registry GR-012 and the [implemented workflow](../04_Technical_And_Tools/12_Artist_Workspace_And_UI_Authoring_Guide.md). Technical verification does not equal owner usability acceptance. Session 042 story expansion remains a later proposal; no numeric, narrative or save-schema revision here.

## Session 042 feedback — clarity confirmed, engagement unresolved

Owner confirms the casual game is easy to understand but describes a linear, obstacle-free loop. Keep romance-first simplicity and real categorical mood. [Meaningful-choice proposal](09_Romantic_Tension_And_Meaningful_Choices_Proposal.md) explores shared memories/chapters, visible invitations/promises, contextual responses, small studio dilemmas and consequences/repair. No new mechanic, numeric change, event schedule or implementation is approved by this discussion. Current runtime remains Session 041; prior tests validate operation, not fun. Favorite-only play remains legitimate.

## Session 041 owner-approved primary direction

Owner authorizes the Session 040 casual redesign and clarifies **character mood, not Stress**. Romance/story/characters lead; studio management supplies a shared goal and character opportunities. [Current rules/implementation](../04_Technical_And_Tools/10_Romance_First_Prototype_Guide.md).

Primary loop uses time only: Work Together grants small project/affection rewards, Personal Time grants stronger affection/character scenes, Producer work grants direct project/Money with no separate duty ladder. Reliable team work proceeds at day end; optional weekly focus replaces required due/buffer/dependency management. This supersedes the former social-only helper/no-direct-Producer-output, time+Stamina and permanent Stress/work-dashboard rules for the primary experience. Mood is categorical, data-authored and does not reduce work speed. Ordinary talk keeps first-completed/person/day rewards and free zero-phase repeats, now without energy cost. Exact values and story remain prototype inputs, not final approval.

The historical table and earlier sessions below retain previous fixture decisions. Where they conflict, Session 041 governs current design. Planning/Studio prototypes remain preserved references, not an additional expert mode. Full duration is still 30 days; Days 1–3 provide the opening content trial. Session 042 owner confirms comprehension; engagement/challenge and final content approval remain pending.

## Current decisions — updated Session 041, 2026-10-25

| Topic | Status |
| --- | --- |
| Genre and presentation | **DECIDED, clarified Session 017:** Visual Novel + light management/life simulation; fixed isometric point-and-click room/scene views with VN conversation overlays; no walking-map exploration. |
| Primary interaction screen | **OWNER DIRECTION, Session 017:** isometric action scenes; HUD for money/time/Stamina/inventory/options/destinations. Phase-driven character poses/positions; contextual person/object/item hotspots; clerk interaction opens shopping. Conversation dims the scene and overlays a large character, dialogue and choices. No walking controller required. |
| Everyday conversation | **OWNER DECISION, Session 022:** available repeatedly; first completed talk per character per day uses a little Stamina and grants a little affection, then repeats are free without additional rewards. Zero phase cost. Context varies with affection/day/phase/events. Implemented trial 3 energy / +2 affection; content and balance remain draft. |
| Relationship conversation duration | **SUPERSEDED:** Session 015 one-phase ordinary conversation was provisional. Session 022 ordinary talk uses zero phases; story-event duration remains independently authored. No automatic duration rule for every narrative scene. |
| Activity time | **REQUIRED, Session 014:** completing work or rest advances one phase. Implemented in Session 019. Cancel/defer consumes no time; completing the last phase ends the current day and waits for explicit next-day input. Other action costs remain provisional. |
| Player action budget | **OWNER DIRECTION:** time plus Stamina. Work/rest use one phase; ordinary talk uses zero phases with first-daily energy only. Legacy AP configs remain supported; save v3 preserves resource identity and migrates v2, while v1 remains AP-only. Exact balance open. |
| Room art / backdrop | **OWNER DIRECTION, Session 019:** supplied isometric rooms are 1024x1024 with alpha, displayed aspect-preserved on a generated black backdrop. UI reference resolution remains separate. |
| Team work schedule | **OWNER DECISION, Session 023:** normal autonomous output when Morning/Daytime/Evening pass; Night is normal team rest. Implemented in the Session 026 Studio resolver with Night off. Session 027 adds optional paid per-person Night OT; requesting it does not advance time. |
| Character status/work forecast | **REQUIRED Session 023; IMPLEMENTED Session 026:** Studio VN shows affection, worker Stress, task/target, sprint delivery, capped helper and separate normal-team output, costs and resulting Stress/blocked reasons. Owner authorized the implementation after the numeric proposal. |
| Full playthrough duration | **DECIDED: 30 in-game days**, explicitly approved by the owner in Session 008; replaces the former 14-day draft. Configure through GameConfigSO. |
| Pacing intent | **DECIDED:** more time for emotional buildup, less compressed management, and room to recover from ordinary mistakes. |
| Backend documentation / user interaction | **DECIDED:** concise English for backend design, technical documents, rules, tasks and handoffs; Thai for user interaction and narrative/character documents. See [AGENTS.md](../../AGENTS.md). |
| Setting | **OWNER DIRECTION, Session 022:** game-studio work/management is the intended gameplay differentiator. Final production plot and scope remain drafts; systems keep generic data-authored domain types. |
| Roster | **WORKING DRAFT:** Plaifah, Grace and Ploy have nine-section profiles. Final production roster and story scripts are not fully locked. |
| Balance | **PROVISIONAL:** AP/day, economy, Stress/Affection thresholds, milestones, event windows, bonuses and ending thresholds. Duration is not provisional. |
| Art scope | **PROPOSED:** four CGs per draft heroine (12 total for three). No automatic increase because the campaign is longer. |
| Minigames / detailed ending rules | **UNDECIDED:** choose mechanics and final content later. |
| Development | Existing room/social/gift/AP fixtures preserved. Studio now includes free rest/paid Night OT orders, contextual OT speech, Producer rewards/stage gates, task-owned sprint credit, colored cards/bars, baseline save v5 and event-enabled v6. See [Studio guide](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md). |

## Session 040 owner direction — romance first, much simpler management

Owner reports that the current gameplay is difficult to understand and too close to professional task management. Prioritize romance/story/characters and substantially simpler everyday play; preserve a light planning link to the cast. This supersedes the management-depth-first priority. It does not retroactively approve the implemented UI/model as suitable casual gameplay.

The [casual-loop proposal](08_Romance_First_Casual_Loop_Proposal.md) recommends clear person/activity choices, immediate progress/relationship rewards, morning growth recap and one weekly support focus, with autonomous reliable team work. Removing mandatory Timeline/dependency/Stress optimization, changing shared/Producer rewards and reducing resource layers are proposals, not implemented or individually approved rules. Runtime stays Session 038; no game edits in this discussion.

## Session 038 UI corrections and Timeline authorization

Owner requests removal of duplicate room board/rest buttons, a left-contained speech box, no repeated action hover text, icon effect values and automatic closure of successful player-rest menus; then explicitly requests continuing Timeline. These are implemented in Planning UI. Cup Rest now means one authored recovery phase; moon End Day is distinct.

Timeline and Queue share one draft. Body move/edge resize snap to phases; actual start/completed history, effort, release gates and external sprint deadlines stay protected. Selected dependency links, estimated work/local productive buffer and actual-history spans assist planning. This is a prototype candidate, not final usability/model/balance approval. Contextual plan-change reactions/week review remain next. [Current guide](../04_Technical_And_Tools/09_Timeline_And_Conversation_UI_Guide.md).

## Session 037 implementation authorization

Owner explicitly says to start. This supersedes the Session 034–036 code/UI hold. The separate Planning prototype implements the first two delivery stages: persisted plan/queue/dates and candidate C work uncertainty, routine boundary handoffs, calendar sprint release, relationship-only shared time, Producer own rewards, atomic draft Apply/Revert and compact draggable queues with permanent three-worker status. Original Studio remains a legacy comparison fixture.

Approval is to prototype, not final model/value/content/usability approval. Timeline, batched character reactions and contextual first-week review remain next. New all-event 30-day diagnostics expose remaining Code capacity/Stress tuning (88% fixed dues, 97% buffer replan, one fixed seed, no voluntary OT); older helper-based completion evidence does not validate this mode. [Guide and evidence boundary](../04_Technical_And_Tools/08_Planning_Prototype_Guide.md).

## Session 036 readiness and drag interaction

Owner asks whether planning is complete/ready and explicitly wants clean cards with fluid drag-and-drop on the Sprint board. Record this as an interaction requirement; the question does not start implementation or revoke the earlier design/UI hold.

The [bounded delivery/drag contract](../04_Technical_And_Tools/07_Planning_Prototype_Delivery_And_Drag_Contract.md) makes a first prototype executable with proposed defaults: domain foundations, clean draggable Kanban, shared Timeline, contextual week slice, then actual-catalog/30-day tuning. Work-model/numeric defaults remain provisional, and final balance/usability/content are not approved. Dragging changes a draft queue/schedule, not earned progress. Session 032 runtime remains unchanged.

## Session 035 design-only follow-up

Owner requests formulas and a planned one-week comparison before implementation; direct team progress/Stress benefits from Producer help should be removed from the proposed direction, while dating/romantic buildup stays a core axis. Long allowance may finish early and tight commitments may still miss. Individual reactions must use context rather than a simple happy/angry switch.

[Work-planning math and paper trial](07_Work_Planning_Math_And_Paper_Trial.md) compares deterministic, per-phase random and task/day random output; candidate task/day variation is recommended, not selected/approved. 15,000 offline paths across a six-task fixture explore buffer, compression, disruption and optional OT. These do not validate current Studio assets, romance/economy or 30-day feasibility. [Thai narrative direction](../02_Narrative_And_World/03_Planning_Reactions_And_Romance_Draft.md) sketches shared experiences and contextual voices, not final story content.

No C#, Unity assets, UI, saves or actual numeric balance changed. UI implementation remains held. Session 032 remains implemented runtime with its old helper/Producer rules; Session 035 is a design proposal pending owner review.

## Session 034 current direction — planning discussion, implementation pending

Owner endorses the first-week gameplay slice but asks to discuss a planning-centered work model before proceeding. Record these directions separately from the unchanged Session 032 runtime:

- Helping a worker builds relationship without accelerating their task. Current helper effort/Stress recovery and reward eligibility need redesign; do not retain hidden indirect speed gains by assumption.
- Producer work must no longer unlock S2 early. Progress follows authored sprint pacing; final Producer duties/rewards and release behavior still need definition. This supersedes the Session 027 early-unlock direction for future implementation.
- Explore due dates, buffer, estimates and adjustable Timeline/Kanban planning; events challenge committed plans and prompt meaningful replanning. Compression/Stress/OT behavior remains under discussion, including how it fits special-event/item-only speed gains.
- UI overload is owner-confirmed; record the [UX requirements](../04_Technical_And_Tools/06_Studio_UX_Redesign_Backlog.md) and **do not implement UI yet**. Permanent three-worker right status with affection, reduced duplicate text and common bottom-right Back are required future corrections.

The [planning/buffer proposal](06_Sprint_Planning_And_Buffer_Proposal.md) distinguishes owner direction, existing mechanics and proposed resolutions. No Timeline, altered help/rates, revised Producer release or new scene is implemented by this discussion. Numeric balance and the final compression model are not approved.

## Studio work direction — Session 022

Owner proposes overall project progress plus weekly minimums across Code/Art/Design, Producer work and variable/event-driven workload. Owner confirms **team baseline production with player help**, rather than requiring the player to create every unit. Per-character counters were the Session 022 baseline; Session 026 implements task-based Studio production/board/review. See [Studio Work and Sprints](03_Studio_Work_And_Sprints.md) for proposed counting, weekly recovery and unresolved capacity timing. Session 026 authorizes the proposed quotas/review dates for prototype testing; final production balance remains open.

Owner requires immediate character VN presentation with Work/Talk/Gift, screen taps instead of Continue and a bottom-right back arrow. Gift preferences/reactions and ordinary-talk variants are data-authored. Gift limits/diminishing returns are undecided; the prototype consumes inventory without a daily cap. A late-campaign three-day Grace absence is a [Thai story idea](../02_Narrative_And_World/01_Studio_Event_Ideas.md), not an approved event schedule.

## Session 023 discussion and asset handoff

Owner explicitly requests planning before implementation. [Sprint proposal](03_Studio_Work_And_Sprints.md#sprint-prototype-proposal--session-023) now details cards/discipline roles, proposed phase ordering, free batch review/carryover and deferred status preview. Only the three productive phases and night rest are newly approved rules; numeric rates, quotas, review cost and Producer support effects remain proposals. [Grace/Ploy mockup request](../03_Art_And_Audio/02_Team_Mockup_Asset_Request.md) covers four required/eight recommended PNGs. No new runtime behavior or cast integration this session.

## Session 025 proposal and Session 026 prototype authorization

Owner asks for work rates, Stress effects and Producer gameplay. [Numeric proposal](04_Work_Rates_Stress_And_Producer_Proposal.md): shared base 4 effort/productive phase; Stress tiers 4/3/2/1 output; normal-output Stress +2, overnight -4; helper +6 effort/Stress -3 for 20 energy/one productive phase. Starter sprint targets Code 80 / Art 72 / Design 64. Proposed Producer actions coordinate two ticks, resolve explicit Producer decisions or give a targeted break. Session 025 supplied ten paper schedules only. The owner subsequently authorized implementation (Session 026), superseding the clarification hold. These rates, free review/carryover, Producer effects and dates now drive a separate configurable Studio prototype, with 85 Edit Mode / 11 Play Mode passes and four feasible full-campaign engine policies. This is approval to prototype, not final balance or story approval. Original fixtures remain unchanged; Grace illness is not scheduled.

## Session 027 owner decisions

- Worker rest is a character command, removed from Kanban. Order it without advancing time; at the current phase boundary Stress falls and that worker produces no work. It expires after that phase. The sofa separately recovers player Stamina.
- Night normally has no worker output. Request OT from each character without advancing time so several can agree. Pay money and add Stress; talk then uses character/affection/Stress-specific OT content. Prototype charges at request and produces effort at Night close, once per person.
- Producer work earns money, own progress and up to +1 per available discipline (card caps/block/rest/absence respected). Completing its current stage unlocks the next sprint's tasks early. Owner explicitly confirms no calendar bypass. Early S2 work credits S2, not the current week; frozen deadline results do not retroactively improve after late acceptance.
- Requested UI mockup handoff includes icons, task-owner heads, discipline colors and one shared grayscale/tinted progress-bar sheet. Core export specification is [here](../03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md). Full art replacement remains pending owner supply.
- Implemented/verified in Session 027 (96 Edit Mode / 15 Play Mode). Trial costs/rates/thresholds and draft OT lines are not final balance/story approval. No minigame is implemented; future graded rewards must use the same transaction/forecast path.

## Session 030 owner direction and event foundation

- Implement events first: both automatic forced scenes and player-triggered character-head bubbles, deterministic ordering when several characters qualify, no overlapping VN/modal transactions. UI integration waits for owner readiness.
- Characters volunteering for OT, shared Night work and relationship moments should connect to actual sprint tasks. This direction is recorded; volunteer economics and genuine joint Night work remain future implementation. Paid requested OT is still the current rule.
- Make task dependencies meaningful and readable: Design brief -> Art -> Code integration/test, with independent parallel tasks. Existing accepted-task prerequisites are the foundation; cross-discipline content/presentation comes after event scheduling.
- Implemented configurable campaign event catalog, priority/deadline/ID arbitration, safe automatic starts, bubbles, group exclusions, deferral/retry, production workload outcomes, conditional callbacks and save v6 for catalog-enabled campaigns. Four temporary Thai scenes demonstrate the path; final story/numbers remain drafts. See [event contract](05_Event_Scheduling_And_Studio_Integration.md).

## Session 031 owner readiness and dependency/UI delivery

Owner confirms updated UI is ready and authorizes continuation, superseding the art-integration hold. Supplied icon colors/portraits/pixels and existing GUIDs are preserved; sprites are now bound to HUD, worker status, action/navigation controls, task cards, shared tinted bars and rest/OT badges. This is prototype integration, not final visual/balance approval.

S1 now exercises Design brief -> Art -> Code integration with independent alternatives and accepted-card gates. A readable inspector links predecessor/successor jobs and ready alternatives; review/priority remain free. Existing Producer decisions remain separate from specialist prerequisites. Other sprint chains and volunteer/joint Night OT remain future content/implementation. See [UI/dependency guide](../04_Technical_And_Tools/04_Studio_UI_And_Dependency_Guide.md) and [verification](../00_Agent_System/WorkLogs/Session_031_Verification/README.md).

## Session 032 authorized Night-work prototype

Owner authorizes continuation after the UI/dependency delivery. Volunteer OT and actual shared Night work now use the existing event scheduler and production resolver. The trial preserves paid compensation and OT Stress, makes joining consume one Night/Stamina, grants affection only for actual positive helper output, and supplies individual affection/Stress voice variants plus later callbacks. Ordinary Work can join already-requested OT without a second fee. Declining is free and has no relationship penalty; authored cooldown groups prevent daily voice-variant spam.

All numeric eligibility/cooldown/reward values and the 36 Thai nodes remain provisional. This does not approve final personal arcs, mandatory OT routes or an illness event. See the [Night-work guide](../04_Technical_And_Tools/05_Studio_Night_Work_Guide.md) for editable data and exact transactional limits.

## Approved gameplay foundation

- A limited campaign with daily phases and resource choices between objective progress and relationships. New room config uses Stamina; legacy AP fixtures remain supported. Costs and recovery remain data-authored trial values.
- Dual tracks: main objective/progress and resources; character affection, stress and personal arcs. Affection alone does not unlock a relationship stage or true ending.
- SP earned through tasks/minigames and spent on skills that support work or unlock special dialogue choices.
- Money, shop and inventory with three item categories: Consumable, Gift and Key Item. Key items unlock authored content/features through conditions.
- VN presentation: configurable typewriter speed and instant reveal, expressive sprites, backgrounds and event CGs, conditional choices.
- Planned VN quality of life: backlog with voice replay, Skip All / Skip Read Only, auto-advance, hide UI, multi-slot/quick/auto saves, route flowchart, CG/music/ending galleries.
- Unity 6 2D URP; content in data, static definitions separate from runtime state; designer-friendly Editor tools.

## 30-day pacing requirements

The extra days must create breathing room rather than a proportionally larger mandatory workload. Keep meaningful choices and a final deadline while allowing routine inefficiency, rest and recovery. Do not turn an ordinary missed day or suboptimal answer into an automatic run failure.

- Spread introductions, trust, personal-arc buildup and resolution across the campaign.
- Give important non-terminal events configurable availability windows and catch-up opportunities. Preferred dates are authoring targets, not single-day route locks by default.
- Keep final project evaluation on Day 30. Intermediate shortfalls should normally expose recoverable consequences and guidance.
- Evaluate AP/economy/progress targets together; do not multiply workload by `30/14` merely to fill the expanded calendar.
- Keep CG scope separate from campaign duration; use reusable backgrounds, dialogue and optional interactions for pacing.
- Specific timing/windows and the acceptable recovery margin need prototype balancing. No claim of validated difficulty yet.
- Session 015 duration was reversible; Session 022 replaces it for ordinary talk with zero phases and first-daily energy/reward. Evaluate time/energy spent on required work, recovery, relationship growth and mandatory events together over 30 days. Test plausible play schedules and routine mistakes; revise costs, output/growth rates, requirements or windows when needed rather than requiring perfect play.

See [Core Systems](01_VN_And_Simulation_Core_Systems.md) for the provisional pacing map.

## Open content and balancing decisions

- Setting candidates: Game Studio (current draft), Fantasy Guild Receptionist, Midnight Co-Working/Barista, Window Greenhouse. Narrative descriptions remain in the Thai [Setting Candidates](../02_Narrative_And_World/00_Setting_Candidates_And_Concepts.md).
- Current profiles: [Plaifah](../02_Narrative_And_World/Characters/02_Character_Profile_Plaifah_Artist.md), `char_01_plaifah`, age 23, 2D Concept Artist; [Grace](../02_Narrative_And_World/Characters/03_Character_Profile_Grace_Programmer.md), `char_02_grace`, age 24, Lead Programmer; [Ploy](../02_Narrative_And_World/Characters/04_Character_Profile_Ploy_Designer.md), `char_03_ploy`, age 23, Game Designer.
- Choose one or two small minigames; 30–45 seconds per round is a draft guideline.
- Finalize AP/day, work targets/checkpoints, money/SP rewards, item prices/effects, Stress thresholds, relationship gates, ending rules and CG count through authoring and playtesting.
- Proposed emotional phases and character-event windows remain editable. They do not override personal-arc prerequisites.

## Canonical sources and change rules

| Information | Source |
| --- | --- |
| Owner approvals / unresolved decisions | This file |
| Implementation / current work | [Task Board](../00_Agent_System/02_Task_Board_And_Roadmap.md), actual files and latest Handoff |
| Character identity, appearance and speech | Thai profiles, sections 1–6 |
| Current character wound | Profile section 5; section 4.3 is historical inference input |
| Data types and system names | [Data Architecture](../04_Technical_And_Tools/00_Designer_Tools_And_Data_Architecture.md) |
| Expression keys and image specs | [Art Pipeline](../03_Art_And_Audio/00_Art_Pipeline_And_Specs.md) |

Keep dialogue/examples/prompts consistent with their profile. Preserve IDs when names change. Handoffs/work logs are dated history: old 14-day assumptions, approval claims and verification limits do not override current decisions. Do not infer approval of all story content from the owner's duration change.
