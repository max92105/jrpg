# S1b. Attributes beside the profile; the Status page's look

Part of [the stats and gear slots plan](../README.md). Decisions 296, 316, 334. After S1a (the layout and its arithmetic are in S1a's file). Holds what was S2 (spaced caps and the bands came with F1).

- **S1b. The five primaries beside the profile, each with what it gives; weaknesses and capped resistances tinted** (Game + text, about 170 lines)
  - **Goal:** the profile band narrows to make room for **Attributes**: each primary's value with a caption of what it gives ("+15 % damage", "+2 % crit, +1 % dodge", "+6 % status", "+14 % HP, +3 % physical resistance", "+2 % magical resistance"), worked out from `StatFormula` against a primary of 0 so a rate change in `rules.json` shows. A resistance below 0 reads in `UiTheme.Danger`, one at the 75 % cap in an accent color; captions in the secondary color. The profile band is 216 px (S1a's arithmetic).
  - **Changes:** `Menus/StatusSheet` (`Attributes`: value and caption per primary; a flag per resistance, weak or capped); `Menus/StatusSheetView` (the Attributes column, the profile narrowed); `Menus/StatusParametersView` (the tints); `ui.json` captions (`status_power_gives` …, one per primary); `Ui/UiTheme` (the capped accent if no existing color fits).
  - **Tests:** `StatusPageSheetTests`: Power beside its damage percent; a member wearing Power gear shows it in Power and its caption; a fire weakness is flagged; the layout test with the 216 px band.
  - **Watch (S1a review):** the label and value font sizes and the label spacing are written in both `StatusSheetView` and `StatusParametersView` (keep them in one place once Attributes adds a third reader); `StatusSheetView.SkillsHeight` guesses three skills of about 70 px rather than reading `StatusSkillsView`.
  - **Verify by hand:** Status for three classes: Attributes and their captions read beside the profile; the magic staff raises Intelligence and its caption; a negative `fireResistance` given in `items.json` shows red (undo after). F12, compare with `menu-status.png`.
- **Done, with deviations:**
