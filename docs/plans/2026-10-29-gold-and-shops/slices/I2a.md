# I2a. Move the rest's showing into `RestPlayback`

Part of [the gold and shops plan](../README.md). Decision 257 (the inn's beat). `Game/Exploration/EventPresenter.cs` is 258 lines, and I2b's question would take it near 300. Design: [design.md](../design.md), "Replacement mapping".

- **I2a. Move the rest's showing into `RestPlayback`** (Game refactor, about 110 lines, tests unchanged)
  - **Goal:** the rest step's showing (start the fade on its first update, wait for the screen to come back, then its page) moves out of `EventPresenter` into its own presenter, as `StillRun` holds the stills. No behavior changes.
  - **New types:**
    - `Game/Exploration/RestPlayback`: internal sealed; plays one rest step: the fade to black and back (decision 257), then says when its message may show. `Start()`, `Advance(TimeSpan delta)`, `bool IsFading`, `bool IsStarted`, `float Blackness`, `Finish()` (the page was read).
  - **Changes:**
    - `Exploration/EventPresenter`: `_restFade` and `_isRestFadeStarted` leave; `ShowRest` asks the `RestPlayback` and keeps showing the page through `ShowPages`; `Blackness` reads the playback's. Its summary keeps the rest's line.
  - **Tests:** existing tests pass unchanged (`EventPresenterRestTests`, `EventPresenterTests`, `StartMapVillagerTests`' innkeeper); counts equal before and after. `RestPlayback` is reached through the presenter's tests (a vacuous direct test adds nothing, the plan lessons).
  - **Watch:** the fade must still start on the step's first update and the page show once the fade is idle (the same update order).
  - **Verify by hand:** talk to the innkeeper: the fade out, a second of black, back in, then "The party rests." — as before.
