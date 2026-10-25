# M1b. Resources load from content; the mage has mana

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Content format", "Tunables and placeholders" (mana row). ADR 0019.

- **M1b. Resources load from `classes.json`; the mage has mana; the screen accepts the new event** (Data + content + Game, about 160 lines)
  - **Goal:**
    - The mage enters every battle with 3 mana (max 10) and gains 2 at the end of each of their turns.
    - Bad resource content stops the start with the class and resource named.
    - The battle screen plays `ResourceChanged` without a beat; mana shows on screen in M3.
  - **New types:**
    - Data `Resources/ResourceDefinitionValidator` (internal static): a class's resources: unique ids, non-empty name, max ≥ 1, 0 ≤ starts at ≤ max.
  - **Changes:**
    - Data: `ClassCatalogValidator` calls `ResourceDefinitionValidator`; problems name the class.
    - Content: `classes.json`, mage: `"resources": [ { "id": "mana", "name": "Mana", "max": 10, "startsAt": 3, "gains": { "perTurn": 2 } } ]`.
    - Game: `ActionPlayback.Read` gets a `ResourceChanged` case adding no beat.
  - **Tests:**
    - `ClassCatalogValidatorTests`:
      - `Validate_StartsAtAboveMax_ThrowsNamingClassAndResource`
      - `Validate_DuplicateResourceId_Throws`
      - `Validate_MaxBelowOne_Throws`
      - `Validate_EmptyResourceName_Throws`
    - `ContentCatalogLoaderTests`: `Load_RepositoryContent_MageHasMana`
    - `ActionPlaybackTests`: `Start_ResourceChanged_AddsNoBeat`
    - `BattleTurnSequenceTests`: `Update_OmerEndsTurn_NoThrowAndNextActorSteps` (repository content, fixed seed: the event reaches the screen).
  - **Verify by hand:** as Omer against the goblins: nothing visible changes, and nothing throws.
