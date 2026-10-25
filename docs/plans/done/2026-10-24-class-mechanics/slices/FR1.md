# FR1. Warrior becomes Fighter

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "What exists, checked" (the class id `warrior`). Decision 207.

- **FR1. Warrior becomes Fighter: the id `fighter` everywhere** (content + tests + docs, about 110 lines, mostly mechanical)
  - **Goal:** the class is `fighter`, shown as "Fighter", in content, tests and current docs, with no alias. Saves hold no class id, so saves don't change. Record the solution's test count before and after.
  - **Changes:**
    - Content:
      - `classes.json`: `"id": "fighter", "name": "Fighter"`; the description keeps its meaning.
      - `party-members.json`: Ragnir's `classId` becomes `fighter`.
      - The barbarian's description loses "A fierce warrior" (placeholder text, decision 167); it becomes "A fierce brawler ...". Otherwise a reader takes the old class name for a description of the barbarian.
      - Grep `content/` for any other `warrior` (art names, maps, new-game): none expected.
    - Tests:
      - `ClassBuilder` and `ContentCatalogBuilder` default to `fighter`; `PartyMemberBuilder`'s default class id too.
      - Every test naming `warrior` as a class (`PartyEquipmentTests`, `ClassGrowthTests`, `ClassCatalogValidatorTests`, `PartyMemberTests` and others) follows. A test using "warrior" as a plain word for a member id renames it too, so a grep stays clean.
    - Docs:
      - `docs/map.md` if it names the id;
      - this plan's own pages already say Fighter.
      - Done plans are history and stay as they are.
  - **Tests:** `ContentCatalogLoaderTests`: `Load_RepositoryContent_RagnirIsAFighter`. Otherwise the existing tests, unchanged in number, are the guard.
  - **Verify by hand:**
    - `git grep -i warrior -- src tests content docs/map.md` finds nothing.
    - The leader choice shows Ragnir, "FIGHTER".
    - An existing save with Ragnir loads.
- **Done, with deviations:** as planned. Tests 1782 before, 1783 after (the new `Load_RepositoryContent_RagnirIsAFighter`). `docs/map.md` didn't name the id. Two test comments that the rename touched were already over 120 columns; I rewrapped them. Review: the new test comment no longer names the old class, so the grep finds nothing.
