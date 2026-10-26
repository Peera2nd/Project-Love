# Session 024 — Team Mockup Art Generation for Grace and Ploy

- **Date:** 2026-10-24
- **Author:** Antigravity (AI Assistant)
- **Topic:** Generation of 8 team mockup character assets (Grace & Ploy) via local Stable Diffusion WebUI and rembg
- **Reference Brief:** [`Docs/03_Art_And_Audio/02_Team_Mockup_Asset_Request.md`](../../03_Art_And_Audio/02_Team_Mockup_Asset_Request.md)
- **Character Profiles:** 
  - [`Docs/02_Narrative_And_World/Characters/03_Character_Profile_Grace_Programmer.md`](../../02_Narrative_And_World/Characters/03_Character_Profile_Grace_Programmer.md)
  - [`Docs/02_Narrative_And_World/Characters/04_Character_Profile_Ploy_Designer.md`](../../02_Narrative_And_World/Characters/04_Character_Profile_Ploy_Designer.md)

---

## 1. Environment & Setup

- **Generation Endpoint:** Local SD WebUI (Forge/reForge) at `http://127.0.0.1:7860/`
- **Active Checkpoint:** `prefectIllustriousXL_40.safetensors` (Illustrious-XL / SDXL architecture)
- **Post-processing Tools:**
  - `rembg` (v2.0.69 / `u2net.onnx`) for background removal and clean alpha matting
  - Pillow (`PIL`) with `Image.Resampling.LANCZOS` for resolution and canvas placement
- **Destination Roots:**
  - Unity Runtime: `Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/` (Room and Dialogue subfolders)
  - Raw Outputs: `Art_Source/Mockup/SD_Generations/`
  - Generator Script: `Art_Source/Mockup/generate_team_mockups.py`
  - Annotated Layout Composite: `Art_Source/Mockup/ref_workspace_team_layout.png`

---

## 2. Delivered Asset Breakdown & Verification

| Asset File | Canvas | Type | Bounding Box | Visual Characteristics |
| :--- | :--- | :--- | :--- | :--- |
| `chr_grace_Seated_Working.png` | 1024 x 1024 | Room | (142, 159, 882, 944) | Isometric seated pose, typing on invisible keyboard, sharp steel-grey eyes, silver half-rim glasses, jet black blunt bob, navy shirt, pencil skirt, black sheer pantyhose, heels. |
| `chr_grace_Standing_Relaxed.png` | 1024 x 1024 | Room | (357, 76, 668, 944) | Isometric standing pose, crossed arms, calm deadpan expression, identical scale, palette and attire to seated pose. |
| `chr_grace_Normal.png` | 1500 x 2000 | Dialogue | (11, 12, 1500, 2000) | Upper-body / cowboy shot VN sprite, calm analytical expression, closed mouth, glasses and dark office blazer. Bottom-anchored. |
| `chr_grace_Smile.png` | 1500 x 2000 | Dialogue | (121, 0, 1462, 2000) | Identical framing, scale and seed to Normal, subtle warm smile, gentle softening gaze. Clean expression transition without sprite jumps. |
| `chr_ploy_Seated_Working.png` | 1024 x 1024 | Room | (172, 77, 852, 943) | Isometric seated planning pose, holding tablet/stylus, warm caramel wavy hair, side ponytail, ahoge strand, pastel crop top, denim jacket and shorts. |
| `chr_ploy_Standing_Relaxed.png` | 1024 x 1024 | Room | (350, 77, 673, 944) | Isometric standing pose, hand on hip, energetic beaming smile, matching scale and outfit with seated pose. |
| `chr_ploy_Normal.png` | 1500 x 2000 | Dialogue | (114, 0, 1385, 2000) | Upper-body VN sprite, cheerful attentive gaze, slight smile, open hazel-gold eyes, ahoge, cute side ponytail, denim jacket. Bottom-anchored. |
| `chr_ploy_Smile.png` | 1500 x 2000 | Dialogue | (97, 16, 1403, 2000) | Identical framing and seed to Normal, radiant joyful smile, cute little fang visible at left corner of mouth, blushing cheeks. |

---

## 3. Annotated Layout Guide

- Created `Art_Source/Mockup/ref_workspace_team_layout.png` (1024 x 1024), compositing `bg_workspace_01_Morning.png` with all three seated team members:
  - **Grace (Lead Programmer / Code):** Positioned near the left desk/screens area `(280, 420)`.
  - **Plaifah (2D Concept Artist / Art):** Retains center-right low table/drawing tablet area `(540, 480)`.
  - **Ploy (Game Designer / Design):** Positioned at the right sofa/coffee table planning area `(660, 580)`.

---

## 4. Verification & Guardrails

- **GR-001 (No hardcoded content):** All sprites delivered to standard asset data paths.
- **GR-006 (Preserve GUIDs/metas):** Existing files (`chr_plaifah_*`, metas, `.gitkeep`) were strictly preserved. New assets delivered with consistent ASCII filenames.
- **GR-010 (Language Policy):** Technical log in English; user presentation in Thai.
- **Integrity Test:** Python script verified dimensions, RGBA channel modes, non-empty alpha transparency and bounding box validity across all 8 PNGs.
