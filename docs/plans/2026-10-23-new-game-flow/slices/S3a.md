# S3a. Choice screen look: the list on the left, header and footer bars

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (screens), "Structure".

Look at `docs/ui_ux_exemples/Menu6.webp`, `Menu.webp` and `MainMenu.jpg` first.

- **S3a. Choice screen look: the list on the left, header and footer bars** (Game, about 170 lines; decision 169)
  - **Goal:** the choice screen gets the reference frame and list:
    - the title background under the menu scrim;
    - a header bar with "Choose your leader" at the top left;
    - a footer bar with the hint;
    - the vertical list on the left, each row with the name over the class in small spaced caps;
    - the highlighted row in the selected-row gradient between hairlines, with the `MenuPointer`.

    The information area stays plain until S3b.
  - **New types (Game):** `Ui/HeaderBar` (static), the bar along the top of the screen under which a screen writes its title. Its `Draw(SpriteBatch batch, Texture2D pixel, int height, float opacity)` mirrors `FooterBar`: `UiTheme.Bar` with a `UiTheme.BarLine` hairline at its bottom.
  - **Changes:**
    - `NewGame/LeaderChoiceScene.Draw`: frame and list per the screen tunables. Row positions and the upper-cased class labels are computed and measured once.
    - `Menus/GameMenuScene.DrawFrame`: its two header-bar lines become `HeaderBar.Draw`. This is a draw-only move: compare screenshots before and after with the game-window-only `play-game.ps1`.
    - `content/text/ui.json` and `Ui/MessageIds`: `leader_choice_hint` ("Choose who leads the journey."), drawn in the footer through `FooterBar.Area`.
    - `docs/map.md`: the Ui row lists `HeaderBar`.
  - **Tests:** none new. The draw is checked by hand; behavior is guarded by S1-S2's tests, and `MessageIdsTests` checks the new id.
  - **Verify by hand:**
    - Compare with `Menu6.webp`: thin dark bars top and bottom, list rows that fade out to the right, a blue gradient and hairlines on the highlighted row, the pointer, nothing boxed.
    - The game menu's header looks unchanged.
- **Done, with deviations:**
  - The frame follows the game menu: title text 28 px at a 48 px margin, footer hint at the menu's text size, and the class caps use the speaker label's letter spacing.
  - Rows now show the name over the class, so S2's "{name}, {class}" row label (`LeaderProfile.RowLabel`, `leader_choice_row`) went unused and was removed, along with the one assertion that read it.
  - The information area moved up to start under the header (top 140) and is otherwise as S2 left it.
  - No screenshots, before or after (privacy rule). `GameMenuScene`'s header is a draw-only move to `HeaderBar` with the same rectangles, and both looks are the developer's hand check. The review added `FooterBar.TextTop` (tested) for the hint's centering; the three older screens moving onto it is in the backlog. Tests: 1769 before, 1770 after.
