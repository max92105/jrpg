# MB0. `ArtReferenceCheck`'s battle art into `ArtReferenceCheck.Battle.cs`

Data, about 70 lines moved, none new. A pure move. Decision 642. Needs the cinematics plan's CA0 (and runs after its
CM2, which adds the cinematics' stills to the same file).

## Goal
`Data/Art/ArtReferenceCheck.cs` (261, about 267 after CM2) has room: MB1's battler strip check (about 20 lines) would
take it near 290 and the next art kind past 300. Its battle art moves to a partial, unchanged.

## Changes
- `ArtReferenceCheck` becomes `public static partial class` (both files).
- New `Data/Art/ArtReferenceCheck.Battle.cs`, summary "The battle art the startup check reads: party members' and
  enemies' battle sprites, enemies' portraits, attack effects and their strips." It takes, as they are, the lines of
  `FindProblems` from `memberBattlers` to `AddPartialFrameStrips(...)` as one private `AddBattleArtProblems(problems,
  catalog, artRoot, members)` called where they stood (the order of problems unchanged), `AddPartialFrameStrips` and
  `IsRowOfSquareFrames`.
- `docs/plans/backlog.md`: the plan's out-of-scope lines (README "Out of scope").
- `docs/map.md`: the `Art` row names the partial.

## Tests
None new. Every `ArtReferenceCheck*Tests` passes unchanged (runner totals for `Jrpg.Data.Tests` before and after; the
problems' order is pinned by the existing tests that list several).

## Verify by hand
None: no behavior changes.

- **Done:** as planned (`ArtReferenceCheck.cs` 262 → 229, the partial 51; `Jrpg.Data.Tests` 890 before and after). Review: no
  Critical or Warning; ADR 0047 now says the strips, not the GIFs, are listed in the manifest.
