import os
import sys
import base64
import time
import shutil
import subprocess
import argparse
import urllib.request

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CHARS_BASE = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Resources", "Chars")
PLAYER_DIR = os.path.join(CHARS_BASE, "player")
EXTRACT_SCRIPT = os.path.join(PROJECT_ROOT, "extract_frames.ps1")

FRAME_SIZE = "480x848"
EXTRACT_FPS = 4
VIDEO_DURATION = 6
VIDEO_RESOLUTION = "480p"
VIDEO_ASPECT = "9:16"
BG_COLOR_HEX = "00FF00"
FUZZ_PERCENT = 25

PLAYER_DESC = (
    "a cute chibi cat character, round chubby cream colored body, "
    "big round head, small dot eyes, simple mouth, whiskers, "
    "two short stubby arms on sides, short stubby legs, pink toe beans, "
    "brown hood with cat ears, gold star brooch on chest, small tail"
)

SIDE_VIEW_PROMPT = (
    f"Side view of {PLAYER_DESC}, facing right, "
    "3/4 angle side view with features slightly visible, "
    "chibi 2-head-tall SD proportions, cel-shaded, thick black outlines, "
    "flat solid colors, toy-like matte appearance, minimal detail, "
    "full body, single character only, "
    "on a solid bright green (#00FF00) background, "
    "no text, no labels, clean illustration, masterpiece, best quality"
)

VIDEO_PROMPTS = {
    "idle": (
        "Animate this exact character with a gentle idle breathing animation, facing right. "
        "Subtle rhythmic breathing - body slightly expands and contracts, small gentle bobbing. "
        "The first frame and last frame must look identical for a perfect seamless loop. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Smooth looping animation. Side view maintained. No audio, no sound, no music."
    ),
    "walk": (
        "Animate this exact character walking in place (treadmill style), facing right. "
        "Arms and legs move in a natural walking rhythm. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Smooth looping animation. Side view maintained. No audio, no sound, no music."
    ),
    "attack": (
        "Animate this exact character performing a quick attack motion toward the right. "
        "Wind up, then strike forward with force, then return to ready pose. "
        "Keep the character exactly identical to the reference image in every detail. "
        "Maintain the solid bright green background throughout. "
        "Side view maintained. No audio, no sound, no music."
    ),
}

ANIM_TYPES = ["idle", "walk", "attack"]


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
    ext = os.path.splitext(path)[1].lower().lstrip(".")
    mime = {"jpg": "image/jpeg", "jpeg": "image/jpeg", "png": "image/png", "webp": "image/webp"}.get(ext, "image/jpeg")
    return f"data:{mime};base64,{base64.b64encode(data).decode()}"


def download_url(url, dest_path):
    print(f"  Downloading -> {dest_path}")
    req = urllib.request.Request(url, headers={"User-Agent": "CatCatGo/1.0"})
    with urllib.request.urlopen(req) as resp:
        with open(dest_path, "wb") as f:
            f.write(resp.read())


def step_image(client):
    dest = os.path.join(PLAYER_DIR, "side.jpg")

    if os.path.exists(dest):
        print(f"[SKIP] player/side.jpg already exists")
        return dest

    print("[IMAGE] Generating player side-view (facing right)...")

    kwargs = {
        "prompt": SIDE_VIEW_PROMPT,
        "model": "grok-imagine-image",
        "aspect_ratio": "3:4",
        "resolution": "1k",
    }

    ref_path = os.path.join(PLAYER_DIR, "ref.jpg")
    if os.path.exists(ref_path):
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


def step_video(client, anim_type):
    dest = os.path.join(PLAYER_DIR, f"{anim_type}.mp4")

    if os.path.exists(dest):
        print(f"[SKIP] player/{anim_type}.mp4 already exists")
        return dest

    side_path = os.path.join(PLAYER_DIR, "side.jpg")
    if not os.path.exists(side_path):
        print("[ERROR] player/side.jpg not found. Run --step image first.")
        return None

    print(f"[VIDEO] Generating player {anim_type} ({VIDEO_DURATION}s, {VIDEO_RESOLUTION})...")

    try:
        response = client.video.generate(
            prompt=VIDEO_PROMPTS[anim_type],
            model="grok-imagine-video",
            duration=VIDEO_DURATION,
            aspect_ratio=VIDEO_ASPECT,
            resolution=VIDEO_RESOLUTION,
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
                print(f"  [ERROR] Video generation expired")
                return None
            print(f"  Status: {status}, waiting 10s...")
            time.sleep(10)
    else:
        url = response.video.url if hasattr(response, "video") else response.url
        download_url(url, dest)
        print(f"  Saved: {dest}")
        return dest


def step_frames(anim_type):
    video_path = os.path.join(PLAYER_DIR, f"{anim_type}.mp4")
    if not os.path.exists(video_path):
        print(f"[ERROR] player/{anim_type}.mp4 not found. Run --step video first.")
        return False

    dest_dir = os.path.join(PLAYER_DIR, anim_type)
    if os.path.isdir(dest_dir) and len(os.listdir(dest_dir)) > 0:
        print(f"[SKIP] player/{anim_type}/ already has frames")
        return True

    print(f"[FRAMES] Extracting player {anim_type} frames (fps={EXTRACT_FPS})...")

    cmd = [
        "powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass",
        "-File", EXTRACT_SCRIPT,
        "-InputVideo", video_path,
        "-Fps", str(EXTRACT_FPS),
        "-BgColorHex", BG_COLOR_HEX,
        "-FuzzPercent", str(FUZZ_PERCENT),
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

    os.makedirs(dest_dir, exist_ok=True)
    frames = sorted(f for f in os.listdir(alpha_dir) if f.endswith(".png"))

    if not frames:
        print(f"  [ERROR] No PNG frames in {alpha_dir}")
        return False

    for i, fname in enumerate(frames, start=1):
        src = os.path.join(alpha_dir, fname)
        dst = os.path.join(dest_dir, f"frame_{i:04d}.png")
        shutil.copy2(src, dst)

    print(f"  Deployed {len(frames)} frames -> {dest_dir}")

    raw_dir = os.path.join(PROJECT_ROOT, f"{base_name}_frames")
    for d in [alpha_dir, raw_dir]:
        if os.path.isdir(d):
            shutil.rmtree(d)
            print(f"  Cleaned: {d}")

    return True


def step_resize(anim_type):
    dest_dir = os.path.join(PLAYER_DIR, anim_type)
    if not os.path.isdir(dest_dir):
        print(f"[ERROR] player/{anim_type}/ not found. Run --step frames first.")
        return False

    frames = sorted(f for f in os.listdir(dest_dir) if f.endswith(".png"))
    if not frames:
        print(f"[ERROR] No frames in player/{anim_type}/")
        return False

    first_frame = os.path.join(dest_dir, frames[0])
    result = subprocess.run(
        ["magick", "identify", "-format", "%wx%h", first_frame],
        capture_output=True, text=True,
    )
    current_size = result.stdout.strip()

    if current_size == FRAME_SIZE:
        print(f"[SKIP] player/{anim_type}/ already {FRAME_SIZE}")
        return True

    print(f"[RESIZE] player/{anim_type}: {current_size} -> {FRAME_SIZE} ({len(frames)} frames)")

    for fname in frames:
        path = os.path.join(dest_dir, fname)
        subprocess.run(
            ["magick", path, "-resize", f"{FRAME_SIZE}!", path],
            capture_output=True, text=True,
        )

    print(f"  Resized {len(frames)} frames")
    return True


def show_status():
    print("\n=== Player Animation Status ===\n")
    side = os.path.exists(os.path.join(PLAYER_DIR, "side.jpg"))
    ref = os.path.exists(os.path.join(PLAYER_DIR, "ref.jpg"))
    print(f"  ref.jpg:  {'O' if ref else 'X'}")
    print(f"  side.jpg: {'O' if side else 'X'}")

    for anim_type in ANIM_TYPES:
        has_video = os.path.exists(os.path.join(PLAYER_DIR, f"{anim_type}.mp4"))
        anim_dir = os.path.join(PLAYER_DIR, anim_type)
        has_frames = os.path.isdir(anim_dir) and len(os.listdir(anim_dir)) > 0
        frame_count = len(os.listdir(anim_dir)) if has_frames else 0

        if has_frames:
            first = os.path.join(anim_dir, sorted(os.listdir(anim_dir))[0])
            res = subprocess.run(
                ["magick", "identify", "-format", "%wx%h", first],
                capture_output=True, text=True,
            )
            size = res.stdout.strip() if res.returncode == 0 else "?"
            status = f"{frame_count} frames ({size})"
        elif has_video:
            status = "mp4 only"
        else:
            status = "X"

        print(f"  {anim_type:>7}: {status}")
    print()


def main():
    parser = argparse.ArgumentParser(description="CatCatGo Player Animation Pipeline")
    parser.add_argument("--step", choices=["image", "video", "frames", "resize"],
                        help="Run specific step only")
    parser.add_argument("--type", choices=ANIM_TYPES,
                        help="Process specific animation type only (default: all)")
    parser.add_argument("--force", action="store_true",
                        help="Overwrite existing files")
    parser.add_argument("--status", action="store_true",
                        help="Show current status")

    args = parser.parse_args()

    os.makedirs(PLAYER_DIR, exist_ok=True)

    if args.status:
        show_status()
        return

    types = [args.type] if args.type else ANIM_TYPES
    client = None

    if args.step is None or args.step == "image":
        client = client or get_client()
        if args.force:
            side = os.path.join(PLAYER_DIR, "side.jpg")
            if os.path.exists(side):
                os.remove(side)
        result = step_image(client)
        if result is None and args.step is None:
            print("[ABORT] Image generation failed.")
            return
        if args.step == "image":
            return

    if args.step is None or args.step == "video":
        client = client or get_client()
        failed = False
        for anim_type in types:
            if args.force:
                mp4 = os.path.join(PLAYER_DIR, f"{anim_type}.mp4")
                if os.path.exists(mp4):
                    os.remove(mp4)
            if step_video(client, anim_type) is None:
                failed = True
        if failed and args.step is None:
            print("[ABORT] Video generation failed.")
            return
        if args.step == "video":
            return

    if args.step is None or args.step == "frames":
        for anim_type in types:
            if args.force:
                d = os.path.join(PLAYER_DIR, anim_type)
                if os.path.isdir(d):
                    shutil.rmtree(d)
            step_frames(anim_type)
        if args.step == "frames":
            return

    if args.step is None or args.step == "resize":
        for anim_type in types:
            step_resize(anim_type)

    show_status()


if __name__ == "__main__":
    main()
