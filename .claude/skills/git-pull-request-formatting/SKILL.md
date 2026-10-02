---
name: git-pull-request-formatting
description: PR title and description format using Conventional Commit types and functional scope. Use when creating or editing a GitHub pull request.
---

## Prerequisites

- **Git**: Branch must be pushed to remote before creating a PR
- **GitHub access**: Must have write access to the repository
- **PR template**: `.github/PULL_REQUEST_TEMPLATE.md` should exist in the repository

## Titles

- Format: `<type>(<scope>): <description>`
- Use the same types as commits: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`
- Scope: a functional noun describing the area of the codebase (e.g., `auth`, `api`, `ci`)
- Description: imperative, lowercase, no period

### Examples

- `feat(auth): add login flow`
- `fix(api): correct timeout handling`
- `chore(ci): migrate to github actions`

## Technical Requirements

When creating a PR via the GitHub MCP `create_pull_request` tool:

- The `body` parameter MUST use real line breaks (multi-line string), NOT escaped `\n` sequences
- Markdown formatting must be preserved as-is with actual newlines between sections
- Use the `gh` CLI with a HEREDOC as fallback if MCP tool does not render markdown correctly

## Description

The PR description should use the template found in `.github/PULL_REQUEST_TEMPLATE.md`.

If the template is not present, use the following template:

```markdown
# 📋 Description

<!--- Describe your changes in detail -->

- Please include a summary of the changes, as a list of bullet points.

## 💡 Types of changes

<!--- Put an `x` in all the boxes that apply. -->

- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] ⚠️ Breaking change (fix or feature that would cause existing functionality to change)
- [ ] 📚 Documentation (change or add documentation)
- [ ] 🏗️ Chore (maintenance, cleanup, tooling, deps)
- [ ] 🧹 Clean code (refactor code without changing functionality)
```
