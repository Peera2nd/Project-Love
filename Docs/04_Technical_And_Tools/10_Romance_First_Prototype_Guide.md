# Romance-first prototype — Session 041

Owner approves implementing the Session 040 casual direction and explicitly replaces Stress with character mood. This is the primary playable direction. Existing Planning/Studio scenes remain historical regression/reference fixtures, not an extra expert mode. Full duration remains 30 configurable days; authored story coverage is an opening trial, not a complete campaign.

**Authoring upgrade, Session 044:** use **Tools > Project Love > Workspace** for Thai dialogue search/editing, event drafts, visual room placement and actual UI Prefab navigation. The main scene now contains serialized room/UI views before Play, with isolated Edit Mode previews and preserved artist layout. See the [current authoring guide](12_Artist_Workspace_And_UI_Authoring_Guide.md). The Session 043 audit describes historical limitations.

## Open and play

Unity: **Tools > Project Love > Open Romance Prototype**, then Play. Dashboard's first button opens the same scene. Direct scene: `Project Love/Assets/_Project/Scenes/Gameplay/StudioRomancePrototype.unity`. The builder prepends this scene in Build Settings; it does not close unsaved owner scenes automatically.

The opening conversation explains the loop. Click a person, choose an activity, read/tap through its short scene and see the actual reward. Work Together and Personal Time each use one phase. Back before completion restores the complete snapshot and costs nothing. At Night either activity can still consume the final phase; there is no OT purchase/pressure rule. End Day resolves remaining time once; Next Day opens a compact growth report. The last day has a final report without a Day 31.

## Trial rules (editable, not final balance)

| Action | Project | Affection | Money | Time |
| --- | --- | --- | --- | --- |
| Work Together | +2 percentage points | +2 to that person | 0 | 1 phase |
| Personal Time | 0 | +3 to that person | 0 | 1 phase |
| Producer work | +4 percentage points | 0 | +10 | 1 phase |
| Ordinary talk | 0 | +1 on first completed talk/person/day, then 0 | 0 | 0 |
| Team baseline | +3 percentage points once at day end | 0 | 0 | No extra player action |

All progress/affection rewards cap honestly at 100. Previews and receipts show the actual capped amount. No AP/Stamina optimization, Stress counter, task speed RNG, mandatory board operation, separate Producer duty ladder, early S2 gates or department buff network runs in this configuration. Unused legacy fields remain solely for compatibility; casual state keeps Stress/AP/Stamina at zero and rejects Stress transactions.

Mood is an authored enum: Calm, Cheerful, Tired, NeedsSupport. UI uses Thai labels and simple icons, not a precise meter. Mood selects a comfort or ordinary Personal Time scene; actions/story choices can change it. It never changes work speed or imposes a hidden failure spiral. Affection can select a warmer scene; no automatic lover status/final arc is implied.

One current milestone and three broad work cards replace the required Kanban/Timeline path. Weekly support focus is optional and locked to one person for that week. First Work Together with that person grants one additional affection and unlocks a topic. Repeats do not farm it. A later week can give its once-only affection bonus again; the topic flag remains unique. Default team output works with no focus selected. Weekly targets are guidance; missing them does not block romance or trigger a game-over in this trial.

Morning reports distinguish the team's day-end gain from player activity gain, include Money before/after, relationship changes, newly unlocked focus topics and today's suggestion. Opening/acknowledging/loading a report grants no rewards. Reports are persisted. Gifts keep inventory consumption/preferences and completion rollback; free travel and gifts are supporting actions. Consumable Stamina-use controls are absent in this path.

## Content and authoring

Canonical definitions: `Data/Romance/RomanceCampaign.asset`, `RomanceRules.asset`, `RomancePresentation.asset`, cloned cast/rooms and `Dialogues/`. Existing art is referenced with its original GUIDs. Edit the SOs in Inspector/Dashboard; runtime never mutates them. Initial fixture copy and Thai content live in `Data/Settings/RomanceSeed.json`; reopening does not regenerate authored assets or overwrite normal Inspector edits. The one-time presentation version 41 repair concerns only the initial undelivered fixture.

`RomanceConfigSO` holds reward amounts, focus cadence, milestones, per-character scenes/topic thresholds, mood definitions/schedule/outcomes and daily guidance. `RomanceState` holds mutable mood/focus/day snapshots/reports/receipt. `RomanceSystem` resolves the single casual loop through the existing clock and dialogue rollback. `RomanceRoomView` renders the simplified HUD, persistent heart/mood roster, activity previews, optional support cards and recap. Existing `GameSession`, `DialogueRunner`, `CharacterInteractions`, `EventScheduler`, room assets and actual input remain shared.

Opening authored content: Day 1 forced welcome (zero phases), Grace's coffee bubble Days 2–4, Plaifah's sketch bubble Days 3–5 (one phase each, two personal response choices). Morning Day 2 sets Grace Tired; Day 3 sets Plaifah NeedsSupport. Mood outcomes commit only when the event completes. Existing priority/window/ID arbitration, per-person top bubble and modal reservation still apply. New mode does not import workload/paid-OT events from the historical catalog. Other activity/talk/gift/focus/warmer scenes are short temporary Thai drafts for all three cast members. Full personal arcs, CGs, endings, minigames, scene variety after the opening and final economy remain pending.

## Persistence and verification

Casual saves use **v8** and a separate namespace; v7 Planning/v6 events/v5 Studio/legacy files remain preserved. Restore validates catalog fingerprint, moods/IDs, one focus/week, zero hidden budgets/Stress, ordered daily reports and safe scene boundaries. Do not load complex-planning saves as casual saves. Begin a fresh romance run.

Evidence: [Session 041 verification](../00_Agent_System/WorkLogs/Session_041_Verification/README.md). 177 Edit Mode and 34 Play Mode cases pass, including existing fixtures, actual pointer controls, gift/scene rollback, mood event choices, save/load and Thai text bounds/glyph coverage. Captures at 1920x1080 and 1280x720 were inspected. Three actual 30-day domain policies need no board/energy/OT: all Personal Time yields 90% project; one Work Together/day yields 100%; three early Producer actions plus Personal Time yields 100%. They exercise clock/reward feasibility, not final route balance or enjoyment. Session 042 owner confirms the loop is easy to understand, but reports linear repetition/no challenge. Next proposed acceptance is a meaningful first-week character/event chain; [design discussion](../01_Game_Design/09_Romantic_Tension_And_Meaningful_Choices_Proposal.md). Current runtime/rates remain unchanged.
