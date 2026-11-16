---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD1 (Database menu moved to Shell/DatabaseMenu.xaml, Tag-dispatched through
MainWindow.Database's CreateDatabaseOpeners dictionary). 2026-11-16. Round 1. Verdict: Ready to commit.
No Critical, no Warning.

- Suggestion, open: OnDatabaseMenuClicked returns silently for an entry with no string Tag, so a later row's entry
  added without a Tag does nothing on click instead of throwing like an unknown title; match `MenuItem entry` and throw
  naming its Header when Tag isn't a known title.
- Checked: implicit MenuItem style lives in App-level EditorTemplates.xaml, so DatabaseMenu's StaticResource resolves at
  InitializeComponent and its Role=TopLevelHeader trigger still applies; no stale OnEdit...Clicked in Shell (the
  Enemies window's own OnEditDropTablesClicked is unrelated); MainWindow.xaml 287 lines.
- Carried from SL: three Suggestions (test comment wording, `with` split, unslowed twin) dropped; not this slice and
  Suggestions aren't carried.
