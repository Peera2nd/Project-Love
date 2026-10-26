# Session 064 — 2026-10-26 — Localization foundation and real Workspace tab

## Request

After clarifying that the designed translation tab did not yet exist, owner explicitly authorizes starting implementation. Deliver the Thai/English foundation and artist workflow, preserve existing structure and report actual adoption.

## Implementation

- Added static catalogs/settings/refs and per-session resolution, fallback/translation validation, global locale preference and detached preview locale. Existing graph/IDs remain canonical.
- Added **Workspace > ภาษาและคำแปล** with contextual search, missing/stale filters, Thai/English source/review editing, source links, font controls, Undo/Save and guarded UTF-8 CSV preview/import/export. Inline writing/choice/pool/name/settings editors use the catalog; catalog Inspector routes to the tool. Markdown remains a writing/reference format, not a runtime importer.
- Opted the current Romance campaign into the new path: 689 catalog entries, 96 draft English strings, 593 missing English. All reachable dialogue/choice source slots are bound; principal current UI and names/moods/hints/goals/receipt labels resolve via the session. The opening direct-advice path and base greetings supply bounded bilingual examples. Other translations and custom/unbound Prefab labels remain explicit gaps.
- Separate localized v9 saves use a mechanical graph fingerprint excluding display text/revisions. v8 import requires exact captured legacy and mechanical baseline plus normal state validation, leaving old files intact. New receipts store title keys; ambiguous old raw titles remain fallback. No weakening of state guards or deletion of user saves.
- Delivered 119 existing assets with only localization bindings/config/IDs, new catalog/settings and tested source/test changes. Original source strings, gameplay values/graph, metas/GUIDs, art and authored layout preserved. A native missing/empty displayName default was normalized in verification only. Backup: `.backups/Session_064`.

## Verification and limits

See [focused evidence](Session_064_Verification/README.md): final delivered-revision 5/5 cases pass after bounded affected view/window rechecks. Inspected actual saved-Prefab Thai/English renders; caught and fixed retained-language speech at choice-ready state. Source/test bytes match tested files; scoped data preservation checked. Invalid runtime format strings show a marker instead of throwing; pending-row global Save and CSV IO guards are code-inspected/compile-checked, not separate button-input tests. No broad suites/build. Player-menu click/preference restart and live desktop tab readability not independently checked; live connector refresh did not return. No English/content/artist acceptance claim.

## Issues and handoff

ERR-092 guessed intake paths; ERR-093 planned/available UI ambiguity; ERR-094 launcher observation; ERR-095 key scopes; ERR-096 fixtures/missing draft; ERR-097 retained line; ERR-098 serializer defaults; ERR-099 live connector/capture limit. Follow their preventions. Current adoption is **L2 — partial**, not full translation/version coverage. Update the catalog through Workspace and continue remaining English/custom UI coverage; do not regenerate existing translations or restore Producer mechanics. Preserve current opening/character approvals and 30-day duration.
