# LK1a. Resource colors

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Screens, resource colors). Look at `docs/ui_ux_exemples/combat_player_info_exemple.webp` first (`.claude/rules/game.md`, "UI look"). Split from LK1 on 2026-10-25, because LK1 came to about 400 lines; the pips are LK1b.

- **LK1a. Resource colors: each resource has its color, and its bar is drawn in it** (Core + Data + content + Game, about 170 lines)
  - **Goal:**
    - Every class resource has a color in content (design.md, "Screens").
    - M3's thin resource bars are drawn in their resource's color, not the shared muted gold.
  - **Changes:**
    - Core: `Resources/ResourceDefinition.Color` (string, `#RRGGBB`).
    - Data: `ResourceDefinitionValidator`: `color` is required and is `#` followed by six hex digits. The test fixtures that build resources gain a color.
    - Content: `classes.json`: every resource's color.
    - Game:
      - `UiTheme.ColorFromHex`, run once per resource when the battle opens;
      - each resource row's bar gradient comes from its color;
      - `UiTheme.BattleResourceBar` goes.
  - **Tests:**
    - `ResourceDefinitionValidatorTests`:
      - `Validate_BadResourceColor_ThrowsNamingClassAndResource`
      - `Validate_MissingResourceColor_Throws`
    - `UiThemeTests`: `ColorFromHex_Valid_Parses`
  - **Verify by hand:** in battle, Omer's mana bar is blue and Jazog's ferocity bar is red.
- **Done, with deviations:**
  - The bars use one shared grey gradient, `UiTheme.BattleResourceShade`, tinted with the resource's color through a new `StatBar` fill tint. That way a battle bakes no gradient texture per resource, since the texture cache keys on the gradient array.
  - `BattleVitalRow` now takes its `StatBar` ready-made instead of building one from a gradient.
  - The validator tests sit in `ResourceDefinitionValidatorTests`, with the bad colors as a theory over three cases. The Data fixtures that build resources gained a test color.
