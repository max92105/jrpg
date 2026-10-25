# CS1. Skills come from the class

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Core changes" (Party), "Replacement mapping". ADR 0020.

- **CS1. Skills come from the class; members lose `skillIds`** (Core + Data + content, about 240 lines, about 60 of them test-site edits)
  - **Goal:**
    - A member's skills are their class's, in the class's order.
    - The cleric (Zanyr) has Heal, Recovery and Heal All, and the mage (Omer) Fire Blast and Fireball, all still costing MP until MP1.
    - Sarxian has none until WL1.
  - **Changes:**
    - Core:
      - `Party/ClassDefinition` gains `IReadOnlyList<string> SkillIds` (empty when absent).
      - `Party/PartyMemberDefinition` loses `SkillIds`.
      - `Party/PartyMember`: `SkillsOf` reads the class's ids. `Recruit` and `Restore` keep their signatures. The private `_class` becomes a public `Class`, which M1a reads.
      - `Party/PartyMemberProgress`'s summary: skills come from the class.
    - Data:
      - `ContentCatalogLoader` loads skills before classes.
      - `Party/ClassCatalogValidator.Validate(fileName, classes, ClassReferences references)`: every id in `skillIds` is a known skill, with no duplicates.
      - New `Party/ClassReferences(IReadOnlyDictionary<string, SkillDefinition> Skills)`: the loaded content a class refers to, a dictionary because M2a reads each skill's changes.
      - `PartyMemberReferences` loses `SkillIds`, and `PartyMemberCatalogValidator` its skill check.
    - Content: `classes.json` gains `skillIds` for the cleric (`heal`, `recovery`, `heal_all`) and the mage (`fire_blast`, `fireball`). `party-members.json` drops every `skillIds`.
    - Tests (mechanical; record the solution's test count before and after):
      - `ClassBuilder.WithSkills(params string[] ids)`. `PartyMemberBuilder.WithSkills` puts the ids on its class.
      - `CommandMenuTests`' heal cases move from Sarxian to Zanyr; its target-cancel case uses `heal`.
      - `ActionPlaybackTests` uses Zanyr's first skill.
      - `PartyMemberCatalogValidatorTests`' skill cases move to `ClassCatalogValidatorTests`.
      - `ContentCatalogLoaderTests`, `GameSessionTests`, `CombatantTests` and `BattleSkillAndItemTests` follow the builders (grep `SkillIds` and `WithSkills` first).
  - **Tests:**
    - `PartyMemberTests`:
      - `Recruit_ClassWithSkills_HasThemInClassOrder`
      - `Restore_ClassWithSkills_HasThem`
    - `ClassCatalogValidatorTests`:
      - `Validate_UnknownSkillId_ThrowsNamingClassAndSkill`
      - `Validate_DuplicateSkillId_Throws`
      - `Validate_NoSkillIds_Passes`
    - `ContentCatalogLoaderTests`: `Load_RepositoryContent_ClericListsHeal`
  - **Verify by hand:** as Zanyr, Skills in a fight lists Heal, Recovery and Heal All. As Sarxian, the Skills entry is greyed.
