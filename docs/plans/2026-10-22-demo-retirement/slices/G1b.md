# G1b. The prompt's look: key cap, panel and fade

Part of [the demo retirement plan](../README.md). Design: [design.md](../design.md), "Game structure added by this plan"; tunables "prompt panel" and "prompt fade". Look at `docs/ui_ux_exemples/` first (`.claude/rules/game.md`, "UI look").

- **G1b. The prompt's look** (Game, about 170 lines)
  - **Goal:**
    - The prompt is a small translucent dark pill with a hairline, in the menus' style, with the key in a small light key cap before the prompt word.
    - It fades in when a target appears and out when it goes, so the edge of an object's reach doesn't make it flicker.
    - Turning from one target to another fades the old one out, then the new one in.
  - **New types:**
    - `Exploration/PromptFade` (pure): the prompt's opacity over time as its target appears, changes and goes.
      - `Show(InteractionTarget target)`, `Hide()`, `Update(TimeSpan delta)`.
      - `Opacity` (0..1).
      - `Shown` (`InteractionTarget?`): the target being drawn, which stays during the fade-out.
  - **Changes:**
    - `Ui/UiTheme`: `PromptFill`, `PromptStroke`, `PromptTextSize`, `PromptPadding`, `KeyCapFill` (tunables). They are values, not the demo's names.
    - `Ui/UiTextures`: the pill and the key cap, baked once. Reuse the rounded-panel 9-slice if it fits at the prompt's radius.
    - `Exploration/InteractionPromptView`:
      - drives a `PromptFade` from `Update`;
      - draws pill, key cap, key and word at `Opacity`;
      - the texts are formatted for `Shown`, still only on a change.
  - **Tests:**
    - `PromptFadeTests`:
      - `Update_AfterShowAndFadeInTime_IsOpaque`;
      - `Update_HalfwayThroughFadeIn_IsHalfOpaque`;
      - `Hide_ThenUpdatePastFadeOut_IsTransparentAndShowsNothing`;
      - `Show_OtherTargetWhileShown_KeepsTheOldTargetUntilFadedOut`;
      - `Show_SameTargetAgainDuringFadeOut_FadesBackInFromCurrentOpacity`.
  - **Verify by hand:**
    - [ ] Side by side with `docs/ui_ux_exemples/`: the pill and key cap read as part of the menus' UI.
    - [ ] Walk past the chest's edge back and forth: a soft fade, no blinking.
    - [ ] Turn from the door to the villager: the old prompt fades, the new one appears.
    - [ ] Windowed small and full screen: the text stays sharp (whole pixels).
  - **Done, with deviations:**
    - The theme values are grouped like the message box's: `PromptPanel` and `KeyCap` (`PanelStyle`s), plus `PromptTextPixels`, `PromptPadding` and `KeyCapText`.
      - The text is the menus' 20 px rather than 16, since 16 reads small at 1920 x 1080. The padding is scaled to match: 10 x 4.
    - The pill and the cap reuse `PanelView`'s 9-slice, which bakes its own texture, so `UiTextures` is unchanged and the view keeps its G1a constructor. `PanelView.Draw` gains an opacity; `DialogueBox` passes 1.
    - `InteractionPromptView.Update` gains the update's `delta` for the fade.
    - One extra test: with nothing shown, the prompt stays transparent.
    - A screenshot shows "E  Talk" in the pill with a light key cap. The fade itself is for the hand check.
