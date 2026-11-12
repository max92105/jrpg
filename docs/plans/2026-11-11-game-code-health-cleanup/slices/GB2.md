# GB2. A page's dialog dims the footer hint too

Game, about 5 lines, hand check. Builds on decision 554 and design.md, "Key decisions".

## Goal
When a menu page opens a dialog (the Party page's notice, the Save and System questions), its scrim dims the footer hint with the rest of the menu. Today `GameMenuScene.Draw` draws the hint after the page, so it stays bright over the scrim.

## Changes
- `src/Jrpg.Game/Menus/GameMenuScene.cs` (234): `DrawFooterHint(batch, opacity)` moves from after the page to right after the header and the tabs, before `DrawPage`. The footer bar is already drawn first (`DrawFrame`); the hint's text and place don't change, and no page draws over the footer area except a dialog's scrim.
- `docs/plans/backlog.md`: delete the PD1 line.

## Tests
None new: a draw order (decision 551). The runner's totals per project equal before and after (Done note).

## Verify by hand
1. Menu open on the tabs: the footer hint reads as before.
2. Party page, try something it refuses (move the leader to the reserve): the notice's scrim dims the footer hint like the header.
3. System, Quit: the question dims the hint; No brings it back bright.
4. Save page, choose a used file: the overwrite question dims the hint.
5. Opening and closing the menu: the hint fades in and out with the menu, as before.

- **Done:** as planned; `DrawFooterHint` runs right after the tabs, before the page, with a comment naming decision 554. The runner's total is 4610 before and after. The PD1 line went.
