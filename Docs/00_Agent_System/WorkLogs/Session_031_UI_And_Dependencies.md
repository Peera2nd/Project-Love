# Session 031 — Supplied UI and Dependencies

2026-10-25. Owner announces updated UI and authorizes continuation. The previous readiness hold is superseded. Scope: integrate supplied UI and proceed with readable dependency gameplay. Volunteer/shared Night work remains the next milestone. No new images, external messages, web research or subagents were used.

## Implementation

Inspected all 48 exports on a dark contact sheet and checked image dimensions/alpha. Owner replacements include colored icons and revised portraits; their pixel bytes and GUIDs are preserved. StudioUiUpdate normalizes Unity importers, binds 36 icon keys/three owner heads/four panels/frames and slices the canonical 1024x64 sheet into four 256x64 sprites with 16px borders using the installed Sprite Editor data-provider API. Existing cell IDs survive reruns. Optional separate bar PNGs remain original exports. The Editor asmdef now references the already-installed Unity.2D.Sprite.Editor assembly; no package added.

RoomPresentationSO stores skin sprites, editable stat tints and a one-time UI version. ProductionConfigSO separately stamps one-time dependency content. Seed strings/links are data; subsequent author edits are preserved on reopen. Runtime UI uses the standard shared UI material and vertex tint, full-width sliced sprites and clipped fill/forecast ranges, rather than generating role/progress PNG variants or material copies.

HUD now exposes phase/day, money, exact Stamina and main progress with icons/bars. Room roster adds heads, current effort/readiness, Stress and rest/OT indicators; full task/block explanations are available on hover. VN retains exact existing status/help/team/cost text and adds affection/Stress/task meters with capped hatched post-help forecast. Action/navigation/save/load/item/gift controls receive supplied icons; the supplied back arrow replaces the vector only in the skin, preserving fallback/legacy scenes. Character bodies still open ordinary menus; event bubbles and modal input blocking remain independent.

Kanban uses supplied rounded cards/heads, discipline stripes/icons, exact effort, state and dependency summaries, plus shared sprint/Producer/task bars. A card inspector lists predecessor work, successors and same-owner ready alternatives. Links rebuild the board under the same modal reservation and scroll the selected card into view. Prioritizing an available alternative remains an explicit free player choice; the simulation does not secretly skip a blocked high-priority task.

S1 content: Ploy character/gameplay brief -> Plaifah character art -> Grace integration/dialogue connection; Grace control tests also wait for accepted controls. Scene art/base Code/other Design cards remain parallel alternatives. Grace integration retains its separate explicit Producer decision. No efforts/quotas/30-day duration/social costs/OT economics changed. Other sprint chains are not newly authored. IsReady/ReadyTasks are read-only projections of the existing release/decision/accepted-prerequisite rules, not a second work resolver.

## Verification

[Evidence](Session_031_Verification/README.md): 124/124 Edit Mode + 21/21 Play Mode passes in isolated Unity 6 6000.3.24f1. Six new asset/dependency cases and three real-mouse UI cases cover bindings/shared texture/IDs, review-vs-acceptance, Producer gates, free alternative prioritization, exact HUD/forecast state, link navigation and visible rest badges. Existing events, save/gift/inventory/AP/room/three-person paid OT/early S2/frozen-review regressions remain covered. A prepared ready-task state is used for the mouse acceptance fixture; it is not a claim that an entire sprint was played manually.

Glyph/overflow/raycast checks remain enabled. Captured 1920x1080 and 1280x720 room/board layouts plus worker, Art/Code dependency, rest badge and event-bubble views. Visual inspection supplements bounds tests. Smaller 16:9 renders are layout evidence, not a usability study or final art approval.

All six engine schedules finish base scope 100/all five Producer stages/120 phases with the new S1 chain. No-bug no-story schedules meet all five quotas; bug and event-enabled cases miss S1 and meet later reports, preserving recovery. These schedules do not validate final route balance, volunteer OT or arbitrary Night/economy policies. No standalone player build or original live Editor Console claim.

## Delivery and issues

44 runtime/Editor sources, 13 tests, 98 Studio SOs. 473 source/data/UI files hash-match the tested clone; all delivered Studio GUID targets resolve. 61 asset/import files changed, all 48 PNG pixel files and their existing GUIDs preserved. 356 protected baseline paths are byte-identical, including non-UI art/source fonts/legacy data/scenes/packages/settings. Verified archive `.backups/session-031-before.zip` contains 869 files. No source image processing or external generation occurred; the contact sheet is QA evidence only.

Schema remains event-enabled v6/baseline v5. The changed task catalog invalidates incompatible older Studio snapshots through existing fingerprint validation; start a fresh run. No invented migration or changes to legacy slot namespaces.

ERR-051 records a wildcard package search failure, corrected with literal discovered paths. ERR-052 records an invalid legacy late-credit fixture after adding prerequisites and an incorrect no-op review expectation (122/124 initially); fixtures now honor the real domain rules. ERR-053 records layout failures discovered by actual checks (12/21, then 14/21 while additional fields were exposed): fixed icon lanes/action sizing, compact roster/full hover detail, full-width card titles and Thai effort-row height; final 21/21 passes. Assertions were retained and failing states captured before layout checks.

## Next

Owner trials the new room/VN/board on a fresh run, follows Art -> Design -> Art -> Code links and uses independent work while waiting for acceptance. Next implementation: volunteer OT, genuine shared player Night work and relationship scenes tied to task pressure. Current paid OT orders remain; ordinary Help still runs only in productive phases. Broader dependency content, final narrative/contextual review, minigames/grades, balance and foreground polish remain pending.

Closeout: all 473 delivered source/data/UI hashes match the tested clone; all 48 supplied PNG hashes match intake; 121 local references across 12 changed/new Markdown documents resolve.
