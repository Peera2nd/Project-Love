"""
Generate Studio UI Mockup assets via Stable Diffusion and PIL processing.
Fulfills Docs/03_Art_And_Audio/03_Studio_UI_Mockup_Asset_Request.md
"""
import urllib.request
import json
import base64
import io
import os
import sys
import numpy as np
from PIL import Image, ImageDraw

SD_URL = "http://127.0.0.1:7860/sdapi/v1/txt2img"
RAW_DIR = "Art_Source/Mockup/SD_Generations/UI"
RUNTIME_UI = "Project Love/Assets/_Project/Art/UI/Mockup"

ICONS_DIR = os.path.join(RUNTIME_UI, "Icons")
PORTRAITS_DIR = os.path.join(RUNTIME_UI, "Portraits")
BARS_DIR = os.path.join(RUNTIME_UI, "Bars")
PANELS_DIR = os.path.join(RUNTIME_UI, "Panels")

os.makedirs(RAW_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)
os.makedirs(PORTRAITS_DIR, exist_ok=True)
os.makedirs(BARS_DIR, exist_ok=True)
os.makedirs(PANELS_DIR, exist_ok=True)

# ---------------------------------------------------------
# 1. SD Generation Helper
# ---------------------------------------------------------
def generate_sd_image(prompt, seed=1001, width=512, height=512):
    payload = {
        "prompt": prompt,
        "negative_prompt": "colorful, realistic, 3d, shading, gradients, photorealistic, noise, blurry, complex, text, watermark, frame, border, circle frame",
        "sampler_name": "Euler a",
        "steps": 22,
        "cfg_scale": 7.0,
        "width": width,
        "height": height,
        "seed": seed
    }
    req = urllib.request.Request(
        SD_URL,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"}
    )
    with urllib.request.urlopen(req, timeout=120) as resp:
        data = json.loads(resp.read().decode("utf-8"))
        return Image.open(io.BytesIO(base64.b64decode(data["images"][0])))

def process_icon_to_256(raw_img, safe_size=208, canvas_size=256):
    img_l = raw_img.convert("L")
    arr = np.array(img_l, dtype=np.float32)
    
    # Threshold background noise: background < 35, foreground > 180
    alpha = np.clip((arr - 35.0) / (180.0 - 35.0), 0.0, 1.0) * 255.0
    alpha = alpha.astype(np.uint8)
    
    white = np.full((*img_l.size[::-1], 4), 255, dtype=np.uint8)
    white[:, :, 3] = alpha
    result = Image.fromarray(white, mode="RGBA")
    
    bbox = result.getbbox()
    if not bbox:
        # Fallback empty canvas
        return Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
        
    cropped = result.crop(bbox)
    cw, ch = cropped.size
    scale = safe_size / max(cw, ch)
    nw, nh = int(cw * scale), int(ch * scale)
    resized = cropped.resize((nw, nh), Image.Resampling.LANCZOS)
    
    final_canvas = Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
    pos_x = (canvas_size - nw) // 2
    pos_y = (canvas_size - nh) // 2
    final_canvas.paste(resized, (pos_x, pos_y), resized)
    return final_canvas

# ---------------------------------------------------------
# Icon Definitions
# ---------------------------------------------------------
ICONS_LIST = [
    # P0 Core Icons (24)
    ("ui_heart", "minimalist flat 2d vector icon of a stylized smooth heart, bold silhouette, clean curves, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_stamina", "minimalist flat 2d vector icon of a sharp lightning bolt inside a clean energy shield outline, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_stress", "minimalist flat 2d vector icon of an overwhelmed tense emoticon face with a single big sweat drop, furrowed brows, wavy mouth, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_money", "minimalist flat 2d vector icon of a clean round coin with plain circular center, smooth rim, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_time", "minimalist flat 2d vector icon of a simple analog clock face with two hands pointing at 3 o'clock, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_bag", "minimalist flat 2d vector icon of a cute compact satchel backpack with shoulder straps, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_item", "minimalist flat 2d vector icon of a simple cube cardboard parcel box, packaging box, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_sprint_board", "minimalist flat 2d vector icon of a kanban board with three distinct vertical columns and task cards on them, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_rest", "minimalist flat 2d vector icon of a cozy soft pillow with a small crescent moon next to it, sleeping rest symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_overtime", "minimalist flat 2d vector icon of a crescent moon and a small analog clock face combined together, overtime symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_producer", "minimalist flat 2d vector icon of an office clipboard checklist with a small gear cogwheel at the bottom corner, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_progress", "minimalist flat 2d vector icon of ascending stair-step chart bars with a bold checkmark above them, progress symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_code", "minimalist flat 2d vector icon of code brackets angle brackets with slash </> programming symbol, software development, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_art", "minimalist flat 2d vector icon of an artist paintbrush with thick paint tip, diagonal brush stroke, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_design", "minimalist flat 2d vector icon of game design flowchart nodes connected by neat lines, geometric node graph diagram, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_work", "minimalist flat 2d vector icon of an open modern laptop computer with screen and keyboard, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_talk", "minimalist flat 2d vector icon of a clean rounded speech bubble, chat dialogue bubble, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_gift", "minimalist flat 2d vector icon of a square gift box with ribbon and bow on top, present box, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_locked", "minimalist flat 2d vector icon of a closed secure padlock, lock symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_blocked", "minimalist flat 2d vector icon of a hazard warning triangle with exclamation mark in the center, warning alert, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_review", "minimalist flat 2d vector icon of a magnifying glass inspecting a paper document page, code review, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_done", "minimalist flat 2d vector icon of a bold confident check mark, success tick mark, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_priority", "minimalist flat 2d vector icon of a bold upward pointing arrow above a small card, priority symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_back", "minimalist flat 2d vector icon of a curved return arrow pointing to the left, undo back arrow, monochrome white icon on pure black background, game ui icon, centered, isolated"),

    # P1 Useful Follow-up Icons (12)
    ("ui_travel", "minimalist flat 2d vector icon of an open door with an exit navigation arrow pointing through it, transition symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_settings", "minimalist flat 2d vector icon of a mechanical gear cogwheel, configuration options icon, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_save", "minimalist flat 2d vector icon of a retro floppy disk with save label, data save icon, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_load", "minimalist flat 2d vector icon of a data disk tray with an upward pointing arrow, load data icon, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_shop", "minimalist flat 2d vector icon of a shopping wire basket with handle, store market basket, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_bug", "minimalist flat 2d vector icon of a software insect bug beetle silhouette with six legs and antennae, software bug, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_dependency", "minimalist flat 2d vector icon of two circular nodes linked together by a connector line, workflow link, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_unlock", "minimalist flat 2d vector icon of an open padlock with the shackle swung open, unlocked symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_day", "minimalist flat 2d vector icon of a bright stylized sun with radiant rays, daytime symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_night", "minimalist flat 2d vector icon of a clean crescent moon with a tiny star, nighttime symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_close", "minimalist flat 2d vector icon of a bold clean X mark, close dismiss cancel symbol, monochrome white icon on pure black background, game ui icon, centered, isolated"),
    ("ui_info", "minimalist flat 2d vector icon of a lowercase letter i inside a solid circular outline, information tooltip symbol, monochrome white icon on pure black background, game ui icon, centered, isolated")
]

# ---------------------------------------------------------
# 2. Generation of Icons
# ---------------------------------------------------------
def generate_all_icons():
    print(f"\n--- Generating {len(ICONS_LIST)} UI Icons via SD ---")
    for idx, (icon_name, prompt) in enumerate(ICONS_LIST, 1):
        raw_path = os.path.join(RAW_DIR, f"{icon_name}_raw.png")
        dest_path = os.path.join(ICONS_DIR, f"{icon_name}.png")
        print(f"[{idx}/{len(ICONS_LIST)}] Generating {icon_name}...")
        
        raw_img = generate_sd_image(prompt, seed=1000 + idx)
        raw_img.save(raw_path)
        
        icon_256 = process_icon_to_256(raw_img, safe_size=208, canvas_size=256)
        icon_256.save(dest_path)
        print(f"  Delivered {icon_name}.png (256x256, bbox: {icon_256.getbbox()})")

# ---------------------------------------------------------
# 3. Generation of Task Owner Portraits (Avatars)
# ---------------------------------------------------------
def generate_owner_portraits():
    print("\n--- Generating Task Owner Portraits (256x256) ---")
    sources = {
        "ui_avatar_grace.png": "Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_grace_Normal.png",
        "ui_avatar_plaifah.png": "Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_plaifah_Normal.png",
        "ui_avatar_ploy.png": "Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_ploy_Normal.png"
    }
    
    for filename, src_path in sources.items():
        if not os.path.exists(src_path):
            print(f"  Warning: {src_path} not found, skipping.")
            continue
        im = Image.open(src_path)
        # Crop head & shoulders square region (800x800 around X=750, Y=0..800)
        crop_box = (350, 0, 1150, 800)
        cropped = im.crop(crop_box)
        resized = cropped.resize((256, 256), Image.Resampling.LANCZOS)
        
        dest_path = os.path.join(PORTRAITS_DIR, filename)
        resized.save(dest_path)
        print(f"  Delivered {filename} (256x256, bbox: {resized.getbbox()})")

# ---------------------------------------------------------
# 4. Generation of Progress Bar Sheet & Individual Cells
# ---------------------------------------------------------
def generate_progress_bar_sheet():
    print("\n--- Generating Progress Bar Sheet (1024x64) & Cells ---")
    sheet = Image.new("RGBA", (1024, 64), (0, 0, 0, 0))
    cell_w, cell_h = 256, 64
    gutter = 4
    radius = 16
    
    x0, y0 = gutter, gutter
    x1, y1 = cell_w - gutter - 1, cell_h - gutter - 1
    
    # Cell 0: Empty dark track
    cell0 = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
    d0 = ImageDraw.Draw(cell0)
    d0.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=(35, 37, 46, 240))
    
    # Cell 1: Grayscale solid fill (White for runtime tinting)
    cell1 = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
    d1 = ImageDraw.Draw(cell1)
    d1.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=(255, 255, 255, 255))
    
    # Cell 2: Light border/frame with transparent center
    cell2 = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
    d2 = ImageDraw.Draw(cell2)
    d2.rounded_rectangle([x0, y0, x1, y1], radius=radius, outline=(220, 225, 235, 255), width=2)
    
    # Cell 3: Neutral diagonal forecast pattern
    mask3 = Image.new("L", (cell_w, cell_h), 0)
    d_mask3 = ImageDraw.Draw(mask3)
    d_mask3.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=255)
    
    stripes = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
    d_stripes = ImageDraw.Draw(stripes)
    stripe_w = 12
    for offset in range(-cell_h, cell_w + cell_h, stripe_w * 2):
        d_stripes.line([(offset, cell_h), (offset + cell_h, 0)], fill=(255, 255, 255, 190), width=stripe_w)
    
    stripes.putalpha(Image.composite(stripes.split()[3], Image.new("L", (cell_w, cell_h), 0), mask3))
    cell3 = stripes
    
    # Assemble sheet
    sheet.paste(cell0, (0, 0), cell0)
    sheet.paste(cell1, (256, 0), cell1)
    sheet.paste(cell2, (512, 0), cell2)
    sheet.paste(cell3, (768, 0), cell3)
    
    sheet_dest = os.path.join(BARS_DIR, "ui_progress_bar_sheet.png")
    sheet.save(sheet_dest)
    print(f"  Delivered ui_progress_bar_sheet.png (1024x64)")
    
    # Optional individual cell exports
    cell0.save(os.path.join(BARS_DIR, "ui_progress_bar_track.png"))
    cell1.save(os.path.join(BARS_DIR, "ui_progress_bar_fill.png"))
    cell2.save(os.path.join(BARS_DIR, "ui_progress_bar_frame.png"))
    cell3.save(os.path.join(BARS_DIR, "ui_progress_bar_forecast.png"))
    print("  Delivered individual bar component cells (_track, _fill, _frame, _forecast)")

# ---------------------------------------------------------
# 5. Generation of Cards & Panels
# ---------------------------------------------------------
def generate_panels_and_cards():
    print("\n--- Generating Cards and Panels ---")
    
    # 1. ui_task_card.png (512x320, 24px 9-slice borders)
    card = Image.new("RGBA", (512, 320), (0, 0, 0, 0))
    dc = ImageDraw.Draw(card)
    dc.rounded_rectangle([4, 4, 507, 315], radius=24, fill=(28, 30, 38, 245), outline=(75, 80, 95, 255), width=2)
    card_dest = os.path.join(PANELS_DIR, "ui_task_card.png")
    card.save(card_dest)
    print("  Delivered ui_task_card.png (512x320)")
    
    # 2. ui_panel.png (512x512, 24px 9-slice borders)
    panel = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    dp = ImageDraw.Draw(panel)
    dp.rounded_rectangle([4, 4, 507, 507], radius=24, fill=(20, 22, 28, 250), outline=(85, 92, 110, 255), width=2)
    panel_dest = os.path.join(PANELS_DIR, "ui_panel.png")
    panel.save(panel_dest)
    print("  Delivered ui_panel.png (512x512)")
    
    # 3. ui_avatar_frame.png (256x256, circular frame outline, transparent center)
    aframe = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    da = ImageDraw.Draw(aframe)
    da.ellipse([12, 12, 243, 243], outline=(220, 225, 235, 255), width=6)
    aframe_dest = os.path.join(PANELS_DIR, "ui_avatar_frame.png")
    aframe.save(aframe_dest)
    print("  Delivered ui_avatar_frame.png (256x256)")
    
    # 4. ui_selection_ring.png (256x256, selection badge ring)
    sring = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    ds = ImageDraw.Draw(sring)
    ds.ellipse([10, 10, 245, 245], outline=(255, 255, 255, 255), width=8)
    sring_dest = os.path.join(PANELS_DIR, "ui_selection_ring.png")
    sring.save(sring_dest)
    print("  Delivered ui_selection_ring.png (256x256)")

# ---------------------------------------------------------
# Main Orchestrator
# ---------------------------------------------------------
def main():
    print("Starting Studio UI Mockup Asset Generation...")
    generate_all_icons()
    generate_owner_portraits()
    generate_progress_bar_sheet()
    generate_panels_and_cards()
    print("\nAll Studio UI Mockup assets generated and delivered successfully!")

if __name__ == "__main__":
    main()
