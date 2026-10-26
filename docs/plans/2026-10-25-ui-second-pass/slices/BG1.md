# BG1. The first menu opening stalls and skips its fade

Part of [the UI second pass plan](../README.md). Decision 259. Design: [design.md](../design.md), "Risks" (the candidates).

Developer: "when I press Esc for the first time to open the menu, the fade doesn't work, only the first time; it feels like a lag."

- **BG1. The first menu opening stalls and skips its fade: find the cause, warm it up, test it** (Game, about 150 lines)
  - **Goal:** the first Esc opens the menu with the same fade as every later one, with no visible stall.
  - **Steps:**
    1. **Measure.** In a Debug run, time the first and second menu openings: the update and draw time of the frames around the press (a temporary `Stopwatch` and `DeveloperLog` lines, removed before the commit), and the deltas `GameMenuScene` receives on its first updates. Write the numbers in the slice's Done note.
    2. **Name the cause.** Candidates:
       - the menu's background (`GameMenu.png`) decoded from disk on first draw (`TextureLibrary.Get`);
       - `UiTextures` baking on first use: the tabs, the scrim, gradients, and `DiamondPortrait` cutting each sidebar member's 1024 px portrait (`GetData`, `DiamondCrop`);
       - FontStashSharp rasterizing glyphs at sizes not used before;
       - the fixed step catching up after one long frame: several updates of 1/60 s arrive at once, so `_sinceOpened` passes the 0.15 s fade before anything is drawn (the "fade doesn't work").
    3. **Fix it at its source**, likely both:
       - warm up what the menu bakes on first use when the map scene loads (the background, the tab and gradient textures, each party member's diamond portrait at the sidebar's size, the menu's fonts at their sizes), through one method on the type that owns each (`UiTextures`, `TextureLibrary`), called from where the textures and fonts are available (`ExplorationScene`'s first draw or the host's content load); never in `Update` (ADR 0008);
       - if the catch-up is part of it, the menu's fade starts on its first drawn frame, not its first update; or its first update counts at most one step of time.
  - **Tests:** one for the pure cause found, for example:
    - `GameMenuSceneTests`: `Update_FirstUpdatesAfterALongFrame_FadeStillStartsFromClear` (several 1/60 s updates before the first draw leave the opacity near zero), if the catch-up is a cause;
    - or `UiTexturesWarmUpTests` can't run without a device, so a warm-up fix is checked by the measurement instead, and the test covers the pure list of what is warmed (`MenuWarmUp.PortraitFiles_Party_AreEveryMemberPortrait`).

    If the cause has no pure part, say so in the Done note and rely on the measurement.
  - **Verify by hand:** start, walk a step, press Esc: the menu fades in like the second time; the first frame time is close to the others (the measurement); repeat after loading a save and after a battle.
