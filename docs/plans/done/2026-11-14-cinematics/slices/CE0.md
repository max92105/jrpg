# CE0. `ContentWriter`'s catalog list into `ContentWriter.Catalogs.cs`

Data, about 60 lines moved, none new. A pure move (the backlog line on `ContentWriter.cs`: "the catalog after drop
tables moves the catalog list into a `ContentWriter.Catalogs.cs` partial first").

## Goal
`Data/Content/ContentWriter.cs` (295 after CM3) has room for CE1: the catalog checks and the catalog files' bytes leave
it for a partial, unchanged.

## Changes
- New partial `Data/Content/ContentWriter.Catalogs.cs`, summary "The catalog files the writer writes: each checked as
  the loader checks it, then serialized sorted by id; one place to add a catalog." It takes `CheckCatalogs`,
  `SerializedCatalogs` and the eight `Sorted...` helpers, as they are, and the `CatalogWrite` record (its only users
  move with it, beside `CheckedCatalogWrites`, which stays: it reads the stamps).
- `ContentWriter.cs`: those members leave (about 295 → about 240).
- `docs/plans/backlog.md`: the `ContentWriter.cs` line is removed (done).
- `docs/map.md`: the Data `Content` row names the partial beside `.ArtImports`.

## Tests
None new. Every `ContentWriter*Tests` and `ContentWriterRepositoryTests` pass unchanged (runner totals for
`Jrpg.Data.Tests` before and after, in the slice's notes).

## Verify by hand
None: no behavior changes.

- **Done:** as planned: `ContentWriter.cs` 295 → 237, `ContentWriter.Catalogs.cs` 69; `Jrpg.Data.Tests` 880 tests before and after.
