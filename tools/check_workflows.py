"""Check workflow YAML syntax and duplicate keys before merging a PR."""
from pathlib import Path
import sys
import yaml


class StrictLoader(yaml.BaseLoader):
    def construct_mapping(self, node, deep=False):
        result = {}
        for key_node, value_node in node.value:
            key = self.construct_object(key_node, deep=deep)
            if key in result:
                raise ValueError(f"Duplicate YAML key {key!r} at line {key_node.start_mark.line + 1}")
            result[key] = self.construct_object(value_node, deep=deep)
        return result


def check(path: Path) -> None:
    with path.open(encoding="utf-8") as stream:
        data = yaml.load(stream, Loader=StrictLoader)
    if not isinstance(data, dict):
        raise ValueError(f"{path}: expected YAML mapping")
    if not data.get("name") or not data.get("on"):
        raise ValueError(f"{path}: missing name or trigger")
    jobs = data.get("jobs")
    if not isinstance(jobs, dict) or not jobs:
        raise ValueError(f"{path}: no jobs")
    for name, job in jobs.items():
        if not isinstance(job, dict):
            raise ValueError(f"{path}: invalid job {name!r}")
        if "uses" not in job and "steps" not in job:
            raise ValueError(f"{path}: job {name!r} lacks steps or uses")


def main() -> int:
    root = Path(".github/workflows")
    paths = sorted(root.glob("*.yml")) + sorted(root.glob("*.yaml"))
    if not paths:
        print("No workflows found.", file=sys.stderr)
        return 1
    failed = False
    for path in paths:
        try:
            check(path)
            print(f"Workflow OK: {path}")
        except (yaml.YAMLError, OSError, ValueError) as error:
            print(f"Workflow invalid: {path}: {error}", file=sys.stderr)
            failed = True
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
