# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

This is an early-stage .NET project. The git repository root is this directory (`C:\Training\docs-assistant`); besides the solution it holds only a placeholder `README.md` and a standard Visual Studio `.gitignore`.

- `docs-assistant.sln` contains `LlmBasics` (under the `src` solution folder) and `LlmBasics.Tests` (under the `tests` solution folder). Solution folders are virtual; the projects live at `LlmBasics/` and `LlmBasics.Tests/` in the root.
- `LlmBasics` is a .NET 8 console app (`net8.0`, `ImplicitUsings` and `Nullable` enabled) using top-level statements in `Program.cs`. It references `Microsoft.Extensions.AI` and `Microsoft.Extensions.AI.OpenAI`. `Tickets/` and `Orders/` hold in-memory practice services (`TicketService`, `OrderService`).
- `LlmBasics.Tests` is an xUnit project referencing `LlmBasics`.
- There is no lint configuration yet.

## Commands

Run from `C:\Training\docs-assistant`:

```sh
dotnet build docs-assistant.sln     # build everything
dotnet run --project LlmBasics      # run the console app
dotnet test docs-assistant.sln      # run all tests
dotnet test --filter "FullyQualifiedName~<TestName>"   # run a single test
```

## Learning mode

I'm using this project to learn LLM integration in .NET.

- Do NOT write code that uses Microsoft.Extensions.AI, Microsoft.Agents.AI or any LLM/embedding API for me. Explain concepts, give hints, point me to the right types and methods, and review my code instead.
- You MAY write boilerplate, project setup, configuration, Docker files and tests.
- When I ask "why", explain the reasoning and trade-offs rather than just giving the fix.
