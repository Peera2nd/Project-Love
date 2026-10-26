# Session 023 — Sprint Clarification and Team Art Request

- **Date:** 2026-10-24, Asia/Bangkok.
- **Scope:** owner explicitly requests recording the character-status improvement for later, settling board/sprint/discipline gameplay first, and issuing an additional character mockup request. Documentation/planning only; no runtime or Unity-asset changes.

## Owner decisions and required follow-up

- Record a clearer character VN status panel: affection, character Stress, current task/goal progress and exactly how much helping adds. Make player Stamina/cost, stress delta and separate team output visible in the proposal. Implementation remains deferred as requested (T2.6).
- Asked the work schedule; owner chooses **Morning/Daytime/Evening autonomous output, Night normal rest**. Team baseline plus player help remains confirmed from Session 022. No overtime rule or numeric productivity was approved.
- New Grace/Ploy mockups are appropriate for distinguishing Code/Art/Design; request minimum four PNGs, recommended eight. No images supplied/generated/imported this session and no final cast/story scope inferred.

## Designed for discussion

Expanded [Studio Work and Sprints](../../01_Game_Design/03_Studio_Work_And_Sprints.md) with a one-week/6–9-card prototype proposal, discipline roles, one active card per worker, player help on shared remaining effort, task acceptance feeding weekly/overall completion once, free batch review/weekly ready-card settlement, unfinished carryover and bounded Producer support. Exact rates/quotas/review cost/penalties remain proposals.

Proposed resolver: snapshot eligible work -> player help/cost -> baseline output for the elapsed productive phase -> record/publish/advance -> later boundary events. Rest passes productive time while normal team work continues; zero-phase talk/gifts/menus do not tick work. Proposed early end-day resolves remaining productive phases through the same once-only path without extra player actions or bypassing due events. This ordering, review logic and early-end behavior are not implemented/approved by the discussion.

Work-preview acceptance details include effective capped help, task/sprint/overall distinctions, separate normal-worker forecast, disabled reasons and actual post-action deltas using the completion calculation. Current placeholder direct aggregate/work counters are not a finished task/sprint implementation.

## Artist handoff

[Team Mockup Asset Request](../../03_Art_And_Audio/02_Team_Mockup_Asset_Request.md):

- Required: `chr_grace_Seated_Working.png`, `chr_ploy_Seated_Working.png` in the existing Room folder (1024x1024 transparent); `chr_grace_Normal.png`, `chr_ploy_Normal.png` in Dialogue (1500x2000 transparent).
- Recommended additions: Standing_Relaxed room pose and Smile VN sprite for each, bringing the total to eight. Normal/Smile registration must match. Keep character art local and independent of furniture.
- Reuse existing Plaifah/rooms/black surround; no new board/UI art, background, CG, illness art, walking animation or player sprite required. Optional annotated `ref_workspace_team_layout.png` helps contact/placement.
- Read the existing Thai Grace/Ploy appearance profiles and inspect the supplied morning room before requesting poses. The room has computer/laptop areas and shared sofa/table space; exact phase placements remain integration work.

## Documentation and incident

Updated Master Decisions, Task Board/current priority, Handoff, studio design, old/new art briefs/pipeline, source-art README and the room guide's deferred requirement note. ERR-032 corrects the old art brief's stale claim that inventory/shop/travel were unimplemented; Session 019 delivery remains historical evidence.

## Verification and limits

Verified local document links/anchors/fences, exact export names/counts and existing destination directories/profile references. Runtime/source and authored assets remain matched to the Session 022 verified copy, excluding the known dynamic font cache. No new compiler, Play Mode, art integration or balance tests; last engine evidence remains Session 022's 58 Edit Mode / 7 Play Mode passes.

Next: settle remaining sprint/effort/review/Producer decisions, receive mockup art, then implement the agreed card/production system and deferred status preview. Do not treat proposed rates, review dates, absence story or optional artwork as approved/delivered.
