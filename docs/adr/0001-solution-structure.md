# 0001. Solution structure

Date: 2026-11-05
Status: Accepted

## Context

PayPilot is an AI assistant for a multi-tenant payroll SaaS, built on synthetic data. Over the course it needs:

- a payroll domain whose calculations are deterministic (the model never does payroll math),
- AI features that change often: provider wiring (Ollama now, Foundry later), prompts, tools, RAG, agents,
- several entry points over time: a web UI, evals, load tests, an MCP server,
- one-command local startup with tracing, so AI behaviour can be inspected instead of guessed.

## Decision

Clean architecture in four layers, plus Aspire for hosting:

```
src/
  Domain/          PayPilot.Domain                       entities, Money, net-pay rules; no dependencies
  Application/     PayPilot.Application                  use cases and interfaces (e.g. IPayslipService)
  Infrastructure/  PayPilot.Infrastructure.Persistence   EF Core + PostgreSQL
                   PayPilot.Infrastructure.Ai            provider switch, prompts, tools, later RAG + agent
  Presentation/    PayPilot.Api                          the only entry point (HTTP)
                   paypilot-web                          Angular front end
  Aspire/          PayPilot.AppHost, PayPilot.ServiceDefaults
tests/             PayPilot.Domain.Tests
```

Dependencies point inward only:

```
Domain                      -> (nothing)
Application                 -> Domain
Infrastructure.Persistence  -> Application
Infrastructure.Ai           -> Application
Api                         -> Application, Infrastructure.*, ServiceDefaults
AppHost                     -> Api (+ Postgres container, Angular app)
```

The front end is Angular talking to the Api over HTTP (Server-Sent Events for streamed answers).

## Why

- **The AI can be removed without breaking the business.** Delete `PayPilot.Infrastructure.Ai` and Domain, Application, Persistence and their tests still compile and pass. Only the Api's chat endpoint disappears.
- **One project per external dependency.** Persistence and AI change for different reasons and at different speeds, so they live apart.
- **One door into the system.** A separate Api means the UI, evals, load tests and the MCP server all use the same endpoints. Streaming over SSE is built explicitly instead of being hidden by Blazor's connection.
- **Angular** because I already know it, so learning time stays on AI; Angular + .NET is also a common pairing in senior job ads.

## Consequences

- `PayPilot.Infrastructure.Ai` holds more than plumbing: prompts, tools and the RAG pipeline are application behaviour living in an infrastructure project. Accepted, with two rules: the dependency only points inward, and there is no home-made `IAiService` wrapper around `IChatClient`. Where Application needs AI it may depend on `Microsoft.Extensions.AI.Abstractions` directly, the way it would use `ILogger`.
- Tools such as `GetPayslip` are thin adapters in the AI project that call Application services; the domain never knows it was called by a model.
- Two languages in the repo (C# and TypeScript) and some extra setup: CORS, a separate build, auth tokens later.
- More projects than a single web app. Each has a real job from Phase 0; worker (`PayPilot.Ingestion`) and MCP server (`PayPilot.Mcp`) projects are added in later phases.
