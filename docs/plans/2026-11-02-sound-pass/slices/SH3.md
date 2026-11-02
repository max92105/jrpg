# SH3. A deal's sound as a purchase, a sale or the Forge's salvage goes through; the picker's count ticks

Part of [the sound pass plan](../README.md). After SH2 and Phase 2. Decisions 421, 430, 435, 437. Design: [design.md](../design.md), "Key decisions" 4.

- **SH3. A deal's sound as a purchase, a sale or the Forge's salvage goes through; the picker's count ticks** (Game + content, about 210 lines; adds no view type)
  - **Goal:** what a question's Yes carries out sounds, the question doesn't (decision 435):
    - **Deal:** the coins sound when Core accepts a deal, however it was confirmed: a stock piece bought at once, a stack bought or sold through the picker's Yes, a piece sold through the question's Yes. A refused deal is silent (decision 421).
    - **Picker** (a value chooser): Confirm on a stack row opens it and asks the confirm sound; a count change asks the move sound (a picker whose most is 1 stays silent); its Cancel asks the cancel sound.
    - **Sale question** (a yes/no question): opening it, moving between Yes and No and answering No are silent; its Yes asks the deal sound once the sale went through.
    - **Forge:** a salvage that went through (after its question's Yes) asks the deal sound too, so the rule is one across screens.
  - **New slot:** `effects/PlaceholderDeal.wav`: coins clinking into a purse or onto a counter. 0.6 s. A manifest entry, the WAV, the credits.
  - **Changes:**
    - `Ui/UiSoundFiles.Effects`: `Deal = "PlaceholderDeal.wav"` (its summary: "the interface's sounds, decision 421, and a deal or salvage gone through, decision 435").
    - `Trade/ShopCounter` (128 → about 145): no list of its own; `Buy(TradeRow row, AskedSounds sounds)`, `Sell(TradeRow row, AskedSounds sounds)` and `Update(InputFrame input, AskedSounds sounds)` write into the scene's, as `PlaybackSoundQueue.Follow(playback, sounds)` does. `Settle` asks `Effects.Deal` when Core dealt; `OpenPicker` asks the confirm; `Update` compares the picker's `Count` around its update and asks the move on a change, the cancel on a picker's No. The class summary gains the sounds sentence and the `sounds` parameters their doc ("the scene's list for this update").
    - `Trade/ShopScene` (about 275 → 276): passes `_sounds` to the counter's three calls.
    - `Crafting/ForgeScene` (241 → 242): `Salvage` asks `Effects.Deal` after `TrySalvage` succeeded.
  - **Tests:**
    - `ShopSceneDealSoundTests` (new; through the scene, what the flow reads):
      - `Update_ConfirmOnAStockPiece_AsksTheDealSound`
      - `Update_ConfirmOnAStackRow_AsksTheConfirmSound`
      - `Update_UpInThePicker_AsksTheMoveSound`
      - `Update_UpInAPickerOfOne_AsksNothing`
      - `Update_CancelInThePicker_AsksTheCancelSound`
      - `Update_YesInTheBuyPicker_AsksTheDealSound`
      - `Update_YesInTheSellPicker_AsksTheDealSound`
      - `Update_ConfirmOnAnOwnedPiece_AsksNothing` (the question opens silently)
      - `Update_NoInTheSaleQuestion_AsksNothing`
      - `Update_YesInTheSaleQuestion_AsksTheDealSound`
      - `Update_UpdateAfterADeal_AsksNothing` (the scene's clear covers the counter's asks)
    - `ForgeSceneSoundTests`: `Update_YesInTheSalvagePrompt_AsksNothing` becomes `Update_YesInTheSalvagePrompt_AsksTheDealSound`: decision 435 changes the rule it pinned (the music plan's S2), so its assertion changes on purpose; say so in the Done note.
  - **Report:** the chosen page, CC-BY flagged if so.
  - **Verify by hand:** Phase 3's listening (decision 437): at the shopkeeper with some gold: switching tabs, entering a list, moving, changing category with Q and R, switching the previewed member and backing out each sound once (SH2), as in the Forge; Right on Sell, Down in a one-row list and Confirm on a greyed row are silent. Buy a piece (coins at once), buy three potions through the picker (ticks as the count changes, coins on Confirm), sell a stack and a piece (the question is silent, coins on Yes). Back out of the picker (cancel) and answer No to the question (silent). Try something too dear: silent. At the forge master, salvage a piece: the question is silent, coins on Yes. Leaving the Shop sounds the cancel as it fades. A vetoed coins sound gets a shortlist of three page links; the pick replaces it before Phase 4.
