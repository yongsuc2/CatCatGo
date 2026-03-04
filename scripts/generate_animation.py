import os
import sys
import base64
import time
import shutil
import subprocess
import argparse
import urllib.request

CHARS_BASE = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "Assets", "_Project", "Resources", "Chars",
)

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
EXTRACT_SCRIPT = os.path.join(PROJECT_ROOT, "extract_frames.ps1")

DEFAULT_BG_COLOR_HEX = "00FF00"
DEFAULT_FUZZ_PERCENT = 25

config = {
    "bg_color_hex": DEFAULT_BG_COLOR_HEX,
    "fuzz_percent": DEFAULT_FUZZ_PERCENT,
}

ANIM_SPECS = {
    "idle": {"fps": 2, "duration": 2},
    "walk": {"fps": 2, "duration": 3},
    "attack": {"fps": 2, "duration": 3},
}

SIDE_VIEW_PREFIX = (
    "Side view of {desc}, facing left, "
    "3/4 angle side view with features slightly visible, "
    "chibi 2-head-tall SD proportions, cel-shaded, thick black outlines, "
    "flat solid colors, toy-like matte appearance, minimal detail, "
    "full body, single character only, "
    "on a solid bright green (#00FF00) background, "
    "no text, no labels, clean illustration, masterpiece, best quality"
)

SIDE_VIEW_PREFIX_PLAYER = (
    "Side view of {desc}, facing right, "
    "3/4 angle side view with features slightly visible, "
    "chibi 2-head-tall SD proportions, cel-shaded, thick black outlines, "
    "flat solid colors, toy-like matte appearance, minimal detail, "
    "full body, single character only, "
    "on a solid bright green (#00FF00) background, "
    "no text, no labels, clean illustration, masterpiece, best quality"
)

VIDEO_PROMPTS = {
    "idle": (
        "Animate this exact character with a gentle idle breathing animation. "
        "Subtle rhythmic breathing - body slightly expands and contracts, small gentle bobbing. "
        "The first frame and last frame must look identical for a perfect seamless loop. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Smooth looping animation. Side view maintained. No audio, no sound, no music."
    ),
    "walk": (
        "Animate this exact character walking in place (treadmill style), facing the same direction. "
        "Arms and legs move in a natural walking rhythm. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Smooth looping animation. Side view maintained. No audio, no sound, no music."
    ),
    "attack": (
        "Animate this exact character performing a quick attack motion. "
        "Wind up, then strike forward with force, then return to ready pose. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Side view maintained. No audio, no sound, no music."
    ),
}

MONSTERS = {
    "player": {
        "desc": "a cute chibi cat character, round chubby cream colored body, big round head, small dot eyes, simple mouth, whiskers, two short stubby arms on sides, short stubby legs, pink toe beans, brown hood with cat ears, gold star brooch on chest, small tail",
    },
    "amoeba": {
        "desc": "a cute chibi amoeba blob creature, slime body, one big eye, purple-pink color",
    },
    "bacteria": {
        "desc": "a cute chibi bacteria creature, round rod shape, tiny flagella, green color, simple face, big cute eyes",
    },
    "paramecium": {
        "desc": "a cute paramecium creature, slipper shaped, cilia around body, blue-green color, simple face",
    },
    "virus": {
        "desc": "a cute virus creature, spiky sphere shape, crown-like spikes, red-orange color, menacing cute face",
    },
    "mold": {
        "desc": "a cute mold creature, fuzzy round body, mushroom-like, grey-green color, spore spots, simple face",
    },
    "elite_tardigrade": {
        "desc": "a cute tardigrade water bear, chubby 8 legs, round body, translucent blue color, armored look, simple face",
    },
    "elite_bacteriophage": {
        "desc": "a cute chibi bacteriophage, geometric head, spider-like legs, metallic purple color, robotic look, simple face",
    },
    "boss_giant_amoeba": {
        "desc": "a large menacing amoeba boss, gelatinous body with multiple nuclei, dark purple color, multiple eyes, crown-like pseudopods",
    },
    "boss_super_bacteria": {
        "desc": "a large cute chibi super bacteria boss monster, round golden armored shell body, big red cute eyes, small spikes on surface",
    },
    "boss_fungus_king": {
        "desc": "a large menacing fungus king boss, giant mushroom cap crown, thick mycelium tendrils, dark brown and toxic green color, glowing spores",
    },
    "ant": {
        "desc": "a cute chibi ant, small round dark brown body, six stubby legs, two long antennae, big round cute eyes",
    },
    "mosquito": {
        "desc": "a cute chibi mosquito character, big round bright red eyes, long pointed nose, two tiny wings, jet black round body with red stripes",
    },
    "beetle": {
        "desc": "a cute chibi beetle, dark blue shiny round shell, two short antennae, four stubby legs, big round cute eyes, small horn on head",
    },
    "caterpillar": {
        "desc": "a cute chibi caterpillar, long chubby green segmented body with 4 round segments, tiny feet on each segment, big innocent round eyes, small antennae, yellow spots",
    },
    "firefly": {
        "desc": "a cute firefly bug, bright green round body, small translucent wings, glowing yellow belly, big round eyes",
    },
    "spider": {
        "desc": "a cute chibi spider character, round dark purple body, eight short stubby black legs, two big cute shiny eyes, round abdomen, two tiny fangs",
    },
    "elite_scorpion": {
        "desc": "a cute chibi red scorpion creature, round body, two big pincers, curved tail with stinger, bright red color, big cute eyes",
    },
    "elite_mantis": {
        "desc": "a cute chibi praying mantis character, bright lime green color, large triangular head, two big round cute black eyes, two scythe-like front arms raised, round chubby body",
    },
    "boss_queen_ant": {
        "desc": "a large cute chibi red ant queen creature, big round body, golden crown on head, bright red color, six legs, big fierce eyes",
    },
    "boss_giant_spider": {
        "desc": "a large cute chibi tarantula boss monster, round dark purple fluffy body, eight thick stubby legs, six small cute yellow glowing eyes, two big fangs",
    },
    "boss_stag_beetle": {
        "desc": "a large cute chibi stag beetle boss monster, shiny dark brown metallic body, two massive antler-like horns on head, round armored shell back, big bright orange eyes",
    },
    "sparrow": {
        "desc": "a cute chibi sparrow, small brown bird, round fluffy body, orange chest, big cute black eyes, short beak, tiny tail feathers",
    },
    "mouse": {
        "desc": "a cute chibi mouse, round body, big round ears, long thin tail, grey-brown fur, pink nose, whiskers",
    },
    "frog": {
        "desc": "a cute chibi frog character, round plump green body, two large bulging eyes on top of head, bright green skin, pale yellow belly, wide happy smile",
    },
    "squirrel": {
        "desc": "a cute chibi squirrel, round fluffy body, big bushy tail, orange-brown fur, big round eyes, tiny paws holding acorn",
    },
    "hedgehog": {
        "desc": "a cute chibi hedgehog, round body covered in short brown spines, small cute face, tiny legs, big round eyes, pink nose",
    },
    "elite_fox": {
        "desc": "a cute chibi fox, fluffy big tail, pointed ears, bright orange-red fur, white chest, big cute sly eyes",
    },
    "elite_eagle": {
        "desc": "a cute chibi eagle, spread wings, white head feathers, brown body, sharp yellow beak, fierce cute eyes, talons",
    },
    "boss_wild_cat": {
        "desc": "a large wild cat boss, muscular build, striped fur pattern, fierce golden eyes, sharp claws",
    },
    "boss_hawk": {
        "desc": "a large hawk boss, spread wings, sharp talons, fierce eyes, brown and white feathers, hooked beak",
    },
    "boss_snake": {
        "desc": "a large cute chibi snake boss, bright green coiled body with darker diamond patterns, large round head, big cute menacing eyes, visible fangs, forked tongue",
    },
    "boss_bear": {
        "desc": "a large cute chibi grizzly bear boss monster, round massive chocolate brown furry body, small round ears, big fierce cute eyes, big paws with claws, red scar on cheek",
    },
    "wolf": {
        "desc": "a cute wolf, grey fur, pointed ears, bushy tail, yellow eyes, sturdy build",
    },
    "boar": {
        "desc": "a cute chibi wild boar, round stocky body, small tusks, bristly dark brown fur, flat snout, stubby legs, big cute eyes",
    },
    "deer": {
        "desc": "a cute deer, slender body, small antlers, brown spotted fur, big gentle eyes, white tail patch",
    },
    "raccoon": {
        "desc": "a cute chibi raccoon, round body, black eye mask, ringed tail, grey-brown fur, small paws",
    },
    "badger": {
        "desc": "a cute badger, stocky body, black and white face stripe, grey fur, strong claws, fierce small eyes",
    },
    "elite_leopard": {
        "desc": "a cute chibi leopard, sleek body, spotted golden fur, long tail, intense eyes, muscular build",
    },
    "elite_hyena": {
        "desc": "a cute hyena, spotted brown fur, large rounded ears, strong jaw, sloped back, laughing expression",
    },
    "boss_crocodile": {
        "desc": "a large cute chibi crocodile boss, armored scaly body, massive jaws, dark green, glowing eyes, spiked tail",
    },
    "boss_gorilla": {
        "desc": "a large cute chibi gorilla boss monster, round massive dark grey furry body, silver back, big round head, small round ears, big fierce cute red eyes",
    },
    "buffalo": {
        "desc": "a cute water buffalo, stocky body, curved horns, dark brown fur, big nose, strong legs",
    },
    "hippo": {
        "desc": "a cute chibi hippo, round massive body, wide mouth, tiny ears, grey-pink skin, stubby legs",
    },
    "rhino": {
        "desc": "a cute rhino, armored thick skin, single horn, grey body, small eyes, massive build",
    },
    "ostrich": {
        "desc": "a cute chibi ostrich, long neck, fluffy round body feathers, long legs, big cute eyes, black and white plumage, pink beak",
    },
    "zebra": {
        "desc": "a cute chibi zebra, bold black and white stripes, round chubby body, short mane, big cute eyes",
    },
    "elite_lion": {
        "desc": "a cute lion, golden fur, majestic mane, strong build, proud stance, amber eyes",
    },
    "elite_tiger": {
        "desc": "a cute tiger, orange with black stripes, powerful build, fierce green eyes, long tail",
    },
    "boss_whale": {
        "desc": "a large cute chibi whale boss, massive blue body, barnacles, ancient markings, glowing eyes, water spout",
    },
    "boss_mammoth": {
        "desc": "a large cute chibi mammoth boss, massive curved tusks, shaggy dark brown fur, big round cute eyes, thick stubby legs, small trunk",
    },
    "boss_elephant": {
        "desc": "a large menacing elephant boss, gray with big floppy ears, long trunk, small cute eyes, thick legs, golden headpiece",
    },
    "dungeon_queen_bee": {
        "desc": "a large queen bee, golden yellow with black stripes, transparent wings, small golden crown, big cute eyes, round fluffy body, stinger",
    },
    "dungeon_old_tree": {
        "desc": "a cute chibi tree spirit, small ancient tree stump with cute face, green moss and leaves on top like hair, two branch-like arms, small root-like feet, glowing green spots",
    },
    "dungeon_tiger": {
        "desc": "a large fierce tiger boss, dark orange with bold black stripes, massive build, glowing green eyes, battle scars",
    },
}


def get_client():
    try:
        import xai_sdk
    except ImportError:
        print("[ERROR] xai-sdk not installed. Run: pip install xai-sdk")
        sys.exit(1)

    api_key = os.environ.get("XAI_API_KEY")
    if not api_key:
        print("[ERROR] XAI_API_KEY environment variable not set")
        sys.exit(1)

    return xai_sdk.Client(api_key=api_key)


def image_to_data_uri(path):
    with open(path, "rb") as f:
        data = f.read()
    ext = os.path.splitext(path)[1].lower()
    mime = {"jpg": "image/jpeg", "jpeg": "image/jpeg", "png": "image/png", "webp": "image/webp"}.get(ext.lstrip("."), "image/jpeg")
    return f"data:{mime};base64,{base64.b64encode(data).decode()}"


def download_url(url, dest_path):
    print(f"  Downloading -> {dest_path}")
    req = urllib.request.Request(url, headers={"User-Agent": "CatCatGo/1.0"})
    with urllib.request.urlopen(req) as resp:
        with open(dest_path, "wb") as f:
            f.write(resp.read())



def char_dir(monster_id):
    return os.path.join(CHARS_BASE, monster_id)


def find_ref_image(monster_id):
    d = char_dir(monster_id)
    for ext in ["png", "jpg", "jpeg", "webp"]:
        path = os.path.join(d, f"ref.{ext}")
        if os.path.exists(path):
            return path
    return None


def step_image(client, monster_id):
    info = MONSTERS[monster_id]
    dest = os.path.join(char_dir(monster_id), "side.jpg")

    if os.path.exists(dest):
        print(f"[SKIP] {monster_id}/side.jpg already exists")
        return dest

    template = SIDE_VIEW_PREFIX_PLAYER if monster_id == "player" else SIDE_VIEW_PREFIX
    prompt = template.format(desc=info["desc"])

    print(f"[IMAGE] Generating side-view for {monster_id}...")

    ref_path = find_ref_image(monster_id)
    kwargs = {
        "prompt": prompt,
        "model": "grok-imagine-image",
        "aspect_ratio": "3:4",
        "resolution": "1k",
    }
    if ref_path:
        print(f"  Using ref image: {ref_path}")
        kwargs["image_url"] = image_to_data_uri(ref_path)

    try:
        response = client.image.sample(**kwargs)
    except Exception as e:
        print(f"  [ERROR] Image generation failed: {e}")
        return None
    download_url(response.url, dest)
    print(f"  Saved: {dest}")
    return dest


def step_video(client, monster_id, anim_type):
    dest = os.path.join(char_dir(monster_id), f"{anim_type}.mp4")

    if os.path.exists(dest):
        print(f"[SKIP] {monster_id}/{anim_type}.mp4 already exists")
        return dest

    side_path = os.path.join(char_dir(monster_id), "side.jpg")
    if not os.path.exists(side_path):
        print(f"[ERROR] {monster_id}/side.jpg not found. Run --step image first.")
        return None

    spec = ANIM_SPECS[anim_type]
    prompt = VIDEO_PROMPTS[anim_type]

    print(f"[VIDEO] Generating {anim_type} for {monster_id} (duration={spec['duration']}s)...")

    try:
        response = client.video.generate(
            prompt=prompt,
            model="grok-imagine-video",
            duration=spec["duration"],
            aspect_ratio="9:16",
            resolution="480p",
            image_url=image_to_data_uri(side_path),
        )
    except Exception as e:
        print(f"  [ERROR] Video generation failed: {e}")
        return None

    if hasattr(response, "request_id"):
        print(f"  request_id: {response.request_id}")
        print("  Polling for completion...")
        while True:
            result = client.video.get(response.request_id)
            status = result.status if hasattr(result, "status") else "unknown"
            if status == "done":
                download_url(result.video.url, dest)
                print(f"  Saved: {dest}")
                return dest
            elif status == "expired":
                print(f"  [ERROR] Video generation expired for {monster_id}/{anim_type}")
                return None
            print(f"  Status: {status}, waiting 10s...")
            time.sleep(10)
    else:
        download_url(response.video.url if hasattr(response, "video") else response.url, dest)
        print(f"  Saved: {dest}")
        return dest


def step_frames(monster_id, anim_type):
    video_path = os.path.join(char_dir(monster_id), f"{anim_type}.mp4")
    if not os.path.exists(video_path):
        print(f"[ERROR] {monster_id}/{anim_type}.mp4 not found. Run --step video first.")
        return False

    dest_dir = os.path.join(char_dir(monster_id), anim_type)
    if os.path.isdir(dest_dir) and len(os.listdir(dest_dir)) > 0:
        print(f"[SKIP] {monster_id}/{anim_type}/ already has frames")
        return True

    spec = ANIM_SPECS[anim_type]

    print(f"[FRAMES] Extracting {anim_type} frames for {monster_id} (fps={spec['fps']})...")

    cmd = [
        "powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass",
        "-File", EXTRACT_SCRIPT,
        "-InputVideo", video_path,
        "-Fps", str(spec["fps"]),
        "-BgColorHex", config["bg_color_hex"],
        "-FuzzPercent", str(config["fuzz_percent"]),
    ]

    result = subprocess.run(cmd, capture_output=True, text=True, cwd=PROJECT_ROOT)
    if result.returncode != 0:
        print(f"  [ERROR] extract_frames.ps1 failed:\n{result.stderr}")
        return False

    base_name = os.path.splitext(os.path.basename(video_path))[0]
    alpha_dir = os.path.join(PROJECT_ROOT, f"{base_name}_frames_alpha")

    if not os.path.isdir(alpha_dir):
        print(f"  [ERROR] Alpha frames directory not found: {alpha_dir}")
        return False

    return step_deploy(monster_id, anim_type, alpha_dir)


def step_deploy(monster_id, anim_type, source_dir=None):
    dest_dir = os.path.join(char_dir(monster_id), anim_type)
    os.makedirs(dest_dir, exist_ok=True)

    if source_dir is None:
        video_path = os.path.join(char_dir(monster_id), f"{anim_type}.mp4")
        base_name = os.path.splitext(os.path.basename(video_path))[0]
        source_dir = os.path.join(PROJECT_ROOT, f"{base_name}_frames_alpha")

    if not os.path.isdir(source_dir):
        print(f"[ERROR] Source directory not found: {source_dir}")
        return False

    frames = sorted(
        f for f in os.listdir(source_dir) if f.endswith(".png")
    )

    if not frames:
        print(f"[ERROR] No PNG frames in {source_dir}")
        return False

    for i, fname in enumerate(frames, start=1):
        src = os.path.join(source_dir, fname)
        dst = os.path.join(dest_dir, f"frame_{i:04d}.png")
        shutil.copy2(src, dst)

    print(f"  Deployed {len(frames)} frames -> {dest_dir}")

    raw_dir = source_dir.replace("_frames_alpha", "_frames")
    for d in [source_dir, raw_dir]:
        if os.path.isdir(d):
            shutil.rmtree(d)
            print(f"  Cleaned: {d}")

    return True


def get_status(monster_id):
    d = char_dir(monster_id)
    status = {}
    status["ref"] = "O" if find_ref_image(monster_id) else "X"
    status["side"] = "O" if os.path.exists(os.path.join(d, "side.jpg")) else "X"
    for anim_type in ANIM_SPECS:
        has_video = os.path.exists(os.path.join(d, f"{anim_type}.mp4"))
        anim_dir = os.path.join(d, anim_type)
        has_frames = os.path.isdir(anim_dir) and len(os.listdir(anim_dir)) > 0
        frame_count = len(os.listdir(anim_dir)) if has_frames else 0
        if has_frames:
            status[anim_type] = f"{frame_count}f"
        elif has_video:
            status[anim_type] = "mp4"
        else:
            status[anim_type] = "X"
    return status


def cmd_list():
    print(f"{'ID':<25} {'ref':>4} {'side':>5} {'idle':>6} {'walk':>6} {'attack':>7}")
    print("-" * 60)
    for mid in sorted(MONSTERS.keys()):
        s = get_status(mid)
        print(f"{mid:<25} {s['ref']:>4} {s['side']:>5} {s['idle']:>6} {s['walk']:>6} {s['attack']:>7}")


def cmd_generate(monster_id, step=None):
    if monster_id not in MONSTERS:
        print(f"[ERROR] Unknown monster: {monster_id}")
        print(f"Available: {', '.join(sorted(MONSTERS.keys()))}")
        sys.exit(1)

    os.makedirs(char_dir(monster_id), exist_ok=True)
    client = None

    if step is None or step == "image":
        client = client or get_client()
        result = step_image(client, monster_id)
        if result is None and step is None:
            print("[ABORT] Image generation failed, stopping pipeline.")
            return
        if step == "image":
            return

    if step is None or step == "video":
        client = client or get_client()
        failed = False
        for anim_type in ANIM_SPECS:
            if step_video(client, monster_id, anim_type) is None:
                failed = True
        if failed and step is None:
            print("[ABORT] Video generation failed, stopping pipeline.")
            return
        if step == "video":
            return

    if step is None or step == "frames":
        for anim_type in ANIM_SPECS:
            step_frames(monster_id, anim_type)
        if step == "frames":
            return

    if step == "deploy":
        for anim_type in ANIM_SPECS:
            step_deploy(monster_id, anim_type)


def main():
    parser = argparse.ArgumentParser(description="CatCatGo Animation Pipeline")
    parser.add_argument("monster_id", nargs="?", help="Character ID to process")
    parser.add_argument("--step", choices=["image", "video", "frames", "deploy"],
                        help="Run specific step only")
    parser.add_argument("--list", action="store_true", help="Show all characters status")
    parser.add_argument("--fuzz", type=int, default=DEFAULT_FUZZ_PERCENT,
                        help=f"Fuzz percent for background removal (default: {DEFAULT_FUZZ_PERCENT})")
    parser.add_argument("--bg", default=DEFAULT_BG_COLOR_HEX,
                        help=f"Background color hex (default: {DEFAULT_BG_COLOR_HEX})")

    args = parser.parse_args()

    config["fuzz_percent"] = args.fuzz
    config["bg_color_hex"] = args.bg

    if args.list:
        cmd_list()
        return

    if not args.monster_id:
        parser.print_help()
        return

    cmd_generate(args.monster_id, args.step)


if __name__ == "__main__":
    main()
