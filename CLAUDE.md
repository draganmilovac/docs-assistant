# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository state

This is an early-stage .NET project. The git repository root is `C:\Training` (one level above this directory); it holds only `README.md`, a standard Visual Studio `.gitignore`, and this `docs-assistant/` folder.

- `docs-assistant.sln` contains one project, `LlmBasics`, nested under an `src` solution folder (a virtual folder only; the project lives at `LlmBasics/`, not `src/LlmBasics/`).
- `LlmBasics` is a .NET 8 console app (`net8.0`, `ImplicitUsings` and `Nullable` enabled). `Program.cs` still holds the default template using top-level statements.
- There are no NuGet dependencies, test projects, or lint configuration yet.

## Commands

Run from `C:\Training\docs-assistant`:

```sh
dotnet build docs-assistant.sln     # build everything
dotnet run --project LlmBasics      # run the console app
```

There are no tests yet. Once a test project is added to the solution, use `dotnet test` and run a single test with `dotnet test --filter "FullyQualifiedName~<TestName>"`.
