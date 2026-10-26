# Authoring tools audit and upgrade proposal

**Historical audit, Session 043.** The owner authorized implementation in Session 044. The current workflow and limits are in [Artist Workspace and UI Authoring](12_Artist_Workspace_And_UI_Authoring_Guide.md); the findings below describe the pre-upgrade state.

Session 043, 2026-10-25. Owner asks to review tools before more event/story work: cannot find dialogue, Dashboard is crowded with SOs, final UI is absent before Play, and Play edits do not provide a reliable authoring workflow. **Audit/proposal only. No Editor/runtime code, assets, layouts or dialogue are changed.**

## Verdict and scope

The prototype supports agent-led implementation and raw asset editing, but does not yet satisfy the artist-first authoring requirement. Treat usable authoring as the next prerequisite for content expansion. Adding documentation or renaming assets alone cannot fix runtime-owned layout and missing content navigation.

Evidence: local source, serialized scene/assets and owner feedback. A read-only Unity `get_scene_info` request did not return during the audit and was stopped; connection/responsiveness cause is unknown. No live Editor UI inspection, scene switching, Play changes or current screenshot claim. [Inventory](../00_Agent_System/WorkLogs/Session_043_Verification/asset_inventory.json), [serialized-scene observations](../00_Agent_System/WorkLogs/Session_043_Verification/scene_authoring_observations.json).

## Findings, ordered by authoring impact

| Priority | Finding / evidence | Consequence |
| --- | --- | --- |
| P1 | [Dashboard](../../Project%20Love/Assets/_Project/Scripts/Editor/Dashboard/DesignerDashboard.cs), lines 22–26, queries every IdentifiedDataSO under `_Project`, sorted by type/name. Inventory finds 257 definitions; 55 are reachable from RomanceCampaign (38 dialogue nodes). 202 lie outside that graph. No search, campaign/character filters or prose snippets. | Current and historical campaign versions compete in one list; matching a visible character name does not identify the live asset. The 55 graph count includes compatibility/ambient references, not 55 distinct playable events. |
| P1 | [IsometricRoomView](../../Project%20Love/Assets/_Project/Scripts/UI/IsometricRoomView.cs), lines 55–81 and 172–205, initializes only on Start, adds RomanceRoomView, assigns room art and creates hotspots at runtime. Serialized Room Frame sprite is null; RomanceRoomView is not serialized in the scene. | Edit Mode shows the initial shell, not the final room/game composition. No existing Edit Mode room/dialogue preview controller was found in owned source. |
| P1 | [RomanceRoomView](../../Project%20Love/Assets/_Project/Scripts/UI/RomanceRoomView.cs), lines 25–46, hardcodes anchors, dimensions, font size and new UI creation; Refresh clears/recreates roster/HUD/etc. [PlanningActionRow](../../Project%20Love/Assets/_Project/Scripts/UI/PlanningActionRow.cs), lines 13–20, resets button label/icon geometry. | Moving the scene shell or runtime children cannot reliably author the final layout; code overwrites some placements and transient children are replaced. Two existing generic button prefabs do not provide an editable full screen. |
| P1 | Dashboard delegates to the ordinary Inspector. [DialogueNodeSO](../../Project%20Love/Assets/_Project/Scripts/Data/DialogueNodeSO.cs) exposes lines/choices/conditions/links, while scene ownership is split across cast references, character greetings/conversations/gifts and event entry nodes. DialogueEditor/DatabaseTools currently have no implementation files. | A stored dialogue schema is not a usable writing workspace. No whole-dialogue text search, scene flow navigation, selected-line preview or jump from played line to source is implemented. |
| P2 | Dashboard has no Play-mode edit guard and calls SetDirty/SaveAssets on Inspector changes. Runtime session state is separate; ordinary Scene/GameObject Play edits are temporary. | It is inaccurate to say all Play edits are discarded: SO source edits can persist, scene edits normally revert, some runtime UI changes disappear on Refresh, and initial values may require a new session. These paths are not explained by the tool. Ordinary Inspector persistence/Undo should not be confused with a missing custom authoring workflow. |
| P2 | [RomancePrototypeBuilder](../../Project%20Love/Assets/_Project/Scripts/Editor/RomancePrototypeBuilder.cs), lines 35–103, creates content when the campaign is missing and only performs a version-gated presentation repair thereafter. It does not continuously sync seed JSON into authored assets. Dashboard's Create menu also omits RomanceConfigSO and creates raw assets without registering an event/character context. | Editing seed JSON, old Studio data or creating an unreferenced SO may produce no change in current play. An author cannot easily tell which source controls the visible result. |

## Reliable temporary edit map (current, not the proposed tool)

Stop Play before normal authoring. In Project, use `_Project/Data/Romance/` as the current campaign root. Do not edit another prototype merely because its visible character name matches. Original art/items may be shared; campaign ownership must be shown by future tooling rather than inferred solely from folder names.

| Intended edit | Actual current source |
| --- | --- |
| Main shared/personal/comfort/warm scenes | `RomanceRules.asset` -> Cast -> chosen character -> corresponding scene -> Lines -> Text |
| Example: Plaifah Personal Time | `Data/Romance/Dialogues/romance_plaifah_personal.asset` -> Lines -> Text |
| Greeting, routine talk, gift reactions | The chosen `romance_*.asset` CharacterDataSO -> Greetings / Conversations / Gift Preferences references; focus talk is bound in RomanceRules and can take precedence |
| Coffee/sketch event, choices and follow-on reply | `romance_coffee.asset` / `romance_sketch.asset` -> Entry Node -> Lines / Choices -> Next Node |
| Reward rates, moods, weekly targets | `RomanceRules.asset`; initial campaign values in `RomanceCampaign.asset` |
| Static labels, icons, colors, font, typing speed | `RomancePresentation.asset`. This does not expose full screen positioning. |
| Room art and phase-specific people | `romance_studio_workspace.asset` / store -> Phases / Hotspots / Placements. Current editing uses normalized values, with no owned visual room editor. |
| Final UI positions/sizes | No complete artist-facing path today: part of the shell is serialized, but final placements and dynamic child styling are code-owned. Do not advertise normal scene dragging as a complete fix. |

JSON is a bootstrap fixture source, not the normal synced writing source. Editing source assets before Play should persist; scene previews/session restarts must make actual use/selection easy to verify. Never promise that changing one generic talk node updates every context-selected line.

## Proposed authoring experience

One Project Love workspace opens on the current campaign, with Thai task labels. Existing historical data stays intact behind an explicit legacy/advanced view; hide it from ordinary work without moving/deleting assets or breaking GUIDs. The author should not need to recognize SO class names.

- **Write:** filter by character and purpose (greeting, talk, shared work, personal time, gift, story event); search the Thai body text and choices; show readable title, first-line snippet, owner and where used. Selecting a result opens ordered lines, speaker/expression previews, choices and clickable follow-on scenes in the same workspace. Use ordinary list/branch navigation first, not a prerequisite full graph editor. IDs stay stable on rename/reorder; technical conditions are secondary controls with clear explanations.
- **Characters:** portraits/expressions, related scenes, gift preferences and relevant current-mode settings together. Hide inactive AP/Stress/work fields in the casual authoring view rather than deleting legacy schema. Show shared-asset use when applicable.
- **Scenes and UI:** choose room/phase, then preview the room, conversation, gift list, weekly focus or morning recap without Play. Use real data/fonts/art and representative long Thai content. UI positioning uses normal RectTransform/Prefab tools; room-placement handles write the existing RoomDataSO normalized placement as the single canonical source.
- **Game settings:** expose current rewards/calendar/moods and editable presentation text, separated from narrative lines. A contextual validator shows missing references and which scene is actually reachable. Creating an event should also bind it to its intended campaign/character rather than leave an orphan asset.

An Editor-only 'open source of this line' action during testing should select the exact node/line, including context variants. Distinguish inspection of a running scene from deliberate source editing; do not silently save simulation changes. Save/dirty state and Undo/Redo must be apparent.

## UI ownership and preview contract

Replace code-created fixed screen structure with persistent, bound prefabs/scene objects for HUD, roster, dialogue, support view and recap. Repeated character cards/action rows instantiate authored prefabs when necessary; updating a value must not rebuild fixed layout or reset artist-owned anchors/typography. Explicitly document whether a style asset or a prefab owns a given property so they do not override each other mysteriously.

Edit Mode preview and runtime must bind the same view/prefab hierarchy; avoid a second approximate layout generator. Preview uses isolated sample/session state, does not advance campaign time or touch saves, and does not invoke normal Start/LateUpdate autosave/event pumping. Merely adding ExecuteAlways to the current runtime controller is not a safe preview implementation. Switching a preview mode should not save transient text/visibility as unintended production state.

Room handles must clearly apply to the selected room/person/phase, mark the actual RoomDataSO dirty and support Undo. Standard UI changes should persist through ordinary prefab/scene saving, survive stop/reopen and appear unchanged at runtime. Keep existing art/meta GUIDs and owner edits. No new art is required for this authoring milestone.

## Recommended sequence and acceptance (not yet implemented)

1. **Current-campaign writing workspace:** scoped discovery, Thai full-text search, character/activity grouping, scene links and clear original-asset ownership. First success: locate a visible Thai line without knowing its filename, edit and reopen it, then see the intended line in play.
2. **Persistent UI/room authoring:** real prefab hierarchy, explicit property ownership and preview selectors; remove runtime placement overrides for migrated views. This is part of the tools milestone, not an indefinite later polish task. First success: stop Play, move/resize a dialogue/portrait/card, save, preview and Play at both resolutions with the placement retained.
3. **Focused content test and authoring safety:** preview selected scene/choices, test a selected event with isolated day/phase/affection/mood context and display failed conditions. Undo/Redo/save/reload, creation/catalog wiring and jump-from-played-line tests complete the workflow before expanding the proposed first-week story.

Functional acceptance includes finding/altering a contextual line, new-event registration, exact source navigation, safe preview with no actual save/time/flag mutation, persisted prefab/room edits and historical GUID/data preservation. Owner performs the writing/layout workflow personally; screenshots or engine tests alone are insufficient artist-tool approval. Current 177 Edit/34 Play passes cover runtime, not these unbuilt tools. No new runtime or custom Editor feature is claimed by this review.
