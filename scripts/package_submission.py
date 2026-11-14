from pathlib import Path
from shutil import copy2
from tempfile import NamedTemporaryFile
from zipfile import ZIP_DEFLATED, ZipFile


ROOT = Path(__file__).resolve().parent.parent
ARCHIVE = ROOT / "Midterm_Darsavelidze_Rezo.zip"
INCLUDE_ROOTS = (
    "GeorgiaTransitIntelligence", "GeorgiaTransitIntelligence.Tests", "docs",
    "screenshots", "data", "web-preview", "scripts",
)
ROOT_FILES = ("GeorgiaTransitIntelligence.sln", "GeorgiaTransitIntelligence.slnx", "database.sql", "README.md")
REQUIRED_FILES = (
    *ROOT_FILES,
    "GeorgiaTransitIntelligence/database.sql",
    "data/kutaisi-municipal/route_patterns.csv",
    "data/kutaisi-municipal/route_stops.csv",
    "data/kutaisi-municipal/timetable.csv",
    "data/georgian-intercity/georgian-intercity-routes.json",
    "screenshots/1_main_window.png",
    "screenshots/2_search_results.png",
    "screenshots/3_add_dialog.png",
    "screenshots/4_edit_dialog.png",
)
EXCLUDE_DIRS = {"bin", "obj", ".vs", ".git", ".idea", "__pycache__", "node_modules"}
EXCLUDE_SUFFIXES = {".user", ".suo", ".tmp", ".zip", ".ndjson"}


def submission_files():
    yield from (ROOT / filename for filename in ROOT_FILES)
    for folder in INCLUDE_ROOTS:
        for path in sorted((ROOT / folder).rglob("*")):
            relative = path.relative_to(ROOT)
            if EXCLUDE_DIRS.intersection(relative.parts) or relative.parts[:2] == ("docs", "superpowers"):
                continue
            if path.is_file() and path.suffix.lower() not in EXCLUDE_SUFFIXES and not path.name.startswith(".preview-"):
                yield path


def main():
    missing = [name for name in REQUIRED_FILES if not (ROOT / name).is_file()]
    if missing:
        raise SystemExit("Missing required submission files:\n" + "\n".join(missing))

    with NamedTemporaryFile(dir=ROOT, suffix=".zip", delete=False) as temporary:
        temporary_path = Path(temporary.name)
    try:
        with ZipFile(temporary_path, "w", ZIP_DEFLATED) as archive:
            for path in submission_files():
                archive.write(path, path.relative_to(ROOT).as_posix())
            missing = set(REQUIRED_FILES).difference(archive.namelist())
            if missing:
                raise RuntimeError(f"Required files were excluded: {sorted(missing)}")
        temporary_path.replace(ARCHIVE)
    finally:
        temporary_path.unlink(missing_ok=True)

    copy2(ARCHIVE, ROOT / "web-preview" / ARCHIVE.name)
    print(f"Created {ARCHIVE.name} ({ARCHIVE.stat().st_size:,} bytes)")


if __name__ == "__main__":
    main()
