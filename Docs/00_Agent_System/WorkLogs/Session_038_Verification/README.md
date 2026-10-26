# Session 038 verification

Actual isolated Unity 6 6000.3.24f1 execution; source art/live Editor preserved. These are technical prototype results, not final content/usability/performance/balance approval.

| Check | Evidence/result |
| --- | --- |
| Actual build | Session038_Build.log, exit 0 |
| Full Edit Mode | 164/164, Session038_EditMode.xml |
| Full Play Mode | 31/31, Session038_PlayMode.xml |
| Delivered file identity | 628 SHA256 matches, delivery_manifest.json |
| Original project files/UI PNGs | 738 protected files and 48 PNGs unchanged, preservation.json |
| Original GUIDs | Preserved against pre-edit archive |
| Pre-edit archive | 1116 entries, integrity checked |

Real input checks: Timeline body and both handles change exact snapped dates; changes stay in one draft across Queue/Timeline and commit without time/output. Started history is locked. Escape, outside drop and Revert retain committed state. Kanban pointer/edge-scroll regressions pass. Rest closes after one phase and recovers energy rather than ending the day. Speech ends before portrait; effect rows use icons and omit duplicate hover. Thai glyph/overflow checks remain enabled. Capture render sizes differ from the 640x480 pointer test window; clipped targets are explicitly scrolled into view and raycast-checked.

Captures: [conversation](Session038_Conversation.png), [1280 conversation](Session038_Conversation_1280.png), [rest menu](Session038_RestMenu.png), [after rest](Session038_AfterRest.png), [Timeline](Session038_Timeline.png), [selected dependency/draft](Session038_TimelineDraft.png), [1280 Timeline](Session038_Timeline_1280.png), [actual-history view](Session038_TimelineHistory.png).

ERR-063–067 track owner-reported UX corrections, clipped custom-graphic lifecycle, Thai title space, queue overflow/teardown/target geometry and inward resize auto-scroll. Final suites pass. No live performance, full keyboard/controller, original foreground Console or standalone claim. Old all-event 30-day diagnostic limitations remain (88% fixed dues /97% replan); no balance change or optional-OT route feasibility claim.

[Guide](../../../04_Technical_And_Tools/09_Timeline_And_Conversation_UI_Guide.md), [work log](../Session_038_Timeline_And_UI_Corrections.md).
