# Session 027 — Management Revision and UI Mockup Request

2026-10-24. Owner requests worker rest moved off Kanban, zero-time paid Night OT with contextual talk, paid Producer work/own-stage unlocks and a complete UI mock asset request. Asynchronous clarifications confirm free current-phase rest orders, mandatory Producer gates without calendar fallback, and early S2 work credited to S2. No repeated permission request or external messaging/art generation occurred.

## Implementation

StaffAndProducerOrders extends the shared production transaction/resolver. Rest reserves a current-phase order without cost/time; at the boundary the worker loses up to five Stress and skips output. OT is Night-only: validate current task/availability/money, charge five money/add up to ten Stress once, retain the menu/time, then produce stress-adjusted work once at Night close. Everyone else rests. Current-phase orders persist, expire once and remain part of whole-dialogue snapshots. Normal talk policy is unchanged; twelve Thai OT nodes select two affection/two Stress bands for each of the three characters.

Producer work costs one phase/20 energy, grants up to ten money/ten own effort and capped +1 to one eligible current card per discipline. Resting/absent/blocked/full workers receive zero; neither bonuses nor normal work spill to a second card. A stage's goal opens its successor before that action's team calculation; the calendar never opens tasks alone. Goals are 40 for S1–S4 and 20 for the final stage. Task-owned sprint references govern early/late credit; immutable deadline reports validate only acceptance through their cutoff. Minigame-grade rewards remain future work.

Studio save v5 uses separate versioned file keys, preserving config IDs and old v4 files; old task/Producer history is not guessed. Orders/stage completion and existing ledgers/cursors are validated. Legacy AP/room v1–v3 behavior stays separate and passing. StudioManagementSeed.json and a version-stamped Editor update seed settings/bindings/tints/dialogue/icon slots once, preserving later author edits.

Kanban now uses editable discipline tints, owner portrait fallbacks and simple task bars, with a Producer bar in its action panel. No worker-break command remains there. Four character actions fit the expanded Studio viewport. Full HUD/status/navigation sprite replacement awaits the requested art; do not claim the requested avatars/icons/sheet were delivered.

## UI handoff

[Asset brief](../../03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md): 24 monochrome 256×256 core icons, three 256×256 head portraits, one 1024×64 four-cell grayscale progress sheet, four neutral panel/frame exports. Includes additional optional icons, exact filenames, 9-slice/safe-area/tint guidance and acceptance criteria. Runtime folders `_Project/Art/UI/Mockup/{Icons,Portraits,Bars,Panels}/` and optional editable `Art_Source/Mockup/UI/` are prepared. Existing PNGs remain unchanged.

## Evidence and preservation

96 Edit Mode / 15 Play Mode passes in isolated `.verify`, including existing regressions, real mouse flows and rendered Thai glyph/bounds. Four full 30-day schedules finish main progress 100%, five Producer stages and 120 phases; daily-social variants reach affection 60 each. Bug schedules miss S1 but recover overall work by Day 30. Full all-OT economy, final difficulty, minigames and production narrative/route balance are not validated.

37 runtime/Editor sources, nine tests, 87 Studio SOs. Twelve new OT nodes and 54 existing SO changes; no scene/PNG import change. Delivery manifest matches 324 tested source/data files and confirms 323 protected baseline files unchanged, with existing GUIDs preserved. Pre-edit archive `.backups/session-027-before.zip` read back successfully (686 entries). No standalone player build or original live Editor Console claim; the user's open scene was not operated/saved.

ERR-041 records an initial rejected patch due to inline attribute context; no code applied. ERR-042 records visually observed fourth-action viewport clipping despite passing mouse/label tests; the Studio viewport was expanded and final tests/previews passed. All seven final PNGs were visually inspected. See [verification](Session_027_Verification/README.md).

## Next

Receive UI artwork, bind shared sprites/tint and heads, reduce remaining text-heavy status/navigation and add clear tooltips/cost/state badges. Owner playtest rest/OT/Producer tradeoffs with recovery, then author first real sprint/event/dialogue content and optional minigame results. No illness trigger is implied.

Closeout: 103 local references across 12 changed/new Markdown documents resolve after URL decoding. All 324 delivered source/data hashes match the tested clone; UI export folders are present.
