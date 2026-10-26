# 12 — Artist Workspace and UI Authoring

Session 044, 2026-10-25. Owner authorizes practical, visible editing tools and makes this the future development standard. This implements the current-campaign portion of the [Session 043 audit](11_Authoring_Tools_Audit_And_Upgrade_Plan.md). Gameplay rules, narrative text, existing art and save schema remain unchanged. Technical verification is recorded in [Session 044 evidence](../00_Agent_System/WorkLogs/Session_044_Verification/README.md); owner usability acceptance is still pending.

## Start here

Stop Play. Open **Tools > Project Love > Workspace**; Dashboard opens the same window. It defaults to `RomanceCampaign`, with five Thai tabs: writing/events, characters, scenes/UI, settings and **ภาษาและคำแปล** (Session 064). Historical prototypes are under the collapsed advanced section. Current campaign counts appear in the window; the old Session 044 counts of 55 definitions/38 nodes are historical and do not include the later opening content.

**เปิดเกม** opens the existing main scene, preserving authored assets. **ดูตัวอย่างก่อนเล่น** opens a detached Edit Mode preview. **บันทึกข้อมูล** saves dirty data assets; save Prefab/Scene edits normally in their own editor. Standard Undo/Redo applies to serialized data and room placement. Stop Play before authoring; the workspace disables data editing during Play.

## Find and edit a line

1. In **เขียนบท / อีเว้น**, type any Thai phrase from the game into **ค้นประโยค / ชื่อฉาก**. It searches every line and choice, not only filenames. Filter by character and purpose when useful.
2. Select a result. The right side shows its source path, readable scene name, ordered speaker/expression/text rows, choices and next-node links. Change the sentence directly. Add/reorder/delete through the row controls; existing line and scene IDs stay stable.
3. Use **บันทึกบทนี้**, then **ดูตัวอย่างบทนี้**. The preview uses the same real view and node; **อ่านต่อ** and choice buttons advance its isolated runner. Rebuild the preview after editing.
4. While playing, **บทที่กำลังแสดง** selects the actual runner node and marks the active line. If the screen is a greeting, it selects that greeting source instead. Stop Play to edit, then restart/re-enter the scene to test.

The descriptive scene name is independent of its stable ID. Links can be opened directly. New branching nodes can be created with the normal Dialogue Node asset menu and assigned in the next-node field; this milestone is not a visual graph editor. IDs and advanced conditions remain an advanced section.

## Multi-speaker opening and continuity limits — Session 066

For the current opening, use **เขียนบท / อีเว้น**, set the character filter to **ทุกคน**, search **วันแรกที่สตูดิโอ**, and choose **วันแรก · สารภาพรัก หรือสมัครงาน**. All eight pre-choice lines are in that one node, ordered narrator → Grace → Ploy → Plaifah → Grace → Ploy → Grace → Plaifah. Each card has a speaker selector, expression and Thai source editor; use the existing add/delete/arrow controls and Save. There is no need to open three separate character editors to edit this exchange.

After these lines, three choices link to separate response nodes. Use the **เปิด** button beside a choice's **ไปฉาก**, or the ordinary **บทต่อไป** link, to open continuation nodes. **ดูตัวอย่างบทนี้** reads a selected path forward with **อ่านต่อ** and choices. The editor currently shows large cards for one node at a time; it does **not** provide a compact continuous script of the entire event or a branch overview. Localization tables/CSV are for text exchange, not play-order reading. Designer usability/owner acceptance is not established by the existing technical checks. [Audit](../00_Agent_System/WorkLogs/Session_066_Opening_Authoring_Continuity_Audit.md).

## Greeting/talk pools and affection bands — Session 046

Open **ตัวละคร**, select the person, then **ทักทาย** or **คุยเล่น** under **ชุดบทและเงื่อนไขความสัมพันธ์**. From a greeting/talk in the writing tab, **จัดการพูล / เงื่อนไขความสัมพันธ์ของบทนี้** jumps here. This exposes existing variant availability and the new optional alternative pool without editing C# or raw character arrays.

- One **ชุด** owns an inclusive affection range, priority, day window, phase and additional conditions. Example only: low band 0–29, closer band 30–100. Set both bounds explicitly; these numbers are not written into current content as balance decisions.
- **+ เพิ่มบทอีกแบบในพูล** adds a separate persisted DialogueNodeSO. The main `node` plus optional `pool` nodes are the alternatives. Enter each one's first sentence directly, or **เปิดบทเต็ม** for expression/speaker/additional lines/choices. Greeting UI displays only the first line; talk can advance through the full scene. Multiple lines in one node are sequential, not pool alternatives.
- **+ เพิ่มชุดตามระดับความสัมพันธ์** creates a blank group and node. New groups initially span the campaign/all affection with priority above existing groups: set their intended bounds before writing. Blank alternatives are visibly marked and excluded from runtime selection. Full project validation reports unfinished content; normal data saving does not approve story.
- After conditions pass, highest priority wins; equal priorities retain stable-ID ordering. Its playable unique alternatives rotate by a stable character/group offset plus day/phase. Re-reading, refreshing, reopening or loading the same context selects the same node; no Unity RNG or save counters are changed. It is context rotation, not a fresh random draw per click.
- **ลองเช็คว่าระบบจะเลือกบทไหน** accepts affection/day/phase, with optional memory/completed-event IDs. **เช็คเงื่อนไขและบทที่เลือก** uses the actual matcher/selector in detached state, reports passed/blocked groups and offers **เปิดบทที่ระบบเลือก**. It checks content selection, not room visibility/forced-event access. The existing unlocked weekly-focus topic still takes precedence over ordinary talk; the checker reports that override when its flag is supplied.

Save with **บันทึกชุดบทของตัวละครนี้**. Serialized edits and registration support Undo; removing a group/alternative only removes its reference and keeps the source asset. Undoing creation can leave an unreferenced writing asset intentionally, preserving authored text. Search follows all pool references. Current nodes, dialogue text, reward values and v8 schema remain unchanged by installing these tools; newly added pool content participates in the existing romance content fingerprint.

## Draft and test an event

**สร้างอีเว้นฉบับร่าง** creates persistent blank node/event assets in the current campaign's `Drafts` folder. A draft appears separately and cannot run in the campaign until **เพิ่มอีเว้นนี้เข้าเกม** succeeds. Fill its dialogue first. Set character, availability dates, phase, duration, room and delivery (context/automatic/bubble); priority/conditions are advanced. Publishing validates the candidate catalog before changing the campaign reference and supports Undo. Blank text is intentionally invalid, not generated story.

**ทดสอบอีเว้นนี้** uses the real eligibility/arbitration rules in a fresh isolated state. Change day, phase, affection and selected-character mood without losing the selected event. A blocked event reports date/phase/room, failed prerequisites or a pending automatic event where applicable. No prerequisites are silently marked complete for event tests. The preview starts in the campaign's starting room; events requiring other rooms/history/flags may be blocked and need a real playthrough or a dedicated fixture. This is not an arbitrary save/history editor.

Ordinary screen/node previews intentionally bypass unrelated forced scenes so layout/text can be inspected directly. They are layout/content previews, not proof that a scene is reachable. Neither preview mode reads/writes actual saves, advances the live session or edits definitions. Choices affect only the detached state.

## Edit real UI, without Play

Open **ฉากและ UI > เปิด UI หลักเพื่อจัดหน้า**. `RomanceWorkspace.prefab` contains the actual room image, characters, HUD, persistent roster, dialogue, menus, support cards and recap. The main scene holds a linked instance. Use Unity's Rect Tool, hierarchy and Inspector, then Save. To share a scene-instance change, apply its Prefab override; otherwise it belongs only to that scene instance. Preview loads the saved main Prefab, not an arbitrary scene override.

Screen buttons open smaller nested Prefabs, active when opened standalone:

| Prefab under `Assets/_Project/Prefabs/UI/` | Owns |
| --- | --- |
| `RomanceWorkspace.prefab` | Main composition, screen bounds, room frame, control references |
| `RomanceHud.prefab` | Day, money, project layout |
| `RomanceDialogue.prefab` | Speech box, portrait, choice viewport, Back |
| `RomanceWeeklySupport.prefab` | Weekly support cards and detail layout |
| `RomanceMorningRecap.prefab` | Growth recap layout |
| `RomanceActionRow.prefab` | Reusable action/choice label, icon, three effect slots, row height |
| `RomanceCharacterCard.prefab` | Reusable portrait, name, mood and heart bar |
| `RomanceHotspot.prefab` / `RomanceEventBubble.prefab` | Reusable room targets/bubbles |

Runtime changes content, values, fill fractions and visibility. It does not recreate fixed screens/roster or reset their authored anchors, offsets, fonts or colors on refresh. Lists reuse rows from the authored template; their Vertical Layout Group controls spacing and Layout Element controls height. Change those properties rather than fighting a layout-driven child RectTransform. TMP auto-sizing computes its displayed size: change its min/max, or disable auto-sizing before assigning a fixed font size. Edit actual fonts/colors/sizes on the Prefab Text/Image components; the presentation's initial-generation font is not a live global theme override.

Dynamic text (names, dialogue, activity labels and numbers) is supplied by data. Editing its sample text in a Prefab changes only the sample. Use the writing/settings tabs for content. Existing artist edits are authoritative: ordinary builder/open actions never regenerate an already authored Prefab or replace a migrated scene.

## Place room characters and objects

In **ฉากและ UI**, choose room and phase. The visual room panel shows the actual sprite/placements. Select a target, drag its body to move or its lower-right handle to resize; numeric fields remain available. Change visibility/pose sprite for that phase. Undo works; **บันทึกห้องนี้** saves the selected RoomDataSO. Other phases/rooms stay independent.

Room target positions belong to RoomDataSO, not the generated hotspot RectTransform. Runtime and preview reapply the selected phase's normalized data. Move/resize the overall room frame in the main Prefab; change a person's placement in the visual room tool. A square 1024 room remains aspect-preserved over black.

## Source ownership and persistence

| Edit | Persistent source | How to verify |
| --- | --- | --- |
| Dialogue/choice text | Current LocalizationCatalogSO for opted-in Romance; raw fields remain compatibility fallback | Save, reopen/search, preview/re-enter |
| Dialogue speaker/order/expression/links and internal scene name | Selected DialogueNodeSO | Save, reopen/search, preview/re-enter |
| Event window/delivery/priority | EventDataSO and campaign registration | Save; selected-event test or real eligibility |
| Cast names/roles | Current LocalizationCatalogSO; portraits/gift responses stay on CharacterDataSO | Save, preview corresponding character |
| Activity rewards/mood rules/milestone values | RomanceRules.asset; localized labels/day hints stay in LocalizationCatalogSO | Save, rebuild preview/new session |
| UI wording | Current LocalizationCatalogSO; icons/typing speed stay on RomancePresentation.asset | Save, rebuild preview/re-enter Play |
| Layout/fonts/colors | Actual UI Prefab or scene override | Save; refresh preview and Play |
| Room pose/position/visibility | Selected RoomDataSO and phase | Save; preview selected phase |

The window shows the selected source path. Current-campaign scope does not imply every asset is private: shared item/art references can affect historical fixtures if edited. Seed JSON is an initial bootstrap, never a live import that overwrites writing edits. Live session/save values are separate; initial values may require a new session. Play GameObject/layout edits normally revert, while source SO edits through other Inspector paths can persist. Use the guarded workspace outside Play for normal work.

## Feature acceptance from now on

Before adding content/system features, specify where the artist finds/edits/saves/previews them. Keep English backend documentation and Thai-facing tools/story/conversation. Require stable sources/IDs, Undo and explicit saving, safe isolated preview, actual rendered checks and retained artist layout. No routine C# edits or Play-only generated screens. One-time migration code may initialize missing assets but must never replace later artist changes. See AGENTS rule 8 and registry GR-012.

Suggested owner trial: search a sentence and save/reopen it; move the dialogue portrait in Prefab Mode and preview/Play; move one room pose for one phase and Undo it. Technical checks pass, but these hands-on authoring tasks still need owner feedback before declaring the tools comfortable.
