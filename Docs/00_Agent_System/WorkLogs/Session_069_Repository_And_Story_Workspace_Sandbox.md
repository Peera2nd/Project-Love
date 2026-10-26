# Session 069 — Repository baseline and isolated Story Workspace

Date: 2026-10-26. Owner authorizes the supplied GitHub repository setup and a sandbox-first narrative authoring trial before production changes.

## Repository

- Initialized `main`, configured `origin` as `https://github.com/Peera2nd/Project-Love.git`, installed the local LFS hook and pushed baseline `5ae974ab2088914daffc195514cbe3ca6fab38ce`. Remote `refs/heads/main` matched that exact hash.
- Tracked the current Unity Assets/Packages/ProjectSettings with metas, documents, Art_Source and Exports. Ignored caches, backups, `.verify`, generated `.sandbox`, local settings and machine-specific raw `.log` diagnostics. Existing local files are preserved. Summary reports/captures remain tracked. Existing baseline whitespace is not reformatted.
- Added README/attributes; current small images use normal Git, future large authoring/audio/video formats use LFS. No force push or old remote history replacement.
- Experimental work is on `sandbox/story-workspace`; production integration must not be merged automatically.

## Implemented sandbox

[Sources and workflow](../../../Sandbox/StoryWorkspace/README.md). The launcher creates `.sandbox/StoryWorkspaceProject` with its own Assets/Packages/ProjectSettings, Library and Editor/test assembly imports. Subsequent launches update tool files only, preserving sandbox content and saved draft overlays. The production project never loads/references these assemblies.

- `StoryIndex`: read-only current-campaign adapter. Retains every serialized non-node root caller; indexes executable next/choice edges, all-branch roster and continuous manuscript with bounded joins/cycles. Source titles fall back to Thai line snippets when old assets have no authored name. Incoming alternatives retain their own root/path gates.
- `StoryDraft`: persistent sandbox-only Thai/English overlays keyed by catalog GUID + stable localization key; Undo/Redo and explicit Save. Existing catalogs remain the read source. No production apply action or duplicate authoritative game text is introduced.
- `DraftPreview`: deep detached copies of reachable definitions, remapped graph/catalog references and overlay application, then the existing `AuthoringPreview`/saved game view. It starts directly at the selected segment, explicitly bypassing entry eligibility. It does not load/write the player's save or locale preferences.
- `StoryWorkspaceWindow`: library, exact localized phrase/speaker search, all-branch speaking-character/type filters, optional authored day/phase range filter, manuscript, exact-line editing, caller/choice destination snippets, route gates, root contexts, back/forward and detached preview. Separate event/conversation probes invoke the existing selector/arbitrator with a new session; exposed day/phase/affection/flags/completed-event inputs do not claim full player-state simulation.
- No production C#/Assets/Packages/ProjectSettings changes, narrative rewrite, backend purchase/install, new command runtime, save migration or mass SO consolidation.

## Verification and UX findings

- Initial focused isolated EditMode run: **5/5** (opening eight ordered lines/three responses/shared join/roster; localized search/source immutability; Undo/Redo/Save/reload; same-view detached clone render with source-file/preferences preservation; window open/repaint).
- Scoped updated run: **2/2**, comprising the window recheck and an additional alternative-root-gates/shared-join/cycle case. Six unique cases passed across the two runs; these are not full game suites.
- Actual Windows sandbox Editor inspection found and corrected an initial auto-scroll defect and repeated backend labels (ERR-105). The actual opening top was rechecked after live recompilation. Further visible interaction findings/evidence are recorded in the [verification notes](Session_069_Verification/README.md).
- Existing copied `DesignerDashboard.OpenTranslation` menu-parameter warning remains outside sandbox scope (ERR-106); no warning-free baseline claim. No production dependency or asset diff was introduced.
- Owner usability trial/approval remains **pending**. Automated passes and agent visual review do not authorize production adoption.

## Remaining acceptance and next agent

Have the owner try finding an exact situation-specific line, reading every opening branch/join, inspecting origins/conditions, editing/Undo/Save/reopen and game preview. Record hesitation/readability and requested changes. Do not call Stage A fully accepted yet. Main-project opening/narrative approval is also separate.

Structured semantic situation search, full editable speaker/branch authoring, generic activity eligibility probes, source-conflict-aware draft application, a separate graph canvas and large-corpus usability measurement remain future scope. New document storage/backend and shake/emote/transitions remain Designed only. Follow AGENTS rule 11 and the canonical plan before any promotion. The generated sandbox/drafts are ignored by Git; back up drafts before deleting/replacing that local project.

## Incidents

ERR-104 records guessed intake paths and noisy baseline whitespace check. ERR-105 records appearance defects missed by window smoke. ERR-106 records the existing copied menu warning and ERR-107 the manual-input tooling limit. Causes/limits are recorded without production edits.
