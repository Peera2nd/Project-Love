# Session 035 offline paper trials

Design arithmetic only; no Unity runtime, assets, UI or save files were changed. All values are provisional and the six-task fixture is not the shipped Studio catalog. [Design spec](../../../01_Game_Design/07_Work_Planning_Math_And_Paper_Trial.md).

## Reproduce

Run `python -X utf8 Docs/00_Agent_System/WorkLogs/Session_035_Paper_Trials/paper_model.py` from the workspace root. It writes only this evidence directory. Uses Python standard library; no Unity, network or installed package required. [Raw summary](summary.json) / [CSV](summary.csv).

Inputs: 1,000 keyed seeds (0–999), three work models, five paired policies = 15,000 offline paths. The fixed model repeats the identical path; its count is a comparison convention, not 1,000 independent random observations. No time-of-day clock, UI opening, ordinary talk or loading can reroll keyed task/day samples in the proposed design. No game implementation exists for that policy.

| Model | Policy | On-time / 1,000 | P10 / P50 / P90 finish | Mean peak Stress | Mean internally late tasks | Mean OT slots |
| --- | --- | ---: | --- | ---: | ---: | ---: |
| fixed | comfortable | 1000 | Day 5 Morning / Day 5 Morning / Day 5 Morning | 33.0 | 0 | 0 |
| fixed | compressed | 1000 | Day 5 Daytime / Day 5 Daytime / Day 5 Daytime | 47.0 | 3 | 0 |
| fixed | disruption_hold | 1000 | Day 7 Morning / Day 7 Morning / Day 7 Morning | 35.0 | 1 | 0 |
| fixed | disruption_replan | 1000 | Day 7 Morning / Day 7 Morning / Day 7 Morning | 33.0 | 0 | 0 |
| fixed | disruption_replan_ot | 1000 | Day 7 Morning / Day 7 Morning / Day 7 Morning | 33.0 | 0 | 0 |
| phase_random | comfortable | 1000 | Day 5 Daytime / Day 5 Evening / Day 6 Morning | 33.0 | 0 | 0 |
| phase_random | compressed | 1000 | Day 5 Daytime / Day 6 Morning / Day 6 Morning | 46.46 | 3 | 0 |
| phase_random | disruption_hold | 808 | Day 7 Daytime / Day 7 Evening / Day 8 Morning | 38.92 | 1.4 | 0 |
| phase_random | disruption_replan | 811 | Day 7 Daytime / Day 7 Evening / Day 8 Morning | 33.22 | 0.19 | 0 |
| phase_random | disruption_replan_ot | 1000 | Day 7 Daytime / Day 7 Evening / Day 7 Evening | 35.69 | 0.02 | 0.262 |
| task_day_random | comfortable | 1000 | Day 5 Daytime / Day 5 Evening / Day 6 Morning | 33.0 | 0 | 0 |
| task_day_random | compressed | 1000 | Day 5 Daytime / Day 5 Evening / Day 6 Morning | 46.28 | 3 | 0 |
| task_day_random | disruption_hold | 797 | Day 7 Daytime / Day 7 Evening / Day 8 Morning | 39.07 | 1.38 | 0 |
| task_day_random | disruption_replan | 801 | Day 7 Daytime / Day 7 Evening / Day 8 Morning | 33.33 | 0.2 | 0 |
| task_day_random | disruption_replan_ot | 998 | Day 7 Daytime / Day 7 Evening / Day 7 Evening | 35.91 | 0.03 | 0.261 |

External milestone: Day 7 Night; work normally occurs Morning/Daytime/Evening only. Late paths continue at most to Day 14 to measure actual completion, rather than dropping failures from percentiles. Peak Stress measures the most stressed worker within Days 1–7. Same seed/key families are paired across plans.

## Read the results carefully

- All comfortable sampled paths deliver by the milestone; compressed plans miss all three tightened internal promises and have more Stress without earlier completion. This is not proof of all-seed feasibility or fun.
- A visible +12 Art/+12 Code change before Day 3 Morning consumes most original buffer. Replanning reduces mean peak Stress from 39.07 to 33.33 in task/day mode, but delivery only changes 797 to 801 out of 1,000. Do not claim schedule extension creates a strong speed buff.
- Bounded, compensated OT raises the replanned event case to 998/1,000 in this fixture, with mean 0.261 Night work slots/1.305 fees per run. It is neither guaranteed nor a mandatory relationship path.
- The stochastic models often finish later than the deterministic point even at identical nominal mean rate. Whole-phase completion/no spill and dependency boundaries produce rounding delays; forecast by simulated completion windows, not effort divided by mean alone.
- The phase-random and task/day models show similar aggregate delivery here. The task/day recommendation is for coherent readable variation and stable work identity, not demonstrated statistical superiority.
- No complete player Stamina/Money/Producer/romance schedule, real Studio SOs, 30-day route balance, final story, UX evaluation or Unity validation was simulated. Old engine test counts are not evidence for the proposed rules.

## One matched seed trace

- [comfortable](trace_seed17_comfortable.csv): all three workers for every phase of Days 1–7, including zero-work waits and Night recovery.
- [compressed](trace_seed17_compressed.csv): all three workers for every phase of Days 1–7, including zero-work waits and Night recovery.
- [disruption_hold](trace_seed17_disruption_hold.csv): all three workers for every phase of Days 1–7, including zero-work waits and Night recovery.
- [disruption_replan](trace_seed17_disruption_replan.csv): all three workers for every phase of Days 1–7, including zero-work waits and Night recovery.
- [disruption_replan_ot](trace_seed17_disruption_replan_ot.csv): all three workers for every phase of Days 1–7, including zero-work waits and Night recovery.
