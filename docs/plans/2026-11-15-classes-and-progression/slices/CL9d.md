# CL9d. The Rules group on the Resources tab

Editor, about 40 lines (about 25 of them tests) plus about 60 of XAML. Split from CL9 on 2026-11-17; needs CL9c.
Design: section 3; decision 951.

## Goal
The last part of the Resources tab: a Rules group under Gains, four check boxes each enabling its row of boxes, so no
resource needs `classes.json` by hand.

## Changes
- `Party/ClassViewModel.Resources.cs`: `IReadOnlyList<string> ResourceIds`, the class's resources' ids as typed,
  raised when a resource's id or the list changes: the strain and dominance boxes offer them (the resource's own id is
  Data's message if picked: "must name another resource").
- `Party/ClassResourcesView.xaml` (CL8's separate view, 131 lines): the Rules group under Gains, labelled as the
  problem line names the boxes ("Will per point", "Backfire chance per point", "Backfire damage", "Strain per missing
  point", "Dominance bonus"); the Will effect combo over `Enum.GetValues<WillEffect>()` (Core's list, ADR 0045's rule;
  a `WillScalingBoxes` choices property); the two id combos editable over the class's `ResourceIds` (reached up the
  tree, as the view's `DataContext` is the chosen resource); `{DynamicResource NumberBox}` for the page's style.

## Tests
- `Party/ClassResourceListTests`: `ResourceIds_IdTyped_RaisesChanged`; `ResourceIds_ResourceAdded_ListsIt`.
- `Party/ResourceRuleTests`: the effect choices are Core's every value.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `druid`, Resources: Growth's Dominance is ticked, over `decay`, 50; Decay's names `growth`.
   `warlock`'s Instability has Backfire 5 and 10 and Will scaling "lower gain". `mage`'s Clarity: Strain on `mana`, 1.
3. Untick Decay's dominance: the line gives Data's "doesn't name ... back" message, Apply greys. Tick it again: clear.
4. Apply without changes, Save all: `git diff content/classes.json` is empty (every rule wrote back as read).
5. On `fighter`'s Stamina tick Backfire, type 10 and 20, Apply, Save all: `classes.json`'s Stamina gains
   `"backfire": { "chancePerPointPercent": 10, "damagePercentOfMaxHp": 20 }` (one field per line) and the game starts.
   Ctrl+Z, Save all to restore.

- **Note from CL9b:** `StrainBoxes.LowersStartOf` and `DominanceBoxes.Over` are ids as typed, with no problem of their
  own: the combos stay editable as planned. A non-editable one would have to offer every value it may hold (the
  editor rule), so an id no longer on the class (a resource removed or renamed while new) stays listed.
- **Done, with deviations:** no `ClassViewModel.ResourceIds`: the review's runtime probe found the id combos, listing
  the class's ids while their text followed the chosen resource, emptied a rule's id on a class switch or a rename.
  Decision 1017: each id box lists its own choices (`StrainBoxes.LowersStartOfChoices`, `DominanceBoxes.OverChoices`,
  69 and 68 lines), the ids `OfferIds` gave and the id as typed (`Party/ResourceIdChoices`, 18), offered by
  `ClassViewModel.Resources` (134) at start, on an id typed and on a list change only. `WillScalingBoxes` (55)
  `EffectChoices`. `ClassResourcesView.xaml` (194): the Rules group, a check box per rule enabling its indented boxes,
  a wider `RuleLabel`, Core's effect names (`LowerGain`). Tests: `ClassRuleIdChoicesTests` (136, 12),
  `ResourceIdChoicesTests` (20, 3), `ResourceRivalRuleTests` (247) 4, `ResourceRuleTests` (277) the effect choices
  pinned. About 390 lines with XAML. The probe's repros keep every id; `open-editor-windows.ps1`: no `ERROR` line.
