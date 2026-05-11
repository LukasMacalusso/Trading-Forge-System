#!/usr/bin/env python3
"""
Generate simple PNG images from code snippets using Pillow.

This avoids external syntax-highlighting dependencies and renders
monospaced text onto a white background.
"""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

SNIPPETS = [
    {
        "file": "TraderForge.Application/Handlers/RegisterTraderCommandHandler.cs",
        "start_line": 52,
        "end_line": 64,
        "out": "register_trader_generate_with_free_trial.png",
    },
    {
        "file": "TraderForge.Domain/Entities/Trader.cs",
        "start_line": 17,
        "end_line": 36,
        "out": "trader_constructor_and_subscription.png",
    },
]


def read_range(path: Path, start: int, end: int) -> str:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    start_idx = max(0, start - 1)
    end_idx = min(len(lines), end)
    selected = lines[start_idx:end_idx]
    return "\n".join(selected)


def find_mono_font():
    # Try common monospace fonts
    candidates = [
        "/Library/Fonts/Andale Mono.ttf",
        "/Library/Fonts/DejaVuSansMono.ttf",
        "/Library/Fonts/Menlo.ttc",
        "/System/Library/Fonts/Monaco.ttf",
    ]
    for p in candidates:
        if Path(p).exists():
            return p
    return None


def render_text_to_png(text: str, out_path: Path, font_path: str | None):
    # Basic layout: measure text and render
    lines = text.splitlines()
    font_size = 14
    if font_path:
        font = ImageFont.truetype(font_path, font_size)
    else:
        font = ImageFont.load_default()

    # compute image size
    max_width = max((font.getsize(line)[0] for line in lines), default=0)
    line_height = font.getsize("A")[1]
    padding = 16
    img_w = max_width + padding * 2
    img_h = line_height * len(lines) + padding * 2

    img = Image.new("RGB", (img_w, img_h), color="white")
    draw = ImageDraw.Draw(img)
    y = padding
    for line in lines:
        draw.text((padding, y), line, fill=(12, 17, 23), font=font)
        y += line_height

    img.save(out_path)


def main():
    parser = argparse.ArgumentParser(description="Render code snippets as PNG images.")
    parser.add_argument('--parent-dirs', type=int, default=1,
                        help='Number of parent dirs from script location to repo root (default: 1)')
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parent
    for _ in range(args.parent_dirs):
        repo_root = repo_root.parent
    out_dir = repo_root / "presentations" / "snippets"
    out_dir.mkdir(parents=True, exist_ok=True)

    font_path = find_mono_font()

    for s in SNIPPETS:
        file_path = repo_root / s["file"]
        if not file_path.exists():
            print(f"Source file not found: {file_path}")
            continue
        code = read_range(file_path, s["start_line"], s["end_line"])
        header = f"// File: {s['file']}  Lines: {s['start_line']}-{s['end_line']}\n"
        full = header + code
        out_path = out_dir / s["out"]
        render_text_to_png(full, out_path, font_path)
        print(f"Wrote {out_path}")


if __name__ == "__main__":
    main()
