# LK2b. Tick tints and the rogue's step

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "When things happen in a turn" (events table). Split from LK2 on 2026-10-25; the status marks are LK2a.

- **LK2b. Tick numbers tinted by their status; the rogue stays forward through a quick skill** (Game, about 130 lines)
  - **Goal:**
    - Tick and backfire numbers rise like impact numbers: Poison purple-tinted, Regen green.
    - The rogue stays forward through a quick skill instead of stepping back and forward.
  - **Changes (Game):**
    - `UiTheme` gets the tick tints;
    - `BattleTurnSequence`: after a quick skill, the same actor goes straight to `AwaitingCommand`. It reads that from Core, with no rule of its own: the same `CurrentActor` and a `PartyTurn` phase.
  - **Tests:**
    - `BattleTurnSequenceTests`: `Update_AfterQuickSkill_StaysForwardAndAwaitsCommand`
    - `FloatingNumberTests`: `ColorOf_PoisonTick_IsPoisonTint`
  - **Verify by hand:**
    - Poison ticks rise purple.
    - Lazarus stays forward after Ready.
- **Done, with deviations:**
  - `FloatingNumber` carries a Game-side `FloatingNumberTone` (Damage, Heal, Poison) instead of Core's `EffectKind`, and its color moved from the view to `FloatingNumber.ColorOf`. Regen keeps the existing heal green; only Poison has a new tint, `UiTheme.FloatingPoison`.
  - Backfire numbers stay white, as damage: the slice's "tick and backfire" color is the tick's only.
  - The two poison playback tests now expect the Poison tone.
  - `BattleTurnSequence` keeps the actor forward when Core still has the same party member acting after the hold; the turn count on screen doesn't change.
  - After review: the old Ready test is renamed `Update_AfterReady_ExtraTurnQueuesAfterTarget` and keeps only the queued-turn check. The actor also stays forward when Core gives the same member two turns in a row (an extra turn right before their own); that's the slice's rule, read from Core, and the comment says so. `ToneOf` throws on an effect kind with no amount, and a test pins the damage and heal colors.
