# I2b. The inn asks with its price

Part of [the gold and shops plan](../README.md). Question 222, decision 257. Reference: `docs/ui-reference/dialog-confirm.webp`. After I2a, I1 and G3 (`Ui/GoldText`). Design: [design.md](../design.md), "Key decisions", "Replacement mapping" (`ScreenParts`), "Risks".

- **I2b. The inn asks with its price; Yes pays and rests; short of gold, a message says so** (Game + text, about 220 lines; draws the existing `ConfirmPromptView`, adds no view type)
  - **Goal:** on a priced rest a question opens over the map, "Rest for 20 G?" (`Ui/ConfirmPrompt`). Yes pays through `InnStay.TryPay()`: the fade, then "The party rests."; when Core refuses, "You don't have enough gold." shows instead and the event continues unrested. No or Cancel continues the event without resting. A free rest asks nothing (as today).
  - **New types:**
    - `Game/Ui/ScreenParts(ScreenText Text, UiTextures Ui)`: internal sealed record; what a screen draws with, passed as one, so `EventPresenter` can build a `ConfirmPromptView` without a fifth constructor parameter (README, "Decisions").
  - **Changes:**
    - `Exploration/EventPresenter(ExplorationState, ScreenParts, PlayerSettings, VoiceChannel)`; builds a `ConfirmPromptView`; `Draw` draws the question while it is open (before the fade, over the map).
    - `Exploration/RestPlayback`: on an inn stay it asks first: `ConfirmPrompt? Question`; the answer decides: paid → the fade and the rest page; refused → the short-of-gold page; declined → done with no page. The presenter continues the event once the page is read or on a decline.
    - `Flow/SceneFactory.CreateExploration`: passes `new ScreenParts(_text, _ui)`.
    - Text `inn_question` ("Rest for {price}?", the price through `GoldText`), `inn_short_of_gold` ("You don't have enough gold."); `MessageIds`.
    - Tests: the 15 `new EventPresenter(` in `EventPresenterTests`, `EventPresenterRestTests`, `EventPresenterForgeTests`, `StartMapScenarios` take `new ScreenParts(text, new UiTextures())` (mechanical).
  - **Tests** (`EventPresenterInnTests`, new file; a test map whose innkeeper charges 20):
    - `Update_PricedRest_AsksWithThePrice`
    - `PromptYes_EnoughGold_FadesThenRests` (gold down by 20, HP full after the page)
    - `PromptYes_ShortOfGold_SaysSoAndLeavesThePartyUnrested`
    - `PromptNo_ContinuesWithoutResting`
    - `Update_FreeRest_AsksNothing` (the case the question doesn't apply to)
  - **Call sites:** `new EventPresenter(` (16).
  - **Watch:** the Confirm that answers must not also turn the page that follows (the presenter's "the press that started it came an update earlier" rule); `EventPresenter` stays under 280.
  - **Verify by hand:** with a content copy whose innkeeper charges 20 (I3 lands the real one): talk, "Rest for 20 G?", Yes: the fade, the rest, the header shows 80 G; spend down to 10 G and try again: "You don't have enough gold."; No: back to the map, unrested.
