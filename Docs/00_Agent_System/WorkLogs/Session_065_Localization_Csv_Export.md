# Session 065 — 2026-10-26 — Saved localization catalog CSV export

Owner asks whether all game text can be viewed as CSV. Exported the entire saved current Romance localization catalog to [the Thai/English CSV](../../../Exports/Localization/RomanceCampaign_TextCatalog_th-en_2026-10-26.csv), using exactly the existing game's six-column exchange schema: key, thai, english, context, sourceRevision and englishReviewedRevision.

The snapshot has 689 entries: 96 nonempty draft English values and 593 missing English values. It includes registered dialogue/choices and content/UI labels, not every unbound custom Prefab literal or historical prototype. Unsaved Editor edits are not part of an on-disk export. Use Workspace > ภาษาและคำแปล > ส่งออก CSV for future saved/current exports; this file does not automatically refresh or write back to the game.

Verified all six columns and 689 rows by exact read-back comparison, UTF-8 BOM, 18 multiline rows and unchanged source SHA-256. [Export verification](../../../Exports/Localization/Export_Check_2026-10-26.json). No code, Unity assets, saves, translation values or adoption stage changed; no new incident or Unity rerun. Session 064 remains L2 partial with its existing verification/translation/live-UI limits. Source-data export only, no Excel workbook created.
