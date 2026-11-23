"""Проверка наличия и содержимого результата dotnet test (TRX)."""
from pathlib import Path
import argparse
import sys
import xml.etree.ElementTree as ET


def validate(path: Path) -> tuple[int, int]:
    if not path.is_file():
        raise ValueError(f"Отчёт TRX отсутствует: {path}")
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError as error:
        raise ValueError(f"Повреждённый TRX {path}: {error}") from error
    counters = root.find('.//{*}ResultSummary/{*}Counters')
    if counters is None:
        raise ValueError(f"Отсутствует ResultSummary/Counters: {path}")
    try:
        total = int(counters.attrib['total'])
        executed = int(counters.attrib['executed'])
        passed = int(counters.attrib['passed'])
        failed = int(counters.attrib['failed'])
    except (KeyError, ValueError) as error:
        raise ValueError(f"Неверные счётчики TRX: {path}: {error}") from error
    if total < 1 or executed < 1 or passed < 1 or failed != 0 or executed != passed:
        raise ValueError(f"Недостаточно или неуспешно выполненных тестов: "
                         f"total={total}, executed={executed}, passed={passed}, failed={failed}")
    return passed, total


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('trx', type=Path)
    args = parser.parse_args()
    try:
        passed, total = validate(args.trx)
    except (ValueError, OSError) as error:
        print(error, file=sys.stderr)
        return 1
    print(f"TRX подтверждён: {passed}/{total} тестов пройдено")
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
