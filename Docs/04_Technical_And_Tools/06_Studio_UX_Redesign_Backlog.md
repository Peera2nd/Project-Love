# Studio UX redesign backlog — owner feedback

Session 034 feedback, updated Session 037, 2026-10-25. Owner now explicitly authorizes implementation, superseding the earlier hold. The separate [Planning prototype](08_Planning_Prototype_Guide.md) implements compact draggable queues, permanent three-worker hearts/Stress/work, icon navigation, selected detail, deduplicated VN status and shared Back. Owner usability approval remains pending; original Studio remains the legacy fixture.

## Owner requirements

- Sprint board is the gameplay center, but excessive text makes even informed testing difficult. Reduce visible detail and prioritize visually using icons, discipline colors and hierarchy.
- Familiar icon buttons may omit persistent labels. Preserve understandable names for unfamiliar actions/tasks; show contextual labels/details on focus, hover or selection. Do not replace paragraphs with an equally confusing wall of unlabeled icons.
- Reserve the right status area for all three workers at all times, including work progress, Stress and affection/heart. Navigation, Producer commands and task inspection must not replace it. VN/board layouts must reserve space rather than cover the roster.
- Remove duplicated legacy VN text where the same affection/Stress/work information is already represented by meters. Keep each value in one authoritative visible representation; an inline number on a bar is permissible, a repeated explanatory paragraph is unnecessary.
- Sprint-board Back uses the same bottom-right arrow convention and position as the character VN.
- Many minor details are also excessive: perform a complete information-hierarchy pass rather than fixing only one screen.

## Session 036 owner addition — clean, fluid drag cards

Clean Sprint cards and smooth direct drag-and-drop are explicit owner requirements. The owner asks for readiness; no implementation starts in this turn. See the [drag contract/delivery plan](07_Planning_Prototype_Delivery_And_Drag_Contract.md) for queue/date semantics, drag ghosts/placeholders/auto-scroll, valid targets, draft Apply/Revert, safe cancellation and actual-pointer acceptance checks.

Cards expose minimal identity/progress/risk; details appear only on selection/focus. Drag priority/plan dates, not fabricated completion. Maintain the permanent three-worker roster and common bottom-right Back. Build responsiveness deliberately rather than placing drag callbacks around the existing full-board rebuild.

## Proposed hierarchy (not an approved final layout)

1. Persistent shell: day/phase/Money/Stamina, three-worker roster with portrait/heart/Stress/work meter, consistent Back slot.
2. Main work area: Kanban or Timeline of the same plan. Compact cards: short task name, owner portrait, progress and one clear state/dependency indicator. Avoid full rule/cost/debug reports on every card.
3. Selection detail: a central drawer/popover showing the selected task's predecessor, forecast, due date, buffer and edit consequences. It must not consume the permanent roster area.
4. Event notification: explain what changed and highlight affected tasks; numerical derivation remains secondary detail.

Use icons/shapes/text hints as well as color for state. Department color must not also ambiguously mean deadline risk. Separate playable information from developer diagnostics. Routine planning should be readable without opening every tooltip.

## Later acceptance checks

All three workers' affection/Stress/work status remain visible across room/board/navigation/VN; no redundant status paragraphs; common Back position; icon hit areas and keyboard/focus hints; long Thai names; clipping/scrolling at supported sizes; a first-time reader can identify ready/blocked/at-risk work. Actual owner usability feedback is required in addition to raycast/font/layout tests.

Session 037 uses supplied sprites; no new asset request. Session 038 adds Timeline, removes duplicate board/rest hotspots, separates player rest/End Day, narrows speech, uses icon effect rows without duplicate hover and closes completed rest panels. Further dependency/risk communication and owner usability iteration remain open.
