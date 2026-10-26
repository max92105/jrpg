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
