# LK2. Status look

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Screens), "When things happen in a turn" (events table). Look at `docs/ui_ux_exemples/Combat1.webp` and `Combat3.webp` first: the enemy's shield and weakness icons, and the small icons beside a member's panel.

- **LK2. Status look: small marks with turns left; tick and backfire pops** (Game, about 220 lines)
  - **Goal:**
    - FT1's plain tags become small diamond marks with a letter and the turns left as a small number. They sit beside an enemy's tag, as Octopath's weakness row does, and left of a member's panel, as its buff icons do.
    - Harmful statuses (Stun, Exposed, Poison) have a warm red rim; helpful ones (Guarded, Regen, Taunt) a cool blue rim.
    - A new mark pops in (scale 1.3 → 1 over 0.2 s); an ending one fades.
    - Tick and backfire numbers rise like impact numbers: Poison purple-tinted, Regen green.
    - The rogue stays forward through a quick skill instead of stepping back and forward.
  - **New types:** Game `Battle/StatusMarksView` (internal sealed): one combatant's status marks, from `StatusTagsText`, with the pop and fade.
  - **Changes (Game):**
    - `StatusTagsText` exposes each tag's kind and turns;
    - `PartyStatusView` and `EnemyTagView` use `StatusMarksView`;
    - `UiTheme` gets the rim colors and tick tints;
    - `BattleTurnSequence`: after a quick skill, the same actor goes straight to `AwaitingCommand`. It reads that from Core, with no rule of its own: the same `CurrentActor` and a `PartyTurn` phase.
  - **Tests:**
    - `StatusMarksViewTests`:
      - `PopScale_JustApplied_StartsAt1Point3`
      - `PopScale_AfterPopTime_Is1`
    - `BattleTurnSequenceTests`: `Update_AfterQuickSkill_StaysForwardAndAwaitsCommand`
    - `FloatingNumberTests`: `ColorOf_PoisonTick_IsPoisonTint`
  - **Verify by hand:**
    - Stun, expose and poison an enemy: three marks with counts.
    - Aura: a blue mark on each covered panel.
    - Poison ticks rise purple.
    - Lazarus stays forward after Ready.
