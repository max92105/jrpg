# AI setup for the JRPG — how to install and use it

## What's in this kit

| Piece | Where | What it does |
|---|---|---|
| Project memory | `CLAUDE.md` | Architecture, commands, coding rules, definition of done. Loaded every session. |
| Area rules | `.claude/rules/*.md` | Extra rules that load only when Claude touches Core, Game, Editor, tests or content. |
| Agents | `.claude/agents/` | `architect`, `devils-advocate`, `code-reviewer`, `qa-engineer`, `balance-analyst`. Each runs in its own fresh context. |
| Commands | `.claude/skills/` | `/plan-feature`, `/challenge`, `/slice`, `/review-slice`, `/qa`, `/adr`, `/verify`, `/lesson`, `/retro`. |
| Checks script | `.claude/scripts/verify.ps1` | Build, tests, format, Core and naming rules in one run, printing one line per check; used by `/verify`, `/slice`, the reviewer and the Stop hook. |
| Skill list | `skillOverrides` in `.claude/settings.json` | Turns off general-purpose skills this project never uses, so their descriptions stay out of every turn. |
| Guardrails | `.claude/settings.json` + `.claude/hooks/` | Auto-approves safe commands, blocks dangerous ones, protects config files, auto build+test when Claude finishes a turn. |
| Code quality standard | `.claude/rules/code-quality.md` | Naming, structure, method size, comments, "no AI slop" list. Loaded whenever Claude touches a `.cs` file; the reviewer checks against it. |
| Build quality | `src/Directory.Build.props`, `tests/Directory.Build.props`, `CodeMetricsConfig.txt`, `.editorconfig` | Nullable on, warnings = errors, analyzers. The build fails on unused code, methods with complexity > 10, naming violations, namespace ≠ folder, unsealed internal classes. Applies only to new projects in `src/` and `tests/`. |
| Codebase map | `docs/map.md` | One line per feature folder with its key types, and the patterns the code follows. Agents read it instead of exploring. |
| Decisions | `docs/decisions/` | ADRs, indexed one line each in `docs/decisions/README.md`. |
| Plans | `docs/plans/` | Where `/plan-feature` writes plans: one folder per plan (slice index, design, one file per slice), indexed in `docs/plans/README.md`; finished ones in `done/`. |

## Install (10 minutes)

1. Copy everything in this kit into your repository root (merge with what's there).
   - Append `.gitignore.additions` to your `.gitignore`, then delete the `.additions` file.
   - The build props live in `src/` and `tests/` on purpose: at the repo root they would also apply to the legacy demo, whose code doesn't meet them (nullable, warnings as errors), and break its build.
   - Delete `.github/copilot-instructions.md` (it only contains Azure rules unrelated to this project).
   - The legacy demo (`JRPG/`, `MapEditor/`, `JRPG.Core/`, `JRPG.sln`) now sits in `legacy/` as reference only and isn't built. The Stop hook and the quality commands build `Jrpg.slnx`.
2. Commit: `git add -A && git commit -m "Add Claude Code setup"`.
3. Open the repo folder in VS Code (`C:\Git\jrpg` itself, not a parent folder) and start a new Claude Code conversation. Accept the **trust this folder** prompt if asked (hooks don't run until you do).
4. Check it loaded:
   - `/context` → `CLAUDE.md` is listed under Memory files.
   - `/hooks` → a `PreToolUse` and a `Stop` hook are listed.
   - Type `/` → you see `plan-feature`, `slice`, `review-slice`, etc.
5. Optional but recommended: run `/plugin`, search for a **C# code intelligence** plugin and install it. It gives Claude real compiler diagnostics and go-to-definition instead of text search.

The hooks use Windows PowerShell, which every Windows machine has. If you work on macOS/Linux later, the hook scripts need a bash version.

## The daily loop

```
/plan-feature <feature>     architect plans → devil's advocate attacks → you answer design questions
        ↓  (you approve)
/slice                      implements the next slice + tests, runs build/test, gets reviewed
        ↓
/qa                         optional: QA agent hunts edge cases with new tests
        ↓
/verify  → you commit       build, tests, format, architecture check
        ↓
/clear                      fresh context for the next slice
```

Other commands when you need them:
- `/challenge <idea>` — before any decision you're unsure about.
- `/adr <topic>` — when you pick between real alternatives. Future sessions will read it.
- `/review-slice` — review your own hand-written code the same way.
- `/lesson <what went wrong>` — when Claude repeats a mistake, turn it into a permanent rule.
- "Use the balance-analyst to check if the forest boss is too hard at level 5" — data instead of gut feeling.

## Suggested first session: migrating the demo

```
/plan-feature Migrate the WPF demo into the new structure: create Jrpg.slnx with
src/Jrpg.Core, src/Jrpg.Data, src/Jrpg.Game (MonoGame), src/Jrpg.Editor (WPF) and the two test projects;
port the demo's game logic from JRPG.Core into src/Jrpg.Core with tests, redesigned to the
code-quality standard (no static GameSystems, no *Manager classes, feature folders);
leave rendering for later slices.
```

The architect will read your demo code and propose the slices, including a mapping from each old class to its new type(s). Answer its design questions, then run `/slice` repeatedly. Delete a legacy file only once its behavior is ported and tested.

## Habits that matter more than the tooling

- **One slice per conversation**, then `/clear`. Long conversations get sloppy; plans and ADRs on disk carry context forward instead.
- **Plan mode** (Shift+Tab until it shows plan mode) for anything you're not sure about: Claude researches and proposes, but can't edit.
- **Read every diff** before committing. If you can't explain a change, ask Claude to explain it or undo it.
- **Game design is yours.** The agents are told to ask, not decide, on balance and design. Keep it that way.
- **Fix the system, not just the bug.** Each time you correct Claude twice for the same thing, run `/lesson`.
- **Keep `CLAUDE.md` short** (under ~150 lines). Run `/doctor prompt-audit` now and then to find outdated or conflicting rules.

## Cost and speed notes

- Architect, devil's advocate and reviewer use Opus (judgment matters). QA and balance use Sonnet (volume work). Change the `model:` line in each agent file if you want.
- The Stop hook builds and tests only when C#/project/content files changed, so chat-only turns stay fast. If builds get slow, raise `timeout` in `settings.json` or remove the hook and rely on `/verify`.

## Troubleshooting

- **A command like `/plan-feature` isn't in the `/` menu:** the VS Code extension hides skills marked `disable-model-invocation: true`. Don't add that flag to skills in this repo; guard them with "Run only when the user explicitly asks for /name." in the description instead.
- **Hooks don't appear in `/hooks`:** check the JSON is valid (no trailing commas) and you trusted the folder. Restart `claude`.
- **A hook errors with "running scripts is disabled":** the command already passes `-ExecutionPolicy Bypass`; make sure you didn't edit that out.
- **Claude keeps looping on a failing build:** the hook lets Claude stop after one retry round (`stop_hook_active`). Then fix it together or run `/verify`.
- **An agent isn't used:** call it by name ("use the code-reviewer agent") or with `@`.
- **Too many analyzer errors on day one:** set `AnalysisLevel` to `latest-default` in `src/Directory.Build.props`, then tighten later. Don't raise the complexity limit in `CodeMetricsConfig.txt` to get a build green; split the method.
