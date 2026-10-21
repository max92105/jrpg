# 0014. Editing the shared catalogs

Status: accepted
Date: 2026-10-21

## Context
The editor plan (`docs/plans/2026-10-21-editor/`) makes the editor change the catalogs that every map shares, not only maps. Painting creates tiles in `tiles.json`, sheets are set up and re-cut in `tilesheets.json`, and objects get collision outlines and above-player zones in `objects.json`. Today the editor holds the catalog read-only (`MainViewModel.Catalog`), and only maps have unsaved state, undo (`EditHistory` per map, ADR 0012) and a validated write (`ContentWriter.WriteMap`).

The question is **how catalog edits are held, saved and undone**. Constraints:
- content lives only in `content/`, written through Data, and the editor never writes what the game would reject (ADR 0011);
- the developer works with one Save (Ctrl+S) and one Undo (Ctrl+Z) today, and wants that to keep holding (decision 117);
- map histories are per map, while catalog changes belong to no map;
- a solo developer must be able to add a new kind of catalog change without touching every panel.

## Options
### A. A catalog draft, saved with Ctrl+S and undone with Ctrl+Z through a journal
A `Catalogs/CatalogDraft` holds the whole working catalog; every panel and the map drawing read it. A catalog change replaces records in it and is a step in a catalog history. A shell journal records whether each step went to the open map or to the catalogs, so Ctrl+Z undoes the newest step whichever it is. Ctrl+S writes the catalogs (if changed), then the open map. Switching maps keeps catalog changes; closing asks if anything is unsaved.
- Pros:
  - one Save and one Undo for everything, as the developer works today;
  - panels stay in the main window, so the map shows a change at once;
  - one write path, validated together;
  - map histories stay per map, unchanged.
- Cons:
  - a second kind of unsaved state beside the map's;
  - the journal must stay in step with two histories.

### B. Modal manager windows with their own Save (legacy Tileset Manager, Object Editor)
- Pros: simple to reason about; proven by legacy.
- Cons: two saves to think about; the map can't show a change until the window closes; no undo after closing; the cut sits away from where tiles are chosen.

### C. Every catalog change written at once
- Pros: nothing unsaved.
- Cons: a file write per click; a validation failure mid-gesture; no undo.

### D. One undo history for map and catalog edits
- Pros: one stack.
- Cons: map histories are per map and catalog edits are global, so switching maps would have to keep or drop the other map's steps inside one stack.

## Decision
**A**, chosen by the developer (2026-10-21), with their decisions:
- **113:** a sheet cell becomes a tile on its first paint. The new tile is not an undo step of its own, since an unused tile is harmless.
- **115:** a sheet's cut is entered as columns and rows and stored as tile size, so the content format doesn't change.
- **117:** Ctrl+S saves the catalogs and the map together, and Ctrl+Z undoes catalog changes too. An undo that would remove a tile, sheet or object still in use is refused, naming where it's used.

The write rule: one `ContentWriter.WriteMapCatalogs` writes `tilesheets.json`, `tiles.json` and `objects.json` after the catalog validators, every map's layout check and the art check. It writes all three or none.

## Consequences
- **Easier:** a new kind of catalog change is one `CatalogDraft` method that builds an undoable edit. Saving, validation, undo and redrawing come with it, and each catalog rule has one home.
- **Harder:**
  - two histories and a journal to keep in step, so the journal's tests pin the order;
  - an undo after a save or a map switch can reach a record a saved map uses, so the in-use check refuses it, and the save's validation is the backstop.
- **Watch:**
  - catalog files are CRLF and sorted by id; an untouched catalog must write back byte for byte;
  - tile ids are never renumbered.
