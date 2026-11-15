# MB6. The GIF import's options dialog

Editor, about 150 lines (about 60 of them tests). Needs MB5. Decisions 637, 638.

## Goal
When the files chosen in **Import...** include a GIF, a small dialog asks first: **Battle height (px)** (128 to start,
a whole number from 16 to 1024), a note under it ("2x the GIF's 64 px: crisp" or "1.6x the GIF's 80 px: softened"), and
**Top-left color is background** (off). OK imports with these options; Cancel imports nothing. A boss is imported taller
by typing a larger height.

## New types
- `Editor/Battle/GifImportOptionsViewModel` (internal sealed partial, `ObservableObject`): the dialog's boxes as typed:
  `HeightText` (through `Catalogs/TypedNumber`), `RemovesBackground`, `Problem` ("Battle height must be a whole number
  from 16 to 1024."), `FactorNote` (from the first GIF's frame height, read when the dialog opens through a lookup;
  `FrameScaler.IsWholeFactor`), `CanApply`, `GifImportOptions ToOptions()` (throws while a problem shows).
- `Editor/Battle/GifImportWindow` (XAML + code-behind): the dialog; `EditorWindow` style; OK greyed through `CanApply`.

## Changes
- `EnemyCatalogWindow.xaml.cs` `OnImportClicked`: when a chosen file ends in `.gif`, opens the dialog first (its frame
  height from `GifReader.Read` of the first GIF, null when unreadable: the note is then empty and the import refuses
  it); Cancel returns before importing; the options go to the import.
- `docs/map.md`: the Editor `Battle` row names the dialog.

## Tests
`GifImportOptionsViewModelTests` (new):
- `New_Default_IsHundredTwentyEightAndKeepsTheBackground`.
- `Problem_HeightFifteen_NamesTheRange`; `Problem_HeightSixteen_IsNull`; `Problem_Height1024_IsNull`;
  `Problem_Height1025_NamesTheRange` (both bounds, both sides).
- `FactorNote_DoubleTheFrame_SaysCrisp`; `FactorNote_OddFactor_SaysSoftened`; `FactorNote_NoFrameHeight_IsEmpty`.
- `ToOptions_Typed_CarriesBoth` (192, background on: not the defaults).
- `PropertyChanged_CanApply_RaisedWhenHeightTyped`.

## Verify by hand
The developer's import of the five gnolls, click by click (GIFs in `art-src\gifs\`):
1. Run the editor. **Database > Enemies...**, choose `goblin`, **Duplicate**, type id `gnoll_warrior`, name "Gnoll
   Warrior" on the **Stats** tab (stats are yours, decision 640).
2. **Art** tab, **Import...** beside **Battle sprite**, open `GnollWarrior.gif`: the dialog shows 128 and the factor
   note. If MB3's finding says this GIF has a solid background, tick **Top-left color is background**. **OK**: the box
   shows `GnollWarrior.png`, Animated with its frames and time; the preview loops it. Tick **Flip** if it faces left
   (enemies face right, toward the party).
3. Repeat (Duplicate, id, name, Import) for `GnollArcher`, `GnollAxeWarrior`, `GnollShaman` at 128, and
   `HyenaWarlord` at a taller height (for example 192), ticking **Boss** on its Stats tab if it is one.
4. Type `15` in the height: the problem shows and OK greys. **Cancel** in the dialog: nothing is imported.
5. **OK** in the Enemies window, **Ctrl+S**. Add one `art-sources.json` line per strip (`battlers/GnollWarrior.png` and
   so on, `"sourceId": "generated"`, decision 611), or ask an agent to; `verify.ps1` fails on `ArtSourcesTests` until
   then.

- **Done, with deviations:** the range tests are two theories (15, 1025 and a word refused; 16 and 1024 taken), and
  `ToOptions_WithAProblem_Throws` is added. The dialog opens only for the battlers' Import... (another folder refuses a
  GIF by name). `GifImportWindow.Ask` shows it, as `MapFolderWindow.Choose` does. Step 2's background box: MB3 found
  `GnollArcher.gif` drawn on near-white, so tick it for that one only.
  Review: the window's `OptionsFor` renamed `AskGifOptions` (it opens the dialog; null is Cancel); the height box
  selects its 128 as it opens; reading the GIF's height also catches a file that can't be opened;
  `FactorNote_HeightTyped_IsRaisedAndReadsTheNewFactor` added.
