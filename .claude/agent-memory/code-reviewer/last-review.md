---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 1c (Text on screen)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): FontStashSharp.MonoGame 1.6.2 (Zlib); Lato placeholder font; message table + fonts failure = console only, exit 1; art errors deferred to ArtReferenceCheck; JrpgGame takes IScene and GameFlow implements IScene; FontLibrary loads every .ttf in ordinal name order; underscore message ids.

- open (Warning): FontLibrary.Load - a corrupt/non-font .ttf makes FontStashSharp throw plain Exception("stbtt_InitFont failed"); IsStartupProblem doesn't catch it, so the game crashes unhandled without naming the file; wrap AddFont failure in ContentValidationException naming art/fonts/<file>
- open (Suggestion): FontLibrary.Font(int pixels) is a noun method; FontAt(pixels) / FontOfHeight reads as a query
- open (Suggestion): JrpgGame field `_game` / ctor param `game` of type IScene inside the Game class; `_root` / `rootScene` says what it is
- open (Suggestion): plan 1c checkbox and "Done, with deviations" note not yet written (Zlib license, Lato, console-only failure for table/fonts, GameFlow : IScene, font file order)
