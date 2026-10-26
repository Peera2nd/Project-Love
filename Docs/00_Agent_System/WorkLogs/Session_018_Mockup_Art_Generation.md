# Session 018 — Mockup Art Generation via Stable Diffusion

- **Date:** 2026-10-24
- **Author:** Antigravity (AI Assistant)
- **Topic:** Local Stable Diffusion asset generation for isometric workspace and VN mockup package
- **Reference Brief:** [`Docs/03_Art_And_Audio/01_Workspace_Mockup_Asset_Brief.md`](../../03_Art_And_Audio/01_Workspace_Mockup_Asset_Brief.md)
- **Character Profile:** [`Docs/02_Narrative_And_World/Characters/02_Character_Profile_Plaifah_Artist.md`](../../02_Narrative_And_World/Characters/02_Character_Profile_Plaifah_Artist.md)

---

## 1. Environment & Setup

- **Generation Endpoint:** Local SD WebUI (Forge/reForge) at `http://127.0.0.1:7860/`
- **Active Checkpoint:** `prefectIllustriousXL_40.safetensors` (Illustrious-XL / SDXL architecture)
- **Post-processing Tools:**
  - `rembg` (v2.0.69 / `u2net.onnx`) for alpha matting and background removal
  - Pillow (`PIL`) with `Image.Resampling.LANCZOS` for clean resolution fitting
- **Destination Roots:**
  - Unity Runtime: `Project Love/Assets/_Project/Art/`
  - Raw / Working Source: `Art_Source/Mockup/SD_Generations/`

---

## 2. Generated Asset Breakdown & Results

### 1. `bg_workspace_Morning.png`
- **Path:** `Assets/_Project/Art/Backgrounds/Mockup/bg_workspace_Morning.png`
- **Canvas:** 1920 x 1080 (generated at 1344x768, upscaled with Lanczos)
- **Prompt:** `masterpiece, best quality, ultra-detailed, isometric, isometric view, game studio interior, cozy indie game development office, modern desks, multi-monitor setups, drawing tablet, ergonomic chairs, cozy sofa resting corner with coffee table and cushions, bookshelf, potted plants, large window, soft morning sunlight, bright clean anime aesthetic, empty room, no humans`
- **Outcome:** Clean isometric anime room composition. Features a large monitor work desk on the left, low table with drawing tablet on a blue rug, and an orange sofa resting area on the right with soft morning sunlight streaming in through the window. Perfectly accommodates phase transitions between working at the table/desk and resting on the sofa.

### 2. `bg_convenience_store_Morning.png`
- **Path:** `Assets/_Project/Art/Backgrounds/Mockup/bg_convenience_store_Morning.png`
- **Canvas:** 1920 x 1080 (generated at 1344x768, upscaled with Lanczos)
- **Prompt:** `masterpiece, best quality, ultra-detailed, isometric view, full room view of cozy japanese convenience store interior, checkout counter with cash register on the right, aisles of shelves stocked with snacks and goods, refrigerated beverage display in back, clean white tiled floor, morning sunlight, anime background art, empty shop, no people, no humans`
- **Outcome:** Wide isometric interior of a Japanese convenience store. Aisles of snack shelves flank the sides, with a central counter for checkout and beverage refrigerators in the rear. Suitable for testing travel and clerk shopping interactions.

### 3. `chr_plaifah_Seated_Working.png`
- **Path:** `Assets/_Project/Art/Characters/Sprites/Mockup/Room/chr_plaifah_Seated_Working.png`
- **Canvas:** 1024 x 1024 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, 1girl, solo, plaifah, 23 years old, clean anime aesthetic, isometric angle, full body, sitting pose, drawing on stylus tablet in lap, focused gentle expression, shoulder-length charcoal black hair, see-through bangs, warm amber-brown eyes, thin round rose-gold glasses, oversized cream knit cardigan over white collared shirt, long olive green skirt, brown shoes, white socks, simple background, white background`
- **Outcome:** Transparent cutout of Plaifah sitting in an isometric perspective drawing on a tablet in her lap. Clothing, amber eyes, round spectacles, and hair match profile specs. Position sits nicely on the workspace rug/low table.

### 4. `chr_plaifah_Seated_Phone.png`
- **Path:** `Assets/_Project/Art/Characters/Sprites/Mockup/Room/chr_plaifah_Seated_Phone.png`
- **Canvas:** 1024 x 1024 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, 1girl, solo, plaifah, 23 years old, clean anime aesthetic, isometric angle, full body, relaxed sitting pose, holding smartphone with both hands, looking down at phone screen, soft calm smile, shoulder-length charcoal black hair, see-through bangs, warm amber-brown eyes, thin round rose-gold glasses, oversized cream knit cardigan over white collared shirt, long olive green skirt, brown shoes, white socks, simple background, white background`
- **Outcome:** Transparent cutout of Plaifah in a relaxed sitting pose looking down at her phone, smiling gently. Shares consistent character scale and outfit palette with the working pose. Suitable for sofa placement during afternoon/rest phases.

### 5. `chr_plaifah_Normal.png`
- **Path:** `Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_plaifah_Normal.png`
- **Canvas:** 1500 x 2000 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, 1girl, solo, plaifah, 23 years old, visual novel sprite, upper body, cowboy shot, calm gentle smile, closed mouth, peaceful warm amber-brown eyes, tareme eyes, subtle beauty mark under right eye, wearing thin round rose-gold glasses, shoulder-length charcoal black hair, gentle inward curl bob, see-through bangs, delicate features, soft blush, oversized cream knit cardigan over crisp white collared blouse, holding small brown sketchbook, clean anime aesthetic, simple background, white background`
- **Outcome:** High-resolution waist/thigh-up VN standing dialogue sprite. Gentle smile, amber eyes, beauty mark under right eye, wearing round spectacles, holding a sketchbook. Top-left generator artifact was cleanly erased.

### 6. `chr_plaifah_Smile.png`
- **Path:** `Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_plaifah_Smile.png`
- **Canvas:** 1500 x 2000 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, 1girl, solo, plaifah, 23 years old, visual novel sprite, upper body, cowboy shot, radiant happy smile, joyful closed-eyes smile, blushing cheeks, wearing thin round rose-gold glasses, shoulder-length charcoal black hair, gentle inward curl bob, see-through bangs, delicate features, subtle beauty mark under right eye, oversized cream knit cardigan over crisp white collared blouse, holding small brown sketchbook, clean anime aesthetic, simple background, white background`
- **Outcome:** Expressions swap for Plaifah with matching body position/framing. Shows a beaming closed-eyes smile (`^_^`), flushed cheeks, holding her sketchbook open to a cute dessert sketch.

### 7. `chr_shop_clerk_Standing_Normal.png`
- **Path:** `Assets/_Project/Art/Characters/Sprites/Mockup/Room/chr_shop_clerk_Standing_Normal.png`
- **Canvas:** 1024 x 1024 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, 1girl, solo, convenience store clerk, standing, full body, welcoming pose, hands in front, anime style, neat short hair, apron over white shirt, pants, shoes, isolated, solid white background, simple background`
- **Outcome:** Clean standalone full-body sprite of a convenience store clerk in apron and slacks, standing straight with hands politely clasped in front, centered on canvas with bottom contact point. Free of extraneous background kiosk/shelf elements.

### 8. `itm_water_bottle.png`
- **Path:** `Assets/_Project/Art/Items/Mockup/itm_water_bottle.png`
- **Canvas:** 256 x 256 (transparent PNG)
- **Prompt:** `masterpiece, best quality, ultra-detailed, single clear plastic mineral water bottle, blue cap, simple clean minimal label, pure fresh mineral water, refreshing droplets, game icon, isolated, centered, clean anime aesthetic, simple background, white background`
- **Outcome:** Crisp 256x256 transparent item icon of a clear mineral water bottle with blue cap and condensation droplets. Reusable as a dropped floor item, shop stock product, and inventory icon.

---

## 3. Verification & Compliance

- **GR-001 (No hardcoded content):** Sprites are placed in standard data folders; names and paths follow canonical data architecture.
- **GR-006 (Preserve GUIDs/metas):** All parent folder `.meta` files and existing assets remained untouched.
- **GR-010 (Language Policy):** Work log documented in concise English; user reporting rendered in Thai.
- **File Integrity:** All 8 PNGs verified for size, RGBA mode, bounding boxes, and visual presentation.
