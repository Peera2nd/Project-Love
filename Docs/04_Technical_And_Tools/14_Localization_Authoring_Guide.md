# 14 — Thai/English Localization Authoring

## Available workflow — Session 064

Open **Tools > Project Love > Workspace > ภาษาและคำแปล**. The existing Romance campaign now references a real catalog/settings asset under `Assets/_Project/Data/Localization`. If Unity has not imported the saved files yet, focus Unity and use **Assets > Refresh**. No package installation or separate story graph is required.

The initial table has **689 entries, 96 draft English translations and 593 missing English translations**. Drafts are not final/owner-approved translations. Initial English examples cover the opening root/three choices, the direct-advice branch and wrap, all three base greetings, Producer routine line and principal UI labels. Other scene branches, hints, roles, room/item labels and further content can still fall back to Thai. Internal node-title entries are also visible; table counts are not a measure of finished story coverage.

## Write and translate

1. Write plot/profile/rough dialogue in `.md` as before. Markdown is not automatically imported into the game.
2. In **เขียนบท / อีเว้น**, create/edit lines and choices normally. For this opted-in campaign the Thai text editor writes into the catalog, with stable line/choice IDs and source navigation. The old raw text remains compatibility data. **เปิดคำแปลของข้อความนี้** jumps to the corresponding table row.
3. In **ภาษาและคำแปล**, search Thai, English, scene/speaker context or key. Filters expose missing English and translations requiring review. Select a row; edit the Thai/English fields, then use **บันทึกข้อความนี้ / ยังเป็นฉบับร่าง**, or explicitly confirm review against the Thai source. Editing Thai increments its source revision; English needs review again. Empty English displays Thai at runtime.
4. Use **บันทึกข้อมูล** / **บันทึกคำแปล** to persist inline edits. Undo/Redo is supported. The catalog Inspector is read-only and routes back to the Workspace so revisions are not silently bypassed. Do not modify frozen raw fields to translate content.
5. **ค้นข้อความใหม่เข้าในตาราง** discovers newly referenced content and fills missing keys without overwriting existing Thai/English edits. New lines/choices bind when authored; existing pool creation/first-line editing uses the same source path. New unreferenced drafts remain outside the published campaign inventory until connected.

The settings tab's Thai hints/mood/milestone labels and the character tab's names/roles also edit catalog sources. Mechanical numeric values, images, flags and graph links remain on existing assets. Locale fonts are editable in the translation tab; an empty locale font preserves each Prefab's authored font. Arbitrary labels embedded in new/custom Prefabs still require an explicit binding; they are not automatically translated by scanning text.

## Excel is optional

A [saved catalog CSV snapshot, 2026-10-26](../../Exports/Localization/RomanceCampaign_TextCatalog_th-en_2026-10-26.csv) is available to open directly (689 entries). It does not refresh automatically and excludes unsaved Editor edits/unregistered text. Export again from Workspace for the latest table.

Use **ส่งออก CSV** for a UTF-8 file with BOM. Excel or another spreadsheet editor can open it. Keep the key, Thai, context and revision columns intact; edit the English column. `englishReviewedRevision=0` means draft; matching `sourceRevision` records translator review, not owner story approval.

**นำเข้า CSV** shows changed-row count, sample changes and validation errors before applying. Duplicate/unknown keys, source-version conflicts, changed Thai, malformed CSV or incompatible placeholders/tags block the whole import. Applying rechecks that the catalog has not changed since preview, supports Undo and saves. Quoted multiline dialogue is preserved. `.xlsx` and `.md` import are not implemented.

## Preview and play

- **ดูตัวอย่างสองภาษา** / the ordinary preview window now has **ภาษาตัวอย่าง: ไทย / English**. Switching this detached preview does not touch live saves, preferences or game progress.
- In Play, open **เมนู** and choose **ภาษา: ไทย** / **ภาษา: อังกฤษ** (English labels when English is active). Selection persists as a global preference, independent of campaign saves. Thai is the default.
- Existing views refresh text in place. An active line is fully revealed on a language change, including the last line retained while choices are visible. Branch, mood, affection and reward state do not change just by selecting a language.

## Save compatibility and limits

Localized Romance saves use a separate **v9** namespace. Translation/source-table edits do not change the new mechanical compatibility fingerprint. The loader can read the previous v8 slot only when its fingerprint exactly matches the captured pre-migration baseline, the current frozen legacy source still matches and the current mechanical graph matches the migration baseline. It validates normal state rules and leaves the old file intact. Different historical content versions are not automatically converted.

New receipt titles use text keys. An imported old receipt without a key can retain its original raw title as an explicit legacy fallback. Do not delete old saves or remove compatibility guards to hide a mismatch.

Five focused cases cover lookup/CSV, source Undo/persistence/new-line authoring, v8/v9 save behavior, same-Prefab language rendering and Workspace repaint. Final affected preview/window rechecks pass. Thai/English choice previews were visually inspected at 1280x720. Live desktop tab readability, actual player-menu clicks/preference restart and full English coverage are not independently verified; no full Edit/Play suites or standalone build were run. [Evidence](../00_Agent_System/WorkLogs/Session_064_Verification/README.md).

Continue from [the architecture/adoption register](13_Localization_Architecture_And_Adoption_Contract.md); extend this implementation rather than creating a second translation system.
