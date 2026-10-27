# P4b. Worn gear adds to the sheet

Part of [the stats and gear slots plan](../README.md). Decisions 293, 295, 328. Design: "The stat sheet" (gear adds to primaries first). After P4a (split from P4 on 2026-10-27).

- **P4b. What a member wears adds to their sheet; max HP and speed follow it** (Core + Data, about 210 lines)
  - **Goal:** what a member wears adds to their primaries (and so to everything those convert into) and to derived stats directly. Putting on Constitution raises max HP, taking it off lowers it (P2's `Vitals.ResizeMax`); speed gear raises the rogue's readiness charges. Today's two slots still apply (nine come in G1a-2).
  - **New:** `Stats/PrimaryStats.Plus`, `Stats/Resistances.Plus`, `Stats/StatBonuses.Plus` (their first caller: P1 and P3b left them).
  - **Changes:**
    - `Party/MemberSheet.Of(ClassStats atLevel, StatBonuses worn, StatRules rules)`: gear primaries add before the conversions, gear on a derived kind after (in permille: a whole-percent bonus × 10; dodge and resistances still capped).
    - `Party/PartyMember`: the sheet is rebuilt on `Wear` and `TakeOff`; max HP follows it through `Vitals.ResizeMax` (decision 328). `LevelUp` raises vitals by the formula's gain from one level to the next with the same gear (P2 review: `Vitals.MaxHp` holds gear Constitution too).
    - `Session/GameSession.RestoreMember`: wears the saved items before `RestoreProgress` sets HP, so a member saved with Constitution gear keeps their HP.
    - `Data/Saving/SaveGameRepair`: clamps HP against the max with the repaired worn items' bonuses.
  - **Tests** (new ones in `MemberSheetTests`, `PartyMemberGearTests`, `GameSessionGearTests`, `SaveGameRepairGearTests`):
    - `MemberSheetTests: Of_PowerBonus_RaisesPowerAndDamagePercent`, `Of_FlatFireResistance_AddsAfterWill`, `Of_DodgeBonusPastCap_StopsAtCap`
    - `PartyMemberGearTests: Wear_ConstitutionArmor_RaisesMaxHpKeepsHp`, `TakeOff_ConstitutionArmor_ClampsHpToNewMax`, `Wear_SpeedBonus_RaisesSpeed`, `GainExperience_WearingConstitution_RaisesHpByTheLevelsGain`
    - `GameSessionGearTests: Load_HpAboveClassMaxWithConstitutionArmor_KeepsHp`
    - `SaveGameRepairGearTests: Repair_HpWithinMaxWithGear_KeepsHp` (a new file: `SaveGameRepairTests` is at 266)
  - **Watch:** `PartyMember.cs` and `SaveGameRepair.cs` (234): sizes in the Done note.
  - **Verify by hand:** equip chain mail on the fighter: the Status page's HP max rises by 3 % of the class HP; take it off at full HP: HP drops to the old max. Simulation table recorded (unchanged: the leader wears nothing).
- **Done, with deviations:**
