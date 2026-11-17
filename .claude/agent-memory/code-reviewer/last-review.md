---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-items-uniques-affixes, slice IU8 (Editor Items/ItemCatalogPage, ItemCatalogViewModel IEditedCatalog +
IconFiles + MissingIconNote, ItemEditingViewModel icon reader, Catalogs/ArtPictureConverter moved from Cinematics, shell
wiring, DatabaseMenu, smoke script). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): Sprites/SpriteImageFile class summary still names only "the Cinematics page's still" for
  the at-width decode; the Items icon is its second caller.
- Suggestion (open, optional): ItemCatalogViewModel raises ChosenKey on selection and on every typed change; nothing
  subscribes (IEditedCatalog's hosts read it once), no other page raises it, no test pins it.
- Suggestion (open, optional): the empty MissingIconNote TextBlock keeps a line of height under the icon row (capture).
- Checked, no finding: Cinematics page keeps key "PictureOf", both bindings and PreviewWidth decode; empty icon is
  refused by ArtReferenceCheck.AddMissing (Path.Combine(...,"") -> folder, File.Exists false), item icons use AddMissing
  (not exact-name) so case-insensitive matches the game; editable ComboBox Text-bound like 6 other editable boxes.
- Fixed (from IU7): explicit Problem raise after SelectedItem in Insert/Remove gone; the "new list" test comment gone.
