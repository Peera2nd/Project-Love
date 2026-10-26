# Session 025 — Numeric Work/Stress/Producer Proposal

- **Date:** 2026-10-24, Asia/Bangkok.
- **Request:** propose worker output, Stress effects and active Producer gameplay.
- **Scope:** design and arithmetic examples only, honoring the prior request to settle gameplay before new implementation. No new numeric owner approval, Unity code/asset changes, cast integration or art generation.

## Proposed prototype values

Canonical details: [Work Rates, Stress and Producer Actions](../../01_Game_Design/04_Work_Rates_Stress_And_Producer_Proposal.md).

- Shared base 4 effort per productive phase; three normal work phases/day, Night off as previously approved. Starter targets Code 80 / Art 72 / Design 64, with nine cards (28/28/24, 24/24/24, 24/20/20).
- Paper starting Stress 30 each; output tiers 4/3/2/1 at Stress 0–39/40–69/70–89/90–100. Actual normal-output tick adds 2 Stress, overnight recovers 4. No hard meter-based illness/automatic stop.
- Player help: one productive phase, 20 energy, capped +6 effort and target Stress -3 before normal work. Own help and team output share remaining effort. No implicit affection gain.
- Coordinate: one phase/15 energy, +1 per eligible worker over two productive ticks, global non-stacking plan. Clarify: one phase/15 energy, remove a specific Producer-decision block without bypassing specialist work. Break/check-in: one productive phase/10 energy, target Stress -15 and no target baseline work that tick; others continue.
- Keep player's 100 energy cap, +40 overnight and +30 one-phase rest as trial inputs; player rest should recover only the player in this proposed system. Existing fixture effects (e.g. worker Stress +3 on work/-10 on player rest) remain unchanged until the new design is approved/implemented.
- Board inspect/priority/review remain proposed free operations. Night desk planning can prepare two future productive ticks; overtime/after-hours worker help is still undesigned. Ordinary talk retains its first-daily policy with no new Stress bonus.

## Reproducible arithmetic exercise

[Model/report](Session_025_Verification/README.md), [script](Session_025_Verification/balance_model.py), [full traces](Session_025_Verification/results.json).

Ten deterministic seven-day cases include card caps, one active card/worker, no same-tick next-card production, immediate free review, Stress, player energy, first-daily talks, a Producer-decision block, bug workload and absence. Checks cover unique phase entries, bounded state, legal/affordable actions, capped work and no normal Night production. This is not engine or player testing.

- No help: Code 74/80 effort, with 56 accepted and six effort remaining on the final card. Art/Design finish. One late focused help can finish the starter week.
- Mixed five helps completes all targets; two missed intended help days still complete with four helps.
- Bug adds Code 24 (target 104); one response schedule completes with nine total helps, leaving 12 unspent productive slots plus seven Night slots.
- Three-day Code absence has a nine-help completion example; first-daily talk is only charged for present characters. This is capacity analysis, not approval of Grace's story trigger.
- Stress 75 across all three is severe: a partial care schedule misses quotas. Nine breaks plus eight helps recover in one example, leaving only four unspent productive slots. This is an explicit tuning concern, not evidence of forgiving balance at every initial state.
- Four coordination actions alone leave Code one effort short; an unresolved focused bottleneck remains visible. A Producer-unblock plus six helps completes the separate blocked case.

None is an optimality proof. Specialist dependency chains, authored story costs, economy/SP/items/gifts, discretionary review delays, full-campaign carryover and release scoring are excluded. Starting Stress 30 and these targets are new proposal inputs, not the current live fixture or an approved 30-day difficulty model.

## Closeout

Updated Master Decisions (proposal-only section), studio spec links, Task Board/current priority and Handoff; preserved Session 024's eight delivered mockups and pending cast integration. Full-campaign balance remains pending. Document links/fences and unchanged runtime/source assets checked. No new tool/runtime incident or engine test; the last engine baseline remains Session 022's 58 Edit Mode / 7 Play Mode passes.

Next: owner reviews the action feel/values, then implementation can use data-driven rates/Stress bands/Producer effects and the deferred truthful status/forecast panel. Full playthrough remains 30 configurable days.
