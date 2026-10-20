# Legacy WPF demo (reference only)

The first version of the game and its map editor: `JRPG/` (game), `MapEditor/` (editor), `JRPG.Core/` (shared code), `JRPG.sln`.

It is kept to read and port behavior from (decision 73 in `docs/plans/done/2026-10-18-shell-cutover-and-map-editing.md`). Nothing builds it, and it no longer runs: its art and map folder, `Assets/`, was removed once `content/` became the only source of content (ADR 0011). Git history has both, for example by checking out the commit before this folder was created.

Don't extend this code. New work goes in `src/` and `tests/`, built by `Jrpg.slnx` at the repository root.
