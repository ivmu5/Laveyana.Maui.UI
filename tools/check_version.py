"""Validate the Laveyana v.YY.MM.vv version scheme used by all packages."""
from __future__ import annotations

import argparse
import re
from pathlib import Path
import sys
from xml.etree import ElementTree as ET

VERSION_PATTERN = re.compile(r"(?P<major>0|[1-9][0-9]*)\.(?P<year>[0-9]{2})\.(?P<month>0[1-9]|1[0-2])\.(?P<revision>[1-9][0-9]*)\Z")


def parts(version: str) -> tuple[int, int, int, int]:
    """Parse global major, two-digit year, month and monthly release number."""
    match = VERSION_PATTERN.fullmatch(version)
    if match is None:
        raise ValueError("Expected v.YY.MM.vv, for example 0.26.11.1")
    return tuple(int(match[name]) for name in ("major", "year", "month", "revision"))


def nuget_normalized(version: str) -> str:
    """Return the normalized NuGet numeric version, without leading zeroes."""
    return ".".join(str(value) for value in parts(version))


def validate(props_path: Path, tag: str | None = None) -> str:
    """Validate the project version and optional exact Git tag."""
    tree = ET.parse(props_path)
    version = tree.findtext("./PropertyGroup/LaveyanaVersion")
    if not version:
        raise ValueError(f"LaveyanaVersion missing in {props_path}")
    parts(version)
    if tag is not None and tag != f"v{version}":
        raise ValueError(f"Git tag {tag!r} must match v{version}")
    return version


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--props", type=Path, default=Path("Directory.Build.props"))
    parser.add_argument("--tag", default=None)
    args = parser.parse_args()
    try:
        version = validate(args.props, args.tag)
    except (ValueError, OSError, ET.ParseError) as error:
        print(f"Invalid version: {error}", file=sys.stderr)
        return 1
    print(f"Version OK: tag=v{version}; NuGet={nuget_normalized(version)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
