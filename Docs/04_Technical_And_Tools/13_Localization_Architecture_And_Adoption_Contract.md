# 13 — Localization Architecture and Adoption Contract

## Authority and current status — Sessions 063–064, 2026-10-26

Owner directs: preserve the existing structure; design the new localization structure now; adopt it during subsequent version development; inform continuing agents and require an adoption report. Initial languages are **Thai (`th`) and English (`en`)**. Thai remains the source/default/fallback language. English support does not mean existing English translations are approved or complete.

**Session 064: L2 — partial adoption implemented and focused checks passed.** Owner explicitly authorizes starting after the planned/available-tab clarification. The real Workspace translation tab, catalog/settings/service/refs, Thai/English preview and localized v9 save path now exist; current Romance content is opted in. Raw source fields, graph/GUIDs/Prefabs and historical fixtures remain preserved. Initial table: 689 entries, 96 draft English strings and 593 missing English. This is not complete English content or live desktop/owner acceptance. [Authoring guide](14_Localization_Authoring_Guide.md), [evidence](../00_Agent_System/WorkLogs/Session_064_Verification/README.md), [pre-implementation audit](../00_Agent_System/WorkLogs/Session_062_Localization_Readiness_Audit.md).

## Mandatory instruction to continuing agents

1. Read this contract before the next playable/content/system version that creates or changes player-visible text. Documentation-only, art-only and unrelated fixes do not force migration.
2. At that development milestone, implement the shared foundation and migrate a bounded end-to-end slice before extending single-language authoring. Newly added or revised player-visible text in that milestone must use the new path; unrelated legacy content can remain on the explicit compatibility path. Do not keep postponing adoption because the foundation has not been coded. This is the owner's standing direction for subsequent version work; do not ask again merely to confirm this direction.
3. Preserve the current working baseline. Use additive data/fields and explicit campaign opt-in; never destructively replace the existing campaign or mass-regenerate assets. No separate story graph per language.
4. Do not claim adoption from a spec, empty table, installed package, interface stub or bypassed test. A language-resolved scene and its affected UI must actually use the new path.
5. On first real adoption, and on each later expansion, update the adoption register below, Task Board and current Handoff. **Report adoption explicitly to the owner in Thai in the same turn's final reply:** status, migrated scope, remaining legacy scope, checks/limits and save compatibility. Record a partial or blocked result honestly; do not silently fall back and report completion.

The original Session 063 was documentation-only. Session 064 now supplies the foundation and first adoption; continuing agents extend it rather than reinstalling/replacing it. This does not resume deferred Producer mechanics or change story/balance approval.

## Separation and ownership

| Layer | Responsibility | Must not own |
| --- | --- | --- |
| Existing game data | Stable identities, dialogue order/branches, expressions, conditions, flags, rewards, time and event eligibility | Separate copies of gameplay per language |
| Localization catalog | Stable text key, Thai source, English translation, context and translation-review metadata | Gameplay state or duplicated conditions |
| Localization service | Resolve key for active locale, format validated arguments, fallback, report missing/stale entries, notify text views | Advance dialogue, reroll greetings, grant effects or save campaign progress |
| Existing views/Prefabs | Bind resolved text and authored locale font settings; preserve actual layout | Rebuild/reposition fixed view structures on language change |
| Thai-facing Workspace | Edit source/translation, locate ownership, Undo/Save, import/export, detached language previews | A second independently editable Thai copy |
| Global user preferences | Selected locale, independent of campaign slots | Story flags, affection, content fingerprint or read history |

The content graph and existing stable IDs remain canonical. A locale change must preserve active line, choices, read/choice history, event transaction and room context. This is text localization; localized voice/art packages are outside the initial scope.

## Implemented initial data and file structure

All owned Unity additions belong under `_Project`. Create folders only when implementing; retain existing metas and GUIDs.

```text
Project Love/Assets/_Project/
  Data/Localization/
    *_localization.asset      # Campaign settings and baseline save mapping
    *_texts.asset             # Thai source / English draft catalog
  Scripts/Data/               # LocalizationCatalogSO.cs / LocalizationSettingsSO.cs
  Scripts/Core/               # LocalizationService.cs / LocalizedGameplayFingerprint.cs
  Scripts/Editor/             # LocalizationAuthoring.cs / LocalizationWorkspace.cs
```

`Resources/Localization` is currently a placeholder, not a required loader. Prefer explicit serialized catalog/settings references on the opted-in campaign; do not add global folder scans as the normal runtime content source. No new third-party package is required by this contract. If a later implementation chooses a package backend, it must satisfy this contract, check its current official APIs and document the choice; do not create competing active catalogs or resolvers.

| Implemented type | Contract |
| --- | --- |
| `LocalizedTextRef` | Serializable stable `key`; existing raw field is the legacy fallback during transition, not a new second editable source |
| `LocalizationEntry` | `key`, `thai`, `english`, `context`, `sourceRevision`, `englishReviewedRevision`; owner asset/field binding for navigation |
| `LocalizationCatalogSO` | Stable catalog ID, entries, validation of duplicate/missing keys; campaign-scoped catalogs plus explicitly shared UI catalogs |
| `LocalizationSettingsSO` | Supported `th`/`en`, default/fallback `th`, explicit catalog references, authored per-locale TMP font/fallback configuration |
| `LocalizationService` | Resolve/format using explicit locale, structured result identifying fallback/missing/stale state, locale-change notification; no source mutation |

`GameConfigSO.localization` is the explicit opt-in; a null reference preserves legacy rendering/save behavior. `LocalizedTextRef` is grouped with catalog definitions; static Thai source properties also support newly authored lines with blank frozen raw fields. Runtime and Editor assemblies remain separate. One campaign catalog/settings pair is implemented; shared multi-catalog composition is future scope.

## Key and content coverage

- Keys use immutable owner IDs and field identity, never array position, current text, locale or display name. Validate uniqueness before mapping existing content; never regenerate existing line/choice IDs to translate them.
- Implemented patterns: `dialogue/{nodeId}/line/{lineId}`, `dialogue/{nodeId}/choice/{choiceId}`, `content/{ownerType}/{ownerId}/{field}`. Presentation fields use `ui/{existingKey}`. Owner type is required because legacy IDs are only unique per data type. These are patterns, not literal asset IDs.
- Milestones receive a persistent `localizationId` once; reordering must not change it. Current hints are explicitly day-indexed, so their keys use semantic `day/{dayNumber}/hint`; changing a day's content edits that day's source. Other unkeyed list content requires persistent identities before migration. Cloning creates a distinct key unless sharing is deliberate and visible.
- Inventory includes dialogue, choices, greetings/talk pools, character names/roles, moods, activities, event/room/item labels, hints, weekly topics/milestones, receipts/recaps, menus, help/error text and player-visible serialized Prefab labels. Internal Editor-only titles need no English game translation.
- Preserve existing presentation keys through a catalog-scoped adapter. Replace direct raw-text reads only in migrated scope; all migrated views, including greeting/choice/blocked states, must use the same resolver.
- Author complete sentences/templates with reorderable placeholders. Validate matching placeholder sets, rich-text tags and supported formatting; avoid joining language-dependent sentence fragments. Machine serialization/hash numbers remain culture-invariant regardless of display locale.

## Resolution and language-change behavior

Resolution order: requested locale value -> catalog Thai -> nonempty legacy source fallback during migration -> a visible missing-text marker with a diagnostic. Do not silently return blank, raw keys or an unrelated translation. An unsupported stored locale resolves to `th`. Missing/stale English is reported; fallback does not count as translated coverage. Stale English may remain visible with an authoring warning until reviewed; release validation must distinguish it from reviewed English.

Persist the explicit selection in global user settings. With no saved choice, use Thai. Loading/restarting a campaign must not reset the user's locale or choose a language from story state. Changing language refreshes text/choices/open menus and receipt labels without selecting a new node or reapplying effects. For an active typewriter line, cancel the old reveal and reveal the full newly resolved line once; do not advance or duplicate read tracking. Cache keys must include locale/revision, including currently cached greeting/line display identity. Validate formatting before display; malformed templates use a safe diagnostic/fallback, never an uncaught formatting error.

TMP bindings must refresh locale font/fallback settings through authored configuration, including persisted Prefabs. Preserve owner-authored anchors, sizes and styles. Preview Thai marks, English expansion, wrapping and readable minimum sizes in the actual affected view. Do not assume Thai font success proves other-language support.

## Authoring and translator workflow

Add **Workspace > ภาษาและคำแปล** when implementing: current-campaign/shared source scope, Thai/English columns, key/context search, source navigation, missing/stale/legacy filters, Undo and explicit Save. In migrated scope the catalog owns Thai source; the original field is retained as frozen compatibility fallback and shown read-only with its source link. Editing Thai increments its revision and makes English stale until reviewed. Prevent silent two-way synchronization.

Provide UTF-8 CSV exchange with quoted multiline cells and columns for key, Thai source, English, context, source revision and reviewed revision. Import must preview the diff and reject duplicate/unknown keys, invalid placeholders and conflicting source revisions before applying. Normal translator import changes English/review data only; it cannot rename keys, rewrite Thai or overwrite newer source edits. Apply as an Undo-capable operation, Save explicitly, and preserve Thai/English newline/rich-text data in a roundtrip check. New translations are drafts until reviewed; never fabricate English completion by copying Thai into English cells.

Detached Edit Mode preview uses the same saved Prefabs/resolver with an explicit preview locale, isolated state and no user-preference or live-save writes. Search/source links must identify table ownership; routine writing requires no C# edits or Play Mode.

## Migration and save compatibility

1. Inventory the bounded slice and capture its baseline: stable IDs/GUIDs, story graph, conditions/effects, source strings and current compatibility fingerprints. Record the source campaign and migration manifest. Do not move/delete old assets to begin adoption.
2. Add settings/catalogs/references and one compatibility adapter. Seed missing Thai entries from current source once. Reruns fill only missing entries and never overwrite artist/translator edits; conflicting duplicate content is reported.
3. Opt in only the intended version/campaign and bind its affected dialogue/UI/authoring. Preserve legacy rendering for non-opted-in fixtures. Do not translate by overwriting hashed old raw fields.
4. In the new path, separate **gameplay compatibility** from **text revision**. Gameplay fingerprint must still cover all relevant identities/branches, conditions/effects, costs, schedules and state rules; exclude resolved text, locale and translation revisions. Keep a source/text revision manifest for change reporting. Do not merely delete old guards.
5. Existing Romance v8 fingerprints include reachable line/choice strings. Define a versioned new format/namespace when the new path needs it; do not relabel or overwrite v8 files. A v8 import may proceed only with a verified migration mapping to the same gameplay baseline and normal state validation. Do not accept an arbitrary old fingerprint, infer a match from display text, clear histories or silently restart. If compatibility cannot be established, leave the original file readable by the legacy path and report the unsupported import explicitly.
6. New receipts store a stable activity/event/text reference plus numeric/typed arguments, resolving their title at display time. For old persisted raw titles, migrate only through an unambiguous source mapping; otherwise retain the legacy title as an explicit fallback and report this limit. Locale-only changes must not mutate gameplay saves.
7. Preserve line/choice/event IDs and already-read/choice/reward state across supported imports. Check translation-only updates and locale changes against the new save guard. Keep a migration manifest/report and rollback through campaign opt-out/original files without losing artist edits.

## Adoption stages and focused acceptance

| Stage | Required result | Evidence before status advances |
| --- | --- | --- |
| L0 — Designed | This contract and continuing-agent instruction | Document/reference checks only; historical Session 063 |
| L1 — Foundation | Real catalog/settings/resolver, preference and legacy adapter; usable Workspace edits | Compile; lookup/fallback/placeholder and actual Undo/Save/CSV checks |
| L2 — Partial adoption | One real Thai/English scene, greeting/choices and affected room/options/feedback UI using the new path | Switch both languages in the actual view; inspect saved-Prefab preview, typewriter/choice state and readable layout; save/load/legacy compatibility evidence |
| L3 — Declared version coverage | All player-visible text in the declared version routes through the resolver; inventory of remaining external legacy fixtures | Scoped coverage/duplicate/missing/stale report, reviewed translation status and representative affected views; no automatic whole-game-suite rerun |

Mark new functionality **Implemented** only when code/assets exist, **Verified** only against named checks, and translation/visual/content approval only with owner evidence. L2 is the first reportable real adoption; it is partial. Full routing does not imply complete English translation, and fallback screenshots do not prove bilingual completion.

## Adoption register — update in place at every adoption milestone

| Session/date | Stage | Migrated scope | Legacy/translation gaps | Verification and owner report |
| --- | --- | --- | --- | --- |
| 063 / 2026-10-26 | L0 — Designed | None; documentation contract only | Entire runtime remains legacy; English not authored/migrated | Document checks only; owner informed that adoption is pending |
| 064 / 2026-10-26 | L2 — Partial adoption | Current Romance config + reachable dialogue/choices, catalog authoring and principal UI text resolution; real translation tab/CSV/fonts/preview; separate v9 save namespace and checked v8 mapping | 96 draft English / 689 entries, 593 missing; other scene branches and custom/unbound Prefab labels remain; old receipt titles may use raw fallback; shared multi-catalog support deferred | Corrected 5/5 cases plus final affected view/window 1/1 each; bilingual renders inspected, source/data preservation checked. Live tab readability and actual player-menu/preference restart unverified. Thai adoption report delivered with Session 064 closeout; no full English approval. |

When adopting, add a row with the implementing session/version, actual campaign/scenes/UI, key/translation coverage, remaining raw paths, save/import outcome, evidence paths and Thai report status. Do not replace historical rows or pre-fill a success claim.

Required owner report fields: **adoption status**, version/campaign and migrated areas, remaining legacy areas and English gaps, checks and visual limits, save compatibility, next migration step. Put the same factual checkpoint in Task Board/Handoff; reference this register rather than duplicating the full contract.
