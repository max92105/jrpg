# W1b. The titled framed window; yes/no prompts become a centered dialog

Part of [the UI second pass plan](../README.md). Decision 252. Design: [design.md](../design.md), "Tunables and placeholders" (titled window, yes/no dialog). After W1a and P1a. Look at `docs/ui-reference/dialog-confirm.webp`, `menu-skill-learned-dialog.webp` and S0's captures of the save, system and leader yes/no first.

- **W1b. The titled framed window with its separator; yes/no prompts become a centered framed dialog** (Game, about 220 lines)
  - **Goal:**
    - The framed window gains a title over a separator line that fades at both ends around a small diamond.
    - Every yes/no question (save over a file, back to the title, the leader choice) shows as a centered titled window: the question as its title, then Yes and No stacked, the selected one in the shared selection look (`DrawSelectedRow` and `MenuPointer`, decision 252). The screen behind is dimmed as today.
  - **Changes (Game):**
    - `Ui/FramedWindowLayout`: `static FramedWindowLayout Titled(Point center, Point bodySize, float titleLineHeight)`; `bool HasTitle`, `Vector2 TitleCenter`, `int SeparatorY`; `Body` starts under the separator.
    - `Ui/FramedWindowView`: draws the separator when the layout has a title (`UiTextures.Gradient` of a new `UiTheme.WindowSeparator`, a `Ui/Diamond` in its middle).
    - `Ui/ConfirmPromptView`: a titled layout 720 px wide around two stacked 260 x 50 buttons, 10 px apart; the question measured when it changes and the layout rebuilt then (a question wider than the window widens it; never wrap). `SelectionFill` goes from here; `UiTextures.SelectionFill` goes if unused.
    - `Ui/ConfirmPrompt`: Up selects Yes, Down selects No; Left and Right keep working. No stays selected first. Its summary says "Yes above No".
    - Callers (`SavePage`, `SystemPage`, `LeaderChoiceScene`) don't change (the backlog line on holding the prompt stays).
  - **Tests:**
    - `FramedWindowLayoutTests`:
      - `Titled_Body_StartsBelowTheSeparator`
      - `Titled_TitleCenter_IsAboveTheSeparator`
      - `Untitled_HasTitle_IsFalse`
    - `ConfirmPromptTests` (existing ones pass unchanged):
      - `Update_Up_SelectsYes`
      - `Update_DownAfterUp_SelectsNo`
  - **Verify by hand:** each yes/no asks in the centered window with its title and separator; Up/Down and Left/Right move the band and the diamond; Yes and No do what they did; Esc answers No. F12, compare with `dialog-confirm.webp`.
