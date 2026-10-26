# Sandbox and Three-Day Prototype — Quick Start

For the current supplied-art room/store build, use the [Isometric Prototype guide](02_Isometric_Prototype.md). These older fixtures retain AP. Session 019 makes work/rest consume one authored phase and the original Sandbox conversation repeat daily across all 30 days; legacy scripted story events remain once-only.

## Open and play

1. Open the Unity project in `Project Love/` and allow import/compilation.
2. Open **Tools > Project Love > Dashboard** and select **สร้าง / เปิดต้นแบบ 3 วัน**. Alternatively open `Assets/_Project/Scenes/Sandbox/ThreeDayPrototype.unity` directly. Press **Play**.
3. Choose work/rest success or failure, or an available conversation. Event buttons appear only when their day, phase and conditions match; change phase to discover later opportunities.
4. Continue first reveals the full line, then advances. At choices, Continue becomes **คุยต่อภายหลัง · คืน AP**. It works even with zero AP or no eligible answer.
5. Save/load outside active dialogue. End day and start next day both update the automatic slot. The prototype stops after Day 3; its campaign config remains 30 days.

The original `FoundationSandbox.unity` remains available through the original Dashboard button. Its AP deadlock is also fixed without rebuilding that scene. Save controls are provided in the new prototype scene.

## AP safety and event scheduling

- Deferral restores the complete event-start runtime snapshot: AP, stats, flags, read/choice history and completion state. Previously completed activities/events remain intact. A deferred conversation is not marked complete and can be retried while its window/conditions allow.
- This avoids both an all-disabled-choice softlock and reward farming through partial replay. Deferral is available at every choice screen; a broken default next link also returns safely when Continue cannot advance.
- Available events sort by priority descending, preferred day ascending, then stable ID. Preferred day never closes an otherwise valid window. Events do not force-start or interrupt an active conversation.
- Completing a once-only story event removes it from availability, including after loading. Daily interactions are tracked separately and return on a new day.

## Suggested three-day trial

- **Day 1, Morning:** choose listening in the introductory conversation. Simulate one work failure, then save manually. Try another activity and load to compare the restored AP/progress.
- **Day 2:** deliberately skip the Daytime opportunities and advance to Day 3.
- **Day 3:** do successful work to recover progress, which now advances automatically to Daytime. The recovery conversation and listening follow-up remain available. Switch to Evening for the short wrap-up, then end the day.
- Alternative: spend every AP before starting a paid conversation, reach the choices and defer. End the day and return with refreshed AP. No completed-event reward is granted by deferral.

All text/characters are temporary fixtures, not approved story. Recovery thresholds and full-campaign balance remain provisional.

## Edit data

Stop Play Mode before editing source assets in the Dashboard or Inspector. Paths are relative to `Assets/_Project/`.

| Asset | Editable content |
| --- | --- |
| `Data/Settings/PrototypeCampaign.asset` | Full duration, AP/day, starting resources and roster |
| `Data/Characters/PrototypeCharacter.asset` | Temporary identity, initial affection/stress and sprites |
| `Data/Dialogues/prototype_*.asset` | Thai lines, stable IDs, choices, costs, effects, flags and links |
| `Data/Events/prototype_*.asset` | Window, preferred day, phase, priority, prerequisites and entry node |
| `Data/Settings/prototype_work.asset`, `prototype_rest.asset` | Simulated outcome costs/effects/flags |
| `Data/Settings/PrototypePresentation.asset` | Thai UI labels, formats and typewriter speed |
| `Scenes/Sandbox/ThreeDayPrototype.unity` | Scene-local `prototypeDays` limit; zero disables the prototype cap |

Original `Sandbox*`/`sandbox_*` assets remain separate. `ThreeDaySeed.json` and `SandboxSeed.json` create missing fixtures only; changing a seed does not overwrite existing authored assets. Reopening either scene preserves data and GUIDs. Restart clears runtime state and preserves disk saves.

Dashboard remains a basic asset editor/validator, not the full planned timeline/node editor. New nested line/choice IDs must be unique and stable.

## Persistence boundaries

- `SaveStore` stores one manual slot and one automatic slot per config ID under `Application.persistentDataPath/PrototypeSaves`. Filenames hash the ID; source SOs are not written by gameplay.
- A versioned JSON envelope contains the runtime payload and checksum; current v3 records resource mode and extended state, v2 migrates its aggregate without inventing per-stream history, and legacy v1 loads only into AP configs. Load checks version/config identity, checksum, configured day/AP bounds, roster/stat ranges and unique nonblank history/flag IDs before applying a deep copy.
- Active dialogue rejects saving/loading. Finish or defer first. Saves use a temporary file, flushed write and replacement; the previous destination is retained as `.bak`. Missing/corrupt/incompatible files leave the current session unchanged; backups are not silently auto-loaded.
- Schema is an initial prototype format. Same-ID content edits are not fully fingerprinted or migrated; global history, arbitrary mid-dialogue saves, multi-slot/quick/pre-branch saves and cross-version migration remain planned.

## Verification — Session 012

Unity `6000.3.24f1` compiled/generated the new scene in an isolated `.verify` copy; the original live Editor was not controlled. Delivered C# and 14 new generated assets match the verified copy.

- **27 Edit Mode cases:** prior foundation plus zero-AP and later-choice deferral rollback, conditional blocking, scheduling/catch-up/once-only completion, disk roundtrip, busy boundaries, replacement/slot isolation, corrupt/missing/incompatible saves, invalid snapshots and I/O rejection.
- **4 Play Mode cases:** original loop; old-scene zero-AP exit through an actual Input System mouse click; three consecutive days with skipped preferred events, failure/recovery, manual/automatic loading, no duplicate event rewards and unchanged definitions; Thai glyphs/layout.
- Previewed initial, blocked and summary screens at 1920x1080. Long dynamic labels fit using 18–22 point autosizing without wrapping. Future content/resolutions still need their own typography checks.
- Reports: [Edit Mode](../00_Agent_System/WorkLogs/Session_012_Verification/EditModeResults.xml), [Play Mode](../00_Agent_System/WorkLogs/Session_012_Verification/PlayModeResults.xml).

No standalone build, final narrative/art, real minigame, full 30-day balancing or production save migration is claimed. The build-scene list and original Sandbox scene were preserved.

## Resources and remaining scope

- Sarabun Regular: [official Google Fonts source](https://github.com/google/fonts/tree/main/ofl/sarabun); OFL/provenance under `_Project/Resources/Fonts/Sarabun/`.
- Required TMP shaders/includes: installed uGUI 2.0.0 Essential Resources, original GUIDs/license under `_Project/Art/UI/TextMeshPro/`.
- Shops/skills, real minigames, final story/art, advanced VN QoL, full tools, flowchart/galleries and production save features remain later work.
