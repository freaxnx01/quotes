# Model Comparison — OpenCode × OpenRouter (and a Claude baseline)

**Date:** 2026-06-27
**Task:** Implement an identical, behaviour-level spec — a `GET /Api/search` endpoint with optional filters + pagination — across multiple coding LLMs, and compare code quality.
**Harness:** the agent-pipeline (`claude-implement.yml`), driving either Claude Code or **OpenCode → OpenRouter**, selected per-issue with `agent:*` + `model:*` labels.

The same spec was handed to each model via its own GitHub issue (only the `model:*` label differed); each model produced its own branch + draft PR; the PRs were then reviewed against the acceptance criteria.

---

## The task (spec handed to every model)

`GET /Api/search` with optional `author`, `q` (text), `from`/`to` (date range), `page` (default 1), `pageSize` (default 20, max 100). Filters combine with AND; results ordered by `Date` desc then `ID` desc; response `{ items, page, pageSize, total }`; validation → `400`.

**Round 2** tightened the spec with two criteria that round 1 exposed as universal blind spots:

- **EF-Core translatability** — `string.Contains(value, StringComparison.OrdinalIgnoreCase)` is *not* translatable by EF Core and throws at runtime; use `EF.Functions.Like` or `.ToLower().Contains(...)`.
- **Compiles cleanly** — no duplicate `using`s, no deleting existing endpoints.

---

## Results — Round 2 (tightened spec)

| Rank | Model | OpenRouter slug | Result | Notes |
|---|---|---|---|---|
| 🥇 | **gpt-oss-120b** | `openai/gpt-oss-120b` | ✅ merged | `EF.Functions.Like` **+ `ToLower()` both sides** (most robust), async end-to-end, exact `yyyy-MM-dd` parse, no defects. **Winner.** |
| 🥈 | **gemini-flash** | `google/gemini-2.5-flash` | ✅ | Most idiomatic: extracted `QuoteSearchParams : IValidatableObject` with `[Range]` + `ModelState` validation. |
| 🥉 | **claude-sonnet** (baseline) | `claude-sonnet-4-6` (Claude path) | ✅ | Clean, exact date parse, `ToLower().Contains`. Solid but not ahead of the top open-weights on this task. |
| 4 | **deepseek-v32** | `deepseek/deepseek-v3.2` | ✅ | Reusable `PaginatedResponse<T>` DTO. |
| 5 | **minimax-m2** | `minimax/minimax-m2.5` | ✅ | Typed `SearchResult` DTO, structured errors. |
| 6 | **qwen3-coder** | `qwen/qwen3-coder-30b-a3b-instruct` | ⚠️ | Variable-shadowing logic bug: `$"%{q}%"` referenced the `Quote` lambda var, not the search string → text filter searches for the type name. Plus a stray submodule gitlink. |
| 7 | **deepseek-v3** | `deepseek/deepseek-chat-v3-0324` | ⚠️ | **Deleted the existing `/Api/random` endpoint** (AC violation) + stray gitlink. |
| 8 | **glm-flash** | `z-ai/glm-4.7-flash` | ❌ | Incomplete run — resolved the model, printed an intro, produced no edits, no PR. |

### Round 1 (looser spec) — for contrast

| Model | Result |
|---|---|
| deepseek-v3 | ✅ PR, but `StringComparison` EF-translation bug + variable shadowing |
| gemini-flash | ✅ PR, but duplicate `using`s + same EF bug |
| codestral (`mistralai/codestral-2508`) | ⚠️ ran but emitted tool calls as plain text → no edits |
| qwen-2.5-coder (`qwen/qwen-2.5-coder-32b-instruct`) | ❌ `No endpoints found that support tool use` |

---

## Key findings

1. **Open-weight models won.** `gpt-oss-120b` (~$0.03/$0.15 per M tokens) produced the cleanest, most correct implementation — edging out the Claude Sonnet baseline. `gemini-flash` showed the most architectural maturity.
2. **Spec tightening is high-leverage.** A single sentence about EF translatability eliminated the universal round-1 runtime bug. Models do what you specify — explicit "compile cleanly / don't delete endpoints" would have caught the round-2 defects too.
3. **OpenCode requires tool-use.** OpenCode drives file edits through function/tool calls. Only OpenRouter models advertising `tools` in `supported_parameters` work:
   - No tool support → hard fail (`No endpoints found that support tool use`) — e.g. `qwen-2.5-coder-32b`.
   - Advertises tools but emits malformed tool calls → silently makes no edits (false `ai:done`) — e.g. `codestral`.
   - Always verify tool support before adding a model to the roster.

## Pipeline bugs surfaced (filed on agent-pipeline)

- **#99** — OpenCode runs commit a stray submodule gitlink (`.claude-pipeline`, `mode 160000`) into the consumer PR via a broad `git add`.
- **#100** — Concurrent runs report `ai:done` but silently open no PR (a `gh pr create` race / unsurfaced failure); branches survive and were recovered manually.

## How to reproduce

Label an issue with `ai-implement` + `agent:opencode` + a `model:*` label (model roster in `agent-pipeline` `docs/CONSUMER-SETUP.md`). For a Claude baseline use `agent:claude` + `model:sonnet`. The consumer pins the agent-pipeline ref in `.github/workflows/claude.yml`.
