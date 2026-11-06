---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice CP2 (ObjectCopying, MainWindow.ObjectCopying, Edit menu Copy/Cut/Paste
objects), round 1 (2026-11-05). Verdict: fix the Criticals. CP1's two Suggestions not carried (new slice).
CP1 follow-up "Make on an empty list throws": fixed (CanPaste requires Copied.Count > 0; Copy reads only uids on map).

- Critical, open: MainWindow.xaml Edit menu Copy/Cut/Paste objects have no CommandTarget; with the map filter box
  focused (Ctrl+F, Enter), the Menu focus scope transfers the command to the TextBox: "Paste objects" pastes clipboard
  text into the filter (greyed with no text). Fix: CommandTarget="{Binding ElementName=MapViewport}" on the three.
- Warning, open: ObjectCopying.Paste has no off-map check; PlaceChosen refuses a center off the map. Ctrl+V over the
  viewport's empty area (or the view center panned off the map) pastes unseen objects that save. Test it.
- Suggestion, open: IsCopyingObjects reads as "a copy in progress"; name it for "the object commands apply"; the
  Placing-not-null clause is defensive (Placing exists whenever a map is open).
- Suggestion, open: Copy_ThenPasteCenteredAt_AddsCopiesWithTheirEventsInOneStep checks no step; drop "InOneStep".
- Follow-up: CanPaste checks only placed ObjectIds, not objects a copied event swaps to; save validation catches it.
- Follow-up (from SN1, still present): window key guards exclude only TextBox, so B/R/F/G eat a focused ComboBox's
  type-to-select.
- Follow-up (from MO2, still present): a wheel zoom mid Shift-drag runs DrawSelectionMarks, resetting the marks'
  transform until the next mouse move.
