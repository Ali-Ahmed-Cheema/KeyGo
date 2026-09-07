# Project Tool API

The project engine is the controlled foundation for the future read-only coding agent.

| Tool | Purpose | Permission |
| --- | --- | --- |
| `OpenProject` | Normalize and open a local project folder | Observe |
| `IndexProject` | Enumerate allowed files and metadata | Observe |
| `SearchFiles` | Search indexed text locally | Observe |
| `ReadFile` | Read a project-relative text file | Observe |
| `GetProjectTree` | Return indexed file structure | Observe |

Write, delete, terminal, commit, push, and deployment operations are not exposed by this API.

Every tool must enforce the project-root boundary before accessing disk. Results should contain relative paths so project context cannot accidentally mix absolute paths from another project.
