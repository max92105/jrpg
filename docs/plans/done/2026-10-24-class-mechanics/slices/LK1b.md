# LK1b. Resource look: pips

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Screens), "Key design decisions" 10, "Structure" (Game). Look at `docs/ui_ux_exemples/combat_player_info_exemple.webp` first (`.claude/rules/game.md`, "UI look"): the BP pips beside each name. Split from LK1 on 2026-10-25; the colors are LK1a.

- **LK1b. Resource look: pips or a thin bar in the resource's color, spaced-caps label** (Game, about 230 lines)
  - **Goal:** each resource reads at a glance, Octopath style:
    - **Small resources** (max ≤ `PipsUpTo`; all of today's) show as a row of small diamond pips, like Octopath's BP: lit in the resource's color per point, dim for the rest. A small spaced-caps label ("MANA") sits on the left, right-aligned with the HP row.
    - **Larger resources** keep M3's thin bar, in the resource's color.
    - **Two forces** (the druid) show as two pip rows. The dominant force's label is lit white; the other's stays secondary.
    - **Lasting resources** (Clarity, Faith) get a small hollow-diamond marker after the label, so the player sees they last until rest.
    - **A change** makes the changed pips flash once (0.25 s), timed from the stage change that refreshed them.
  - **New types:**
    - Game `Ui/PipRow` (internal sealed): draws N diamond pips, M lit, in a color. It's shared so a menu can reuse it, and depends only on `Ui`.
    - Game `Battle/ResourceGaugeView` (internal sealed): one member's resources (label, pips or bar, flash), reading `ResourceRowsText`.
  - **Changes** (Game; the colors come from LK1a):
    - `PartyStatusPanel` uses `ResourceGaugeView` instead of its resource `BattleVitalRow`s;
    - `UiTheme` gets pip and label sizes.
  - **Tests:**
    - `ResourceGaugeViewTests` (pure part):
      - `ShapeOf_MaxAtPipsUpTo_IsPips`
      - `ShapeOf_MaxAbove_IsBar`
      - `FlashShare_AfterChange_FadesOverFlashTime`
      - `LabelColor_DominantForce_IsLit`
  - **Verify by hand** (combat_player_info_exemple beside the screen):
    - Omer's MANA as ten pips, filling as turns pass, and CLARITY with its lasting marker.
    - Thaali's two rows with the dominant one lit.
    - Jazog's pips flash when hit.
    - Five members fit beside the turn-order strip, and nothing blurs.
- **Done, with deviations:**
  - The pips need no numbers, so `ResourceRowsText` and its tests are gone. `ResourceGaugeView` reads the meters itself.
  - A large resource (none in content today; the largest max is 10) shows only its thin colored bar, without M3's number.
  - Pip sizes sit in `PipRow` and label sizes in `ResourceGaugeView`, as design.md places them, rather than in `UiTheme`.
  - Two small records join `PipRow`: `PipRowLook` and `PipFlash`. `OutlinedFont.Draw` takes an optional letter spacing for the spaced caps.
  - The view's flash clock runs through a new `PartyStatusView.Update`, which `BattleScene` calls.
  - Test names: `ShowsPips_*` instead of `ShapeOf_*`, since the shape is a bool. Extra tests: the force label when not dominant, and `PipFlashTests`.
  - After review: the flashing range is `PipFlash.Between(before, after)`, tested for a rise and a drop; `LabelColor` takes only whether the resource dominates, since only a force can.
