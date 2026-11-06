---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

No plan: Objects > Duplicate object (decision 517, recorded in the object composer plan's README, done folder),
2026-11-06. Round 1. Verdict: ready to commit.

- Open (Suggestion): ObjectComposing.CanEdit now gates Duplicate too (MainWindow.xaml binds both to it); rename to
  e.g. CanComposeChosenObject so the Duplicate binding doesn't read as a copy-paste slip.
- Open (Suggestion): docs/map.md's Composing/ObjectComposerWindow entry still says "Objects > New object and Edit
  object" only.
- Open (Suggestion): test name ForDuplicate_IdIsTypedAndTheTitleNamesTheCopiedObject lacks the Scenario part.
- Previous review (music fade fix) items not carried: different change; its open Warnings were the developer's call.
