# 01 — Project Context and Architecture

## Environment

- Project: **Project Love**; owner: artist-first solo designer/writer.
- Unity: `6000.3.24f1`; URP `17.3.0`, Renderer2D.
- Runtime UI: uGUI `2.0.0` / TextMeshPro. Editor UI: UI Toolkit or IMGUI.
- Input System: `1.20.0`; New Input System enabled.
- PSD Importer: `12.0.2`; 2D Animation: `13.0.6`.
- Unity root: `D:/Work/Unity/Project_Game/Visual Novel/Project Love`.
- Documentation root: `D:/Work/Unity/Project_Game/Visual Novel/Docs`.
- Full campaign: **30 days**, configured through `GameConfigSO`; see [Master Decisions](../01_Game_Design/00_Master_Vision_And_Decisions.md).
- Implementation: **76 runtime/Editor C# files and 23 test files**. Primary playable direction is the romance-first casual loop. [Current guide](../04_Technical_And_Tools/10_Romance_First_Prototype_Guide.md); [Task Board](02_Task_Board_And_Roadmap.md).

Session 041 implements the owner-approved Session 040 simplification, explicitly using mood rather than Stress. `GameConfigSO.romance` selects `RomanceConfigSO`/`RomanceSystem` and separate `RomanceState`; production/work streams are disabled. `GameSession` keeps the shared phase clock, `DialogueRunner` snapshot rollback, `CharacterInteractions`, room presentation and `EventScheduler` arbitration. Major rewards commit on scene completion, team baseline commits once per day, and day reports preserve their separate causes. Time is the main budget; Stress/AP/Stamina remain zero and cannot become hidden work-rate penalties. Mood is authored categorical state. Optional weekly focus is one simple character choice; ordinary scheduling runs without a board.

`RomanceRoomView` uses existing supplied art/UI with a project bar, persistent heart/mood roster, compact previews/receipts, support cards and morning/final recap. `RomancePrototypeBuilder` creates a new campaign/scene without replacing historical assets. Save v8 has its own namespace/fingerprint; prior v7/v6/v5/legacy paths remain intact. Existing Planning Queue/Timeline/uncertainty and Studio/OT fixtures remain regression references, not an expert mode or the main design. No full-campaign story or final balance claim. Session 041 evidence: 177 Edit /34 Play passes; Session 042 owner confirms comprehension, but reports insufficient challenge/meaningful variation. The next discussion is a bounded character-event week with remembered decisions, not restored scheduling complexity. [Proposal](../01_Game_Design/09_Romantic_Tension_And_Meaningful_Choices_Proposal.md). No runtime additions are authorized by that discussion.

## Story Workspace sandbox boundary — Session 069

The owner authorizes an isolated UX prototype before production adoption. Tracked tool sources live outside the production Unity Assets in [Sandbox/StoryWorkspace](../../Sandbox/StoryWorkspace/README.md); its launcher creates an ignored `.sandbox/StoryWorkspaceProject` copy. Only that copy imports the experimental Editor/test assemblies. Source index → draft overlay → detached clone/preview are separate; production has no references to the sandbox assembly. `main` preserves the initial game baseline; `sandbox/story-workspace` holds the trial. Existing project assets, saves, gameplay and localization ownership remain unchanged. Real owner find/edit/read/context/Save/preview acceptance gates integration; tests alone do not pass that gate.

## Unified sandbox writer — Session 071

Owner trial replaces the reader/overlay UX with a [unified writer](../04_Technical_And_Tools/17_Unified_Story_Workspace_And_Execution_Trace.md): one persisted Workspace.asset holds copied campaign/graph/catalog definitions as subassets. Inline line/choice cards, speaker/expression, insertion/drag/reorder, links, typed caller/condition/results editing, Flow/variable references and actual Test/State/Trace share this isolated source. A saved-Prefab renderer uses the same detached GameSession as the inspected values. Production has no new fields/references. Cross-block moves retain canonical line/text IDs; lookup aliases are generated only in runtime clones. English tool chrome and hover help supersede the old Thai-tool policy here; narrative/game text stays Thai. Old TextDraft edits migrate once and remain preserved. First UI load waits until compilation/import completes. Focused tests and actual UI checks are bounded evidence; owner acceptance and production migration remain pending.

## Audio design — Session 070 (not implemented)

Owner requests comprehensive music/ambience/SFX/UI/sparse-voice design, prioritizing context-aware designer selection over a global clip list. [Audio authoring/playback plan](../04_Technical_And_Tools/16_Audio_Authoring_And_Playback_Plan.md) proposes collection banks, stable line/cue IDs, explicit None for unvoiced lines, separate exact recordings/reusable barks, contextual speaker/locale/intent filters and one presentation owner. Existing owned code/assets have no implemented audio service/library/picker; historical written “voice variants” are not recordings. Edit Mode audible preview, lifecycle, source/save/settings compatibility and owner UX must be proved in isolation before adoption. No audio implementation or sandbox acceptance is inferred. [Current Thai trial guide](../02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md).

## Localization adoption — Session 064

Owner authorizes implementation after Session 063 design. **L2 partial adoption:** `GameConfigSO.localization` opts the current Romance campaign into real catalogs/settings/service/refs, **Workspace > ภาษาและคำแปล**, guarded CSV/source revision tools and detached language preview. Thai is source/default/fallback; English is optional per entry. Current 689-entry table includes 96 English drafts and 593 missing translations. Existing graph/raw text/GUIDs/Prefabs remain; localized saves use separate v9 and an exact-baseline v8 import rather than weakening guards. [Architecture/adoption register](../04_Technical_And_Tools/13_Localization_Architecture_And_Adoption_Contract.md), [workflow](../04_Technical_And_Tools/14_Localization_Authoring_Guide.md), [focused evidence](WorkLogs/Session_064_Verification/README.md). Five affected cases and final scoped view/window checks pass; same-Prefab bilingual renders inspected. Live desktop/readability, actual player-menu/preference restart and complete English content are unverified. Continuing agents extend this implementation for new/revised text and report scope/gaps. Historical fixtures stay supported; Producer remains deferred.

## Current authoring workflow — Session 044

**Session 067 forward plan — Designed only:** [scalable story authoring/presentation plan](../04_Technical_And_Tools/15_Scalable_Story_Authoring_And_Presentation_Plan.md) and [official-tool comparison](../04_Technical_And_Tools/Research/01_VN_Authoring_Systems_Comparison.md) propose complete-event manuscript/flow/index views and scene/collection documents with internal blocks. Start with a legacy opening reader; prove source/transaction/presentation/save fit before a broad refactor or external backend adoption. Typed effect lifecycle and measured scaling are future work, not current runtime/UI. Preserve catalog-owned Thai and current game ownership unless an explicit tested migration changes them.

Owner authorizes the Session 043 tool upgrade and requires tangible artist editing as a permanent standard. **Tools > Project Love > Workspace** replaces the global SO list with Thai campaign-scoped writing/events, characters, scenes/UI and settings. Search actual dialogue/choice text and character/purpose; edit ordered lines with stable IDs/Undo/Save, follow source links, create separate drafts and validate before campaign publication. During Play, the active-line source can be selected but ordinary data editing is disabled.

`RomanceWorkspace.prefab` and eight nested/reusable UI prefabs are the main scene's real serialized layout. `AuthoredRomanceLayout`, `AuthoredMetric`, `AuthoredActionRow` and `AuthoredButtonList` bind content/visibility and pool variable rows/room targets; fixed views persist. Runtime does not reapply anchors/fonts/colors to these migrated views. Room placement remains phase-specific RoomDataSO data, edited by a visual drag/resize panel. Historical generated UI branches remain only for prior fixtures. Ordinary builder/open actions preserve existing authored layout.

`AuthoringPreview` renders the same saved Prefab with a detached GameSession in PreviewRenderUtility's isolated scene and a game-type camera. It never initializes SaveStore or live subscriptions. Screen/node previews bypass unrelated forced scenes for direct inspection; selected-event tests retain actual eligibility/arbitration and report blocking context. Day/phase/affection/selected mood are local controls. It is not an arbitrary history/save editor and starts in the configured starting room. Preview uses saved Prefabs, not unapplied scene overrides.

Verified: 189 Edit /35 Play passes and 12 authoring rechecks after final Editor-only cleanup; real clicks, stable fixed views/artist layout, text save/reload/GUIDs/Undo, draft publication, phase placement, event context and nonblack Edit Mode renders. No gameplay rates, authored narrative, existing art/data or v8 schema changes. Hands-on owner authoring acceptance remains pending. [Workflow/source ownership](../04_Technical_And_Tools/12_Artist_Workspace_And_UI_Authoring_Guide.md), [evidence](WorkLogs/Session_044_Verification/README.md). New work must meet AGENTS rule 8/GR-012.

Session 046 exposes character greeting/talk groups in `ConversationPoolEditor`: creation, inclusive affection/day/phase/extra conditions, optional alternative nodes, direct first-line editing and detached actual-selector probes. `ConversationVariant.node` remains the first pool member; optional `pool` nodes rotate by stable character/group/day/phase context after normal eligibility/priority selection. Legacy one-node content behaves unchanged. Search/validation/content fingerprints follow all pool nodes; v8 mutable state is unchanged. Five targeted checks pass after one fixture correction; current narrative assets/rates were not edited. See the workflow guide and Session 046 handoff for limits.

Session 050 records the owner's combined narrative direction and an application plan: shared romance-game production/release, personal confidences, and outside-contact opportunities feeding team discussion/decisions. This is design only; current runtime remains Session 046. Existing event/flag/mood/pool infrastructure is reusable, while contextual Producer scenes, persistent opportunity/promise status, phone entry and reactive recap notes require bounded design/implementation. Do not claim current static `producerScene`/`dailyHints` already provide those systems. Keep external contacts separate from the three-person romance progression roster. [Application plan](../01_Game_Design/10_Producer_Opportunities_And_Romance_Integration_Plan.md).

## Narrative planning — Session 057

Session 057 supplies a [30-day plot/route proposal](../02_Narrative_And_World/05_Thirty_Day_Story_Spine_And_Character_Routes.md) and [research/evidence review](../02_Narrative_And_World/Research_References/06_Research_Review_And_Story_Design_Summary.md). Runtime remains Session 046. Preserve Session 056 story-first priority and Producer implementation deferral; contextual agenda/recap/Wishlist in the longer outline are not already implemented. Plaifah's Session 055 alternatives remain open; the new worked route is not approval of her replacement wound.

Session 059 authors the approved short opening into the existing Romance campaign: group advice returns in a Day 3 build, one personal vignette determines response-specific memory/invitation, selected-phase bubbles provide a personal payoff, and greeting variants show reminders/aftercare. Windowed recovery extends through Days 8–10. Content remains ordinary editable SOs under `Data/Romance/Opening`, discoverable in Workspace; no new runtime systems or UI generation. Existing casual/Producer rewards and v8 mutable schema are unchanged. New content fingerprints require a fresh trial run; no saves deleted. [Play/edit guide](../02_Narrative_And_World/06_Opening_Story_Play_And_Authoring_Guide.md).

## Language

Backend documents and internal comments: concise English. User interaction and narrative/character documents: Thai. Preserve identifiers and existing English machine-facing prompts. Inspector labels/tooltips should help the owner work in Thai. See [AGENTS.md](../../AGENTS.md).

## Documentation layout

Keep documentation outside Unity Assets to avoid importing it.

| Path | Purpose / language |
| --- | --- |
| `Docs/00_Agent_System/` | Protocol, architecture, task board, handoffs, incidents; English |
| `Docs/00_Agent_System/WorkLogs/` | Detailed dated logs and template; English |
| `Docs/01_Game_Design/` | Decisions, gameplay and balancing specifications; English |
| `Docs/02_Narrative_And_World/` | Setting candidates, story, character profiles/templates, dialogue; Thai |
| `Docs/03_Art_And_Audio/` | Asset production and import specifications; English |
| `Docs/04_Technical_And_Tools/` | Data architecture and designer tool specifications; English |

Optional editable art and annotated references live outside Unity in `Art_Source/Mockup/`. Runtime PNG exports stay under `_Project/Art/`; see the [workspace mockup brief](../03_Art_And_Audio/01_Workspace_Mockup_Asset_Brief.md) for prepared Mockup subfolders and alignment rules. Owner direction in Session 017 is fixed isometric scenes, phase-authored character pose/anchor changes and dimmed VN overlays. Only scene backgrounds/foregrounds share a full frame; room-character poses are local sprites. Session 019 binds the supplied 1024x1024 rooms over black through RoomDataSO phase/hotspot placements and IsometricRoomView. See the [room quick start](../04_Technical_And_Tools/02_Isometric_Prototype.md).

## Unity layout

All newly authored game assets belong in `Project Love/Assets/_Project/`. Existing template `Assets/Settings/` and `Assets/Scenes/SampleScene.unity` need not be moved just for consistency.

```text
_Project/
  Art/
    Backgrounds/
    Characters/Sprites/      # Standing sprites and expressions
    Characters/Portraits/    # UI portraits
    CGs/
    Items/
    UI/
    VFX/
  Audio/BGM/
  Audio/SFX/
  Audio/Voice/
  Data/
    Characters/             # CharacterDataSO assets
    Dialogues/              # DialogueNodeSO assets
    Events/                 # EventDataSO assets
    Items/                  # ItemDataSO assets
    Skills/                 # SkillDataSO assets
    Settings/               # Config, activity and presentation assets
    WorkStreams/            # Generic discipline definitions; completed units are runtime state
    Rooms/                  # RoomDataSO art/hotspot/phase placement assets
    Studio/                 # Isolated Studio prototype config/rooms/cast/sprints, Tasks/ and Dialogues/
  Prefabs/Systems/
  Prefabs/UI/
  Prefabs/Minigames/
  Resources/Fonts/
  Resources/Localization/
  Scenes/Core/              # Boot/menu scenes, planned
  Scenes/Gameplay/          # IsometricPrototype.unity and StudioSprintPrototype.unity
  Scenes/Sandbox/           # FoundationSandbox.unity and isolated experiments
  Scripts/
    ProjectLove.Runtime.asmdef
    Core/                   # GameSession, RuntimeGameState and SaveStore; audio/event bus planned
    Data/                   # Definitions and serializable data models
    VN/                     # DialogueRunner, presentation, choices, history
    Simulation/             # DayPhaseManager, ProductionSystem, resources/stats/events; skills planned
    Minigames/              # IMinigame implementations
    UI/                     # Views/presenters
    Utilities/
    Editor/
      ProjectLove.Editor.asmdef
      Dashboard/
      DialogueEditor/
      DatabaseTools/
      Validators/
```

Folder presence does not mean feature implementation. Do not add extra structure until needed.

## Code conventions

- Runtime namespaces: `ProjectLove.Core`, `.Data`, `.VN`, `.Simulation`, `.UI`; Editor: `ProjectLove.EditorTools`.
- Data-driven ScriptableObjects for authoring, serializable classes for mutable runtime/save state, event-driven UI to reduce direct manager/view coupling.
- Use canonical [data/system names](../04_Technical_And_Tools/00_Designer_Tools_And_Data_Architecture.md) and [expression keys](../03_Art_And_Audio/00_Art_Pipeline_And_Specs.md).
- No hardcoded story, character names, item prices, skill labels, thresholds, or 30-day limits in runtime logic. Config stores the approved content duration.
- Runtime assembly references TextMeshPro, Input System and UnityEngine.UI; Editor references runtime and is Editor-only. Tests use separate test assemblies. Add direct references only when required.
- Keep `.meta` files with assets; never regenerate existing GUIDs arbitrarily.
