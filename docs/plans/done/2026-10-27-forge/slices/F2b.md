# F2b. Salvage, behind a prompt

Part of [the forge plan](../README.md). Decisions 354 (salvage asks to confirm), 358. After F2a. Was the salvage part of the draft's F2. Reference: `docs/ui-reference/dialog-confirm.webp`. Design: [design.md](../design.md), "Key decisions".

- **F2b. Salvage, behind a prompt** (Game + text, about 130 lines; reuses `Ui/ConfirmPromptView`, adds no view type)
  - **Goal:** Confirm on an available Salvage asks "Salvage the Bronze Sword?" (`Ui/ConfirmPrompt`, No first); Yes salvages through `Forge.TrySalvage`: the piece leaves the list, the materials panel counts the yield, the cursor stays on the same row (or the last), and a notice line says what it gave ("Salvaged: 2 Scrap, 1 Essence") until the cursor moves. No or Cancel keeps the piece. Cancel wins when nested: the prompt, then the actions, then the Forge. Confirm on a greyed Salvage does nothing.
  - **New types:** none.
  - **Changes:**
    - `Crafting/ForgeScene`: the prompt state; Yes calls `TrySalvage`, then `ForgeList.Show(row)` and `MaterialStock.Refresh()`, closes the actions and sets the notice; the notice clears on a move.
    - `Crafting/ForgeView`: the prompt through `ConfirmPromptView`; the notice in the reason line's place.
    - Text: `forge_salvage_confirm` ("Salvage the {name}?", a rolled piece's name with its rarity first), `forge_salvaged` ("Salvaged: {materials}"); `MessageIds`.
  - **Tests** (`ForgeSceneSalvageTests`, new file):
    - `Confirm_Salvage_AsksFirst` (the piece is still owned)
    - `PromptYes_Salvages_PieceGoneMaterialsUp`
    - `PromptNo_KeepsPiece`
    - `Cancel_InPrompt_ClosesThePromptOnly` (the actions stay open)
    - `Confirm_GreyedSalvage_DoesNothing` (the guard; a full Scrap stack makes Core refuse it: no prompt, nothing changes)
  - **Call sites:** none.
  - **Verify by hand:** salvage a plain Leather Cap: the prompt starts on No; Yes: the cap is gone, Scrap is one more, the notice says so; with 99 Scrap, Salvage is greyed "Stack full" and Confirm does nothing.
- **Done, with deviations:** the notice reuses the Salvage row's formatted materials instead of formatting the quote again. A rolled piece is named in the question through a `forge_rolled_piece_name` message ("{rarity} {name}") rather than a word order in code. The notice is drawn by `ForgeDetailView` (the view that owns the reason line), passed through both `Place`s. Added `Down_AfterSalvage_ClearsTheNotice`; the review added the cursor-row tests (a middle row stays, the last row moves to the new last) and clears the notice when another piece's actions open. Not captured (unattended loop): hand check yours. Tests 2895 → 2904.
