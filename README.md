# JRPG

A 2D JRPG written from scratch in C# (.NET 10): a MonoGame game and a WPF map editor, sharing game logic and content loading.

## Open and run

- **Visual Studio:** open `Jrpg.slnx`, pick **Game and editor** in the toolbar's startup list, press F5. Without that profile, F5 starts the game alone.
- **Command line:**
  - Game: `dotnet run --project src/Jrpg.Game`
  - Map editor: `dotnet run --project src/Jrpg.Editor`

In Debug, both read the repository's `content/` folder directly: a map saved in the editor shows on the game's next start.

## Build and test

```
dotnet build Jrpg.slnx --nologo -v q
dotnet test --solution Jrpg.slnx
dotnet format Jrpg.slnx --verify-no-changes
```

## Layout

| Folder | What's there |
|---|---|
| `src/Jrpg.Core` | All game rules: stats, battle, inventory, maps, events, dialogue, saves. No UI library. |
| `src/Jrpg.Data` | Loading and checking the JSON content, and writing maps for the editor. |
| `src/Jrpg.Game` | The MonoGame game: loop, input, drawing. |
| `src/Jrpg.Editor` | The WPF map editor. |
| `tests/` | xUnit tests for each project, and headless battle simulations. |
| `content/` | Game data (JSON) and art (`content/art/`); the only source of content. |
| `docs/decisions/` | Architecture decision records. |
| `docs/plans/` | Feature plans, built slice by slice. |

`CLAUDE.md` holds the architecture rules and coding standard; `AI-SETUP.md` explains the Claude Code setup used to build it.
