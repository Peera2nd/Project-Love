# Session 028 — Studio UI Mockup Art Generation

- **Date:** 2026-10-24
- **Author:** Antigravity (AI Assistant)
- **Topic:** Generation and delivery of Studio UI mockup assets (36 icons, 3 portraits, progress bar sheet, 4 panels/cards) via local Stable Diffusion WebUI and PIL
- **Reference Brief:** [`Docs/03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md`](../../03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md)
- **Destination Root:** `Project Love/Assets/_Project/Art/UI/Mockup/`

---

## 1. Environment & Setup

- **Generation Endpoint:** Local SD WebUI (Forge/reForge) at `http://127.0.0.1:7860/`
- **Active Checkpoint:** `prefectIllustriousXL_40.safetensors` (Illustrious-XL / SDXL architecture)
- **Post-processing & Compositing Tools:**
  - Python PIL/Pillow with `LANCZOS` resampling, numpy alpha mask thresholding, and geometric antialiasing
- **Destination Folders:**
  - `Icons/`: 36 icons (256×256 RGBA, pure white silhouette, 208×208 safe region, 24 px outer margin)
  - `Portraits/`: 3 task owner portraits (256×256 RGBA, transparent background, head/shoulders)
  - `Bars/`: 1 reusable progress bar master sheet (1024×64 RGBA) + 4 individual component cells (256×64 each)
  - `Panels/`: 4 cards and frames (Task Card 512×320, Modal Panel 512×512, Avatar Frame 256×256, Selection Ring 256×256)
- **Source / Working Files:**
  - Raw SD outputs: `Art_Source/Mockup/SD_Generations/UI/`
  - Generator script: `Art_Source/Mockup/generate_ui_mockups.py`
  - Visual overview composite: `Art_Source/Mockup/UI/ref_ui_mockup_overview.png` (1920×1080)

---

## 2. Delivered Asset Breakdown

### Icons (`Art/UI/Mockup/Icons/`, 36 files, all 256×256 RGBA)
- **P0 Core Icons (24):**
  - Affection / Status: `ui_heart.png` (affection), `ui_stamina.png` (player energy), `ui_stress.png` (worker Stress)
  - Resources / Time: `ui_money.png` (coin), `ui_time.png` (clock), `ui_rest.png` (pillow with moon), `ui_overtime.png` (moon + clock)
  - Items / Board: `ui_bag.png` (satchel), `ui_item.png` (box), `ui_sprint_board.png` (three-column board)
  - Disciplines / Work: `ui_code.png` (`</>`), `ui_art.png` (paintbrush), `ui_design.png` (flowchart nodes), `ui_producer.png` (clipboard + gear), `ui_progress.png` (ascending bars + check)
  - Actions: `ui_work.png` (laptop), `ui_talk.png` (speech bubble), `ui_gift.png` (wrapped present)
  - Task States / Controls: `ui_locked.png` (closed padlock), `ui_blocked.png` (warning triangle), `ui_review.png` (magnifier + page), `ui_done.png` (check mark), `ui_priority.png` (up arrow), `ui_back.png` (return arrow)
- **P1 Useful Follow-up Icons (12):**
  - `ui_travel.png` (door + arrow), `ui_settings.png` (gear), `ui_save.png` (diskette), `ui_load.png` (disk + up arrow), `ui_shop.png` (basket), `ui_bug.png` (software beetle), `ui_dependency.png` (linked nodes), `ui_unlock.png` (open padlock), `ui_day.png` (sun), `ui_night.png` (moon), `ui_close.png` (X mark), `ui_info.png` (info circle)

### Task Owner Portraits (`Art/UI/Mockup/Portraits/`, 3 files, all 256×256 RGBA)
- `ui_avatar_grace.png`: Grace head & shoulders, dark hair, rectangular silver glasses.
- `ui_avatar_plaifah.png`: Plaifah head & shoulders, bob hair, round glasses.
- `ui_avatar_ploy.png`: Ploy head & shoulders, wavy caramel side ponytail, cheerful gaze.
- Extracted and scaled directly from canonical Normal VN sprites, ensuring 100% exact character identity match without baked frames.

### Progress Bar Sheet (`Art/UI/Mockup/Bars/`, 5 files)
- `ui_progress_bar_sheet.png`: 1024×64 RGBA master sheet, four 256×64 cells:
  - Cell 0 (0..255): Dark track `(35, 37, 46, 240)`
  - Cell 1 (256..511): Solid white fill `(255, 255, 255, 255)` for runtime UI shader tinting
  - Cell 2 (512..767): Light 2 px border frame `(220, 225, 235, 255)` with transparent center
  - Cell 3 (768..1023): Neutral diagonal forecast stripes on transparent background
- Individual cells also exported: `ui_progress_bar_track.png`, `ui_progress_bar_fill.png`, `ui_progress_bar_frame.png`, `ui_progress_bar_forecast.png`.

### Panels and Cards (`Art/UI/Mockup/Panels/`, 4 files)
- `ui_task_card.png`: 512×320, 24 px 9-slice borders, neutral rounded card background.
- `ui_panel.png`: 512×512, 24 px 9-slice borders, modal / section background.
- `ui_avatar_frame.png`: 256×256, outline ring for owner avatars.
- `ui_selection_ring.png`: 256×256, active selection / OT / rest badge ring.

---

## 3. Verification & Compliance

- **GR-001 (No hardcoded content):** All graphics are grayscale/neutral PNGs; discipline colors, role stripes, and labels are driven by data/shader tint.
- **GR-006 (Preserve GUIDs/metas):** Existing folder metas and unrelated assets remained completely untouched.
- **GR-010 (Language Policy):** Technical docs in English; in-game labels remain Thai.
- **Integrity Test:** Python automated test verified all 48 delivered files:
  - Exact canvas dimensions matched specifications.
  - RGBA mode confirmed for all files.
  - Non-empty bounding boxes and active alpha channels verified.
  - 0 failures across 48 assets.
