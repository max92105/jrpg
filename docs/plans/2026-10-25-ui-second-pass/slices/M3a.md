# M3a. Status page content

Part of [the UI second pass plan](../README.md). Decision 260. Design: [design.md](../design.md), "Tunables and placeholders" (status sections). After C4. Look at `docs/ui-reference/menu-status.webp` (profile, then the character's actions in rows with a description line) and S0's Status captures first.

- **M3a. Status page content: level, class, speed, resources and skills** (Game, about 230 lines)
  - **Goal:** the Status page shows, for the shown member, what the reference's does in our game's terms:
    - the profile: name, level, class, speed beside HP, attack and defense;
    - a "Resources" section: each of the class's resources by name with its max, and for a lasting one its current amount out of its max (Clarity 2/3);
    - a "Skills" section: each skill the member has (`PartyMember.Skills`, in order) with its description on the line under it, like the reference's actions.
    - Experience and equipment stay.
  - All of it is read from Core as it is (`PartyMember.Speed`, `Skills`, `Class.Resources`, `LastingAmounts`): no Core change.
  - **Changes (Game):**
    - `Menus/StatusSheet`: new fields built with the sheet (texts formatted once): `Speed`, `Resources` (name and amount text per resource), `Skills` (name and description per skill).
    - `Menus/StatusSheetView`: the two sections under the parameters, with `SectionTitle`; a member with many skills: the section shows as many as fit and the page's height decides (no scrolling in this slice; note how many fit at SZ1's sizes).
    - `content/text/ui.json` and `Ui/MessageIds`: `status_speed`, `status_resources`, `status_skills`, `status_lasting_amount` ("{amount}/{max}").
    - Skill costs aren't shown: `Battle/SkillCostText` reads a battle's `ResourceMeters`. Say so in the Done note as a follow-up.
    - Watch `StatusSheetView`'s size: if it nears 300, the two sections go into a `StatusSectionsView`.
  - **Tests** (`StatusPageTests`, through `ShownStats`-style read accessors on the page):
    - `Open_Mage_ShowsManaAndClarity`
    - `Open_LastingResourceSpent_ShowsItsAmountOutOfMax`
    - `Open_Member_ListsTheClassSkillsInOrder`
    - `Open_Member_ShowsSpeed`
  - **Verify by hand:** open Status for the mage, the cleric and the fighter: their resources, Clarity or Faith as an amount after a battle, their skills with descriptions; Left and Right change all of it. F12, compare with `menu-status.webp`.
- **Done, with deviations:**
  - **One Core change, the developer's choice (2026-10-26):** `Core/Battle/ResourceStarts` is public, with no rule change, because the Rogue's Readiness takes its max from speed. The sheet reads each max from it, so the Status page and battles can't disagree. A lasting resource shows the member's `LastingAmounts` out of that max.
  - **Layout:** speed sits beside HP in Parameters. Equipment and Resources share a row, half the width each (no class has more than two resources). Skills fill the rest of the page. Review: each description wraps to the sheet's width (built when the sheet or the width changes) and skills stack by their real height, as many whole ones as fit. With today's two-line placeholder descriptions that is about 2 of a class's 3; shorter real descriptions fit 3. No scrolling.
  - **Accessors:** the page has one read accessor, `ShownSheet`, which replaces `ShownStats`. The speed caption and the lasting-amount format (`StatusLabels.LastingAmountText`) live in `StatusLabels`.
  - **Tests:** 5 new in `StatusPageTests`, including the Rogue's speed-scaled max. The spent-Clarity test goes through a save, since Core keeps lasting amounts only through a battle or a save. 2158 in the solution.
  - **Follow-up:** skill costs aren't shown, because `Battle/SkillCostText` reads a battle's `ResourceMeters`.
