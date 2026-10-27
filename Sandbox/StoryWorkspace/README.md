# Story Workspace sandbox

**Session 071:** unified writing and logic authoring, isolated from production. Open `.sandbox/StoryWorkspaceProject` in Unity 6000.3.24f1, then **Project Love > Story Workspace**. The outer `Sandbox/` folder contains tool source; it is not the Unity project to open.

## Use

Write inline, choose speaker/expression, click insertion gaps, drag line handles across blocks, insert blocks, add/reorder choices and link responses. **Inspect** edits trigger/conditions/results; **Flow** traces callers/links/variables; **Test**, **State** and **Trace** share an actual detached GameSession and the saved game view. Concise English chrome uses hover tooltips; Thai story text is preserved.

[Thai owner guide](../../Docs/02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md), [architecture/execution semantics](../../Docs/04_Technical_And_Tools/17_Unified_Story_Workspace_And_Execution_Trace.md), [verification and limits](../../Docs/00_Agent_System/WorkLogs/Session_071_Verification/README.md).

## Sources and persistence

The launcher initially copies the current production project into the ignored `.sandbox/StoryWorkspaceProject`. Subsequent launches update only tool files, preserving sandbox content. They do not automatically synchronize later production changes.

The first unified-window open creates `Assets/_Project/StorySandboxDrafts/Workspace.asset`: one editable campaign/graph/catalog copy with subassets. Existing text overlays from `TextDraft.asset` are imported once and retained. **Save** persists workspace edits; Undo/Redo covers writing and structure. Existing workspaces are never regenerated. Back up the entire StorySandboxDrafts folder before replacing/deleting the generated project; it is ignored by Git.

No production apply/merge action exists. Future adoption requires a reviewed manifest, source-conflict detection, validation and explicit owner acceptance. Production assemblies never reference this Editor-only sandbox assembly. Native assets/GUIDs/player saves remain unchanged.

## Testing boundaries

**Run Caller** uses real entry gates/selection. **Preview Block** explicitly bypasses entry gates in detached state. Choices, completion callbacks, state diffs and rollback use the current runtime. Test does not import/write a player save or preferences. Typed event/choice conditions differ from a per-sentence scheduler; State/Trace makes that boundary visible.

Line cards are freely movable across blocks; whole branching subgraphs are not silently flattened/reordered. Gift inventory simulation, arbitrary saved-state restoration, large-corpus virtualization and broad production migration remain future work. [Audio design](../../Docs/04_Technical_And_Tools/16_Audio_Authoring_And_Playback_Plan.md) is still Designed only. Passing tests and agent UI inspection are not owner UX approval.
