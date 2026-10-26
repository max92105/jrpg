# MO1. The menu slides in and fades out

Part of [the UI second pass plan](../README.md). Decision 262. Design: [design.md](../design.md), "Tunables and placeholders" (`MenuMotion`), "Key decisions" (the closing waits for its fade). After BG1 and SZ1. Look at `docs/ui-reference/menu-tabs.webp` first.

- **MO1. The menu slides in and fades out (`MenuMotion`)** (Game, about 240 lines)
  - **Goal:**
    - Opening the menu, the bars and the picture fade in (0.15 s) while the tabs slide in from the left one after another (each 0.2 s, 0.03 s after the one above, from 32 px left).
    - Closing it (Cancel or Menu on the tabs), everything fades out over 0.12 s over the map (`SceneStack` draws the map under it), then the map is back.
    - Input works during the slide-in (a quick player isn't slowed); none is read while closing.
  - **New types:** Game `Menus/MenuMotion` (internal sealed): the menu's opening and closing over time: each tab's slide and opacity, the frame's opacity, whether the closing is done.
    - `void Advance(TimeSpan delta)`, `void StartClosing()` (a second call changes nothing), `bool IsClosing`, `float FrameOpacity`, `float TabOffset(int index)` (pixels left of its place), `float TabOpacity(int index)`, `bool IsClosingDone`.
    - Uses `Easing.ShareOf` and `Easing.CubicOut`.
  - **Changes (Game):**
    - `Menus/GameMenuScene`: `_openFade` and `_sinceOpened` go for a `MenuMotion` (keeping BG1's fix: the opening starts from clear on the first drawn frame). `UpdateTabs` starts the closing instead of returning `MenuClosed`; while closing, `Update` reads no input and returns `MenuClosed` once, on the update the closing is done. `DrawTab` offsets each tab by `TabOffset`.
    - It is at 196 lines; it should end near 220.
  - **Tests:**
    - `MenuMotionTests`:
      - `TabOffset_FirstTabAtOpening_IsTheFullSlide`
      - `TabOffset_SecondTabBeforeItsDelay_IsTheFullSlide`
      - `TabOffset_AfterTheEntrance_IsZero`
      - `FrameOpacity_HalfwayThroughClosing_IsBetweenZeroAndOne`
      - `IsClosingDone_BeforeClosingStarts_IsFalse`
      - `IsClosingDone_AfterTheClosingFade_IsTrue`
    - `GameMenuSceneTests`:
      - `Update_CancelOnTabs_RequestsMenuClosedOnceFaded` (null on the press, `MenuClosed` after 0.12 s, then null)
      - `Update_ConfirmWhileClosing_DoesNothing` (no page opens, `MenuClosed` still comes once)
      - `Update_ConfirmDuringSlideIn_OpensThePage`
      - The three tests that expect `MenuClosed` on the press (`Update_CancelOnTabs_RequestsMenuClosed`, `Update_MenuOnTabs_RequestsMenuClosed`, `Update_EscapeInPage_ReturnsToTabsWithoutClosing`) step past the closing through a helper `CloseAndWait(menu, button)`: about 30 lines.
  - **Verify by hand:** Esc on the map: the tabs slide in; Esc again: the menu fades out over the map, then walking works; opening and closing quickly never leaves the menu half-drawn; Back to title still fades to the title.
