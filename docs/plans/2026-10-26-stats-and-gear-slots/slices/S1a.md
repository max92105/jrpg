# S1a. The Status page's Combat, Resistances and Resources

Part of [the stats and gear slots plan](../README.md). Decisions 296, 316, 334, 337. Revised on 2026-10-27 against the Status page F1 redid (decision 334): the developer approved the layout below, then the split of S1 into S1a and S1b (S2 folded into S1b). Reference: `docs/ui-reference/menu-status.png`.

The approved layout (S1b adds the Attributes column beside the profile):

```
┌──────────┬───────────────────────────┬─ ATTRIBUTES (S1b) ──────────────────┐
│ portrait │ Name   LV 1               │ POWER  15   +15 % damage …         │
│  160 px  │ Class                     │ …                                  │
│          │ EXP 0 ─────────── NEXT 100│                                    │
├─ COMBAT ──────────────────────────────┬─ RESISTANCES ──────────────────────┤
│ HP 250 / 250 (bar) · ATTACK · DEFENSE │ BLUNT · PIERCING · SLASHING (left) │
│ SPEED · ACCURACY · CRIT (chance ·     │ FIRE · ICE · LIGHTNING · LIGHT ·   │
│ ×damage) · DODGE · CROWD CONTROL      │ DARK (two sub-columns of four)     │
│                                       ├─ RESOURCES ────────────────────────┤
├─ SKILLS ──────────────────────────────┴────────────────────────────────────┤
```

Height (828 px under the 20 px top margin), with S1b's profile band: 216 (profile and Attributes) + 16 + 318 (Combat, 8 rows of 34 px under a 46 px band; Resistances 4 rows + Resources 2 rows beside it fit in 300) + 16 + 256 (Skills, 3 of about 70 px) = 822.

- **S1a. Combat, Resistances and Resources replace the Parameters and Equipment sections** (Game + text, about 230 lines)
  - **Goal:** the middle band of the Status page shows Combat (HP with its bar, Attack and Defense while the battle uses them (decision 337), speed, accuracy, crit chance and crit damage on one line, dodge, crowd control) beside Resistances (eight, two sub-columns of four) with Resources under them; the Equipment rows leave the page (the Equipment page shows what is worn). Values read through `StatSheet.ValueOf` and `Ui/StatText`, so gear shows. Rows are 34 px.
  - **New type:** Game `Menus/StatusParametersView` (internal sealed): draws the Combat, Resistances and Resources sections of a `StatusSheet`.
  - **Changes:** `Menus/StatusSheet` (`Combat`, `Resistances` replace `Stats`, `Speed`, `Worn`); `Menus/StatusLabels` (the combat and resistance labels; slot labels go); `Menus/StatusSheetView` (the profile, then the band through `StatusParametersView`, then Skills; a `ContentHeight` the layout test checks); `ui.json` and `MessageIds`: `status_combat`, `status_resistances`, `status_crit`, `status_crit_value`, `status_crowd_control`, and a short name per damage type (`damage_type_blunt` …, which D3 reuses).
  - **Tests:** `StatusPageSheetTests`: the eight resistances in type order; crit chance as a percent; chain mail's slashing resistance shows; `StatusLayoutTests: ContentHeight_WithSkillsAndProfile_FitsThePage`; the `StatusPageTests` reading speed, the old stat labels and the worn rows change (counted in the Done note).
  - **Verify by hand:** Status for the fighter, the mage and the rogue: Combat and Resistances fit with nothing cut; chain mail raises the physical resistances; Left and Right change all of it. F12.
- **Done, with deviations:** the crit line reads "8 % · 157 %" (`status_crit_value` "{chance} · {damage}"), not "×157 %". HP's bar runs under its label, short of its value, so a 34 px row never puts the bar under the number. `StatusSheetView.ContentHeight` (786 with today's 176 px profile; 842 once S1b's 216 px band lands, of 848) is what the layout test checks against the page area's real height. `status_parameters` went (nothing reads it). Existing tests changed: four in `StatusPageTests` (HP and the attack and defense lines read `Combat`; speed and the caps labels found by their label through a `CombatValue` helper). From the review: the map's Status entries say what each view draws now; `StatusParametersView.MostResources` (2) with `StatusLayoutTests.Resources_EveryClassInContent_FitTheBand`, since a third resource would run into Skills; S1b's file notes the font sizes written in two views and the skills height estimate. Look not checked by me (machine busy): the developer's hand check. Tests: 2420 → 2425.
