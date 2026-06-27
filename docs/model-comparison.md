# Model Comparison — provenance stub

This repo hosted a benchmark run of the agent-pipeline's OpenCode × OpenRouter coding
agents: an identical `GET /Api/search` spec was handed to each model via its own issue,
and the resulting draft PRs were reviewed against the acceptance criteria.

**The canonical, living report now lives in agent-pipeline** (it guides model selection
for every consumer of the pipeline, not just this repo):

➡️ **<https://github.com/freaxnx01/agent-pipeline/blob/main/docs/model-comparison.md>**

## Run provenance (this repo)

- **Per-model draft PRs:** #28–#34 — winner **gpt-oss-120b** = #34.
- **Result headline:** `gpt-oss-120b` produced the cleanest, most correct implementation,
  edging out the Claude Sonnet baseline at ~100× lower cost than Opus; `gemini-flash` was
  the most idiomatic. Full ranking and findings in the canonical report above.
- **Stack:** .NET (this repo's `WebApplication`).
