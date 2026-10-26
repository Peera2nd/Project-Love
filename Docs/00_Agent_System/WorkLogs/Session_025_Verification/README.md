# Session 025 — Paper Sprint Model

**Evidence:** arithmetic examples for proposed values, not gameplay implementation, player testing, balance approval or a full 30-day simulation. Run `python -X utf8 balance_model.py` from this folder or invoke it by path. [Model](balance_model.py), [full output](results.json), [numeric proposal](../../../01_Game_Design/04_Work_Rates_Stress_And_Producer_Proposal.md).

Assumptions: seven days, three productive phases/day, normal Night rest; initial player energy 100 and worker Stress 30 (75 for severe-stress cases); nine cards Code 28/28/24, Art 24/24/24, Design 24/20/20. Free review accepts completed cards at each boundary; no output carries into another card within that tick. A bug adds one 24-unit Code card.

| Scenario | Effort C/A/D | Accepted C/A/D | Active player actions | Free daytime slots | Min energy | All quotas met? |
| --- | --- | --- | --- | --- | --- | --- |
| No player work | 74/72/64 | 56/72/64 | none | 21 | 91 | No |
| Late focused help | 80/72/64 | 80/72/64 | 1 help | 20 | 71 | Yes |
| Mixed daily help | 80/72/64 | 80/72/64 | 5 help | 16 | 71 | Yes |
| Code bug +24 with response | 104/72/64 | 104/72/64 | 9 help | 12 | 42 | Yes |
| Two missed help days | 80/72/64 | 80/72/64 | 4 help | 17 | 71 | Yes |
| High stress 75: partial care | 74/63/59 | 56/48/44 | 7 help, 6 care | 8 | 42 | No |
| High stress 75: deliberate recovery | 80/72/64 | 80/72/64 | 8 help, 9 care | 4 | 13 | Yes |
| Code worker absent Days 4-6 | 80/72/64 | 80/72/64 | 9 help | 12 | 13 | Yes |
| Four coordination actions only | 79/72/64 | 56/72/64 | 4 plan | 17 | 76 | No |
| Producer unblock plus focused help | 80/72/64 | 80/72/64 | 6 help, 1 clarify | 14 | 51 | Yes |

All seven Night slots are unused in these schedules and are excluded from the free-daytime column. The model charges first-daily talk for all present characters (21 interactions, or 18 during the absence). Talk consumes no phase and does not change Stress. Work on finished cards is not attempted; all modeled actions are eligible/affordable. No items, paid energy recovery or automatic gift/stress bonuses are assumed.

Code 74/80 effort with 56/80 accepted means two cards are accepted and the final 24-unit card still needs six effort; it does not mean earlier work was erased. The same distinction applies to coordination-only Code 79/80 with one effort left.

Limits: no Unity execution, complete specialist dependency graph, story-phase costs, economy/SP/gifts, complete campaign or optimization proof. Failed quotas are intentional modeled outcomes. The severe-stress recovery consumes 17 of 21 daytime actions and is a tuning concern; ordinary two-missed-day recovery uses four. Rate/quotas/Stress values remain proposals.

Checks passed: 28 unique phase entries per case; no negative/over-cap player energy; Stress in 0–100; no normal Night production; accepted <= effort <= authored total; all scheduled actions eligible; ten cases recorded.
