# B1a. The encounter freezes the field's last frame and flashes

Part of [the UI second pass plan](../README.md). Decision 258. Design: [design.md](../design.md), "Tunables and placeholders" (`EncounterFlash`), "Key decisions" (the capture is the host's).

- **B1a. The encounter freezes the field's last frame and flashes before the fade** (Game, about 200 lines)
  - **Goal:** when a battle is asked for, the field's last frame is kept (B1b breaks it), the screen shows that frozen frame with a white flash (up to 85 % in 0.04 s, down in 0.08 s), then the usual fade to the battle plays over the frozen frame. The battle starts at black as today.
  - **New types:** Game `Flow/EncounterFlash` (internal sealed): the white flash's opacity at a time since an encounter was asked for. `void Start()`, `void Advance(TimeSpan delta)`, `float Whiteness`, `bool IsPlaying`.
  - **Changes (Game):**
    - `Flow/GameFlow`: on `BattleRequested`, it sets `WantsFrozenFrame`, starts the flash and holds the request; when the flash ends it hands the request to the fade (`FadeGate.StartBehindFade`). Scenes get no input meanwhile. `Draw` draws the frozen frame (once given) and the white over it, under the black. It stays at 4 constructor parameters (the flash is created inside) and near 175 lines.
    - `JrpgGame.Draw`: after the virtual screen is drawn, if the flow wants a frozen frame, copy the render target into a second 1920 x 1080 one the host owns (created once) and give it to the flow (`flow.FreezeFrame(Texture2D)`); the flow drops it when the battle has started. The update path never touches the device (ADR 0008).
  - **Tests:**
    - `EncounterFlashTests`:
      - `Whiteness_BeforeStart_IsZero`
      - `Whiteness_AtThePeak_IsTheMaximum`
      - `IsPlaying_AfterTheFlash_IsFalse`
    - A new `GameFlowEncounterTests` class (`GameFlowTests` is at 369 lines): `Update_BattleRequested_FlashesBeforeTheFade`, `Update_BattleRequested_WantsAFrozenFrame`. The existing battle-start tests in `GameFlowTests` step through the flash if they stop at a fixed time; they stay seed-independent.
  - **Verify by hand:** walk in the forest until an encounter: the field freezes, flashes white, fades, the battle starts; no flash when a battle ends, on a warp or when the menu opens.
- **Done, with deviations:**
  - **A seam for the host:** `JrpgGame` knows its root scene only as an `IScene`, so the new `Scenes/IFreezesFrame` (`WantsFrozenFrame`, `FreezeFrame`) is what it checks after drawing; `GameFlow` implements it.
  - **The flash runs before the fade's update** (`GameFlow.AdvanceFlash`), so the fade starts on the update the flash ends and one long update (the headless tests' 10 s steps) still reaches the battle; the existing battle tests pass unchanged.
  - **The white waits for the copy** (after review): with two fixed-step updates before the first draw the flash is already lit, so the flow draws the white only over the frozen frame, which the copy never contains.
  - **The host keeps its copy target** once made (about 8 MB), rather than freeing it after each transition; the flow drops its reference when the battle starts at black.
  - **Tests:** the three planned `EncounterFlashTests` and a fourth (halfway down); `GameFlowEncounterTests` has the two planned and `Update_FlashOver_NoLongerWantsAFrozenFrame`, reusing `GameFlowTests.StartedFlow` and `SessionOf` (now internal). 2166 in all.
