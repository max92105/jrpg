---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice EN7 (Editor: EnemyCatalogWindow, Database > Enemies...,
TakeImport/Pick, TacticsSummary, SpriteImageFile.LoadFrom, ImportBattleArt). 2026-11-13. Round 1.
Verdict: fix the Criticals. Pick going through the view model is a developer decision, don't re-raise.

- EN6 items: BattleArtImport third copy fixed (ArtImageImport); ApplyTo rename fixed; MissingOf "(none)" fixed;
  BattleArtChoicesTests moved fixed.
- Critical (open): MainWindow.Database.cs:40 battler loader has no PngSize.IsReadable guard; an unreadable PNG in
  battlers named by an enemy throws from BitmapImage.EndInit in EnemyCatalogWindow.ShowArt and closes the editor.
- Warning (open): EnemyCatalogViewModel.TakeImport `SelectedEnemy?.` guards a state the greyed panel rules out
  (editor rule); use `SelectedEnemy!`.
- Warning (open): MainViewModel.ImportBattleArt copies ImportFieldSprites (third shell import body with
  ImportSheetImages); one shared ImportArtImages(artFolder, paths).
- Warning (open): SpriteImageFile class summary still says "A field sprite image".
- Suggestion (open): window's ShowArt after TakeImport redundant; ShowArt on every Problem reloads the PNG per keystroke.
- Suggestion (open): TacticsSummary test lives in EnemyCatalogImportTests; slice named EnemyViewModelTests.
- Suggestion (open): SpriteImageFile.LoadFrom `file.Length == 0` clause redundant with File.Exists.
- Follow-up: ArtStatus.OnCatalogChanged refreshes only on sheets/objects; an enemies step leaves the band stale.
