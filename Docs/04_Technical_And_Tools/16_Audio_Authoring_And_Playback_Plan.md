# 16 — Audio Authoring and Playback Plan

Session 070 · 2026-10-27. **Designed only.** Owner requests current-sandbox test guidance and a comprehensive audio plan, especially designer discovery at hundreds of sounds. This does not authorize production integration or audio implementation in this session. [Existing Story Workspace plan](15_Scalable_Story_Authoring_And_Presentation_Plan.md) and AGENTS 11 isolation/owner-acceptance gate remain.

## 1. Source-backed starting point

- Owned runtime/Editor sources have no AudioSource/AudioClip/AudioMixer playback implementation; no owned wav/ogg/mp3/aif/aiff/mixer assets were found. Unity's audio capabilities are available, but a project audio library, selector, routing, voice mapping and playback lifecycle are not implemented. Historical tests use “voice variants” to mean alternate written dialogue, not recorded audio.
- `DialogueLine` already has a stable `lineId`, speaker, expression and localized text reference; choices/events/characters have established identities. Use these anchors, not a sentence string, array offset or filename. [Current schema](<../../Project Love/Assets/_Project/Scripts/Data/DialogueNodeSO.cs>).
- `DialogueRunner` owns advancement/choice/deferral; `IsometricRoomView.Continue` distinguishes revealing text from advancing. Generic state notifications/Refresh and locale redraw occur more than once per line. Starting audio from these render calls would replay it incorrectly.
- Current sandbox stores **text overlays only**; its same-view detached preview does not provide new audio controls. The voice/music examples below must not be described as available UI.
- `SaveStore` rejects busy/dialogue saves; v9 does not supply arbitrary mid-line resume. `LocalizedGameplayFingerprint` reflects serialized fields except explicit exclusions: adding audio fields to existing definitions is not automatically save-compatible. Preserve the current baseline and test a bounded integration later.

**Recommendation:** build one author-facing audio library and context-aware picker, optional voice slots on story lines, and scene-level sound directions. Keep audio execution behind a narrow presenter/provider boundary. Start with Unity audio, not a simultaneous FMOD/Wwise/vendor integration; revisit the output adapter only when actual needs justify it. This is our proposed fit, not measured performance evidence or a vendor decision.

## 2. Authoring categories and ownership

| Kind | Normal authoring location | Designer controls |
| --- | --- | --- |
| Music | Room/phase default soundscape; scene overrides at explicit anchors | Theme/mood preset, play/retain/stop, crossfade, loop; same cue does not restart by default |
| Ambience | Soundscape or scene background layer | Room/weather/time tags, bounded layers, fade and lifetime |
| Story SFX | Direction beside a line/choice/scene anchor | Intent such as door/phone/chair, timing, gain preset and lifetime |
| UI SFX | Reusable UI sound theme bound to the existing authored UI | Confirm/cancel/notification; hover/click cooldown; no need to write a cue on every dialogue line |
| Line recording | Optional voice slot on one stable line | Exact recording/take, spoken locale, transcript/direction, end/advance behavior |
| Reusable vocal reaction (bark) | Same optional slot, explicitly marked as a short reaction | Speaker + intent (laugh, assent, hesitation), energy/emotion; not a claimed full-line recording |

Voice and bark share the Voice volume group; their asset-selection workflows differ. A narrator has its own voice identity; an untagged human recording must not be offered as every character's voice. Typewriter blips are a separate optional future preset, off by default, never one bark per Thai code point/combining mark.

### Silence contract

- A new/empty line voice slot is **None / ไม่มีเสียงพูด** and plays no voice. Do not automatically fill it from emotion, a character default, matching prose, file naming or randomness. On advancing into an unvoiced line, stop the previous line's voice under the default stop-on-advance policy.
- “None” is an intentional state, distinct from “recording planned but missing”, “file missing”, “wrong speaker/locale” or “recording needs review”. Missing optional speech must not block the story; missing assigned assets need an author warning, not a false successful assignment.
- A character voice profile narrows valid choices and mix defaults. It never gives permission to speak on every line. Any advanced cross-line sustain must be a visible directing instruction; it cannot be inferred from an empty slot.
- Music/ambience have **inherit / replace / stop** controls. These are different from line-voice None: inheriting a soundscape should not be mistaken for a new voice assignment.

## 3. The picker: choose an intention before browsing files

The normal path is the selected story line, not Unity's unfiltered Object Picker. Preserve the scene and neighboring dialogue while the picker is open. The compact line control is `เสียงพูด: ไม่มี [เลือกเสียง]`; an assigned row adds a meaningful Thai label, duration, audition, replace and clear. Details stay collapsed until requested.

Proposed flow for a Plaifah line:

1. Open **เลือกเสียง**. Header shows **ปลายฟ้า · เสียงไทย · ฉากปัจจุบัน**, derived from stable data and a separate spoken-language choice. Explain each active filter; allow deliberate changes.
2. Start with **บันทึกตรงประโยคนี้**: only this line's linked recordings/takes. Zero takes means “ยังไม่มีเสียงบันทึกของประโยคนี้”, not a list of hundreds of unrelated recordings.
3. For a reaction choose **เสียงสั้นใช้ซ้ำ**, then a purpose chip such as **หัวเราะ / ตอบรับ / ลังเล / ตกใจ**. Show counts before expansion. Speaker, spoken locale and active voice set are hard compatibility filters; expression/energy, scene-pack matches, favorites and recency rank only compatible results.
4. Display a small paged/virtualized shortlist, for example “หัวเราะเบา · นุ่ม · 1.1 วินาที”, with a manual **ฟัง** button, take identity and **เลือก**. A selection preview can show its transcript and the current story line. Do not auto-play hundreds of matches or silently assign the first match. The example names/counts/durations are illustrative; no clips exist yet.
5. **ค้นคลังทั้งหมด** is an explicit secondary path with visible filters, facets, search and a no-results explanation. Expanding scope must not silently remove the speaker constraint and suggest another actor. **ไม่มีเสียงพูด / ล้างการเลือก** stays easy to find.

Exact recordings and barks must remain visibly distinguishable: a reusable “อืม” can support a long written sentence, but is not its literal dub. Shared generic SFX stay in SFX rather than leaking into character speech results. No semantic AI matching is a prerequisite; optional future suggestions must explain their match and require selection.

### Candidate cards and safe editing

- Show Thai author label, actor/voice set, locale, intent, duration, energy, recording transcript when relevant, review state and used-in count. Filenames, stable IDs, audio encoding and raw import controls are advanced details.
- Solo audition uses one transport and an explicit local preview volume. Stop/replace the current audition when switching clips; stop on picker close/reload/Play transition. Do not change player preferences or a shared cue's authored gain merely to audition it.
- Audition is separate from selection. Confirm a replacement, record one Undoable binding edit, mark unsaved state, and retain it through navigation/Save/reopen. Do not require a different Asset Inspector for every sound.
- Gain/fade overrides on a line belong to that binding. Editing the reusable cue affects all usages and must show that scope/usage list; offer “เฉพาะจุดนี้” versus an explicit library edit. A label rename or asset move preserves all IDs.
- Where-used search returns exact scene/line/branch/direction anchors, including all callers of shared content. Replacing many bindings requires a scoped before/after preview and Undo, not “replace all similar names”.

### Music and effects discovery

Music starts from the scene's selected soundscape/theme/mood and an intentional change point; ambience starts from room/time/weather. SFX starts from a visible purpose (phone, door, UI) and the current scene pack. A curated pack is a shortcut over the same cue IDs, not a duplicate audio source. Favorites/recent/pack results still honor kind and compatibility. Global keyword/tag browsing remains available as a fallback.

## 4. See the whole scene's sound design

Keep speech controls inline beside manuscript lines and add a compact **เสียงของฉาก** view using the same selection/anchors as the manuscript. It lists music/ambience defaults, overrides, start/stop/fade markers and SFX in story order. Clicking a row jumps to the corresponding line or boundary. Show inherited music with its origin rather than pretending every line owns a BGM field.

Provide **Solo**, **With scene mix**, mute-by-kind, replay-current-voice and play-from-anchor controls once the preview adapter is proven. Expose gaps, stale recordings, missing clips and excessive overlap in a scene summary. A shared join can have different incoming background states; show them as alternatives, or require an explicit reset cue for a consistent join. Do not invent one universal inherited soundtrack for all branches.

Visual shake/emote/transition and audio directions share the planned presentation command/anchor contract. Audio does not introduce another story scheduler or require Unity Timeline per sentence. Sample-level synchronized effects are later scope; routine writers use “ก่อนประโยค / ตอนเริ่มประโยค / หลังจบเสียง / ตอนเลือก / จบฉาก” and presets. An advanced offset is an explicit authored media-time value, never a new in-game phase clock.

## 5. Proposed data boundaries (not existing types)

| Definition | Contents and source of truth |
| --- | --- |
| `AudioLibrarySO` | Campaign library configuration, bank references, tag taxonomy and audio settings/theme references; no author-facing giant clip array |
| `AudioBankSO` | A collection of cue records, grouped by character/recording batch/scene/theme; many clips per bank, not one SO per utterance |
| `AudioCueRecord` | Immutable cue identity, author label, kind/actor/voice-set/locale, tags/intent/energy, asset locator, duration/analysis metadata, gain/loop defaults, takes and review/source information |
| `VoiceBinding` | Explicit None / LineRecording / Bark, selected cue identity, optional take, timing overrides and language policy; no copied editable dialogue text |
| `StoryAudioSheetSO` | Initial legacy-adapter presentation sheet for a scene/document, containing line voice bindings and anchored directing cues, separate from source node files during the trial |
| `SoundscapePresetSO` / UI sound theme | Reusable room/phase music/ambience/UI defaults and mix behavior; source ownership and per-scene overrides are visible |
| `AudioPresentationState` | Reconstructible active music/ambience/mix stack; not AudioSource objects, playback coroutines or a second game state machine |

Use namespaced immutable cue IDs, e.g. bank identity + record identity. Label/path/prose changes cannot rekey cues. Story anchors include canonical owner identity + stable line/choice/entry ID, validated for duplicates; an array index is not an identity. An exact-line voice is authored once for shared content. Context-specific music/SFX overrides target an explicit caller/entry context, not cloned dialogue.

Initially keep **one authoritative sidecar** for the bounded legacy scene in the sandbox; drafts overlay it locally. Do not also add editable duplicate voice fields to `DialogueLine`, CSV and script. If Stage B later embeds presentation data into `StoryDocumentSO`, migrate the same IDs/bindings with a manifest and retire or derive the old sidecar. The selected storage/backend determines the final serialized layout before wider adoption.

AudioClip files are still native Unity assets; a bank improves the designer's authoring unit, not the physical fact that recordings need files. Prefer imported real clips/GUIDs and stable build locators. Keep a resource-provider seam: start with bounded per-scene/bank references and measured load/unload behavior; metadata search must not load every decoded clip. Addressables or an external engine are optional measured follow-ups, not packages required by this plan. Future owned Unity assets belong under `Assets/_Project/` inside the appropriate sandbox/production project.

### Localization and recording production

- Reuse current localization keys/source revisions as read-only transcript context. Do not create a second editable Thai sentence inside the audio library. Recordings bind stable line IDs plus speaker/spoken locale and the relevant recorded-text revision/hash.
- Text language and voice language are separate choices: English UI/text with Thai voice is valid if explicitly selected. Changing UI language redraws subtitles without replaying an already-playing voice. Do not silently substitute a different spoken language or another speaker when a recording is missing; use silence by default unless an explicit fallback policy is selected and shown.
- Editing a recorded line marks its exact recording as needing review, without deleting/reassigning the clip. A speaker change marks an incompatible binding. Reused barks have separate direction/appropriateness review, not an exact full-sentence match requirement.
- Optional VO exchange exports stable IDs, scene/branch context, speaker, current text, spoken locale, performance notes, take/status and recorded revision. CSV is optional interchange; Excel is not required for normal editing. Batch import previews exact ID/locale/take matches, collisions, stale text, missing assets and unknown speakers before any assignment. Prose-based guessing is not a publishing action.
- Intentional None does not reduce a required-recording coverage score or generate a missing-audio warning. Planned recording requirements are a separate explicit production list. Rights/source/credits metadata can be stored with bank assets; no new recording pack is acquired in this session.

## 6. Runtime structure and routing

Proposed dependency direction:

```text
Story Workspace -> library index + serialized bindings/drafts
Story/audio adapter -> typed presentation events -> AudioPresenter
AudioPresenter -> asset provider + Unity audio/mixer output
Player preferences -> user-volume stage
Detached preview -> separate preview session/output adapter
```

Runtime never calls the Editor picker. AudioPresenter never mutates story flags, grants rewards or advances the day. If a future backend wins the fit check, adapt its voice IDs/audio service to this contract and retain one playback owner; do not run two managers against the same story events.

Use Master with child Music, Ambience, SFX, UI and Voice groups. Voice can support per-character gain/mute later. Reusable author mix presets and voice-triggered music reduction are separate from user-volume parameters so fades/snapshots cannot reset a player's sliders. Route voice outside the group it ducks. Keep duck attack/release and overlapping reasons centrally owned; a second voice/audition must not permanently lower music.

Music needs bounded A/B sources for crossfades; regular speech is non-overlapping by default. SFX uses a bounded pool with category/instance priority and cooldown; UI clicks cannot grow an unbounded queue. Mostly 2D presentation fits this fixed-room VN; spatial playback is an explicit future cue option, not walking/physics scope. Clip gain/limiting are not proof that a clipped source recording sounds clean; listen to representative mixes.

## 7. Exact playback and interruption rules

Emit typed SceneEntered/Exited, LineEntered/Exited and choice/anchor presentation events from actual execution boundaries via a thin adapter. Use session generation + line occurrence/visit identity so a real loop/revisit can play again, while repaint, selection highlighting, typewriter reveal, stats refresh and localization redraw cannot replay it. IDs alone cannot distinguish two legitimate visits to the same line.

| Trigger | Proposed behavior |
| --- | --- |
| Enter voiced line | Resolve compatible cue, start once; late asynchronous loads must match the same generation/occurrence or be discarded |
| Empty voice slot | No new voice, stop previous line voice by default; retain subtitles and normal reading behavior |
| First click while text reveals | Reveal text only; do not restart voice or play the next line |
| Advance / choice / jump | Cancel outgoing voice under default policy, cancel stale loads; apply explicit destination cues once |
| Auto-read (future) | Respect minimum text-reading time and optional audible-voice completion plus a configured pause; None, muted or missing voice cannot cause an indefinite wait |
| Skip (future) | Stop speech and suppress routine one-shot bursts; reduce music/ambience commands to the final intended persistent state, without starting every skipped song |
| Replay voice / backlog (future) | Replay audio only for the selected binding, without re-entering story execution, reapplying choices or rewards |
| Text locale redraw | Redraw text only; preserve current speech occurrence; voice-locale changes stop/re-resolve at an explicit safe boundary |
| Same music requested again | Keep playing unless restart is explicit; avoid a reset on every room refresh |
| Scene exit / call-return / event deferral | Release scoped audio overrides and restore the prior room/parent soundscape; cancel scene one-shots and voice; do not replay transient effects when restoring |
| Pause / focus loss / device reset | Explicit platform policy; pause/resume or fade by category; rebuild scheduled handles safely after device reset, not from stale DSP timestamps |
| Picker close / preview dispose / domain reload | Stop owned preview transports, release clips/duck handles and listeners; leave the player's scene/settings untouched |

Default scene overrides are scoped; **inherit / stop / persist beyond scene** is explicit. For shared joins, either preserve the actual incoming sound state or apply a visible reset cue; static authoring summaries show all incoming possibilities. Cancellation restores presentation state only, leaving the existing GameSession transaction/rollback owner intact.

Do not promise seamless looping just because AudioSource.loop is set. Prepare loopable assets; intro/loop/outro regions and beat-aware transitions require explicit asset segments/scheduling and audible proof. Unity DSP scheduling is a media timing tool, not another game clock.

## 8. Preview, settings, save compatibility and scale

**Required preview proof before polishing the picker:** demonstrate one Edit Mode solo audition with reliable stop/dispose, then scene-mix preview using the same cue resolver/mix policies and detached story state. Existing `AuthoringPreview.Render` proves an image, not audible routing. Verify Unity 6000.3's supported Editor preview integration; public Editor preview hooks exist but do not guarantee our desired audio transport or mixer isolation. Do not build the foundation on private AudioUtil reflection and call it supported. A small isolated Play Mode audio harness can establish runtime behavior, but does not satisfy the owner's Edit Mode authoring-preview acceptance on its own. Record any API limitation before choosing an alternative workflow.

Player settings should expose Master/Music/Ambience/Effects/UI/Voice volume, mute and eventual spoken-language/auto-read options with subtitles always usable. Designer preview has separate local audition gain. Do not write user preferences during preview. Important events need readable visual/text feedback as well as sound.

At today's safe-point saves, restore reconstructible BGM/ambience identities and intended mix state if persisted, not live AudioSources/fade handles/one-shot SFX. Explicitly choose the audio persistence owner/version before integration. No arbitrary mid-line restore is promised. Audio catalog, gameplay compatibility hash and optional presentation-state compatibility need separate treatment backed by tests; do not weaken v9 guards or assume a new `GameConfigSO.audio` field is excluded. Original save files/GUIDs remain available for rollback.

Search an incremental metadata index; virtualize cards and calculate waveform/analysis lazily. Preload only active/next scene or a bounded voice window, cancel on branch changes and release bank references safely. Consider streaming longer music, suitable compression for dialogue and preloaded short latency-sensitive SFX, measuring tradeoffs on the target hardware. Do not mark every tiny cue as streaming or decode the entire voice catalog for a picker. Existing Git LFS audio rules are useful for source recordings; caches/previews belong in ignored generated folders. No measured RAM/latency/binary-size guarantee is made here.

## 9. Sandbox stages and acceptance

| Stage | Deliverable | Gate |
| --- | --- | --- |
| P0 — this session | Current owner trial guide, current-code audit and this design | Docs/source/reference checks only; no new sound controls or usability approval |
| P1 — next prototype | Context picker/library metadata, per-line None/recording/bark, scoped draft save/Undo; isolated audition proof | Owner can find a Plaifah laugh without opening all voices; changing actor/locale never silently assigns an incompatible cue; clear/None is obvious |
| P2 — one-scene audio slice | Existing opening with sparse owned/test voice, one BGM change, ambience and an SFX; detached mix preview | Demonstrate silent lines, exact branch/join behavior, reveal-versus-advance, non-overlap, fades, cancel/dispose and unchanged source/save/preferences |
| P3 — scale and workflow | Representative 1,000-record metadata library, real representative audible clips, recording import/review and where-used | Test 1,000 records without 1,000 simultaneously decoded sounds; owner finds by character + intent, auditions alternatives, saves/reopens and identifies scope without filename hunting. Record time/hesitation and cold/warm responsiveness; no invented performance result |
| P4 — bounded adoption | Owner-accepted UX, chosen backend/storage/audio ownership, saved Prefab/config integration and one-scene migration manifest | Explicit owner approval, IDs/catalog/saves/settings checks and rollback; preserve artist edits. No automatic merge from sandbox |
| Later | Broader voice production/locales, reusable packs, backlog/auto, music segments/adaptive layers or middleware | Each feature needs a concrete use case and its own bounded proof; do not implement everything merely because the design allows it |

Focused acceptance cases: unvoiced line and assigned missing file are distinct; mixed voiced/unvoiced opening; wrong actor/locale excluded; text edit marks recording stale; Undo/Save/reopen and batch import collision preview; no restart on repaint/locale redraw/reveal; real revisit replays once; interrupted asynchronous load cannot play in the next line; skip burst suppression; shared join inherits or resets correctly; voice duck releases under overlapping/disposal cases; user mute remains effective; original assets/saves/preferences remain unchanged. Audible listening and owner discovery/editing are separate from compile/test counts.

## 10. Documentation evidence and limits

Primary sources checked 2026-10-27:

- [Unity 6000.3 Audio Mixer](https://docs.unity3d.com/6000.3/Documentation/Manual/AudioMixer.html): grouping/routing, snapshots and ducking are documented engine facilities. This does not prove our mixer/picker exists.
- [Unity 6000.3 Audio Clip import settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioClip.html): loading/compression modes involve memory/CPU/latency tradeoffs. Per-kind settings above are proposed starting points requiring profiling.
- [Unity AudioSource.PlayScheduled](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource.PlayScheduled.html): DSP-timed playback supports scheduled clip sequencing. Our crossfade/loop scheduling is not implemented or audibly verified.
- [Unity Editor.OnPreviewGUI](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Editor.OnPreviewGUI.html) / [OnPreviewSettings](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Editor.OnPreviewSettings.html): public preview extension points, not a guarantee of an isolated audio audition implementation.
- [Naninovel v1.21 Voicing](https://naninovel.com/guide/voicing.html): line-ID-based voice mapping, a mapping utility and independent voice-language workflow provide useful reference patterns. This project has not installed/adopted Naninovel; v1.22-preview search results are not the selected baseline.
- [Ren'Py Voice](https://www.renpy.org/doc/html/voice.html): optional per-line/ID-associated playback and voice replay/character controls illustrate the separation of voice from dialogue text. Our explicit None/bark picker is a project proposal, not a claim that all engines have this UX.

No sound assets were added/listened to, Editor audition API implementation tested, runtime changed, vendor purchased or new Unity tests run in this document-only session. The owner has acknowledged the prior sandbox and intends to test; that is not UX acceptance or an instruction to merge it.
