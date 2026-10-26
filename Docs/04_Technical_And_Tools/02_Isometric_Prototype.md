# Isometric Room Prototype — Quick Start

Updated Session 022, 2026-10-24. Supplied-art overview with character interaction, player work streams and save v3. Story, balance and production presentation remain draft. [Studio/sprint design and implemented boundaries](../01_Game_Design/03_Studio_Work_And_Sprints.md).

Legacy fixture guide: controls below describe IsometricPrototype (Session 022). **Session 026 adds a separate [Studio Sprint prototype](03_Studio_Sprint_Prototype.md)** with Grace/Ploy integration, autonomous team work, Kanban/quotas/reviews, Producer actions, complete VN status/forecast and save v4. Open the new Studio Dashboard button to test that loop; this original room scene keeps its prior rules.

## Open

1. Stop Play Mode and allow import.
2. Open **Tools > Project Love > Dashboard > เปิดต้นแบบห้อง Isometric**, or `Project Love/Assets/_Project/Scenes/Gameplay/IsometricPrototype.unity` directly.
3. Press Play. The delivered data is already upgraded. Reopening does not reset authored tuning. The builder preserves original scenes/build settings. For an older copy missing the new data, use **Tools > Project Love > Apply Character Interaction Update** outside Play Mode.

## Character actions

- Click Plaifah: the scene dims and her large portrait/greeting plus **Work / Talk / Gift** appear immediately. Opening is free. Background clicks/actions, saves and day transitions are blocked while this view is open.
- Tap the screen to reveal the whole line, then tap again to advance. There is no separate Continue button in this room scene. Choices require their own explicit click.
- Use the bottom-right arrow: gift selection -> character menu -> room. During unfinished talk/reaction it cancels that interaction and restores its entire start state, including reserved energy/history/items/rewards.
- **Work together:** one phase and trial Stamina -20; Art work +6 for Plaifah. The desk does Producer work +6 instead. Both currently also grant aggregate progress +6, money +5, SP +1 and character Stress +3, using authored success effects. Code/Design counters exist but those workers are not yet in this art fixture. There is no real minigame, Kanban/review or passive team-production tick yet.
- **Talk:** first completed talk **per character per day** reserves trial Stamina 3 and grants affection 2. It uses **zero phases**. Further talk that day is free with no affection/choice effects/flags; it remains available even at zero energy. First talk with insufficient energy is disabled; the arrow still exits to rest. Completion gates persist through save/load and refresh next day.
- Contextual variants support day range, phase, affection range, flags and completed story-event conditions. Highest priority wins, then stable ID. Supplied fixture has default/evening/close/post-event variants; `prototype_shared_task_done` is a test flag, not an implemented story scene. The large dialogue library and topic selection remain authoring/future work.
- **Gift:** selects giftable owned inventory. Reading the reaction to completion consumes one item and applies its character-specific affection change; backing out refunds the entire unfinished interaction. Gift does not also apply the item's consumption effect. Trial costs: no phase/energy, no daily gift cap. Neutral water +1, liked caramel pudding +5, clearly labeled disliked test gift -2. Pudding matches the profile's likes; the disliked fixture is not new character canon. New gift icons are placeholders/absent rather than fabricated art.

## Other actions and days

- Rest at the sofa: one phase, trial energy +30 and character Stress -10. Available at zero energy. Stamina cap/start 100; overnight recovery +40.
- Pick up the floor bottle once. Inventory use restores 15 energy, is unavailable at full energy and consumes one bottle. Collection/removal persists through travel/days/saves.
- Travel to the convenience store and click the clerk. Trial stock: water 5, pudding 8, test gift 2; stack caps 9. Return through travel. Travel/pickup/purchase/use currently cost no phase.
- Work/rest completed at Night ends the current day without auto-starting tomorrow. Select the next-day button. The player returns to the workspace. Full campaign remains 30 configurable days.
- Options provide manual save/load, automatic-slot load and restart. Automatic save occurs at day end/start. Manual load does not replace the newer automatic checkpoint. Restart preserves disk files.

## Edit without code

Paths below are under `Project Love/Assets/_Project/`. Stop Play before source edits; author in Dashboard/Inspector.

| Asset | Editable content |
| --- | --- |
| `Data/Settings/RoomCampaign.asset` | Duration 30, Stamina/recovery, starting resources, character/item/room/work-stream catalogs |
| `Data/Characters/RoomCharacter.asset` | Greeting/conversation variants and priority/conditions, first-daily cost/reward, work activity, gift preferences/responses |
| `Data/Settings/room_art_work.asset`, `room_work.asset`, `room_rest.asset` | Costs/duration, success/failure effects, success/failure work-stream output |
| `Data/WorkStreams/work_*.asset` | Stable discipline IDs and display names; completed units are runtime state |
| `Data/Dialogues/social_*.asset` | Temporary Thai greeting/talk/gift reactions; ambient choices branch without transaction effects |
| `Data/Items/room_*.asset` | Giftable, price/stack, optional consumption recovery/icon |
| `Data/Rooms/room_workspace.asset`, `room_store.asset` | Phase artwork/placements/visibility, hotspots, stock and entry duration |
| `Data/Settings/RoomPresentation.asset` | Thai UI copy, typewriter speed, dim opacity |

`CharacterInteractionsSeed.json` bootstraps the new fixture. The upgrade skips an already-authored character with conversations/work bound; editing a seed does not overwrite current SOs. Legacy `room_daily_talk`/`room_daily` assets are retained but the new menu uses the character's social variants and independent daily policy. Scripted story events retain their authored phase/cost/choice effects.

## Art and persistence

- Rooms remain 1024x1024 with alpha, aspect-preserved over black. UI reference is 1920x1080. Five workspace images map four action phases plus ended-day LateNight. Original PNG pixels/GUIDs remain unchanged.
- Room poses use per-phase local placement; supplied Phone pose is upright. Fine seating and foreground furniture occlusion remain polish work. No walking controller.
- Transparent sprite clicking uses readable, uncompressed FullRect sprite imports. Release memory/compression settings remain provisional.
- Save **v3** adds per-character `lastTalkDay` and per-stream work units, while preserving resource identity/room/inventory/pickups/history/legacy event gates. It uses the existing checksum, validation and atomic replacement/backup flow.
- Same-config/mode v2 saves migrate with existing aggregate progress preserved and **zero newly attributed stream work**; old saves cannot reveal how earlier progress was split. New daily social gates start unused. v1 remains AP-only. v3 requires a matching stream catalog and valid unique records; arbitrary catalog edits still need migration design.
- Save/load is blocked during character menus and unfinished dialogue. Full multi-slot/quick/mid-dialogue saves, global histories and broader migrations remain pending.

## Verification

Unity `6000.3.24f1`, isolated `.verify` project: **58 Edit Mode + 7 Play Mode passes**, 0 failed/skipped. Includes prior regressions, per-character daily/repeat policy, rollback/no farming, gifts/reactions/stock, conditional content, work attribution/atomicity, v1/v2/v3 persistence/mode checks, actual mouse interactions and modal blocking, arrow geometry/rendered pixels, Thai glyphs/text bounds and black-backdrop captures. [Edit report](../00_Agent_System/WorkLogs/Session_022_Verification/EditModeResults.xml), [Play report](../00_Agent_System/WorkLogs/Session_022_Verification/PlayModeResults.xml).

No original live Console check, standalone player build, full 30-day balance, autonomous production, sprint/board/review implementation or final story approval is claimed.
