#!/usr/bin/env python3
"""
Generate PNG images from code snippets in the repository.

This script reads the specified files and line ranges, renders them
with syntax highlighting using Pygments and writes PNG files to
presentations/snippets/.

Usage: run from the repository root.
"""
import os
from pathlib import Path

from pygments import highlight
from pygments.lexers import get_lexer_for_filename
from pygments.formatters import ImageFormatter


SNIPPETS = [
    {
        "file": "TraderForge.Application/Handlers/RegisterTraderCommandHandler.cs",
        "start_line": 52,
        "end_line": 64,
        "out": "register_trader_generate_with_free_trial.png",
        "title": "GenerateTraderWithFreeTrialAsync",
    },
    {
        "file": "TraderForge.Domain/Entities/Trader.cs",
        "start_line": 17,
        "end_line": 36,
        "out": "trader_constructor_and_subscription.png",
        "title": "Trader constructor & subscription",
    },
]


def read_range(path: Path, start: int, end: int) -> str:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    # convert 1-based to 0-based indices
    start_idx = max(0, start - 1)
    end_idx = min(len(lines), end)
    selected = lines[start_idx:end_idx]
    return "\n".join(selected)


def ensure_output_dir(path: Path):
    path.mkdir(parents=True, exist_ok=True)


def render_png(code: str, filename: Path, lexer_name: str):
    # Choose lexer based on filename
    lexer = get_lexer_for_filename(lexer_name)
    formatter = ImageFormatter(
        font_name="DejaVu Sans Mono",
        font_size=16,
        line_numbers=False,
        image_pad=10,
        style="friendly",
    )
    data = highlight(code, lexer, formatter)
    with open(filename, "wb") as f:
        f.write(data)


def main():
    repo_root = Path(__file__).resolve().parents[1]
    out_dir = repo_root / "presentations" / "snippets"
    ensure_output_dir(out_dir)

    for s in SNIPPETS:
        file_path = repo_root / s["file"]
        if not file_path.exists():
            print(f"Source file not found: {file_path}")
            continue
        code = read_range(file_path, s["start_line"], s["end_line"])
        # add a small header comment to indicate file and lines
        header = f"// File: {s['file']}  Lines: {s['start_line']}-{s['end_line']}\n"
        full_code = header + code
        out_path = out_dir / s["out"]
        try:
            render_png(full_code, out_path, file_path.name)
            print(f"Wrote {out_path}")
        except Exception as e:
            print(f"Failed to render {file_path}: {e}")


if __name__ == "__main__":
    main()
