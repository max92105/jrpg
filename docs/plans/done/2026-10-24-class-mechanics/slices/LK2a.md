# LK2a. Status marks

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Screens). Look at `docs/ui_ux_exemples/combat_targeting_ennemies_exemple.webp` and `combat_player_info_exemple.webp` first: the enemy's shield and weakness icons, and the small icons beside a member's panel. Split from LK2 on 2026-10-25, because LK2 came to about 375 lines; the tick tints and the rogue's step are LK2b.

- **LK2a. Status marks: small diamond marks with turns left, pop and fade** (Game, about 240 lines)
  - **Goal:**
    - FT1's plain tags become small diamond marks, each with a letter and the turns left as a small number. They sit beside an enemy's tag, as Octopath's weakness row does, and left of a member's name, as its buff icons do.
    - Harmful statuses (Stun, Exposed, Poison) have a warm red rim; helpful ones (Guarded, Regen, Taunt) a cool blue rim.
    - A new mark pops in (scale 1.3 → 1 over 0.2 s); an ending one fades.
  - **New types:** Game `Battle/StatusMarksView` (internal sealed): one combatant's status marks, from `StatusTagsText`, with the pop and fade.
  - **Changes (Game):**
    - `StatusTagsText` exposes each mark's kind and turns;
    - `PartyStatusView` and `EnemyTagView` use `StatusMarksView`;
    - `UiTheme` gets the rim colors.
  - **Tests:**
    - `StatusMarksViewTests`:
      - `PopScale_JustApplied_StartsAt1Point3`
      - `PopScale_AfterPopTime_Is1`
  - **Verify by hand:**
    - Stun, expose and poison an enemy: three marks with counts.
    - Aura: a blue mark on each covered panel.
- **Done, with deviations:**
  - `StatusTagsText` is gone rather than extended. `StatusMarksView` reads the `StatusSet` itself and keeps each mark (`StatusMark`) with its pop and fade clocks.
  - The status-name lookup moved to `StatusNames.IdOf`; `ActionPlayback` uses it for its cue.
  - The `battle_status_tag` message is removed, and `StatusTagsTextTests` became `StatusMarksViewTests`.
  - The marks' clocks run through new `Update` methods on `PartyStatusView` and `EnemyTagView`, which `BattleScene` calls.
  - Extra tests: the fade, which statuses are harmful, and the three ported refresh tests.
