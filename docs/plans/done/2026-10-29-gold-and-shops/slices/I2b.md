# I2b. The inn asks with its price

Part of [the gold and shops plan](../README.md). Question 222, decisions 257, 380 (the price drawn under the question with the coin), 382 (always asks; Yes without the gold shows the refusal). Reference: `docs/ui-reference/dialog-confirm.webp`. After I2a and T3 (`ConfirmPrompt.Price`). Design: [design.md](../design.md), "Key decisions", "Replacement mapping" (`EventLook`), "Risks".

- **I2b. The inn asks with its price; Yes pays and rests; short of gold, the refusal shows** (Game + text, about 220 lines; draws the existing `ConfirmPromptView`, adds no view type)
  - **Goal:** on a priced rest a question opens over the map, "Rest at the inn?", with its price under it (the coin and the number). Yes pays through `InnStay.TryPay()`: the fade, then "The party rests."; when Core refuses, "You don't have enough gold." shows instead and the event continues unrested. No or Cancel continues the event without resting. A free rest asks nothing (as today).
  - **New types:**
    - `Game/Exploration/EventLook(ScreenText Text, UiTextures Ui)`: internal sealed record; what a map event's pages, stills and the inn's question are drawn with, passed as one, so `EventPresenter` can build a `ConfirmPromptView` without a fifth constructor parameter. One user, so it stays in `Exploration/`.
  - **Changes:**
    - `Exploration/EventPresenter(ExplorationState, EventLook, PlayerSettings, VoiceChannel)`; builds a `ConfirmPromptView`; `Draw` draws the question while it is open (over the map, under the fade).
    - `Exploration/RestPlayback`: on an inn stay it asks first: `ConfirmPrompt? Question` (with `Price`); the answer decides: paid → the fade and the rest page; refused → the short-of-gold page; declined → done with no page. The presenter continues the event once the page is read or on a decline.
    - `Flow/SceneFactory.CreateExploration`: passes `new EventLook(_text, _ui)`.
    - Text `inn_question` ("Rest at the inn?"), `inn_short_of_gold` ("You don't have enough gold."); `MessageIds`.
    - Tests: the 15 `new EventPresenter(` in `EventPresenterTests`, `EventPresenterRestTests`, `EventPresenterForgeTests`, `EventPresenterShopTests`, `StartMapScenarios` take `new EventLook(text, new UiTextures())` (mechanical).
  - **Tests** (`EventPresenterInnTests`, new file; a test map whose innkeeper charges 20):
    - `Update_PricedRest_AsksWithThePrice`
    - `PromptYes_EnoughGold_FadesThenRests` (gold down by 20, HP full after the page)
    - `PromptYes_ShortOfGold_ShowsTheRefusalAndLeavesThePartyUnrested`
    - `PromptNo_ContinuesWithoutResting`
    - `PromptYes_TheAnsweringConfirm_DoesNotTurnTheRestPage` (the page shows after the fade; the Yes press can't also close it)
    - `Update_FreeRest_AsksNothing` (the case the question doesn't apply to)
  - **Call sites:** `new EventPresenter(` (16).
  - **Watch:** the Confirm that answers must not also turn the rest page or the refusal page that follows (the presenter's "the press that started it came an update earlier" rule; `PromptYes_TheAnsweringConfirm_DoesNotTurnTheRestPage` guards it). `EventPresenter` stays under 280.
  - **Verify by hand:** with a content copy whose innkeeper charges 20 (I3 lands the real one): talk, "Rest at the inn?" with 20 and the coin, Yes: the fade, the rest, the header shows 20 less; spend down to 10 G at the shop and try again: "You don't have enough gold."; No: back to the map, unrested.

- **Done, with deviations:** as planned; `RestPlayback` holds the stay and its question as one value (`_asking`) and exposes `IsDeclined` and `LineId` (the party rests, or the refusal), so the presenter's `StartReading` picks the rest page's line from it; a decline continues the event on the update after the answer. `EventLook` lives in `Exploration/`. Tests 3130 → 3136; the 16 call sites take `new EventLook(...)`. Review: `RestPlaybackTests` pin which line the page says (the party rests, the refusal) and the decline.
