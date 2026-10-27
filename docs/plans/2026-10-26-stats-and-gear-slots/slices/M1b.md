# M1b. "Weapon damage" on the Equipment and Status pages

Part of [the stats and gear slots plan](../README.md). Decision 337. Split from M1 on 2026-10-27. After M1a.

- **M1b. "Weapon damage" on the Equipment comparison and the Status Combat column** (Game + a small Core query, about 90 lines)
  - **Goal:** the Equipment comparison's Attack line becomes **Weapon damage**: the weapon's damage raised by Power, or the unarmed damage without one. The Status page's Combat column shows the same line. Core works the number out; the Game only draws it.
  - **Changes:**
    - Core: one public place that scales a raw value by a damage permille, which `Battle/EffectScaling` also uses, so the page's number is the one an attack lands with.
    - Game `Menus/StatComparison`: the Attack line becomes Weapon damage, scaled; `Menus/StatusSheet`'s Combat column the same.
    - `ui.json` and `MessageIds`: `stat_attack` "Attack" becomes the weapon damage's name.
  - **Tests:** the scaling place (Core); `StatComparisonTests` and `StatusPageTests` for the scaled number (a Power change alone moves Weapon damage).
  - **Verify by hand:** the Status page shows the fighter's Weapon damage; on the Equipment page, the bronze sword's line compares it with the unarmed one. F12.
- **Done, with deviations:**
