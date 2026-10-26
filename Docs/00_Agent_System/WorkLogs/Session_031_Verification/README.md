# Session 031 verification

Unity 6 `6000.3.24f1`, isolated `.verify`. **124/124 Edit Mode + 21/21 Play Mode passes**. Six new asset/dependency cases and three new mouse-driven UI cases supplement the prior suites. No standalone build or original live Editor Console check is claimed.

## Coverage

- Owner-supplied 36 icon bindings, three avatars, four panels/frames and a four-cell canonical bar sheet. All cells are 256x64 rects in one 1024x64 texture, with 16px borders. Panels use 24px borders. One-time binding reruns preserve authoring and sprite local IDs.
- Design brief -> Art -> Code dependencies, independent alternatives and Producer decision gates. Ready-for-review is not accepted. Free review/priority preserve time/resources; repeated review is a no-op. Definitions remain unchanged during play.
- Actual mouse paths follow Art -> Design predecessor -> Art successor -> Code successor, then select/prioritize independent Code work. The inspector keeps the board modal reserved and scrolls the focused card into view.
- HUD exact values/icons, worker affection/Stress/progress, capped hatch forecast (initial help 6 + team 4 = 10/28), default shared UI material, new owner portraits and actual quota/task bars.
- Real review unlocks Art; rest state is visible in the room without reopening conversation. Existing three-person Night OT, gifts, inventory, event bubbles, rollback, saves, ready review and early S2 credit remain covered.
- Thai glyph/overflow checks plus 1920x1080 and 1280x720 captures. Smaller captures verify the same normalized layout at 16:9; they are not a user usability study.

Six engine policies complete all base work (100), all five Producer stages and 120 phases in 30 days with the new S1 chain: four no-story bug/no-bug x daily-talk/no-talk schedules and two schedules with the real event catalog. Bug/event schedules miss S1 and meet later quota reports; no-bug baseline schedules meet all five. [Summary](schedule_summary.json). This is evidence for these recovery policies, not final difficulty, volunteer OT, all-Night economy or route balance.

## Evidence

- [Edit Mode XML](Session031_EditMode_Final.xml), [Play Mode XML](Session031_PlayMode_R4.xml)
- [Room](Session031_Room.png), [room 1280](Session031_Room_1280.png), [worker and forecast](Session031_Worker.png)
- [Board](Session031_Board.png), [board 1280](Session031_Board_1280.png), [Art predecessor](Session031_ArtDependency.png), [Code predecessor/alternatives](Session031_CodeDependency.png), [rest badge](Session031_RestBadge.png), [event bubbles](Session031_EventBubbles.png)
- [Supplied art contact sheet](Session031_AssetContact.png), [asset intake](asset_intake.json)
- [Delivery manifest](delivery_manifest.json): 473 source/data/UI files match the tested clone; 44 runtime/Editor sources, 13 test files, 98 Studio SOs. All delivered GUID targets resolve.

48 PNG pixel files and all their existing GUIDs remain unchanged. Only importer metadata is normalized for the supplied UI; the sheet has stable named sub-sprites. 356 protected baseline files are byte-identical, including source font cache, non-UI art, legacy data/scenes and package/project settings. 61 asset/import files changed. Pre-edit archive `.backups/session-031-before.zip` passed integrity (869 files).

ERR-051: a wildcard package search failed and was replaced with resolved literal package paths. ERR-052: the first Edit Mode run passed 122/124; fixture assumptions were corrected without weakening prerequisite/save validation. ERR-053: skinned layout runs exposed long labels, title width and a short Thai effort row; fixed icon lanes/action heights/full-width titles/row space, then all 21 actual mouse/render tests passed. No bounds/glyph/raycast assertion was removed.
