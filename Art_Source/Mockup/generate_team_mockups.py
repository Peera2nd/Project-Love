"""
Generate team mockup character sprites for Grace and Ploy via local SD WebUI.
Handles generation, background removal with rembg, and exact canvas fitting.
"""
import urllib.request
import json
import base64
import io
import os
import sys
from PIL import Image
import rembg

SD_URL = "http://127.0.0.1:7860/sdapi/v1/txt2img"
RAW_DIR = "Art_Source/Mockup/SD_Generations"
ROOM_DIR = "Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/Room"
DIALOGUE_DIR = "Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue"

os.makedirs(RAW_DIR, exist_ok=True)
os.makedirs(ROOM_DIR, exist_ok=True)
os.makedirs(DIALOGUE_DIR, exist_ok=True)

ITEMS = [
    # 1. Grace Seated Working (Room 1024x1024)
    {
        "id": "chr_grace_Seated_Working",
        "type": "room",
        "dest": os.path.join(ROOM_DIR, "chr_grace_Seated_Working.png"),
        "gen_w": 1024,
        "gen_h": 1024,
        "seed": 101,
        "target_w": 1024,
        "target_h": 1024,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 24 years old, clean anime aesthetic, isometric angle, full body, sitting pose, typing on invisible keyboard, focused serious expression, sharp cat-like steel-grey eyes, tsurime, silver half-rim rectangular glasses, jet black straight hair, blunt shoulder-length bob, smart casual dark navy blue collared blouse with folded sleeves, black pencil skirt, black sheer pantyhose, black high heels, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, desk, table, computer, monitor, chair, furniture, background objects, text, watermark"
    },
    # 2. Grace Standing Relaxed (Room 1024x1024)
    {
        "id": "chr_grace_Standing_Relaxed",
        "type": "room",
        "dest": os.path.join(ROOM_DIR, "chr_grace_Standing_Relaxed.png"),
        "gen_w": 1024,
        "gen_h": 1024,
        "seed": 102,
        "target_w": 1024,
        "target_h": 1024,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 24 years old, clean anime aesthetic, isometric angle, full body, standing pose, crossed arms, relaxed confident posture, calm deadpan expression, looking slightly to the side, sharp cat-like steel-grey eyes, tsurime, silver half-rim rectangular glasses, jet black straight hair, blunt shoulder-length bob, smart casual dark navy blue collared blouse with folded sleeves, black pencil skirt, black sheer pantyhose, black high heels, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, background objects, text, watermark"
    },
    # 3. Grace Normal (Dialogue 1500x2000)
    {
        "id": "chr_grace_Normal",
        "type": "dialogue",
        "dest": os.path.join(DIALOGUE_DIR, "chr_grace_Normal.png"),
        "gen_w": 896,
        "gen_h": 1152,
        "seed": 201,
        "target_w": 1500,
        "target_h": 2000,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 24 years old, visual novel sprite, cowboy shot, upper body, waist up, standing facing viewer, calm serious analytical expression, closed mouth, cool beauty, sharp cat-like steel-grey eyes, tsurime, silver half-rim rectangular glasses, jet black straight hair, blunt shoulder-length bob, dark navy blue blouse, black blazer folded at elbows, clean anime aesthetic, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, text, watermark, cropped head"
    },
    # 4. Grace Smile (Dialogue 1500x2000)
    {
        "id": "chr_grace_Smile",
        "type": "dialogue",
        "dest": os.path.join(DIALOGUE_DIR, "chr_grace_Smile.png"),
        "gen_w": 896,
        "gen_h": 1152,
        "seed": 201,  # Same seed for alignment
        "target_w": 1500,
        "target_h": 2000,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 24 years old, visual novel sprite, cowboy shot, upper body, waist up, standing facing viewer, subtle soft smile, gentle gaze, softening eyes, cool beauty, sharp steel-grey eyes, silver half-rim rectangular glasses, jet black straight hair, blunt shoulder-length bob, dark navy blue blouse, black blazer folded at elbows, clean anime aesthetic, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, text, watermark, cropped head"
    },
    # 5. Ploy Seated Working (Room 1024x1024)
    {
        "id": "chr_ploy_Seated_Working",
        "type": "room",
        "dest": os.path.join(ROOM_DIR, "chr_ploy_Seated_Working.png"),
        "gen_w": 1024,
        "gen_h": 1024,
        "seed": 301,
        "target_w": 1024,
        "target_h": 1024,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 23 years old, clean anime aesthetic, isometric angle, full body, sitting pose, holding tablet and stylus, smiling, planning, sparkling hazel-gold eyes, warm caramel brown wavy hair, side ponytail, prominent ahoge on head, pastel yellow crop top under loose blue denim jacket, denim shorts, sneakers, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, desk, table, chair, furniture, background objects, text, watermark"
    },
    # 6. Ploy Standing Relaxed (Room 1024x1024)
    {
        "id": "chr_ploy_Standing_Relaxed",
        "type": "room",
        "dest": os.path.join(ROOM_DIR, "chr_ploy_Standing_Relaxed.png"),
        "gen_w": 1024,
        "gen_h": 1024,
        "seed": 302,
        "target_w": 1024,
        "target_h": 1024,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 23 years old, clean anime aesthetic, isometric angle, full body, standing pose, hand on hip, energetic cheerful pose, beaming smile, cute little fang, looking at viewer, sparkling hazel-gold eyes, warm caramel brown wavy hair, side ponytail, prominent ahoge on head, pastel yellow crop top under loose blue denim jacket, denim shorts, white sneakers, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, background objects, text, watermark"
    },
    # 7. Ploy Normal (Dialogue 1500x2000)
    {
        "id": "chr_ploy_Normal",
        "type": "dialogue",
        "dest": os.path.join(DIALOGUE_DIR, "chr_ploy_Normal.png"),
        "gen_w": 896,
        "gen_h": 1152,
        "seed": 401,
        "target_w": 1500,
        "target_h": 2000,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 23 years old, visual novel sprite, cowboy shot, upper body, waist up, standing facing viewer, cheerful attentive expression, slight smile, sparkling hazel-gold eyes, open expressive eyes, warm caramel brown wavy hair, side ponytail, prominent ahoge on head, cute little fang, pastel yellow crop top under loose blue denim jacket, clean anime aesthetic, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, text, watermark, cropped head"
    },
    # 8. Ploy Smile (Dialogue 1500x2000)
    {
        "id": "chr_ploy_Smile",
        "type": "dialogue",
        "dest": os.path.join(DIALOGUE_DIR, "chr_ploy_Smile.png"),
        "gen_w": 896,
        "gen_h": 1152,
        "seed": 401,  # Same seed for alignment
        "target_w": 1500,
        "target_h": 2000,
        "prompt": "masterpiece, best quality, ultra-detailed, 1girl, solo, 23 years old, visual novel sprite, cowboy shot, upper body, waist up, standing facing viewer, radiant joyful smile, cute little fang visible, happy eyes, blushing cheeks, energetic, sparkling hazel-gold eyes, warm caramel brown wavy hair, side ponytail, prominent ahoge on head, pastel yellow crop top under loose blue denim jacket, clean anime aesthetic, simple background, solid white background",
        "negative": "worst quality, low quality, bad anatomy, bad hands, extra digits, missing fingers, text, watermark, cropped head"
    }
]

def generate_image(payload):
    req = urllib.request.Request(
        SD_URL,
        data=json.dumps(payload).encode('utf-8'),
        headers={'Content-Type': 'application/json'}
    )
    with urllib.request.urlopen(req, timeout=180) as resp:
        data = json.loads(resp.read().decode('utf-8'))
        img_b64 = data['images'][0]
        return Image.open(io.BytesIO(base64.b64decode(img_b64.split(',', 1)[0])))

def fit_room_canvas(cutout, target_w=1024, target_h=1024):
    bbox = cutout.getbbox()
    if not bbox:
        return cutout.resize((target_w, target_h), Image.Resampling.LANCZOS)
    cropped = cutout.crop(bbox)
    cw, ch = cropped.size
    
    # Scale character so maximum dimension fits naturally (height around 860-890)
    max_h = 880
    max_w = 750
    scale = min(max_h / ch, max_w / cw)
    nw, nh = int(cw * scale), int(ch * scale)
    resized = cropped.resize((nw, nh), Image.Resampling.LANCZOS)
    
    # Create target canvas
    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    pos_x = (target_w - nw) // 2
    # Align bottom around Y = 950
    pos_y = 950 - nh
    if pos_y < 50:
        pos_y = 50
    canvas.paste(resized, (pos_x, pos_y), resized)
    return canvas

def fit_dialogue_canvas(cutout, target_w=1500, target_h=2000):
    bbox = cutout.getbbox()
    if not bbox:
        return cutout.resize((target_w, target_h), Image.Resampling.LANCZOS)
    cropped = cutout.crop(bbox)
    cw, ch = cropped.size
    
    # Scale to fill height to 2000 or width to 1300
    target_draw_h = 2000
    scale = target_draw_h / ch
    nw, nh = int(cw * scale), int(ch * scale)
    if nw > target_w:
        scale = target_w / cw
        nw, nh = int(cw * scale), int(ch * scale)
    resized = cropped.resize((nw, nh), Image.Resampling.LANCZOS)
    
    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    pos_x = (target_w - nw) // 2
    pos_y = target_h - nh  # Bottom aligned
    canvas.paste(resized, (pos_x, pos_y), resized)
    return canvas

def main():
    print(f"Starting generation of {len(ITEMS)} assets...")
    for idx, item in enumerate(ITEMS, 1):
        item_id = item["id"]
        raw_path = os.path.join(RAW_DIR, f"{item_id}_raw.png")
        dest_path = item["dest"]
        print(f"\n[{idx}/{len(ITEMS)}] Generating {item_id}...")
        
        payload = {
            "prompt": item["prompt"],
            "negative_prompt": item["negative"],
            "sampler_name": "Euler a",
            "steps": 28,
            "cfg_scale": 6.5,
            "width": item["gen_w"],
            "height": item["gen_h"],
            "seed": item["seed"]
        }
        
        raw_img = generate_image(payload)
        raw_img.save(raw_path)
        print(f"  Raw image saved: {raw_path} ({raw_img.size})")
        
        print("  Removing background with rembg...")
        cutout = rembg.remove(raw_img)
        
        if item["type"] == "room":
            final_img = fit_room_canvas(cutout, item["target_w"], item["target_h"])
        else:
            final_img = fit_dialogue_canvas(cutout, item["target_w"], item["target_h"])
            
        final_img.save(dest_path)
        print(f"  Delivered to runtime: {dest_path} ({final_img.size}, bbox: {final_img.getbbox()})")
        
    print("\nAll 8 assets generated and delivered successfully!")

if __name__ == "__main__":
    main()
