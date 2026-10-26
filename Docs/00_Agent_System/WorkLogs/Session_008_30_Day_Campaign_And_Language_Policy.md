# Session 008 — 30-Day Campaign and Language Policy

- **Date:** 2026-10-23, Asia/Bangkok.
- **Owner request:** expand the full game to 30 days for emotional buildup and less demanding management; translate backend documents into concise English while retaining Thai story/characters and Thai user interaction.
- **Tasks:** T0.12 and T0.13. Documentation/rules only; no gameplay implementation.

## Approved decisions

- Full campaign = **30 in-game days**, replacing the earlier 14-day draft. This is approved, not a provisional number or pending decision.
- Use added time for gradual bonding and recovery from routine mistakes. Do not automatically scale mandatory workload or CG scope to fill the calendar.
- English: backend design/system specs, architecture, plans/tasks, handoffs/work logs, incidents, agent rules and internal comments.
- Thai: user conversation/updates/replies; story, dialogue, worldbuilding, character profiles and their authoring documents under `Docs/02_Narrative_And_World/`.
- Preserve identifiers/links and useful existing English machine-facing prompts inside Thai character documents. Artist-facing tooltips and game text remain Thai-friendly.

## Changes

- Rewrote backend specifications, agent documents, historical handoffs, audit/reconciliation reports and log template in concise English. Retained all Session 001–007 history with superseded assumptions marked.
- Updated AGENTS.md, onboarding, Architecture and `.gemini` rules with the same language policy and 30-day decision. Translated existing incident records and added language/duration guardrails.
- Recorded 30 days as decided in Master Decisions; setting/final roster, exact balance, minigame, event timing and CG scope retain their existing draft/unresolved status.
- Added a provisional pacing map: Days 1–7 introductions; 8–15 trust; 16–23 deeper personal arcs; 24–30 resolution/catch-up and final evaluation.
- Retimed proposed opening events: Ploy Day 18/window 16–24, Grace Day 20/window 18–26, Plaifah Day 22/window 20–28. These dates/windows are proposals, still subject to prerequisites and balancing. A preferred date is not a single-day route cutoff.
- Updated all current Thai 14-day setting/deadline references, including Ploy's family ultimatum and roleplay prompt, to Day 30. Retained character IDs, ages, core wounds and existing unrelated story content.
- Extended the planned EventDataSO sketch with startDay/endDay/preferredDay. No schema implementation or asset migration occurred.
- Updated Task Board/Handoff, marked T0.12/T0.13 complete, removed duration from pending approval and added A0.6 for recovery/balance validation.

## Files and scope

- English: AGENTS.md, `.gemini/rules/project-protocol.md`, all Markdown under `Docs/00_Agent_System/`, `Docs/01_Game_Design/`, `Docs/03_Art_And_Audio/`, and `Docs/04_Technical_And_Tools/`.
- Thai narrative edits: Setting Candidates, Master Character Template, short template, three profiles and chemistry/player-theme analysis. The existing Thai character-conception method remains unchanged.
- Filenames/links retained. No new runtime code, Unity asset, scene, prefab, SO asset or package change.

## Verification

Checks cover backend English/Thai narrative boundaries, Markdown file links/fences, stale active 14-day references, approved-versus-provisional duration wording, coordinated deadline/event windows, character IDs/ages and Unity file hashes against a verified pre-edit archive in the permitted visualization workspace.

**Results:** 21 Docs Markdown files checked; 15 backend/rule documents contain no Thai-script text, while all eight narrative documents retain Thai. No broken local file links or unclosed fences. All three IDs/ages, new event targets/windows and approved-duration prompt statements passed. Unrelated existing intimacy-scene sections match the pre-edit archive. All 103 Unity asset files match baseline hashes. Existing backend/rule documents decreased from 149,211 to 59,595 bytes before the final small registry/schema wording adjustments; this is a file-size comparison, not a measured token count.

No compile/build/Play Mode, market/research validation or gameplay balance test was run. The longer schedule and recovery rules are documented design requirements, not demonstrated difficulty results. Historical references to the old 14-day draft and an unrelated example birthday are intentionally retained.

## Handoff

- No new runtime/build incident; ERR-001–005 remain in the English registry.
- A0.1 version control and A0.5 prototype sequencing remain pending. A0.2 still concerns setting/production roster, not duration. A0.6 is pending prototype balancing.
- Continue implementation only when assigned. Implement configurable 30-day campaign data, preserve static/runtime separation and validate recoverability before locking numeric balance.
