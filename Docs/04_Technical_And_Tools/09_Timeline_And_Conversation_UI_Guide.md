# Timeline and conversation UI — Session 038

**Historical prototype:** Session 041 replaces this as the primary gameplay path. Use [Romance-first prototype](10_Romance_First_Prototype_Guide.md) for current play/authoring; retain this guide for regression/reference only.

Owner authorizes the pictured UI corrections, then Timeline implementation. Use the existing **Tools > Project Love > Open Planning Prototype** scene entry; stop/restart Play to import the delivered changes. No new images, scene copies or save namespace are needed.

## Conversation and rest

Planning room views omit the redundant Producer-board and player-rest hotspots. Their data remains available; supplied room art and character hotspots are preserved. Navigation now separates the cup **Rest one phase** action from moon **End Day**. Rest opens the current room's authored recovery activity; successful completion closes the panel before the phase notification. Failed actions do not silently discard the panel. This is separate from ordering a worker to rest.

Speech and choices occupy the left of the portrait; the right roster is unchanged. Planning action rows show a short action name plus icon/signed-number effects for energy/time/affection/Stress/Money. Benefits and costs use appropriate colors (lower Stress is beneficial; elapsed time is a cost). Hover does not repeat the same action text. Gift/story content remains Thai; no narrative dialogue was rewritten.

## Switch and edit

Open the board, then use its clock icon for **Timeline** or board icon for **Queue**. Both edit the same in-memory `PlanningDraft`; mode switches preserve edits and selection. Apply validates/commits the complete draft without elapsed time. Revert restores the committed plan. Back discards the draft; Escape/Back first cancels an active gesture. Save remains blocked during the board/modal.

| Gesture | Meaning |
| --- | --- |
| Drag an untouched planned bar body | Move start and due together by snapped phases |
| Drag its left white handle | Change planned start only |
| Drag the right white handle | Change internal due only, including unfinished work already started |
| Gesture outside the viewport/allowed calendar, Escape or focus loss | Restore the bar; no partial domain/resource/RNG change |
| Selected-task date/shift buttons | Non-drag editing fallback; Timeline due edits step one phase, Queue due edits retain one-day steps |

Work already started keeps its historical start; completed work is read-only. Dates cannot precede authored release/calendar gate or move editable commitments into the past. Moving/resizing never changes effort, owner, earned output, task identity or external sprint deadlines, and never shifts successors silently. A blocked task remains blocked. Deliberately late carryover dues can be planned within the campaign, while the gold sprint deadline stays fixed.

Use the bottom horizontal scrollbar or drag empty grid space to pan. Scroll vertically over the grid to reach other rows. Horizontal edge auto-scroll follows drag intent; it does not counter-scroll an inward resize. A handle outside the clipped viewport must be brought into view before pressing it. The reference grid spans the configured campaign, not a baked-in day count.

## Read the display

- Solid discipline-colored bars are **allowance windows**, not estimates of required work duration. Night bands are dark; day/phase icons have date hints.
- Hatched strips show known estimated work through forecast p90; green strips show local productive-slot buffer after median completion. Future work/reactions/Stress are not guaranteed by these strips.
- Muted actual-history spans are read-only elapsed ranges; they do not claim continuous production every phase.
- Gold vertical lines are authored Sprint deadlines; cyan is current time. Selected details show planned dates, forecast range, signed local buffer/overrun and blocking work.
- Selecting a task reveals lines to/from its immediate prerequisites. Links may extend beyond the currently clipped rows; pan/scroll to inspect the other end. Status icons/stripes supplement color.
- Tasks belonging to an unactivated workload event remain hidden until that workload activates. Released known carryover stays visible; locked future sprint work does not open early.

Forecast fields now include mean projected start and signed productive-slot buffer. The estimate still uses independent reference seeds/current Stress means/known constraints, not the actual campaign seed or a complete future Stress/event model. Local buffer is not a calibrated success probability or full downstream slack calculation.

## Authoring and evidence boundary

Existing Planning rules expose cell width and row height in Thai Inspector tooltips. Presentation text is seeded from `PlanningSeed.json`; the builder adds missing keys without overwriting existing authored text. All new behavior is opt-in Planning UI; legacy room/Studio fixtures keep their semantics. No new mutable save fields are added (v7 retained).

See [Session 038 evidence](../00_Agent_System/WorkLogs/Session_038_Verification/README.md) and [baseline Planning guide](08_Planning_Prototype_Guide.md). Owner feel/comprehension, full keyboard/controller operation, live performance and final 30-day balance remain unapproved. Batched plan-change character reactions and contextual week review are still the next narrative/gameplay stage.
