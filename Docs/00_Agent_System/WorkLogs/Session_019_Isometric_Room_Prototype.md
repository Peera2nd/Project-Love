# Session 019 — Playable Isometric Room Prototype

- **Date:** 2026-10-24, Asia/Bangkok.
- **Authorization:** owner supplied assets, clarified 1024-room textures over a code-generated black backdrop, and requested implementation.
- **Backup:** durable `project-love-session-019-before.zip` in the permitted visualization directory, 344 baseline files, verified before mutation.

## Delivered

- New `Scenes/Gameplay/IsometricPrototype.unity`, `RoomActionButton.prefab` and 11 supporting SO assets. Uses supplied workspace variants, convenience store, two room poses, two VN expressions, clerk and bottle with preserved image pixels/GUIDs.
- New ItemDataSO, RoomDataSO/phase-hotspot placements, RoomPresentationSO, IsometricRoomView and Editor builder. Total 23 runtime/Editor C# files and four test files. Dashboard opens the new prototype and exposes the data types.
- Black backdrop plus square/aspect-preserved 1024 room art on a 1920x1080 UI reference canvas. Four gameplay phases; fifth LateNight artwork is an ended-day visual. No image editing or walking controller.
- Contextual person/work/rest/pickup/clerk interactions, dimmed VN overlay with background input blocking, TMP typewriter/reveal/choices, HUD and travel/inventory/options panels.
- Fixed ERR-020: daily dialogue completion tracked separately from permanent story completion; next-day access returns. Work/rest authored at one phase, relationship conversation at one phase per completed interaction, no time on deferral.
- Stamina is config opt-in; legacy AP campaigns retain their mode. Trial cap/start 100, work cost 20, rest +30, overnight +40, conversation energy zero. Core validates resources/effects/duration before committing. Night completion ends the current day and waits for explicit next-day input.
- Minimal inventory: once-only world pickup, one-unit purchases gated by current shop/money/stack cap, recovery consumption gated below full Stamina. Trial navigation/pickup/use/purchase time zero, explicitly visible in UI/guide and editable through data where implemented.
- Extended serializable state/save v2 includes Stamina, room, inventory, collected pickup IDs and daily gates. Version-1 AP saves are accepted only for legacy AP configs, not reinterpreted as Stamina. Manual load preserves the newer auto checkpoint.
- Existing FoundationSandbox daily conversation now spans all 30 configured days and refreshes daily; legacy work/rest fixtures also advance one authored phase. Original Sandbox scene files and build settings preserved.

## Verification

- Actual Unity `6000.3.24f1` compile/generation and testing in the existing short `.verify` project. Did not close or control the user's live Editor.
- **41 Edit Mode passed, 0 failed/skipped.** Resource/time atomicity, zero-energy recovery, last-phase/day-30 bounds, daily/story separation, deferral rollback, shop/pickup/item invariants, modal API gating, persistence and legacy compatibility, plus earlier regressions.
- **6 Play Mode passed, 0 failed/skipped.** Existing Sandbox/three-day regressions and new real Input System mouse-driven room flow, modal click blocking, expression/choice/typewriter presentation, phase pose/location/background switching, pickup removal, purchase/use, save/auto restoration and next-day daily-talk refresh. UI rendering/glyph/bounds and black pixels verified.
- Rendered/inspected morning, afternoon, conversation, choice and shop screens. Pose anchors tuned in data. Supplied Phone art is upright, so rest-area placement is a mockup rather than a final seated performance; no foreground occluders supplied/implemented.
- Delivered scripts/tests and 13 generated scene/data/prefab assets match verified copy. Twelve imported texture metas preserve their prior GUIDs; original PNG bytes unchanged. Folder/meta/GUID and baseline checks recorded at closeout.
- Reports: [Edit Mode](Session_019_Verification/EditModeResults.xml), [Play Mode](Session_019_Verification/PlayModeResults.xml).
- Closeout audit: 344 baseline files retained; supplied PNG bytes and all existing meta GUIDs preserved; old scenes/packages/settings unchanged. All 66 compared source/authored files match the tested copy, excluding the independently expanding dynamic Sarabun font-feature cache. All 165 owned meta pairs have unique GUIDs. Latest test logs contain no C# compiler warnings/errors. Final XML reports refreshed to 41/6.
- No standalone build, original live Console check, full campaign/story, real minigame or final numerical/typographic/placement approval.

## Incidents

- ERR-022: builder initially omitted UnityEngine.TextCore for OTL_FeatureTag; corrected namespace, compile succeeded.
- ERR-023: initial expanded click assertions misread RaycastAll membership as topmost hit, and synchronous restart/load sampling preceded the next rendered UI frame. Added top-hit/alpha-visible click selection and a frame boundary after forced state rebuild; all six Play Mode tests passed. No runtime click-through was inferred from the initial membership failure.
- ERR-024: pre-delivery review added a v2 resource-mode marker to reject same-ID AP/Stamina model changes, including zero-AP snapshots; new regression passed.
- ERR-025: broad delivery byte audit encountered the expected dynamic Sarabun font-feature cache; inspected the diff and retained the original font. Source/authored-fixture comparisons and meta/baseline checks passed.
- ERR-026: repaired two inherited Session 018 work-log relative links during the full documentation audit; all documentation links/fences passed.
- ERR-020's previously documented gameplay gaps are now fixed and covered by regressions. ERR-021's static full-room character assumption is replaced by data-authored local pose placement.

## Handoff

Open Tools > Project Love > Dashboard > เปิดต้นแบบห้อง Isometric, then Play. [Quick start and editable asset map](../../04_Technical_And_Tools/02_Isometric_Prototype.md).

Next: owner evaluates room framing/hotspots, transitions and the day loop. Tune exact art anchors/foreground occlusion/UI from feedback; then assess resource balance and select a concrete work mechanic. All numbers remain provisional. Full shops/gifts/keys/skills, inventory UI polish, save QoL and production content remain later work.
