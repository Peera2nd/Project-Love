# VN Authoring Systems — Primary-Source Comparison

Session 067, checked 2026-10-26. Owner requested delegated research and a scalable authoring/presentation plan. Three research agents covered Ren'Py/Naninovel, ink/Yarn and articy/Pixel Crushers/Unity; the parent checked current project data and Dialogic/Unity references. No products were installed, purchased or integrated. Documentation capabilities are not Project Love verification, market-share evidence or a universal industry standard.

## Findings

| System | Documented authoring/execution pattern | Implication for this project — inference |
| --- | --- | --- |
| Ren'Py | Scenario files mix ordered dialogue, labels, choices and directing statements. Translation identifiers and statement/presentation-aware saves are documented. | Keep a readable scenario together; use stable identities and explicit presentation restoration. Moving this project to Ren'Py would be an engine migration, not a storage fix. |
| Naninovel | `.nani` scenario documents, row-based Story Editor, hierarchical script/label graph and content search; typed extensible commands, asynchronous completion/cancellation and reusable effects. | Closest documented full-VN feature candidate to evaluate within Unity. Integration with our scheduler, saved Prefabs, catalog and transaction boundaries remains work. |
| ink / Inky | Knots/stitches/includes, choices/diverts/gathers in continuous text; play-as-you-write and source-linked diagnostics; host-driven presentation and JSON story state. | Good reference for readable branches/joins and authoring feedback. It does not automatically supply Unity portraits/effects, our saves or the current localization pipeline. |
| Yarn Spinner | Multiple named nodes can share a text file; writer tools include graph/preview/search-related navigation. Explicit line tags, translation tables and a Line Provider; registered awaitable C# commands. | Dialogue compilation/authoring candidate. Host presentation, cancellation, source-language ownership and game transaction integration must be designed. Built-in variable saving is not arbitrary dialogue-position saving. |
| articy:draft X | Hierarchical narrative data, screenplay-like writing, broad search and branch-aware localization ordering; Unity importer/database. Document-to-Flow conversion creates independent copies. | Strong reference for context/navigation. Our manuscript/graph views should share a source rather than copy the documented independent-document conversion workflow. |
| Pixel Crushers Dialogue System | Conversation containers contain entries; a Sequence field handles presentation. Maintainer examples discuss completion/messages and separate conversation-state/custom-save integration. | Another Unity integration candidate; reinforces separation between dialogue, sequencing and game saves. Current package compatibility and full manual coverage were not verified. |
| Godot Dialogic | Text-backed timelines with dialogue, choices, labels, transitions and character events; visual/text authoring is documented. | Useful event-list/directing reference. It is not a reason to port the working Unity game to Godot. |

These products demonstrate several workable patterns, not one prescribed file extension. None establishes that 117 SO files are intrinsically too slow, that a giant graph is usable, or that a package will preserve this game's state automatically.

## Verified sources and limits

### Ren'Py

- [Quickstart](https://www.renpy.org/doc/html/quickstart.html): readable scenario statements and scene/actor/audio/choice operations.
- [Translation](https://www.renpy.org/doc/html/translation.html): translation units/identifiers and localization beyond dialogue.
- [Saving/loading/rollback](https://www.renpy.org/doc/html/save_load_rollback.html): statement location and presentation/return state matter; cross-release compatibility is not guaranteed.

### Naninovel — use the checked v1.21 documentation

- [Story Editor](https://naninovel.com/guide/editor): manuscript rows, hierarchical navigation, global content search and source-linked execution context.
- [Scenario scripting](https://naninovel.com/guide/scenario-scripting): commands, labels, nested flow, explicit text identity and async sequencing.
- [Special effects](https://naninovel.com/guide/special-effects): parameterized shake/transition/effect facilities and custom spawned effects. No built-in feature specifically named Emote was established; our emoji/emote can be a custom preset/handler.
- [Custom commands](https://naninovel.com/guide/custom-commands): typed command parameters and distinct cancellation/instant-completion behavior.
- [Compatibility](https://naninovel.com/guide/compatibility): checked v1.21 documents Unity 6.0/6.3 LTS, URP/uGUI/TMP/Input System support, with Story Editor platform/WebView and domain-reload requirements. This does not prove our integration. Do not mix the linked v1.22-preview requirements/APIs with this version or upgrade Unity for this research.

### ink and Yarn

- [Writing with ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md): knots/stitches, includes, branches and gathers.
- [Inky](https://github.com/inkle/inky): authoring, live diagnostics/play pane and source navigation; no full Project Love visual preview inferred.
- [Running ink](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md): host APIs, tags/external functions, state JSON and side-effect/lookahead considerations. Checked pages did not establish a drop-in line-table workflow equivalent to Yarn's documented provider.
- [Yarn language](https://yarnspinner.dev/docs/cheat-sheet/): script/node control-flow constructs.
- [Official Yarn VS Code extension](https://marketplace.visualstudio.com/items?itemName=SecretLab.yarn-spinner): documented editor graph/preview/diagnostic/localization tools, not proof that every graph edit synchronizes bidirectionally.
- [Yarn localization/assets](https://yarnspinner.dev/docs/unity/06-localisation-and-assets/): line identity, CSV and providers. Native source-language script ownership differs from our catalog-owned Thai.
- [Yarn commands](https://yarnspinner.dev/docs/unity/05-connecting-to-your-game/01-commands/): registration and awaited handlers. Defining an action name does not implement its visuals.
- [Yarn variables/storage](https://yarnspinner.dev/docs/unity/05-connecting-to-your-game/04-variables-and-storage/): built-in persistence saves variables, not dialogue position; the host must manage resume policy.

### Other authoring references

- [articy Unity integration](https://www.articy.com/en/downloads/unity/): central imported data/traversal and debugging integration.
- [articy Document View tutorial](https://www.articy.com/en/adx_basics_documents/): screenplay workflow; converted Flow/document copies are independent.
- [articy search](https://articy.com/help/adx/UI_Search.html): project/subtree/property/language-aware search.
- [articy Localization View](https://articy.com/help/adx/Localization_View.html): selectable traversal order, discontinuity markers and translation status. A branching script has no single universal flat order.
- [Pixel Crushers VN Framework manual](https://pixelcrushers.com/dialogue_system/download/Visual_Novel_Framework_Manual.pdf): conversation/entry/Sequence information was available in official indexed excerpts; direct manual/PDF access returned 403. Do not describe this as a full current-manual review.
- [Pixel Crushers maintainer sequencing explanation](https://forum.pixelcrushers.com/post/run-sequencer-command-after-continue-13718836): completion/messages and timing distinction.
- [Pixel Crushers custom-save discussion](https://forum.pixelcrushers.com/post/dialogues-save-with-custom-save-system-13719652): conversation state and custom game saves need coordination.
- [Dialogic timeline syntax](https://docs.dialogic.pro/timeline-text-syntax.html): text-backed timelines, labels, choices and directing events.
- [Dialogic character events](https://docs.dialogic.pro/event-character.html): entry/exit/update animation, movement and transition settings.

### Unity implementation references

- [Unity 6.3 ListView](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-element-ref/uie-uxml-element-list-view): recycled visible rows support a scalable editor approach. Actual Thai wrapping/edit/search performance must still be measured.
- [ScriptedImporters](https://docs.unity3d.com/6000.0/Documentation/Manual/ScriptedImporters.html): a custom source can produce imported assets/subassets with stable import identities. This is an optional architecture technique; no custom importer is required merely to consolidate existing SO contents.

## Project-specific conclusion

Adopt scene/event-oriented authoring, continuous script + focused flow views, contextual indexed search, stable text/entry/command IDs and explicit execution contracts. Keep one authority each for story structure, Thai source and gameplay state. Evaluate a full-VN backend before rebuilding mature VN functions, but do not combine several runtimes or silently replace existing ownership/save contracts.

The [proposed Project Love plan](../15_Scalable_Story_Authoring_And_Presentation_Plan.md) specifies the default data/UI direction, bounded backend evaluation, migration and acceptance. It is designed, not implemented or owner-approved in detail.
