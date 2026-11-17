# 0003. Prompts are versioned files; temperature 0 by default

Date: 2026-11-11
Status: Accepted

## Context

The system prompt was a string constant in `ChatController`. Its answers were vague, it invented taxes and currencies that weren't in the data, and it answered off-topic questions. Prompts change more often than code, and as C# strings they are hard to review and impossible to reuse in tests.

## Decision

- Prompts are Markdown files in `PayPilot.Infrastructure.Ai/Prompts/`, embedded in the assembly and loaded by name with `PromptLibrary.Get(...)`.
- The chat system prompt holds only what is true in every conversation: role and task, grounding rules, and the exact "I don't know" and off-topic replies. Anything that depends on the data, such as the answer template for pay questions, is added by code in the same message as that data.
- Every chat call uses temperature 0 unless the caller sets it, configured once in the AI pipeline.

## Why

- On the same model, the payslip explanation went from 5/18 runs with all amounts (unstructured prompt) to 7/18 (rules) to 9/9 (answer template). Describing the answer wasn't enough; showing its exact shape was.
- Classification, extraction and refusals gave identical answers at temperatures 0, 0.7 and 1.2, so a higher temperature brings nothing where there is a right answer. Temperature 0 isn't fully deterministic, but it's the most consistent.
- As files, prompt changes are reviewable diffs, and the evals in Phase 5 can test the exact prompt the app ships.
- With the answer template in the system prompt and no payslip in the conversation, the model invented a complete payslip in 9 out of 9 runs, even with an explicit "only when a payslip is present" condition. With the template moved next to the data, the same rules gave "I don't know." 9/9 without a payslip and the full calculation 9/9 with one. A small model follows what it can see over what it is told, so data-dependent decisions are made in code.

Evidence: [prompt kata findings](https://github.com/nikola-stojkovski/ai-labs/blob/main/phase-01/prompt-kata/FINDINGS.md).

## Consequences

- Until Phase 2 gives PayPilot payslip data through tools, there is no payslip and no template in the conversation, so questions about pay get "I don't know." An honest "I don't know" beats invented numbers.
- Every prompt that uses data is tested both with and without that data.
- Changing a prompt needs a rebuild. Accepted: a prompt ships with the code version it was tested with.
- A feature that needs variety, such as drafting messages, sets its own temperature per call.