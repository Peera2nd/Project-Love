# Agent Protocol — Project Love

Unity 6 `6000.3.24f1`, 2D URP. Owner: an artist-first solo developer.

## Required intake order

Before code, Unity assets, or design edits, read these files in order:

1. [Mistakes and Prevention](Docs/00_Agent_System/04_Mistakes_And_Prevention_Registry.md).
2. [Latest Handoff](Docs/00_Agent_System/03_Handoff_And_Work_Logs.md): current snapshot and latest session first.
3. [Architecture](Docs/00_Agent_System/01_Project_Context_And_Architecture.md).
4. [Master Decisions](Docs/01_Game_Design/00_Master_Vision_And_Decisions.md).

Then inspect the task board and relevant source files. Do not treat historical assumptions as current decisions.

## Language policy — owner decision, Session 008

- **English:** internal design/system specifications, architecture, roadmaps, tasks, handoffs, work logs, incident records, agent rules, and internal code comments. Write concise English; link to canonical details rather than duplicating them.
- **Thai:** user-facing conversation, progress updates and final replies; story, dialogue, worldbuilding, character profiles, and narrative/character authoring documents in `Docs/02_Narrative_And_World/`.
- Preserve identifiers, filenames, links, and established terminology. Existing English generation/roleplay prompts inside Thai character documents may remain English; dialogue examples remain Thai.
- Artist-facing Inspector labels/tooltips and game text should remain understandable in Thai. English backend documentation does not change the game's language.
- **Session 071 supersedes authoring-tool chrome language:** new/revised Story Workspace UI uses concise English labels and English hover tooltips, with minimal always-visible explanatory text. Thai conversation, narrative content and game text remain unchanged. This overrides older Thai-facing-tool wording for this authoring UI; do not translate the story itself.

## Core rules

1. **Artist-first:** expose editable content through ScriptableObjects and designer-friendly Editor tools. Do not hardcode dialogue, names, prices, stats, or story flags in C#.
2. **Setting-agnostic systems:** use generic domain names and canonical data types from Data Architecture. Content remains swappable through data; the setting is not fully locked.
3. **Approved duration:** a full playthrough is **30 in-game days**, replacing the former 14-day draft. Store it in `GameConfigSO`; do not bake it into systems. Allow gradual emotional buildup and recovery from routine planning mistakes. Exact AP, economy, event windows and thresholds remain provisional.
4. **Unity assets:** create owned assets under `Project Love/Assets/_Project/`. Preserve `.meta` files and GUIDs when moving or renaming assets. Separate Editor and runtime code, static definitions and mutable save state.
5. **No walking-map scope:** use fixed isometric room/scene views with clickable people/objects and dimmed VN conversation overlays. Phase changes may swap character poses/positions; no exploration movement or unnecessary physics. Owner clarification: Session 017.
6. **Evidence:** distinguish Designed, Implemented, Verified, and Approved. Never claim compile success, content approval, scientific validation, or market evidence without support.
7. **No repeated mistakes:** read the registry first. Immediately record a build/script/tool failure, corrected misunderstanding, or fixed issue with its cause and prevention. Do not invent a root cause when unknown.

8. **Tangible authoring — owner decision, Session 044:** routine writing, balance, art/room placement and UI layout must be discoverable, editable and persistable through Thai-facing Editor tools, serialized data and actual prefabs/scenes. Do not make C# edits or Play-only generated UI the normal artist workflow. Runtime updates values and respects authored layout; fixed view structures must not be rebuilt or repositioned on refresh. Provide safe Edit Mode previews using the same views and isolated state, clear current-campaign scope/source ownership, search, Undo and Save. Initial generation/migration code is allowed; it must not overwrite later artist edits. Include the authoring workflow in feature acceptance, not just compile/runtime tests.

9. **Proportionate verification — owner direction, Session 045:** match checks to the change. For a local Editor/UI fix, check compilation and the affected window/interaction; do not rerun full Edit/Play suites or rebuild unrelated assets without a concrete regression risk. Prefer inspecting the actual affected layout over accumulating unrelated test passes. Reuse valid prior evidence, stop after relevant checks pass, and report visual-verification limits honestly.

10. **Localization transition — owner direction, Session 063:** preserve the existing baseline; initial game locales are Thai (`th`, source/default/fallback) and English (`en`). Before the next playable/content/system version that adds or changes player-visible text, read the [Localization Architecture and Adoption Contract](Docs/04_Technical_And_Tools/13_Localization_Architecture_And_Adoption_Contract.md). Implement its shared foundation and adopt it for that version's new/revised text through a bounded migration; unrelated legacy fixtures remain supported. Do not treat this designed contract as an existing runtime system. At first actual adoption and subsequent expansions, update its adoption register, Task Board and Handoff, and explicitly report migrated scope, remaining legacy/English gaps, verification and save compatibility to the owner in Thai. Documentation-only/art-only/unrelated fixes do not force migration. This does not change the backend/narrative-document language policy.

11. **Story Workspace isolation gate — owner direction, Session 069:** develop the new narrative authoring UX in the [sandbox](Sandbox/StoryWorkspace/README.md) before production adoption. Keep a separate Unity project and one-way sandbox dependencies; preserve the current game assets/runtime. Owner trial must cover finding/editing/reading whole scenes, incoming/outgoing context, conditions, Undo/Save and preview. Compilation or automated tests do not grant UX approval. Record actual trial results and obtain explicit owner acceptance before production integration/migration; do not silently copy experimental code/data into the game.

## Required closeout

Localization checkpoint: Session 064 implements the first **L2 partial** adoption and real Workspace tab. Read the contract's current register and extend the delivered catalog/service/tools; do not recreate the foundation or mistake incomplete English coverage for a missing system.

Before the final reply:

1. Update [Task Board](Docs/00_Agent_System/02_Task_Board_And_Roadmap.md).
2. Add a structured English entry at the top of [Handoff History](Docs/00_Agent_System/03_Handoff_And_Work_Logs.md); update its current snapshot. Add a detailed English work log for substantial work.
3. Record encountered/fixed issues in [Mistakes and Prevention](Docs/00_Agent_System/04_Mistakes_And_Prevention_Registry.md).
4. Verify changed references and appropriate behavior; report checks and limits accurately. Reply to the user in Thai.
