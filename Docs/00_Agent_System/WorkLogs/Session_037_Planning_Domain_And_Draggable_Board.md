# Session 037 — Planning domain and draggable board

2026-10-25. Owner explicitly authorizes work. Required intake read in order, then task/source/design contracts. This supersedes the prior design/UI hold. No delegation, external messaging, new images or package changes. Pre-edit `.backups/session-037-before.zip`: 1065 files, integrity checked.

## Implementation boundary

Deliver Stages 1–2 from the [contract](../../04_Technical_And_Tools/07_Planning_Prototype_Delivery_And_Drag_Contract.md) at the requested early feedback point. Keep the original Studio scene/config as a legacy regression fixture and add `StudioPlanningPrototype`/four new Planning SOs. Both use the same ProductionSystem, day clock and event scheduler, with `ProductionConfigSO.planning` selecting the replacement domain. Source count 53; tests 17.

Static rules and Thai text are editable assets. Runtime PlanningState persists seed, task dates/fraction/history, worker/day Stress snapshots, shared participation and actual OT nights. SHA256-keyed task/day variation has no mutable random cursor. A separate forecast ensemble never reads the campaign seed; it freezes current Stress/known constraints and is explicitly approximate.

Normal team output, fractional caps, Stress/pressure/idle/rest/overnight behavior resolve once at the existing phase boundary. Capture ready worker targets before any boundary acceptance, so no same-phase dependency spill. Routine completion auto-handoffs; manual checkpoints remain explicit even at sprint deadline. Sprints release by calendar, Producer duties carry without freezing team. Producer earns only own effort/Money; shared time only first-person/day affection/co-presence, zero extra work/direct Stress healing. Existing Night invitation/callback scheduler uses the new participation ledger when planning is enabled.

Save envelope7/additional filename key preserve old slots. Disabled event catalogs normalize inline JSON null independently of schema; enabled catalogs remain validated. Exact reload retains fractional residue/seed/future output. Draft commands validate catalog/cursor/revision/unique full priority permutation/dates/history; Apply commits atomically without advancing time, Back discards and saves stay modal-safe.

UI caches cards by stable taskID, lifts the actual card with a placeholder/insertion line, follows pointer without forecast/domain work each move, bounds edge scroll and restores invalid/Escape/outside drops. Three worker lanes show current tasks/carryover, short status/due/progress; selection contains forecast/dependency/date editing and an explicit decision/review command. Existing bars/icons are reused. Persistent right roster reserves affection/Stress/quota work across room/board/menus/VN. Duplicate legacy VN paragraphs are absent. Apply/Revert/common corner Back and Producer duty meter complete the current shell. No Timeline or plan-reaction story batching yet.

## Verification and findings

Actual isolated Unity build succeeds. Final 161 Edit Mode and 27 Play Mode pass, including old fixtures, deterministic replay/save, draft purity/rollback, calendar/no-buff semantics, dependency boundaries, social daily gates and planning shared-OT preview/commit/no-double-fee/overnight/callback/save validation, plus actual mouse reorder/cross-owner/Escape/outside/edge-scroll. 1920x1080 room/board/conversation/dependency/inventory captures, an additional 1280x720 board and Thai bounds reviewed. Stable identity checked during drag; this is not live performance profiling or owner feel approval.

First Edit run 155/156: v7 empty inline disabled-event state rejected on reload; fixed normalization and reran. First Play run 24/26: Unity fake-null CanvasGroup handling threw and card State text overflowed; fixed Unity-aware null handling, Thai text slots, visible meter height, roster draw order, supplied icon keys, navigation duplication/overlap and corner menu Back. Later render review corrected the Producer completed-release wording through a numeric released-duty meter. ERR-060–061 record these. ERR-062 records an incomplete Assets-only GUID index; package scripts were valid and no asset repair was needed.

Four all-event one-seed diagnostic policies run the actual catalog to 30 days, 120 phases, five reviews and all five Producer duties, with safe-state checks after boundaries/events. Fixed dues, social fixed dues and delayed first-week administration each finish 88% scope; Code reaches Stress 100. Daily buffer replan finishes 97%, Code Stress 77, nine committed revisions. All decline voluntary OT. These reveal unfinished capacity/recovery tuning rather than establish optional-OT feasibility or a final romance schedule. Existing helper-based 100% evidence must not be reused for Planning.

## Delivery and next work

622 source/data/UI/scene files hash-match the tested clone. 727 existing non-code project files and 48 supplied UI PNGs remain byte-identical; original GUIDs retained; all new scene/data references resolve against project and installed packages. Source fonts, project settings, original scenes/assets and owner PNGs preserved. Evidence includes tests, captures, policy states, manifests and preservation report.

Updated current/structured handoff, task board, architecture, Master, UX backlog, delivery contract and [usage/authoring guide](../../04_Technical_And_Tools/08_Planning_Prototype_Guide.md). Local links checked. Owner can now inspect the Stage 2 board. Next: same-plan Timeline/snapped move-resize, then contextual replanning/week review and actual-catalog capacity/Stress/romance tuning. Keyboard/controller completeness, live profiling, standalone build and live original Editor Console remain unverified.
