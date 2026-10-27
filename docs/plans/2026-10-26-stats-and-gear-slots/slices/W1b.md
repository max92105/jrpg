# W1b. Will raises Faith's max and shrinks Instability's gains

Part of [the stats and gear slots plan](../README.md). Decisions 291, 319, ADR 0019, ADR 0024 (lasting resources). Design: "Formulas" (`WillFormula`), "Tunables" (Will table), "Dead-stat check". After W1a.

- **W1b. Will raises Faith's max and shrinks Instability's gains** (Core + Data + content, about 170 lines)
  - **Goal:** the two classes W1a left without a Will effect get one (decision 319):
    - **A lasting resource may take `raiseMax`** (the cleric's Faith): its max rises with Will. Faith starts full, so a rest refills it to the raised max, and the amount a battle leaves is clamped to the raised max. Taking off Will gear lowers the max and clamps the kept amount.
    - **`lowerGain`** (the warlock's Instability): every positive change to the resource, from a skill's `resourceChanges` or a trigger, is lower by the Will points, never below 1 (so a skill still builds some). Drains (negative changes) are untouched.
  - **Changes:**
    - `Resources/WillEffect`: gains `LowerGain`.
    - `Party/PartyMember`: the lasting max per resource reads the Will-raised max (`KeepLastingAmount`'s clamp, `Rest`'s refill, `HasAnythingToRest`), through one private `LastingMaxOf(resource)`; the sheet's rebuild on a gear change clamps the kept amounts.
    - `Battle/ResourceStarts`: a lasting resource's max is the raised one.
    - `Battle/Combatant.ChangeResource` (the one place a change is applied): a positive change to a `lowerGain` resource is lowered first; `ResourceMeters` stays a meter.
    - `Data/Resources/ResourceDefinitionValidator`: allows `willScaling` on a lasting resource (`raiseMax` only); `lowerGain` on any resource.
    - `Data/Saving/SaveGameRepair`: clamps a lasting amount to the max with Will (gear included) rather than the definition's.
    - `content/classes.json`: faith `raiseMax`, instability `lowerGain` (willPerPoint 10).
  - **Tests** (new ones in `PartyMemberLastingResourceTests` and `ResourceWillTests`):
    - `PartyMemberLastingResourceTests: Rest_FaithWithWill_RefillsToRaisedMax`
    - `PartyMemberLastingResourceTests: TakeOff_WillGear_ClampsFaith`
    - `ResourceWillTests: ChangeResource_LowerGainThreePoints_AddsTheRest`
    - `ResourceWillTests: ChangeResource_LowerGainBeyondTheGain_AddsOne`
    - `ResourceWillTests: ChangeResource_LowerGainOnADrain_DrainsInFull`
    - `ResourceDefinitionValidatorTests: FindProblem_RaiseStartOnLasting_NamesWillScaling`
  - **Call sites:** `PartyMember`'s three uses of a lasting resource's `Max`/`StartsAt`; `SaveGameRepair` 1.
  - **Size:** about 55 Core, 20 Data and content, 6 tests about 80: about 160-170.
  - **Watch:** `PartyMember.cs` and `SaveGameRepair.cs`: sizes in the Done note.
  - **Verify by hand:** the cleric (Will 14) has 6 Faith after a rest; the warlock's Eldritch Bolt builds 1 Instability instead of 2. The Status page shows Faith out of 6.
- **Done, with deviations:**
