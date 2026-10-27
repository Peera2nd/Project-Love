# Session 070 — Owner trial guidance and comprehensive audio plan

Date: 2026-10-27 (Asia/Bangkok). Owner acknowledges the delivered sandbox, intends to test and requests concrete trial guidance plus a comprehensive designer-first audio design. No audio implementation or production adoption requested.

## Delivered

- [Thai story-authoring trial guide](../../02_Narrative_And_World/08_Story_Workspace_Owner_Trial.md): opening read-through, all three branches/shared join, phrase/speaking-character discovery, incoming/outgoing/path conditions, Thai/English draft editing, Undo/Save/reopen, preview and optional day/phase eligibility probe. Includes a blank owner-feedback table, current-language-preview restart requirement and explicit possible-path versus playable-state distinction.
- [Audio Authoring and Playback Plan](../../04_Technical_And_Tools/16_Audio_Authoring_And_Playback_Plan.md): music/ambience/SFX/UI/optional speech; exact-line recordings versus reusable barks; None means no speech; speaker/locale/intent/context-filtered audition/selection rather than unfiltered lists; bank collections and stable anchors, shared-cue scope/where-used, revisions/VO import, one runtime owner, mixing/ducking, skip/reveal/cancel/locale/rollback, settings/save compatibility, scale, isolated Editor audition proof and gated adoption.
- Master/Architecture/Data Architecture/Task Board/Handoff and sandbox/authoring-plan links updated. ERR-108 records the corrected stale “new Workspace does not exist” footer; this does not revise historical session outcomes.

## Evidence and decisions

- Ordered intake and current sandbox/source reads establish actual controls. The current preview takes the English toggle when constructed; instructions therefore require reopening preview after changing it rather than promise instant language switching.
- Scoped owned source/asset audit found no project audio playback API use or native audio/mixer files. Historical test names referring to character “voice” mean written variants. Unity audio support is not the same as an implemented project audio system.
- Current line IDs/speakers/localization keys support stable audio anchors. Generic Refresh/state notifications cannot be the playback trigger. Existing save busy guards and reflected localized gameplay hash require explicit later compatibility work; audio additions are not automatically harmless to saves.
- Official Unity 6000.3 mixer/import/scheduling/Editor preview references and stable Naninovel v1.21/Ren'Py voice docs checked. Context filtering, explicit silence, bank granularity and stage gates are project proposals, not proof that those engines supply this exact UI. Public Editor preview hooks do not prove an isolated audible transport; the plan requires a technical spike plus real listening.

## Verification and limits

Verified 327 local Markdown references across the 11 changed/new documents with no missing targets, current-control names/status claims, and empty production/sandbox-code/test diffs; whitespace check passes. No C#/Unity assets/packages/settings/saves/CSV changes or new Unity/test/audio runs. Prior Session 069 5/5 + 2/2 bounded evidence remains historical evidence; it does not verify future audio or current owner keyboard acceptance. No recording downloaded, playback performed, vendor selected or UX result invented.

## Continuing agent

Owner trial feedback is pending. Do not treat this acknowledgment as approval or merge the sandbox. Audio remains Designed; future implementation starts with contextual selection and audible preview proof in isolation, followed by one sparse-voice opening scene, larger metadata discovery trial and explicit owner acceptance. Preserve empty-slot silence, current catalog ownership/stable IDs, existing saves and authored views. See the canonical plan for details rather than duplicating it in code comments/tasks.
