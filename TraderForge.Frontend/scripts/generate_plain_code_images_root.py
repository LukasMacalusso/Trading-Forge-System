#!/usr/bin/env python3
"""
Thin wrapper for repo-root execution.

Intended to be copied/placed at the repository root.
When run from there, delegates to the canonical script with --parent-dirs=0.
"""
import subprocess
import sys
import pathlib

_REPO_ROOT = pathlib.Path(__file__).resolve().parent
_CANONICAL = _REPO_ROOT / "TraderForge.Frontend" / "scripts" / "generate_plain_code_images.py"
sys.exit(subprocess.call([sys.executable, str(_CANONICAL), "--parent-dirs", "0"]))
