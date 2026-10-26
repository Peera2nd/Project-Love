# Session 027 verification

Unity 6 `6000.3.24f1`, isolated `.verify`. Final **96/96 Edit Mode + 15/15 Play Mode passes**, including existing AP/room/Studio regressions. Source: 37 runtime/Editor C# files, nine test files. Actual XML results are included; no standalone player build or original live Editor Console check is claimed.

## New coverage

- Multiple free current-phase rest orders: no phase/stamina/money advance, no immediate Stress change, one recovery/zero worker work at the boundary, duplicate rejection and automatic next-phase resumption.
- Night OT: no default Night work, per-person money/Stress charged once at request, multiple orders without time passing, capped stress-based output only at Night boundary, no extra normal-work Stress penalty, expiry and rejection for wrong time/insufficient funds. Paid orders survive save/load without payment/reward repetition.
- OT conversation selection for all three characters across low/high affection and low/high Stress; character-specific nodes and first-daily/repeat rewards remain independent.
- Producer preview/commit: money, own effort, per-discipline +1 caps, no same-tick spill; gate S2 early, no calendar bypass, early credit to the task's own sprint, immutable deadline reports after late acceptance. Four full campaign policies use actual Producer/relationship/rest transactions.
- Real mouse flows: no worker-rest button on Kanban, Producer work/bar, rest in character menu, all-three OT/talk at Night, early S2 unlock/priority/help/review. Colored cards, owner portrait fallback, actual bars, Thai glyph/bounds and explicit URP captures checked.

Four 30-day policies finish base work at 100%, complete five Producer stages and resolve 120 phases: bug/no bug × daily talk/no talk. Social variants reach affection 60 per character. Both bug policies miss their first sprint deadline but recover all base work by Day 30; other reports meet fixture goals. This is feasible/recoverable-policy evidence, not final difficulty, all-OT economics or authored route balance. Full-campaign policies do not use OT; OT is separately covered by unit and mouse tests.

## Previews

- [Board](Session027_Board.png), [Producer progress](Session027_Producer.png), [early S2 delivery](Session027_Early_S2.png)
- [Rest order](Session027_Rest.png)
- [Grace OT](Session027_OT_studio_grace.png), [Plaifah OT](Session027_OT_studio_plaifah.png), [Ploy OT](Session027_OT_studio_ploy.png)

All seven rendered previews were visually inspected. ERR-042 records a viewport clipping issue found despite passing mouse/label-bound tests; the Studio viewport was expanded and final tests/captures rerun. ERR-041 records an initial rejected patch with no code application.

`delivery_manifest.json`: 324 source/data files match the tested clone; 323 protected baseline files are byte-identical. 54 existing SOs changed, 12 new OT nodes and their metas were added, plus five new UI-folder metas. Existing GUIDs, PNG pixels/imports, source fonts, legacy scenes/data and project/build settings are preserved. No new artwork was generated. UI mockup exports are requested, not delivered; current cards/bars use simple UI geometry and existing portraits.
