#!/usr/bin/env python3
"""Convert JSON prototype files to Lua format for Project Porcupine."""

import json
import os
import re
import sys
from pathlib import Path


def lua_escape(s: str) -> str:
    """Escape string for Lua double-quoted string."""
    return s.replace("\\", "\\\\").replace('"', '\\"').replace("\n", "\\n").replace("\r", "\\r")


def is_valid_lua_key(s: str) -> bool:
    """Check if string is a valid unquoted Lua identifier."""
    return bool(s) and re.match(r"^[a-zA-Z_][a-zA-Z0-9_]*$", s) and s not in ("true", "false", "nil")


def value_to_lua(obj, indent: int = 0) -> str:
    """Convert JSON value to Lua representation."""
    pad = "  " * indent
    if obj is None:
        return "nil"
    if isinstance(obj, bool):
        return "true" if obj else "false"
    if isinstance(obj, (int, float)):
        if isinstance(obj, float) and obj == int(obj):
            return str(int(obj))
        return str(obj)
    if isinstance(obj, str):
        return '"' + lua_escape(obj) + '"'
    if isinstance(obj, list):
        if not obj:
            return "{}"
        lines = []
        for item in obj:
            lines.append(pad + "  " + value_to_lua(item, indent + 1) + ",")
        return "{\n" + "\n".join(lines) + "\n" + pad + "}"
    if isinstance(obj, dict):
        if not obj:
            return "{}"
        lines = []
        for k, v in obj.items():
            if is_valid_lua_key(str(k)):
                key_part = k
            else:
                key_part = '["' + lua_escape(str(k)) + '"]'
            lines.append(pad + "  " + key_part + " = " + value_to_lua(v, indent + 1).lstrip() + ",")
        return "{\n" + "\n".join(lines) + "\n" + pad + "}"
    return "nil"


def json_file_to_lua(json_path: Path, lua_path: Path) -> None:
    """Convert a single JSON file to Lua."""
    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)
    lua_body = value_to_lua(data)
    content = f"""-------------------------------------------------------
-- {lua_path.name} (converted from {json_path.name})
-------------------------------------------------------

return {lua_body}
"""
    with open(lua_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)


def main():
    base = Path(__file__).resolve().parent.parent
    proto_dir = base / "ProjectPorcupine" / "Assets" / "StreamingAssets" / "Data" / "Prototypes"
    if not proto_dir.exists():
        print("Prototypes dir not found:", proto_dir)
        sys.exit(1)

    json_files = sorted(proto_dir.glob("*.json"))
    for jf in json_files:
        lua_path = proto_dir / (jf.stem + ".lua")
        print("Converting", jf.name, "->", lua_path.name)
        json_file_to_lua(jf, lua_path)
    print("Done. Converted", len(json_files), "files.")


if __name__ == "__main__":
    main()
