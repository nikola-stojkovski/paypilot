# 0002. Local chat model: qwen3:4b-instruct

Date: 2026-11-11
Status: Accepted

## Context

PayPilot runs on Ollama locally, on a laptop without a GPU, so every model runs on the CPU. Since the start it has used qwen3:4b. That model always reasons before it answers: neither `/no_think` in the prompt nor `think=false` in the options turned it off. Most of its output is hidden reasoning the user has to wait for.

## Decision

Use qwen3:4b-instruct, the non-thinking version of the same model size, as PayPilot's local chat model.

## Why

Measured in the Phase 1 labs, on the same requests and the same machine:

| | qwen3:4b | qwen3:4b-instruct |
| --- | --- | --- |
| Time to first answer word (cold start) | 34–115 s | 4.5 s |
| Output tokens for a one-sentence answer | 366–1,139 | 27 |
| Average time per call, 10 payroll tasks | ~140 s | ~2.3 s |
| Classification, extraction, refusals | 20/21 | 63/63, in two runs |
| Payslip explanation with the final prompt | not run | 9/9 |

- Reasoning didn't make answers more correct on PayPilot's tasks. It made them 6–60× slower, and its length varied about 3× between runs of the same question.
- With streaming, the thinking model's first token came almost immediately, but it was reasoning: the first word of the answer took up to two minutes.
- On a cloud model the same difference becomes cost: reasoning is billed as output, which raised the estimated cost per answer about 7×.

Evidence: [cost calculator findings](https://github.com/nikola-stojkovski/ai-labs/blob/main/phase-01/cost-calculator/FINDINGS.md), [prompt kata findings](https://github.com/nikola-stojkovski/ai-labs/blob/main/phase-01/prompt-kata/FINDINGS.md).

## Consequences

- The model name is configuration only (`ConnectionStrings:chat` in the AppHost); no code changes.
- If a later feature really needs multi-step reasoning, it gets its own model and its own evidence, instead of switching the whole app back.
- The evals in Phase 5 will re-check this choice against a golden set. The cloud model is a separate decision (Phase 9).