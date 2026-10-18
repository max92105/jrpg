# 0012. Editor architecture

Status: accepted
Date: 2026-10-18

## Context
Slices 11a-11j build `src/Jrpg.Editor`, the WPF map editor that replaces the legacy MapEditor now that the converter is gone (ADR 0011). It must open a map, show it as the game does, and edit it: tile painting with brush, rectangle and fill, object placement, map settings, object events and cell events, with undo and redo. It saves through Data, so it can never write content the game would reject (`editor.md`).

The legacy MapEditor was WPF code-behind: handlers in the windows changed the map directly, commands (`IEditorCommand`, `UndoManager`) lived beside the views, and nothing was tested. Constraints: solo developer; the editor references Core and Data, never Game; `editor.md` asks for MVVM with `CommunityToolkit.Mvvm` and no hand-rolled `INotifyPropertyChanged`; decision 47 approved that package.

Two choices: how the UI is structured, and where the editing logic (edits, undo, flood fill, ids) lives.

## Options
### UI structure
- **A. MVVM with `CommunityToolkit.Mvvm` source generators** (`[ObservableProperty]`, `[RelayCommand]`). Pros: thin views, view models testable without a window, little boilerplate, the convention `editor.md` already states. Cons: one package; generated code to learn.
- **B. MVVM with a hand-written base** (`ObservableObject`, `RelayCommand`). Pros: no package. Cons: boilerplate `editor.md` rules out; it's what the toolkit already does, done worse.
- **C. Code-behind like legacy.** Pros: fastest first screen. Cons: logic in views, untestable, the legacy problem again.
- **D. An external editor (Tiled) with an importer.** Pros: a mature editor for free. Cons: a second content format and importer to keep in step, no Data validation on save, and events and settings still need our own tool; the developer wants a WPF editor sharing Data's validators.

### Editing logic
- **1. Headless in the editor's `Maps/Editing`:** a `MapDraft` (mutable copy of a `MapDefinition`), `IMapEdit` commands (one per stroke or action, each with undo), `EditHistory`, `BrushCells`, `TileFloodFill`, next-free id rules; no WPF types, tested in `Jrpg.Editor.Tests`. Pros: tool logic stays out of the game's Core; fast tests without a window. Cons: the editor's own model of a map beside Core's records.
- **2. Edits in Core.** Pros: one home for all map code. Cons: Core grows tool-only code (undo, brushes) the game never runs, against "Core is game logic".

## Decision
**A + 1** (developer, 2026-10-18; A as in decision 47).

Rules that come with it:
- Content I/O only through Data: `ContentCatalogLoader` to read, `ContentWriter` to save, which runs the loader's validators first and writes nothing on failure (11a).
- Drawing with WPF `DrawingVisual`, ordered by Core's `DrawOrderKey`; canopies clipped with WPF geometry, as legacy did (the game's baked canopies are Game code the editor can't use). Same 48 px per cell at 100 % zoom.
- Content root through `ContentRootLocator` (Debug metadata or `--content`), the same rule as the game, so F5 runs both on the repository's `content/`.
- Never references Game, checked by a test on the editor assembly's references.
- View models hold no `DispatcherObject` (no `BitmapSource`, no `Brush`), so tests need no STA thread; images live in the drawing code.

## Consequences
- Easier: editing rules (fill, ids, undo) are unit-tested; the views stay thin; a save can't break the game.
- Harder: two models of a map (Core's immutable records, the editor's draft), converted on open and save; a round-trip test (open, save untouched, no diff) guards it.
- Watch: the draw order and canopy rules exist in two places (game and editor) on top of one Core key; a visual difference means one of them drifted from `DrawOrderKey`.
