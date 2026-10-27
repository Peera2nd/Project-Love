# Story Workspace sandbox

**Status:** experimental Editor UX; not integrated into the production game. This is Stage A of the [authoring plan](../../Docs/04_Technical_And_Tools/15_Scalable_Story_Authoring_And_Presentation_Plan.md).

## Open and preserve

Run `Open-StorySandbox.ps1` with PowerShell, or use `-PrepareOnly` then add `.sandbox/StoryWorkspaceProject` to Unity Hub (6000.3.24f1). Open **Project Love > ทดลองหน้ารวมบท (Sandbox)**. The first run copies the current project. Subsequent runs update only the experimental tool code, preserving that sandbox's content and drafts. They do not synchronize changed production content automatically. Keep the sandbox project open to continue its existing trial; no changes are applied to `Project Love/`.

Drafts live at `Assets/_Project/StorySandboxDrafts/TextDraft.asset` inside the generated project. Save explicitly; Ctrl+Z/Redo affects the draft. Back up that folder before discarding the generated project. Nothing here applies drafts to production. Future promotion needs a reviewed stable-key diff, stale-source detection, validation and explicit owner acceptance, not a directory copy.

## Owner trial

[Thai owner trial guide](../../Docs/02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md) gives exact current controls, expected observations and a blank feedback table. Owner acknowledgment/intention to test is not acceptance. The [audio plan](../../Docs/04_Technical_And_Tools/16_Audio_Authoring_And_Playback_Plan.md) is future scope; this sandbox does not yet select/play the proposed music/SFX/voice bindings.

1. Read the initially selected opening in manuscript order. Follow every choice, its response and shared continuation; click a join to revisit it.
2. Search a known phrase in Thai or English. Filter by speaking character, select the exact line and clear the query to read its context.
3. Select a shared passage. Inspect all direct callers and registered root contexts; navigate back/forward. Expand the root-to-passage routes to inspect each path's separate choice gates.
4. Change one Thai/English line, Undo/Redo, Save, reopen and confirm persistence. These edits are sandbox overlays over the existing localization keys.
5. Open the detached game preview. It uses the same saved game view and cloned graph/catalog, starting at the selected segment while bypassing entry eligibility. The separate context probe uses the real event scheduler/conversation selector on a new session with explicit inputs. It is not a player save simulator.

Record whether each task is clear, where you hesitated and whether the visible context is sufficient. Passing tests is not owner UX acceptance. Do not migrate production until the owner has tried and accepted the interface.

## Boundaries and limits

- Production runtime/editor assemblies never reference this assembly. The only dependency direction is sandbox → existing read-only adapter/runtime/preview APIs.
- Graph/index, draft overlay and UI are separated. All registered serialized root references are kept; incoming alternatives are not combined into one AND condition. Only executable continuations are indexed (node-level next is ignored when choices exist, matching the runner).
- Search/filter, continuous all-branch manuscript, direct callers, root contexts, route gates, destination snippets, roster, Undo/Save and detached preview are implemented for this trial.
- Generic activity/gift caller eligibility is not simulated. Event/conversation probes retain initial room/stress/work state; day/phase/affection/flags/completed events are the exposed inputs. The optional library day/phase filter checks authored ranges only. Global semantic situation search, a separate graph canvas, in-manuscript speaker/branch editing, draft source-conflict detection, large-corpus UX measurement and production migration remain future work.
- Shake/emote/transition commands and storage/backend migration remain designed only. No vendor/runtime package is installed.
