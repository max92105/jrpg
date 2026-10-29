# F2a. A piece's actions, with Core's costs and refusals

Part of [the forge plan](../README.md). Decisions 351, 352, 354, 357, 370 (a refused action greyed with its reason). After F1d. Was the first part of the draft's F2 (split on 2026-10-29; the "can it be done" rules moved to Core's `Forge`, the review's blocker 3). Reference: `docs/ui-reference/shop-sell.webp` (a value column), `menu-equipment.webp`. Design: [design.md](../design.md), "Key decisions" (quotes, refusal order).

- **F2a. Confirm on a piece opens its actions, with Core's costs and refusals** (Game + text, about 200 lines; drawn by `ForgeDetailView`, adds no view type)
  - **Goal:** Confirm on a piece opens its three actions under an "Actions" band in the detail column: Salvage with its yield ("2 Scrap, 1 Essence"), Upgrade with its cost, Reroll with its cost. An action Core refuses is greyed, and the highlighted one's reason shows under the actions (highest rarity, no affix left, nothing to reroll, short of materials, a full stack; "nothing to salvage" for a rarity without a yield). Nothing is carried out yet (F2b, F2c: Confirm on an action does nothing here). Cancel closes the actions first, then the Forge (Cancel wins when nested).
  - **New types:**
    - `Game/Crafting/ForgeAction`: the three actions, in the order shown: `Salvage`, `Upgrade`, `Reroll`.
    - `Game/Crafting/ForgeActionRow(ForgeAction Action, string Label, string Materials, string? Reason)`: one action as shown, formatted once; `bool IsAvailable => Reason is null`.
    - `Game/Crafting/ForgeActionChoice`: the actions open on one piece, their cursor and gliding highlight. `ForgeActionChoice(GearPiece piece, Forge forge, IReadOnlyDictionary<string, ItemDefinition> items, UiMessages messages)`; `IReadOnlyList<ForgeActionRow> Rows`, `ForgeActionRow Selected`, `SelectionMotion Selection`, `MoveUp()`, `MoveDown()`, `Advance(TimeSpan)`. Asks `Forge.QuoteSalvage`, `QuoteUpgrade`, `QuoteReroll` when built: it decides nothing.
    - `Game/Crafting/MaterialListText`: a quote's materials as the player reads them ("2 Scrap, 1 Essence"). `static string Of(IReadOnlyList<InventoryEntry> materials, IReadOnlyDictionary<string, ItemDefinition> items, UiMessages messages)`.
    - `Game/Crafting/ForgeRefusalNames`: each `ForgeRefusal`'s message id, as `Ui/RarityNames` (`static string IdOf(ForgeRefusal refusal)`).
  - **Changes:**
    - `Crafting/ForgeScene`: Confirm on a piece opens the choice; Up and Down move it while open; Cancel closes it (then the Forge, as before).
    - `Crafting/ForgeDetailView`: the Actions band, the rows (label, materials right-aligned, greyed when refused, the shared selection), the reason line (design.md, F1d's arithmetic).
    - Text: `forge_actions`, `forge_salvage`, `forge_upgrade`, `forge_reroll`, `forge_material_count` ("{count} {name}"), `forge_material_separator`, and the reasons `forge_refused_highest_rarity`, `forge_refused_no_affix_left`, `forge_refused_no_affixes`, `forge_refused_short`, `forge_refused_stack_full`, `forge_refused_no_yield`; `MessageIds`.
  - **Tests** (`ForgeSceneActionTests`, new file; the session gets Scrap and Essence from the catalog):
    - `Confirm_OnPiece_OpensActionsWithTheirMaterials` (Salvage's line is the content's yield, formatted)
    - `Actions_RarePiece_UpgradeGreyedWithHighestRarity`
    - `Actions_PlainPiece_RerollGreyedWithNothingToReroll`
    - `Actions_NoMaterials_UpgradeGreyedShort`
    - `Cancel_InActions_ClosesActionsOnly` (the Forge stays open)
  - **Watch:** `ForgeActionChoice`'s constructor is at four parameters; a fifth means it does too much. The guard test for Confirm on a greyed action belongs to F2b and F2c, where Confirm does something.
  - **Verify by hand:** at the forge with no materials: Confirm on a plain piece: Salvage available with its yield, Upgrade greyed "Short of materials", Reroll greyed "Nothing to reroll"; on a Rare piece: Upgrade greyed "Highest rarity"; Cancel closes the actions, Cancel again the Forge.
