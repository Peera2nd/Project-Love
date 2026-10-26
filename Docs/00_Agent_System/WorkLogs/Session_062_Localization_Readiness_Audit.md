# Session 062 — 2026-10-26 — Localization readiness audit

## Request and scope

Owner asks whether the current systems already support localization. Source/data/package inspection only; no implementation requested. Current story priority and Producer deferral remain unchanged.

## Findings — verified by static inspection

| Area | Current evidence | Readiness |
| --- | --- | --- |
| Package integration | `Project Love/Packages/manifest.json` and `packages-lock.json` have no `com.unity.localization`; no localization/locale API matches in owned scripts | No installed/integrated localization system found |
| Reserved folder | `_Project/Resources/Localization` contains only `.gitkeep` | Placeholder, not a feature |
| Dialogue/choices | `DialogueNodeSO.cs`: stable `lineId`/`choiceId`, one raw `text` string each | Useful identity foundation, no translation references |
| UI copy | `RoomPresentationSO.cs`: `PresentationText.key/value`, `Get(key)` and one font reference | Partial centralized copy; one value per key, no locale lookup or missing-translation fallback |
| Other content | `CharacterDataSO` names/roles; `RomanceConfigSO` mood/work/topic/milestone labels and daily hints remain raw strings | Localization must cover more than dialogue |
| Rendering | `IsometricRoomView.RenderDialogue` assigns raw line/choice/speaker text; `AuthoredRomanceLayout` uses presentation keys plus raw content labels | No shared localized-text resolution or language-change refresh path found |
| Saves | `RomanceSystem.Fingerprint()` hashes reachable line/choice text; `CanRestore` requires the same fingerprint. `RomanceReceipt` persists a display title | Replacing source text with translations can reject old saves for hashed content; saved display titles also require an explicit policy |

Owned scripts checked for localization, locale, language setting and PlayerPrefs APIs; no matches. This is not a live Editor, compile, build, locale-switch or font-coverage test. Existing Thai/TMP rendering evidence does not establish support for additional writing systems.

## Recommended future scope — proposed, not approved or implemented

- Keep a single language-independent story graph, stable IDs, event conditions and rewards; resolve display text through locale tables. Validate key uniqueness, using composite keys where required.
- Preserve Thai as the source/fallback; include UI, greetings, names, items, room/event labels, hints and serialized Prefab text in the inventory.
- Add a player language preference, defined safe refresh behavior, suitable fonts/fallbacks and actual layout previews per language. Do not overwrite artist-authored layout on refresh.
- Expose translation search/edit/Undo/Save, missing/stale translation reporting and translator import/export through the Thai-facing Workspace. Source/translation ownership must be explicit.
- Plan a one-time migration/compatibility policy for existing content fingerprints and saved receipt titles. Future language switches or translation-only updates should not alter gameplay identity; do not simply remove save guards.
- Trial one Thai/English scene, its greeting/choices and affected HUD/menu labels before broad content migration. Verify missing-key fallback, language preference, save/load and real text fit; English is an example, not an approved target-language decision.

## Changes and handoff

Updated Task Board and current Handoff with this audit; recorded read-path incident ERR-091. No C#, Unity assets, narrative, saves or gameplay values changed. No Unity tests rerun. Localization remains a recorded gap, without implementation authorization or a change to current production priority.
