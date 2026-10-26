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
  - **The flow:** it holds the `BattleRequested` while the flash plays and doesn't update the scenes at all meanwhile, since the field is frozen. It advances the flash before the fade, so one long update that ends the flash also runs the fade it starts; that keeps `Update_DefeatThenMainMenu_EndsSession`'s 10 s step working unchanged. The white is drawn only over the frozen frame, so the frame the host copies holds none of it. The frame is dropped at black, when the battle starts. `IsFlashing` is internal, for the tests. 202 lines.
  - **The host:** `JrpgGame` keeps the root scene as a `GameFlow` when it is one. Right after the virtual screen is drawn, it copies it into a render target it creates on first use. It never sets the virtual screen as the target again in that frame, since a render target loses its contents when set.
  - **Tests:** 5 in `EncounterFlashTests` (with halfway-down and during-the-flash checks) and 2 in `GameFlowEncounterTests`. Their start-up helpers and the fade timings (`FadeOut`, `WholeFade`, `OneUpdate`) moved from `GameFlowTests` into `Flow/FlowStarts`, which both use through `using static`. Review: the flash-then-fade test steps at the host's rate for 0.55 s and asserts no battle yet, which a fade started with the flash would fail. 2180 in the solution.
