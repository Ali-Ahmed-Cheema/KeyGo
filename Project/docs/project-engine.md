# Project Engine

KeyGo treats each opened repository as a local `ProjectWorkspace`. The original folder remains in place; KeyGo stores metadata and an index, not a copied project.

## Current foundation

`ProjectWorkspaceService` provides:

- project path normalization and validation
- technology and framework detection from root evidence
- Git directory detection
- configurable ignored directories and extensions
- maximum file-size enforcement
- file classification
- local project indexing
- case-insensitive text search with file and line references
- safe project-relative file reads

Default ignored directories include `.git`, `node_modules`, `bin`, `obj`, `dist`, `build`, `.cache`, `coverage`, `venv`, and `__pycache__`.

## Security boundary

All reads are rooted in the normalized project path. Absolute paths and traversal outside the project root are rejected. Binary files and files larger than the configured limit are excluded from text indexing.

## Next extensions

- persistent SQLite file index and incremental change detection
- Roslyn symbol extraction for C#
- read-only Git inspection
- context ranking and token budgeting
- secret scanning before cloud requests
- project settings persistence
