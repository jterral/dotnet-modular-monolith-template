---
name: git-branch-naming
description: Git branch naming convention. Use automatically when creating a git branch.
---

# Branch Naming Convention

Format: `{type}/{slug}`

## Types

- `feature` — new functionality (default if unknown)
- `fix` — bug fix
- `chore` — maintenance, dependencies, config
- `docs` — documentation only

## Rules

- Default to `feature` if the type cannot be determined
- Slug in kebab-case from the change summary, max 5 words
- Example: `feature/get-user-address`
