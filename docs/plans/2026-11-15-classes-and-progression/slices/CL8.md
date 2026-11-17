# CL8. The Resources tab

Editor, about 60 lines (about 40 of them tests) plus about 120 of XAML. Needs CL7. Design: section 3; decision 950.

## Goal
A Resources tab: the class's resources left (Add, Remove, Up, Down under the list), the chosen one's form right in a
scroll viewer: General (id, read-only once saved; name; color text with a swatch beside it; Lasting), Amount (From
speed box; Max and Starts at, greyed while from speed; Speed per charge, greyed otherwise), Gains (the eight boxes in two
columns, each with the tooltip from `ResourceGains`' own summaries: "At the end of each of the holder's own turns",
...).

## New types
- `Party/HexColorBrushConverter` (`IValueConverter`): a `#RRGGBB` text to a brush for the swatch, transparent while
  unreadable, so a half-typed color never raises a binding error. One-way; `ConvertBack` throws
  `NotSupportedException`.

## Changes
- `Party/ClassCatalogPage.xaml`: the Resources tab; the converter as a page resource.
- `docs/map.md`, Editor `Party` row: the Resources tab.

## Tests
- `Party/HexColorBrushConverterTests` (new): `Convert_SixHexDigits_GivesThatColor` (`#5B8CFF` gives 0x5B, 0x8C,
  0xFF); `Convert_HalfTyped_GivesTransparent` (`#5B8`); `Convert_NoHash_GivesTransparent`;
  `Convert_NotHex_GivesTransparent` (`#GGGGGG`).

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `mage`, Resources tab: Mana (blue swatch) and Clarity (lasting ticked, its per battle fought
   -1). The id box is read-only.
3. Choose `rogue`: Readiness has From speed ticked, Max and Starts at greyed, Speed per charge 10.
4. Type `#5B8` in Mana's color: the swatch empties and the line names Data's color rule; no binding-error line. Type
   `#FF0000`, Apply, Save all, run the game as Omer: the Mana pips are red in battle and on the Status page. Ctrl+Z,
   Save all to restore.
5. Add a resource to `fighter`: id `focus`, name `Focus`, max 3: Apply, Save all: `classes.json`'s fighter lists it
   after Stamina with no `isLasting` or rule fields. Ctrl+Z, Save all.

- **Note from CL7:** CL7 was split in three: the gain boxes (CL7), one resource as typed (CL7b,
  `Party/ClassResourceViewModel`) and the class's list (CL7c, `ClassViewModel.Resources`); this slice needs CL7c. The
  gain boxes read their gains through `Typed` (null while a box doesn't read), not a throwing `ToGains`.
- **Note from CL7 review:** the Gains labels on the tab use the names `ResourceGainsBoxes.Problem` uses ("Per turn" ...
  "Per battle fought", "Heavy hit share"), so the problem line names a box the form shows.
- **Note from CL7c:** CL7c was split; the list's selection and its Add, Remove, Up and Down are CL7d, which this slice
  now needs.
