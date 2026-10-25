# LK1. Resource look

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Screens), "Key design decisions" 10, "Structure" (Game). Look at `docs/ui_ux_exemples/Combat1.webp` and `Combat3.webp` first (`.claude/rules/game.md`, "UI look").

- **LK1. Resource look: pips or a thin bar in the resource's color, spaced-caps label** (Game + Data + content, about 240 lines)
  - **Goal:** each resource reads at a glance, Octopath style:
    - **Small resources** (max ≤ `PipsUpTo`; all of today's) show as a row of small diamond pips, like Octopath's BP: lit in the resource's color per point, dim for the rest. A small spaced-caps label ("MANA") sits on the left, right-aligned with the HP row.
    - **Larger resources** keep M3's thin bar, in the resource's color.
    - **Two forces** (the druid) show as two pip rows. The dominant force's label is lit white; the other's stays secondary.
    - **Lasting resources** (Clarity, Faith) get a small hollow-diamond marker after the label, so the player sees they last until rest.
    - **A change** makes the changed pips flash once (0.25 s), timed from the stage change that refreshed them.
  - **New types:**
    - Game `Ui/PipRow` (internal sealed): draws N diamond pips, M lit, in a color. It's shared so a menu can reuse it, and depends only on `Ui`.
    - Game `Battle/ResourceGaugeView` (internal sealed): one member's resources (label, pips or bar, flash), reading `ResourceRowsText`.
  - **Changes:**
    - Core: `Resources/ResourceDefinition.Color` (string, `#RRGGBB`).
    - Data: `ResourceDefinitionValidator`: `color` is required and is `#` with six hex digits.
    - Content: `classes.json`: every resource's color (design.md, Screens).
    - Game:
      - `PartyStatusPanel` uses `ResourceGaugeView` instead of its resource `BattleVitalRow`s;
      - `UiTheme` gets pip and label sizes and a hex parser, run once when the battle opens;
      - `UiTheme.BattleResourceBar` goes.
  - **Tests:**
    - `ClassCatalogValidatorTests`: `Validate_BadResourceColor_ThrowsNamingClassAndResource`
    - `ResourceGaugeViewTests` (pure part):
      - `ShapeOf_MaxAtPipsUpTo_IsPips`
      - `ShapeOf_MaxAbove_IsBar`
      - `FlashShare_AfterChange_FadesOverFlashTime`
      - `LabelColor_DominantForce_IsLit`
    - `UiThemeTests`: `ColorFromHex_Valid_Parses`
  - **Verify by hand** (Combat1 and Combat3 beside the screen):
    - Omer's MANA as ten pips, filling as turns pass, and CLARITY with its lasting marker.
    - Thaali's two rows with the dominant one lit.
    - Jazog's pips flash when hit.
    - Five members fit beside the turn-order strip, and nothing blurs.
