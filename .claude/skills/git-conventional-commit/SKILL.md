---
name: git-conventional-commit
description: Instructions for writing commit messages following the Conventional Commits specification.
---

## Prerequisites

- **Git**: Must be installed and configured with `user.name` and `user.email`
- **Pre-commit hooks** *(optional)*: If `.pre-commit-config.yaml` exists, hooks will enforce the format automatically

## Format

- `<type>(<scope>): <description>`
- **Scope is mandatory** — never omit it
- Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`
- Scope: a functional noun describing the area of the codebase affected (e.g., `auth`, `parser`, `ci`)
- Description: imperative, lowercase, no period

## Examples

- `feat(parser): add ability to parse arrays`
- `fix(auth): correct password validation`
- `chore(ci): migrate to github actions`
- `docs(api): update endpoint documentation`
- `refactor(database): simplify connection pooling`
