# 15 — Scalable Story Authoring and Presentation Plan

Session 067 design, updated Session 069, 2026-10-26. **Production architecture remains a proposal. Session 069 implements a bounded Stage A sandbox; production integration, new command/runtime/backend and data migration remain unimplemented.** Current Session 064 localization and existing gameplay remain active.

**Session 069 isolation gate:** owner explicitly authorizes repository setup and a separate sandbox trial before changing the production game. [Executable sandbox/workflow](../../Sandbox/StoryWorkspace/README.md) reads a copied legacy campaign, adds manuscript/context/search and saved text overlays, and uses detached clones for the existing game preview. Production assemblies never reference sandbox code. Record appearance/interaction checks and owner task results separately; only explicit owner UX acceptance authorizes subsequent production integration. No mass migration or new execution backend is authorized by sandbox compilation.

**Session 068 owner clarification:** the SO concern is designer discoverability and editing/context UX, not runtime speed. Required outcomes are tracing a selected passage's upstream sources, every choice destination/continuation, relevant conditions and all speaking characters, plus finding what a character can say in a particular situation. These are explicit authoring acceptance requirements below; reducing file count or meeting a latency target alone is not completion. No implementation start or backend approval is implied by this clarification.

[Primary-source comparison](Research/01_VN_Authoring_Systems_Comparison.md) distinguishes documented features from our recommendations. The design below is a project-specific inference, not a claim that every professional studio uses the same workflow.

## 1. Recommendation and decision boundary

**Keep Unity and the existing gameplay/presentation ownership. Make a readable event/scene document the authoring unit; provide a manuscript-first Story Workspace with a focused flow view and indexed search.** Store ordered lines, small branches and shared joins inside the document instead of requiring a top-level asset for every short branch. Continue SOs for campaign definitions, characters, reusable presets and document containers where appropriate.

The default storage proposal is a `StoryDocumentSO` containing serialized entries/blocks, with localized text references into the existing catalog. This avoids requiring the artist to learn a new programming language or making a new parser a prerequisite. The manuscript is an editing view of structured data, not a second independently editable text file. These type names are proposed, not existing declarations.

Before building a complete new VN execution stack, run a bounded backend fit comparison: **pinned Naninovel v1.21 versus a thin document adapter over the current runner**. Naninovel is the first external candidate because its documented VN editor/presentation features overlap the requested direction and its checked compatibility includes Unity 6.3. Yarn is a secondary candidate if a dialogue compiler is preferable to a full VN framework; it still needs our visual-command layer. This is an evaluation recommendation, not a selected/installed dependency.

If an external backend wins, use its supported authoring/compiler format and one explicit ownership mapping; revise the container implementation before mass migration. Do not build a custom text DSL and several vendor adapters in parallel. A failure to preserve our authored UI, catalog identity, transaction/cancellation and saves is a reason to retain the current runner initially, not to silently replace those requirements.

## 2. Current evidence and actual problem

Static traversal from saved `RomanceCampaign.asset` finds:

| Current metric | Count | Meaning |
| --- | ---: | --- |
| Reachable data SOs | 160 | Includes current definitions and localization assets |
| Dialogue node SOs | 117 | Existing physical narrative fragments, not one SO per speech |
| Spoken/narrated lines | 265 | Already grouped as serializable arrays inside nodes |
| Choice options | 58 | Across 24 nodes with choices |
| One-line nodes | 40 | Asset granularity can create substantial navigation overhead |
| Shared continuation targets | 7 | More than one incoming dialogue link; migration must preserve sharing |
| Localization entries | 689 | Includes UI/names/labels as well as narrative, not 689 spoken lines |

Current `DesignerDashboard` edits large cards for one node. `AuthoringCatalog` follows linked SOs and sorts entries by character/category/title; its search reconstructs strings while filtering. The localization view iterates the full table and shows one detail entry. Neither is a compact whole-event manuscript/branch navigator. `DialogueRunner` is a line/choice runner with event-start rollback, not a general asynchronous directing engine; speaker expressions use a fixed enum and the VN view primarily displays the current speaker portrait.

**No performance benchmark was run.** These findings establish granularity/navigation and extensibility gaps, not that Unity cannot manage 117 or 1,000 assets. Consolidating files alone would leave the usability problem intact.

## 3. Content hierarchy and identity

| Concept | Owner/workflow |
| --- | --- |
| Campaign | Existing `GameConfigSO`; configured duration, cast and event registration |
| Story library/index | Search/navigation records for the current campaign, including legacy documents and source ownership |
| Story document | A meaningful event/scene, chapter section or reusable conversation collection; normally one editable asset/file, many internal blocks |
| Entry | Stable named entry point inside a document; an event or greeting pool selects an entry without requiring a new file |
| Block | Dialogue, choice group, condition/branch, join/label, call/return or directing command; internal serialized data, not a designer-managed SO per block |
| Text record | Existing catalog key and Thai/English/review data; editable through the manuscript or translation view |
| Preset | Reusable visual/audio/effect configuration with Thai labels and persistent artist edits |

Group small greeting variants in an appropriate conversation collection rather than creating an obligatory document per greeting. Split genuinely independent scenes/chapters for ownership and manageable editing; avoid one giant campaign asset. Shared sub-scenes remain explicit calls with stable targets, not pasted copies. The author sees titles/breadcrumbs; internal IDs are normally hidden.

Use stable document/entry/block/line/choice IDs. Ordering and file paths are not identity. Reordering keeps IDs; duplicating an independent block allocates new identity; deliberate reuse is an explicit reference. A branch diagram and manuscript must edit the **same structure**, not synchronize independent copies later.

## 4. Designer workflow — proposed screens

The primary screen should read like a script. Thai controls, ordinary keyboard editing, persistent Save/Undo and source ownership remain acceptance requirements.

1. **Library:** event/chapter/route/tag filters, involved speakers, editable title, draft status, timing window and translation/validation summaries. Availability comes from current config/events; do not bake the 30-day campaign into the editor.
2. **Manuscript (default):** compact speaker + text rows. Inline choice sections, explicit conditions and shared continuation markers. Collapse a branch without hiding that it exists. Select a row to expose detailed expression/conditions/effect settings on the side; do not expand every technical field by default.
3. **Flow:** a focused event/entry map showing branches, joins, calls and endings. Start at event scale and expand detail on demand, rather than drawing thousands of speech bubbles. Clicking a flow element selects the same manuscript block; show breadcrumbs and back/forward navigation.
4. **Preview:** the actual saved Prefab views with isolated game/presentation state. Change Thai/English/context, read a branch, inspect why an option is blocked, and return to the exact authored row. Preview never writes actual saves/preferences or grants live rewards.
5. **Diagnostics:** missing text/art/presets, invalid argument types, duplicate IDs, missing targets, unreachable entries, suspect cycles and translation review status. Each issue opens its exact row. Clearly distinguish warnings, hard publish blockers and draft placeholders.

Illustrative layout only — no such new manuscript screen exists yet:

```text
Library                 Event: Day 1 opening             Selected-row details
  Opening               [Entry transition: preset]       Speaker / expression
  Personal scenes       Narrator: ...                    Target / effect preset
  Greetings             Grace: ...                       Duration / wait policy
                        Ploy: ...
                        [Emote: Ploy, question preset]
                        Plaifah: ...
                        Choice A -> response -> join
                        Choice B -> response -> join
                        Choice C -> response -> join
                        [Shared continuation]
```

Stage directions here are an authoring demonstration, not approved changes to the opening scene. In the current project all three characters do not automatically have separate active large VN portraits; target/rendering support must be explicit.

### 4.1 Required context navigation — Session 068

Selecting a passage must retain its owning event/entry context and expose the following without searching the Project folder:

| Required view | Designer outcome / acceptance |
| --- | --- |
| Upstream / Where used | List all registered callers: events, previous branches/choices, shared sub-scene calls and greeting/talk pools. Show source event/title, relationship/choice label and a clickable exact source row. Multiple incoming paths must not collapse to the first catalog match. Unregistered/dynamic entry sources are explicitly unknown, not invented. |
| Downstream / Each option | Show each choice's wording, destination title, beginning of its response, continuation and shared join. Navigate forward and back with breadcrumbs and restored selection/scroll. Explicitly label an event end/return to the host; missing targets are errors, not silent ends. Expand cycles/calls on demand rather than infinitely unroll them. |
| Conditions and consequences | Show readable conditions inherited from the entry event, selected path and local choice/selector, plus flags/resources/time changes that matter to the destination. A shared node's alternative incoming paths retain separate condition explanations; do not AND every incoming route into one impossible gate. |
| All speakers | Derive the speaking-character roster from all included branches, not only the entry owner or selected path. Distinguish narrator, conditional speakers and characters merely present visually. Clicking a speaker highlights/filters their actual lines while preserving event context. |
| Context probe | Let the artist set day/phase/room/affection and relevant memory flags or other supported selector state. Use the real eligibility/selection/scheduling logic on detached state. Explain eligible-but-not-selected, higher-priority group/forced-event queue, completed/cooldown and unmet conditions distinctly. Surface missing context instead of claiming one certain line from time alone. |

For example, the artist should be able to start at a line found in search, see which greeting band or event can reach it, inspect all incoming alternatives, follow each outgoing response and see where branches rejoin. A visually connected arrow is not proof that an event is reachable in the current campaign state; static possible paths and a tested context/path are labeled separately.

## 5. Find any line without hunting assets

Build an incremental campaign index keyed by stable IDs and source revisions. Search Thai/English, speaker, event/title/tags, used flags/conditions, entry names, commands, asset/preset references and translation status. Results must include the matching phrase, speaker, event/branch context and neighboring lines, with a direct jump to the exact row. Support reference/usage lookup and safe rename/move; update only affected index records.

Support structured discovery such as character -> greeting/talk/story purpose -> day/phase or situation, with visible authored ranges and conditions. Searching for a phrase is only one entry path; the artist must also find the relevant writing when they remember the character and occasion but not the words. Preserve every incoming reference edge separately from the unique content record so source tracing and shared-context display are reliable.

The selected document's unsaved edits must be searchable and visibly marked unsaved. Undo, reload and source changes invalidate affected records. Avoid searching by loading and formatting every source on each repaint. Use virtualized/recycled rows, not thousands of editable GUI controls at once. Unity's documented ListView is one implementation option, not a measured performance guarantee.

Provide **script-review export** separately from translation CSV: speaker, source text, entry/branch/condition, line ID and jump/join markers. A chosen-path transcript and a branch-expanded review are different exports; a branching story has no single universal flat order. Preserve the current six-column translation CSV contract.

## 6. Presentation commands and future growth

Represent directing instructions as typed data. A command definition specifies a stable command ID, Thai display label, typed argument schema, target kind, optional preset, completion mode and skip/cancel/restore policy. A programmer implements a handler once; routine scene authors pick controls/presets, not C# method names or arbitrary object-name strings. Do not hardcode scene-specific dialogue or effects in the handler.

Initial vocabulary to prove with the opening:

| Instruction family | Author controls / boundary |
| --- | --- |
| Say / Choice / Branch / Join / Call / Return | Ordered flow, existing text/choice IDs and typed conditions; same scene context |
| Portrait show/hide/expression | Character/slot, visual definition, authored transition preset; preserve legacy enum mapping |
| Shake | Target actor/stage/overlay, preset/intensity/duration; apply a transient offset rather than overwrite authored placement |
| Emote / emoji | Character anchor, sprite/animation preset and duration; not a requirement to put emoji Unicode glyphs in the dialogue font |
| Transition in/out | Explicit target, preset/duration/easing and completion mode |
| Audio / Wait | Asset/channel or cancellable duration; no independent gameplay clock. [Session 070 audio design](16_Audio_Authoring_And_Playback_Plan.md) specifies contextual selection, optional voice and playback ownership; not implemented. |
| Gameplay intent | Calls existing validated `GameSession` transaction APIs, never an independent effect-handler reward system |

Use an authored effect/preset registry and a presentation target registry. Resolve actors by stable character ID plus stage/portrait context; do not assume a room sprite and the VN portrait are the same target. Put transient offsets/overlays under a suitable authored presentation layer. Extend visual definitions using data-authored expression keys when needed, with legacy mappings; do not require C# enum edits as the normal expression-authoring workflow.

New command families should require a new handler/schema/preset, not a rewrite of story storage or existing scripts. Initial scope does not include walking movement, arbitrary physics, a cinematic 3D system or integrating every animation middleware.

## 7. Execution, concurrency and interruption contract

An ordered sequence needs defined behavior when the player reveals text, continues, skips, changes language, exits, cancels, loads or previews from a point. Typed arguments alone are insufficient.

| Family | Normal completion | Skip / instant finish | Cancel / exit / rollback |
| --- | --- | --- | --- |
| Say | Typewriter then player continuation | Reveal text; changing language must not replay earlier commands | Retain stable cursor or leave via event cancellation |
| Choice | Wait for valid player selection | Never silently select an answer | Existing event rollback/deferral rules |
| Show/hide/transition | Await when authored as blocking; otherwise track in scene scope | Apply documented final visual state once | Stop animation and restore the event/checkpoint visual baseline |
| Shake/emote | Usually transient, optionally awaited | Suppress or complete/clear according to preset | Remove offsets/overlays, release pooled resources |
| Audio/stateful presentation | Apply channel/state changes with explicit lifetime | Apply final intended state without duplicate starts | Restore reconstructible baseline, not live coroutine handles |
| Wait | Finish duration or explicit signal | Complete immediately | Cancel without advancing world time |
| Gameplay intent | Existing validation, payment/effect/history policy | Preserve exactly-once application | Restore the same existing event-start snapshot policy |

Commands need cancellation tokens/scope, completion signals, explicit parallel groups and teardown. Define timing such as before a line, after typing or on Continue; typing completion and clicking Continue are different events. Unknown commands or missing required targets block validation rather than silently disappear.

Keep persistent presentation state (active background/portraits/expression/visibility/audio identifiers) separate from ephemeral shake/tween/emote progress. Reconstruct durable visuals on restore; do not serialize Unity coroutine/tween handles. Visual preview replay must not reapply world effects.

The current host may change flags/resources at a validated choice and roll them back if deferred. Preserve that semantics through a single game-state bridge; do not invisibly move all commits to a new point and break conditional branches. No second scheduler, phase clock or reward ledger. Future mid-dialogue save/rollback/backlog/auto/skip features need their own checkpoint/read-history contracts; initial migration can retain current safe-point saves.

## 8. Thai/English ownership and interchange

Default proposal preserves the **existing catalog as the Thai source authority**. A story document stores text references; the manuscript edits those catalog entries inline. Translations/revisions remain catalog-owned. Word edits, document moves and reordered lines must not regenerate IDs or mark unrelated translations stale.

Current keys include old node IDs. Preserve them verbatim through consolidation; physical packing is not permission to re-key 689 records. A migration manifest maps old GUID/node/line/choice/text keys to new document/entry/block locations. New keys get stable generated identities independent of positions or visible names.

Native Yarn/Naninovel source formats can own inline base-language text. If selected, explicitly choose one authority: either a supported provider/generated-backend path preserves catalog ownership, or an approved migration makes the native script own Thai and the catalog's Thai becomes a read-only derived mirror. Prove this in the fit check. Never let script, SO, catalog and CSV all own independently editable copies of the same Thai sentence.

`.md` remains suitable for plot/research/drafts; it is not automatically executable. Translation CSV remains an exchange view, not story flow. Optional text/Fountain/native-script interchange is later scope with parse errors, stable IDs, roundtrip/ownership rules and no silent overwrite; a new custom DSL is not required now.

## 9. Migration and compatibility

1. Start with a **legacy graph reader**: view the current complete opening and all three responses/shared continuation in one context before changing storage. Preserve shared links; loops/calls require explicit markers instead of infinite expansion. Do not infer single ownership from the first catalog result.
2. Define document/entry/reference contracts and deterministic validation/index data. Separate authoring sources from generated runtime caches. If using an importer, register dependencies and stable import identifiers; never turn generated output into a second editable source.
3. Convert one bounded event with an explicit old-to-new ID/reference manifest. Preserve branch conditions, choice IDs, effects, flags, current event windows/costs and source/translation keys. New entry references can coexist with legacy node references via an explicit adapter.
4. Keep current SO assets/metas and original save files available for rollback. Migrate event/greeting references deliberately and validate every incoming/shared edge. Do not clone a shared continuation into multiple independently editable texts or delete original GUIDs during the trial.
5. Current localized v9 compatibility hashes the serialized graph. Consolidating physical structure can change that hash even when story intent is unchanged. Define/check a versioned compatibility mapping; do not pretend storage migration is translation-only, weaken guards or assume the previous v8-to-v9 mapping covers it. Bump/namespace a future format if required; it is not chosen in this plan.
6. If a future runner persists execution position, use document/entry/block IDs, compatible content revision, host state and reconstructible presentation state. Arbitrary line indexes, file paths or a package's variable JSON alone are insufficient. Safe-point-only behavior stays valid until mid-scene resume is designed and verified.
7. Expand by coherent event/collection after owner authoring acceptance. Update the adoption register/Handoff with real source ownership, coverage and evidence; no success from merely creating a container or drawing a graph.

## 10. Build/reuse choice

| Candidate | Why consider it | What still needs proof |
| --- | --- | --- |
| Scene documents + current runner adapter — default conservative route | Preserves the current host/Prefab/catalog semantics; targeted authoring improvement can ship first | We own new command scheduling/presentation/QoL maintenance; do not accidentally rebuild an entire VN engine without evaluating reuse |
| Naninovel v1.21 — first external fit candidate | Documented VN-oriented editor, typed async commands and presentation facilities; checked Unity 6.3 compatibility | Existing layout/font/art ownership, catalog IDs/Thai authority, event cancellation/transactions, save bridge, Story Editor WebView/domain-reload requirements and actual owner workflow |
| Yarn — secondary dialogue backend candidate | Documented scene-sized scripts, line IDs, compiler/provider and awaited handlers | Thai-facing authoring adapter, custom effect/stage implementation and safe resume/rollback; built-in variable persistence alone is insufficient |
| ink, articy, Pixel Crushers | Useful alternatives for narrative structure, planning or Unity dialogue integration | Only evaluate if the first candidates miss a concrete requirement; no simultaneous migration to several backends |
| Ren'Py / Godot Dialogic | Strong architectural references for scenario-oriented authoring | Switching engines would be a broader project migration; not recommended for this task |

No price/licensing, performance or compatibility guarantee beyond the cited documentation is established. Use a representative isolated scene to choose, not a popularity assertion or the word professional.

## 11. Delivery stages and acceptance

| Stage | Concrete deliverable | Required evidence / gate |
| --- | --- | --- |
| A — Complete-event authoring proof | Legacy-reader manuscript/index of the three-person opening: root, all three responses and shared join; source/destination navigation, all-speaker roster, path-aware conditions and exact contextual search | Owner finds the event or a character's situation-specific passage, sees every registered upstream path/choice destination/join and all speakers, understands relevant gates, navigates back, edits/reorders, previews and saves/reopens without hunting SOs. Source IDs/English stay intact. This is the recommended next implementation milestone. |
| B — Backend/storage fit and one-event migration | Compare current-runner document adapter with pinned Naninovel on that same event; resolve text authority and host/presentation bridge before full runtime work | No purchase/install inferred from this research. Select a backend only after the opening's branches, cancellation, once-only effects, translation IDs, layout and save policy are demonstrated. If evaluation access is unavailable, record the limit and continue the legacy authoring proof rather than declare a package winner. |
| C — Directing foundation | One shake, one graphical emote, one entry/exit transition preset; typed Thai controls and real isolated preview | Confirm wait/nonblocking behavior, reveal/Continue distinction, skip completion, cancellation/rollback cleanup and saved artist layout. No full effects library required. |
| D — Scaling and wider adoption | Incremental index/virtualized manuscript, validation, branch-aware review export, staged event/collection migration | Use synthetic Thai/English content plus the real opening. Check identity/reference preservation, translation review, representative saves and actual authoring usability. Extend existing evidence proportionately. |

Proposed benchmark corpus: **10,000 dialogue lines, 2,000 choices, 200 documents**, with joins, referenced sub-scenes and Thai text. Proposed warm-index targets on a recorded development machine: search result p95 under 200 ms; opening/jumping to a matching row under 500 ms; routine row edits without perceptible blocking (target under 100 ms). Record cold-load/import cost separately. These are acceptance targets to validate, not measured results or guaranteed engine limits. Do not create thousands of story assets merely to benchmark or run unrelated full-game suites after each UI edit.

These technical budgets support the owner workflow, but do not replace it. Primary acceptance is successful discovery/context/editing tasks in section 4.1, including finding a character's situation-specific line without remembering its filename or exact text. The owner has explicitly stated that the concern is authoring UX rather than game runtime performance.

Use a compact semantic check set: all three opening branches/shared join, unchanged IDs/keys after move/reorder, valid/missing command arguments, nonblocking/blocking cancellation, skip/language changes without duplicate effects, compatible/incompatible save cases, editor Save/Undo/reopen and representative Thai/English wrapping. Expand testing only for changed risks. Owner discoverability/authoring acceptance is separate from compile/runtime success.

## Current handoff

**Session 071:** owner trial requests a unified writer, concise English labels/hover help and explicit execution/state visibility. The [implemented sandbox revision](17_Unified_Story_Workspace_And_Execution_Trace.md) adds inline authoring, performance/drag/insertion, typed entry/choice logic and shared actual Test/State/Trace. This supersedes the original text-overlay-only UX, not the production adoption/backend gate. Full branching-subgraph drag, large-corpus proof and audio remain future work. [Updated Thai trial](../02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md).

Session 069 delivers a bounded [Stage A sandbox](../../Sandbox/StoryWorkspace/README.md), not the proposed document backend or command runtime. Session 070 adds the [owner trial guide](../02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md) and [audio plan](16_Audio_Authoring_And_Playback_Plan.md); audio controls remain unimplemented. Owner acceptance of the existing sandbox is pending; acknowledgment/intention to test is not approval. Keep Localization L2 and its CSV workflow active, preserve production assets and use the sandbox gate before integration. Complete actual discovery/read/edit/Save/preview acceptance, with Stage B deciding runtime/storage ownership before broad migration. Do not rewrite story/balance, resume Producer mechanics, regenerate assets or advertise planned controls as available.
