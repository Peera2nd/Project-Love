# Studio UI and Dependency Guide

Session 034: owner reports information overload and duplicate metrics. This guide describes the existing UI, which is not usability-approved. [Redesign requirements](06_Studio_UX_Redesign_Backlog.md) are recorded; UI implementation is explicitly held while gameplay is discussed.

Session 031. Owner confirms updated UI assets are ready. The supplied skin and a first-sprint dependency chain are integrated; Session 032 also delivers [volunteer/shared Night work](05_Studio_Night_Work_Guide.md). [Verification](../00_Agent_System/WorkLogs/Session_031_Verification/README.md).

## Try it

Open Dashboard -> Studio Sprint, then Play and start a fresh run. The changed S1 prerequisite catalog rejects earlier incompatible v6 snapshots; no task history is invented. Old AP/room scenes and Studio v4/v5 slot namespaces remain separate.

The room shows resource icons/exact values and shared tinted bars; the roster shows each worker's current effort, task readiness, Stress and rest/OT badge. Hover for full task/block information. During conversation, affection/Stress/task meters and a hatched post-help forecast supplement exact numerical help/team costs. Hatches represent the full projected task delta, not a separate guaranteed bonus. Generic worker-free events do not assume a production owner.

Open Kanban and click a card's Details button. The right panel shows its owner/progress/status, predecessor work, successor work and ready alternatives for that owner. Click a linked job to inspect and scroll to its card. Set priority explicitly to choose independent work while another card is blocked; inspection and priority do not advance time or spend resources. Return to the Producer panel using the inspector's top button.

S1 content now includes:

- Ploy `studio_w1_ploy_1`: character/gameplay brief, independent.
- Plaifah `studio_w1_plaifah_2`: character art, requires acceptance of that brief.
- Grace `studio_w1_grace_2`: art integration/dialogue connection, requires acceptance of that art plus the existing explicit Producer decision.
- Grace `studio_w1_grace_3`: control-system tests, requires accepted control-system work (`studio_w1_grace_1`).

Scene art, base Code and other Design work provide parallel alternatives. Other sprint chains are not newly authored in this session.

Filling a card moves it to Review. **Accept it before dependent work unlocks.** Deadline acceptance remains automatic; manual review is free and earlier acceptance lets the team start downstream work sooner. Resolving a Producer decision does not bypass a missing specialist predecessor. Task readiness describes dependency/release state; a worker's current rest, absence, Night schedule and costs still govern actual work. Existing priority behavior is explicit: a blocked top-priority job can stall a worker until the player selects an available alternative or removes the blocker.

## Edit content and presentation

- `Data/Studio/StudioPresentation.asset`: icon keys, four bar sprites, panels/frames, shared palette and Thai copy. `useStudioSkin` is initialized before Play. PNGs remain in `Art/UI/Mockup/{Icons,Portraits,Bars,Panels}/` with their original GUIDs/pixels.
- `CharacterDataSO.taskAvatar` and `WorkStreamSO.icon/tint` bind supplied heads and role identity. Icons with supplied color retain it; neutral bar layers use the default shared UI shader and per-instance vertex tint. No role-specific PNG or material copy is generated.
- `WorkTaskSO.prerequisites`: drag predecessor task assets. Cross-role references work through the existing accepted-task ledger; `ownerId` in runtime state remains assignment-aware. Keep independent alternatives and avoid cycles.
- `StudioUiSeed.json` supplies initial Thai UI strings and four S1 content bindings. `StudioUiUpdate` uses separate presentation/dependency version stamps; reopening preserves subsequent authored edits. The importer uses the installed Sprite Editor data-provider API and preserves named cell IDs on reimport.
- The canonical sheet is sliced as `ui_bar_track`, `ui_bar_fill`, `ui_bar_frame`, `ui_bar_forecast`. The four separately exported component PNGs remain optional originals, not extra rendered layers. Layout clips full-width sliced fills/forecast ranges instead of generating completion images.

Runtime implementation: `StudioSprintView` with presentation/dependency partial files, shared `StudioUiWidgets`/hover hints and room action/order skinning. UI reads the simulation; it does not implement another dependency resolver. No scene walking, new minigame or volunteer OT policy is added.
