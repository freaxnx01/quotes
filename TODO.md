# TODO

## Issue #4 — REST API to CRUD Quotes (agent-pipeline stuck)

- [ ] Agent-pipeline hit the 30-turn cap twice without pushing a branch or opening a PR. The build succeeds but the agent runs out of turns before committing. Fix options:
  - **Raise the turn limit** in `.github/workflows/claude.yml` (add `max-turns` input if supported by `claude-implement.yml`)
  - **Or simplify the plan** in `docs/superpowers/plans/2026-06-25-quotes-crud-api.md` — the agent spends many turns on NuGet restore + build iteration; stripping the test project from the plan scope may let it finish within 30 turns
  - Check `freaxnx01/agent-pipeline` for a `max-turns` workflow input and wire it up in `claude.yml`
- [ ] Once a draft PR is open, review it with `/gh:review 4`

## Untracked files in quotes repo

- [ ] `README.md` is untracked — write content and commit, or add to `.gitignore` if not needed

## Cosmetic

- [ ] Issue #4 has a redundant early comment ("Implementation Plan") posted before the body was updated — consider deleting it for cleanliness
