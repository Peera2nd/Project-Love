# Session 022 — Studio Work Direction and Character Interactions

- **Date:** 2026-10-24, Asia/Bangkok.
- **Request:** expand the studio-specific work loop (overall plus weekly discipline progress, Kanban/review and workload events); change character interaction to immediate VN presentation, screen-tap reading, back arrow, Work/Talk/Gift.
- **Confirmed:** first ordinary talk per character/day costs a little energy and grants affection; repeats are free/no reward and use no phase. Team performs baseline work independently, with player help. Specific quotas/event timing remain ideas.
- **Backup:** verified 403-file `project-love-session-022-before.zip` in the permitted visualization directory before code/assets/design edits. Registry intake/path correction preceded the archive.

## Delivered

- Immediate character portrait/greeting and Work/Talk/Gift actions. Screen taps reveal then advance; no separate Continue in the room. Bottom-right back-arrow icon returns through gift/menu/room or restores an unfinished interaction. Legacy scene buttons remain unchanged.
- CharacterInteractions separates access, deterministic variant selection and reward policy. CharacterDataSO owns day/phase/affection/condition variants, authored talk cost/reward, work link and gift preferences. Condition supports completed/not-completed story events in addition to existing flags/stats.
- First completed talk per character/day: trial Stamina 3 / affection 2 / zero phases. Repeat choices apply no extra cost/effect/flag rewards. Deferral restores the start snapshot. The per-character daily gate persists in runtime/save state, independently of scripted event completion.
- Giftable inventory with liked/neutral/disliked character-specific reactions; completion consumes exactly one item and applies affection, cancellation preserves the whole start state. No energy/item-consumption double effect. Trial gifts use no phases/energy and no daily cap; final gift balancing remains open.
- Fixture: neutral water +1, liked pudding +5, explicitly labeled non-canonical disliked test gift -2. No new item images generated. Existing art/profile retained; temporary dialogue is not final narrative approval.
- Generic WorkStreamSO/output definitions, separate mutable completed units, atomic activity attribution. Plaifah helps Art; desk helps Producer. Code/Design counters are present without extra worker art. Existing aggregate success effects remain a prototype bridge, not the proposed task-derived overall formula.
- Save v3 preserves talk/work state. Valid same-config/mode v2 migration retains aggregate/history/items/resources without inventing earlier discipline attribution; new stream counters/gates initialize empty. v1 remains AP-only. New-format snapshots validate stream catalogs/daily ranges; menu/dialogue saves are Busy. Existing checksum/atomic disk/backup flow retained.
- Artist-editable SOs and idempotent upgrade/seed; reopening an authored prototype preserves its tuning. New sources total 28 runtime/Editor C# files and five test files. Six existing fixture assets updated, 16 new SOs created; all original GUIDs preserved.

## Design recorded, not implemented

[Studio Work and Sprints](../../01_Game_Design/03_Studio_Work_And_Sprints.md) distinguishes owner direction from proposals: overall base-scope progress plus independent weekly minimums, small Kanban cards, review/carryover and event-driven workload. Owner confirms team baseline output with player help. Quotas, review dates, output-tick timing/order, dependencies and Producer leverage remain open. No Kanban, weekly evaluator or passive-production tick was added.

Late-campaign Grace absence is preserved as a [Thai event idea](../../02_Narrative_And_World/01_Studio_Event_Ideas.md). No illness trigger, medical detail, schedule, auto-difficulty or final story was implemented/approved. Recommend foreshadowing, viable recovery/capacity alternatives and opportunities that are not restricted solely to high progress.

## Verification

- Unity `6000.3.24f1` compile/asset upgrade in isolated `.verify`; original live Editor/scene/build settings left untouched.
- **58 Edit Mode passes**, 0 failed/skipped: earlier foundation/room regressions plus daily policy per person, zero-energy/repeat behavior, no choice farming, full rollback, gift reactions/inventory, conditional variants, work-stream targeting/atomicity, save v3 and v1/v2/mode validation.
- **7 Play Mode passes**, 0 failed/skipped: legacy regressions plus real-mouse immediate menus, screen taps, back navigation, modal blocking, gifts/cancellation, work streams, daily/save/next-day flow, shop/inventory and day end. Rendered arrow mesh/pixels, Thai glyphs/text bounds and black backdrop checked. Preview set covers room/menu/talk/gifts/context.
- Reports: [Edit Mode](Session_022_Verification/EditModeResults.xml), [Play Mode](Session_022_Verification/PlayModeResults.xml).
- Closeout: all 403 baseline files retained, original PNG bytes/existing GUIDs/scenes/packages/settings preserved. All 88 compared source/authored files match the verified copy, excluding the dynamic font cache. All 189 owned meta pairs/GUIDs and links/fences in 32 documents checked; final test logs contain no C# compiler warnings/errors. Reports refreshed to the final tap-verification run. Updated the previous snapshot's board link after the priority heading changed.
- No original live Console check, standalone build, full content/30-day balancing or authoring approval claimed. Dynamic Sarabun feature-cache churn is excluded from source/fixture identity checks (ERR-025).

## Incidents

- ERR-029: guessed class-based source paths and an unexpanded package glob failed; corrected using actual inventory/package paths.
- ERR-030: the old modal-blocking test clicked a newly overlapping Gift action, then expected Talk. The world remained blocked; changed the test coordinate to clear speech-panel background. No click-through defect inferred.
- ERR-031: visual inspection caught an invisible custom arrow caused by missing CanvasRenderer. Added required/explicit renderer and mesh plus rendered-pixel assertions. Diagnostic GetMesh output-argument compile failure was corrected to the installed no-argument API. Final integration passed.

## Next

Implement one sprint/task board and an explicit once-only passive-production boundary, then review/carryover and one visible workload event. Use current assets and temporary tasks; evaluate work/social/missed-day/bug/absence schedules before committing quotas across the 30-day campaign. Current ordinary-talk rule supersedes Session 015; scripted story durations stay independent.
