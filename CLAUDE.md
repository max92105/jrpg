# JRPG project

A 2D JRPG written from scratch in C#. Personal project, single developer.
<!-- Keep this file under ~150 lines. Add a rule here only when Claude gets something wrong twice. -->

## Architecture (non-negotiable)

- `src/Jrpg.Core` — all game logic: stats, battle, inventory, maps, events, dialogue, save/load.
  **Must not reference MonoGame, WPF, or any UI/rendering library.** Pure .NET only.
- `src/Jrpg.Data` — content loaders and schemas (JSON). Depends on Core only.
- `src/Jrpg.Game` — MonoGame shell: game loop, input, rendering, audio. Thin. Reads Core state, sends commands to Core.
- `src/Jrpg.Editor` — WPF tools (map, database, dialogue editors). References Core + Data, never Game.
- `tests/Jrpg.Core.Tests` — xUnit unit tests for Core.
- `tests/Jrpg.Data.Tests` — xUnit tests for content loaders and validators.
- `tests/Jrpg.Game.Tests` — xUnit tests for the Game's pure parts (screen fitting, scene stack, flow, presenters); nothing that needs a window.
- `tests/Jrpg.Simulations` — headless battle/balance simulations.
- `content/` — JSON game data (items, monsters, skills, maps, dialogue) and art (`content/art/<kind>/`, ADR 0009). The only source of truth for content (ADR 0011).
- `docs/decisions/` — architecture decision records (ADRs). Read the relevant ADR before changing a design area.
- `docs/plans/` — feature plans produced by `/plan-feature`.

Dependency direction: Game → Data → Core; Editor → Data → Core. Never the reverse.

## Current state (migration in progress)

- `JRPG/`, `MapEditor/`, `JRPG.Core/` are the **legacy WPF demo** (.NET 10, `net10.0-windows`, `JRPG.sln`). It is reference material for the migration: read it, port behavior out of it, don't extend it. Build: `dotnet build JRPG.sln --nologo -v q`. Run: `dotnet run --project JRPG` / `dotnet run --project MapEditor`.
- Legacy `JRPG.Core` still references WPF (`UseWPF`) for `Rect`/`ImageSource` in map types. The new `src/Jrpg.Core` must not.
- `Assets/` is the legacy demo's art and map JSON, frozen since the content cutover (ADR 0011): the new code reads only `content/`, and `Assets/` goes with the legacy code in part 3c.
- New code goes only in `src/` and `tests/`, built by `Jrpg.slnx`.

## Commands

- Build: `dotnet build Jrpg.slnx --nologo -v q`
- Test: `dotnet test --solution Jrpg.slnx`
- Format check: `dotnet format Jrpg.slnx --verify-no-changes`
- Format fix: `dotnet format Jrpg.slnx`
- Run game: `dotnet run --project src/Jrpg.Game` (Debug reads the repository's `content/` directly, so content edits show on the next start without rebuilding; `-- --content <folder>` reads another folder). A broken content file stops the start with the file named on screen, in the console and in the Visual Studio Output window (only the console and Output window if the message table or fonts are what's broken). A Release build only finds content when published (`dotnet publish`) or given `--content`.
- Visual Studio: F5 starts `Jrpg.Game`, the default startup project (`DefaultStartup` in `Jrpg.slnx`). If a fresh clone starts another project, right-click `Jrpg.Game` > Set as Startup Project.

## Coding rules

- Nullable reference types on; warnings are errors (see `Directory.Build.props`). Fix warnings, never suppress them without a comment explaining why.
- Prefer immutable records for data (`ItemDefinition`, `SkillDefinition`). Mutable state lives in clearly named runtime classes (`PartyState`, `BattleState`).
- No static mutable state. No singletons. Pass dependencies in.
- **Determinism:** Core never calls `DateTime.Now`, `Random.Shared`, or `new Random()` without a seed. Randomness goes through `IRandom` injected with a seed. Time goes through the game loop's delta, never the wall clock.
- No content hardcoded in C#: stats, names, damage values, dialogue text go in `content/` JSON.
- Formulas (damage, XP curves, hit chance) live in one static, pure place per domain, with the formula's source or reasoning in a comment, and are unit-tested.

## Code quality (the bar is a professional team's code review)

Full standard: `.claude/rules/code-quality.md`, loaded automatically for `.cs` files. The essentials:

- Names carry the intent. A type name states one responsibility; no `Manager`/`Helper`/`Utils` grab-bags. Methods are verb phrases.
- Organize by feature (`Battle/`, `Inventory/`), never by kind (`Models/`, `Enums/`, `Managers/`). Namespace = folder.
- `internal sealed` by default. Small methods, guard clauses, max 4 parameters, no flag arguments.
- Comments say *why*, never *what*. No commented-out code, no narration.
- No speculative code: no interface, factory or parameter without a current user. Search for existing code before writing new code.
- The build enforces part of this (unused code, complexity > 10, naming, namespace/folder match). The reviewer enforces the rest.

## Testing rules

- Every new Core behavior ships with tests in the same change.
- Test names: `Method_Scenario_ExpectedResult`.
- Use fixed seeds in tests. A test that passes randomly is a bug.
- Save/load changes require a round-trip test and, if the format changed, a migration + test loading the old format.

## How we work

- Work in **small vertical slices**: one behavior, end to end, tests included, buildable at the end.
- Before non-trivial work, plan first (`/plan-feature`). Don't start coding a feature that has no plan.
- When a design choice has real alternatives, write an ADR (`/adr`) rather than deciding silently.
- When unsure about a requirement or a game-design choice, **ask me** instead of guessing. Game design decisions are mine.
- Don't refactor code unrelated to the current slice. Note it as a follow-up instead.
- Don't add NuGet packages without asking.
- Keep commits small, one slice per commit, message in imperative mood.

## Definition of done (a slice)

1. Builds with zero warnings.
2. All tests pass, new behavior is tested.
3. `dotnet format --verify-no-changes` passes.
4. Reviewed with `/review-slice`, critical findings fixed (readability and structure findings included).
5. Plan file in `docs/plans/` updated (checkbox ticked, deviations noted).
