# G1a. Members wear gear pieces, with a rarity and affixes from birth

Part of [the loot plan](../README.md). ADR 0029, decision 339. Design: "Gear pieces" (worn). After C2. Holds what was R1 (folded in on 2026-10-27).

- **G1a. Members wear gear pieces, with a rarity and affixes from birth** (Core + Game, about 250 lines: src about 110, tests 9 new at about 13 each, 45 lines of call sites; the top of the budget)
  - **Goal:** what a member wears is a `GearPiece` with a rarity and rolled affixes; the sheet sums each piece's base bonuses and affixes. Every piece in play is plain here, so nothing a player sees changes; tests build rolled pieces by hand. `PartyEquipment` still takes items (it wraps them as plain pieces and puts a removed piece's base back), until G1b-2.
  - **New types:**
    - `Items/Rarity`: the gear rarities, lowest first: Common, Magic, Rare, Unique (decision 339; Unique reserved).
    - `Items/RolledAffix(string AffixId, StatKind Stat, int Value)`: one rolled line on a piece, its value in the stat's bonus unit (points or whole percents, as item bonuses).
    - `Items/GearPiece`: one piece of gear as owned or worn. `Base`, `Rarity`, `Affixes` (roll order), `Kind`, `Damage`, `DamageType`, `Bonuses` (the base's plus each affix's, through `StatBonuses.Plus`). `static Plain(ItemDefinition item)` (Common, none) and `static Rolled(ItemDefinition item, Rarity rarity, IReadOnlyList<RolledAffix> affixes)` throw `ArgumentException` naming the item when it isn't equipment. `Equals`/`GetHashCode` compare base, rarity and affixes in order (a record compares a list by reference, as `PartyMemberProgress` notes).
  - **Changes:**
    - `Party/WornEquipment`: `Dictionary<EquipmentSlot, GearPiece>`; `Bonuses` sums `piece.Bonuses`; `WeaponDamage` from the weapon piece; `ItemIds` reads `piece.Base.Id` (saves stay by id until S1a); `With(slot, GearPiece?)`.
    - `Party/PartyMember`: `Weapon` and `EquippedIn(slot)` return `GearPiece?`; `SheetWith(GearPiece piece)` and `SheetWith(GearPiece piece, EquipmentSlot slot)`; `Wear(slot, GearPiece)`.
    - `Party/WornItem(PartyMember Wearer, GearPiece Piece)` (`Item` renamed).
    - `Party/PartyEquipment` (interim, gone in G1b-2): wears `GearPiece.Plain(item)`; puts `replaced.Base` back; `IsWorthTaking` compares pieces.
    - `Battle/MemberBattleStats`: `member.Weapon?.DamageType`.
    - `Session/GameSession.RestoreMember`: `member.Wear(slot, GearPiece.Plain(catalog.Items[itemId]))`.
    - Game: `EquipmentSheet.Worn` is `ImmutableArray<GearPiece?>` and `EquipmentSheetView` reads `piece.Base`; `EquipmentChoice.Highlighted` stays an `ItemDefinition?`, reading `WornAt(row)?.Piece.Base` (interim); `EquipmentComparison` compares `SheetWith(GearPiece.Plain(item), slot)` (interim); `SlotChoice`'s "no room" notice names `worn.Base.Name`.
  - **Tests:**
    - `GearPieceTests: Plain_Consumable_Throws`
    - `GearPieceTests: Rolled_Consumable_Throws`
    - `GearPieceTests: Plain_Sword_TakesItsDamageAndBonuses`
    - `GearPieceTests: Bonuses_AffixOnABaseStat_AddsUp`
    - `GearPieceTests: Bonuses_TwoAffixesOnOneStat_AddUp`
    - `GearPieceTests: Equals_TwoPlainPiecesOfOneItem_IsTrue`
    - `GearPieceTests: Equals_OtherAffixValue_IsFalse`
    - `GearPieceTests: Equals_OtherRarity_IsFalse`
    - `PartyMemberGearTests: Wear_RolledRing_AffixRaisesSheet`
  - **Call sites:** `EquippedIn`, `.Weapon`, `SheetWith`, `Wear` in about 45 test lines over 15 files (`Assert.Same(sword, member.Weapon)` becomes `Assert.Same(sword, member.Weapon?.Base)`); `WornItem.Item` in `EquipmentChoiceTests` and `PartyEquipmentTakeTests`.
  - **Watch:** `PartyMember.cs` 250 (signatures change, size doesn't).
  - **Verify by hand:** equip, take from another member and remove on the Equipment page; the Status page's numbers follow as before.
