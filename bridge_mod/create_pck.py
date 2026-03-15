"""Create a minimal Godot 4.5.1 PCK file containing project.godot and mod_manifest.json."""
import struct
import json
import hashlib

def create_pck(output_path: str, assembly_name: str = "STS2BridgeMod"):
    # Files to embed in the PCK
    project_godot = f"""config_version=5

[application]
config/name="{assembly_name}"
config/features=PackedStringArray("4.5", "C#", "Mobile")

[dotnet]
project/assembly_name="{assembly_name}"

[rendering]
renderer/rendering_method="mobile"
""".encode("utf-8")

    mod_manifest = json.dumps({
        "pck_name": assembly_name,
        "name": "STS2 Bridge",
        "author": "sts2-rl-agent",
        "version": "0.1.0"
    }, indent=2).encode("utf-8")

    files = [
        ("res://project.godot", project_godot),
        ("res://mod_manifest.json", mod_manifest),
    ]

    with open(output_path, "wb") as f:
        # Header
        f.write(b"GDPC")                           # magic
        f.write(struct.pack("<I", 3))               # pack format version
        f.write(struct.pack("<I", 4))               # godot major
        f.write(struct.pack("<I", 5))               # godot minor
        f.write(struct.pack("<I", 1))               # godot patch
        f.write(struct.pack("<I", 0))               # flags (0 = no encryption)
        f.write(bytes(16 * 4))                      # 16 reserved uint32s
        f.write(struct.pack("<I", len(files)))       # file count

        # Calculate file data offset
        # Header: 4 + 5*4 + 16*4 + 4 = 92 bytes
        # Each file entry: 4 (path_len) + path_padded + 8 (offset) + 8 (size) + 16 (md5)
        header_size = 92
        entries_size = 0
        for path, _ in files:
            path_bytes = path.encode("utf-8") + b"\x00"
            padded_len = (len(path_bytes) + 3) & ~3  # align to 4 bytes
            entries_size += 4 + padded_len + 8 + 8 + 16

        data_offset = header_size + entries_size

        # Write file entries
        current_data_offset = data_offset
        for path, data in files:
            path_bytes = path.encode("utf-8") + b"\x00"
            padded_len = (len(path_bytes) + 3) & ~3

            f.write(struct.pack("<I", padded_len))
            f.write(path_bytes.ljust(padded_len, b"\x00"))
            f.write(struct.pack("<Q", current_data_offset))  # offset (64-bit)
            f.write(struct.pack("<Q", len(data)))             # size (64-bit)
            f.write(hashlib.md5(data).digest())               # md5

            current_data_offset += len(data)

        # Write file data
        for _, data in files:
            f.write(data)

    print(f"Created PCK at {output_path} with {len(files)} files")


if __name__ == "__main__":
    import sys
    output = sys.argv[1] if len(sys.argv) > 1 else "STS2BridgeMod.pck"
    create_pck(output)
