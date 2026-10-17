# 0005. Map content format and source of truth

Status: accepted
Date: 2026-10-17

## Context
Part 2 brings maps, tiles, objects and their legacy triggers into the new code. They currently live in `Assets/` (`Tiles/`, `Objects/`, `Maps/`, `Maps/Back/`) in the legacy editor's format: int tile ids, display names used as object ids, `"x,y"` point strings, a per-cell tile list, stale object sizes, and a polymorphic trigger format. ADR 0002 already allows a Data DTO where a file's format doesn't match the model.

The developer keeps editing maps in the legacy MapEditor until the part 4 editor exists (plan decision 42), so whatever we load must stay in step with `Assets/`. Some per-map data the legacy editor can't express at all: display name, encounter table, odds per step, grace steps. Constraints: solo developer, strict content loading (file, id and field named on error), no lost hand-written values, and save files keyed by map ids, object uids and event ids.

## Options
### A. Copy the legacy files into `content/` and read them through permanent Data DTOs
- Pros: no tool to write; the legacy editor's output is loaded as is.
- Cons: the legacy quirks (display-name ids, string points, stale sizes) become our permanent content format, with DTOs and mapping kept forever. The settings the editor can't express still need a second file. Copies drift from `Assets/` unless re-copied by hand.

### B. A re-runnable converter from `Assets/` into a new schema loaded straight into Core records
A console tool (`tools/Jrpg.ContentConverter`) reads `Assets/` and rewrites `content/generated/` completely on every run. Hand-written `content/maps/{id}.json` holds the per-map settings; the loader composes the two.
- Pros: content gets a clean schema that loads like every other file (ADR 0002 option A), so no DTOs in Data. Legacy quirks are fixed once, in the tool, with tests. Re-running is safe because the tool never writes outside `generated/`. Part 4 deletes the tool and nothing else changes.
- Cons: one more project and its tests. Two sources of truth until part 4. A hand edit to a generated file is silently lost on the next run. Forgetting to re-run after editing a map leaves stale content (the game loads the old layout).

### C. Convert the catalogs (tiles, objects) once; read maps as is
- Pros: less conversion code at first.
- Cons: maps keep the legacy format and need DTOs anyway, so we pay for both A and B. The converted catalogs go stale when the legacy editor adds a tile or object.

## Decision
**B** (developer, 2026-10-17), with these rules:
- `Assets/` plus the legacy MapEditor are the source of truth for map geometry (tiles, objects, placements, legacy events) until part 4. This is a temporary, explicit exception to "`content/` is the source of truth".
- The converter owns `content/generated/` and rewrites all of it on every run. It never writes anywhere else, and generated files are never edited by hand.
- Whatever the legacy editor can't express lives in hand-written `content/maps/{id}.json`, composed with the generated layout at load. A map with no settings file, or a settings file with no map, fails loading.
- Generated content holds only what current code uses. Later parts extend the converter and re-run it (render data in part 3). No lossless round trip is needed while `Assets/` is the source.
- Ids: tile ids stay ints (editor-generated, thousands of references, 0 = empty); object and map ids become snake_case strings by one rule (`"Chest Opened"` → `chest_opened`, `Interior1.json` → `interior_1`). Each layer is stored as one row grid. All maps are preloaded.
- Input the data can't contain (e.g. an `Item` trigger, which no file has) is a conversion error naming the file. Content the schema can't express yet (e.g. `Battle` triggers) is skipped and listed in `conversion-report.txt`.
- Part 4 ends the exception: the new editor writes `content/` directly and the converter is deleted.

## Consequences
- Every map edit in the legacy editor needs a converter run (`dotnet run --project tools/Jrpg.ContentConverter -- Assets content`) before the game sees it. Read the report after each run.
- Renaming a map or object in the legacy editor changes its id: the map's settings file is orphaned (loading fails loudly) and saved progress keyed by the old id is dropped by save repair.
- Event ids are `{uid}_{index}`, so reordering an object's events in the legacy editor changes them, and a "once" event can run again after loading an older save. The part 4 editor must keep ids stable.
- The converter writes with the same `ContentJsonOptions` the loader reads with, and an end-to-end test converts the real `Assets/` and loads the result, so the two can't drift.
