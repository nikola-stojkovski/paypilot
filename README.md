# PayPilot

![CI](https://github.com/nikola-stojkovski/paypilot/actions/workflows/ci.yml/badge.svg)

An AI assistant for a multi-tenant payroll SaaS, built in .NET 10 while learning AI engineering. Employees and payroll admins ask questions about payslips and payroll policy; PayPilot answers using tools and cited sources, and asks a human before any change.

**Synthetic data only.** No real employees, payslips or client data are used anywhere in this repo.

## Stack

.NET 10 · Aspire · Microsoft.Extensions.AI · PostgreSQL · Angular · Ollama (local) / Microsoft Foundry (cloud)

## Structure

See [ADR 0001](docs/adr/0001-solution-structure.md).

## Run it

_Coming in the next step: one command starts the API, the Angular app and PostgreSQL through the Aspire AppHost._
