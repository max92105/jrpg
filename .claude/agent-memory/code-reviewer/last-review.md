---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice PM4 (first part: the Party members page, boxes and art marks; pictures
and imports split to PM4b). 2026-11-17. Round 1. Verdict: Ready to commit.

- PM3 Follow-up (Remove of a member applied earlier): fixed (restart shows it saved; editing test
  Start_AfterApplyingAnAddedMember_CannotRemoveIt).
- PM3 Follow-up (CombinedHistory at 291 lines): dropped, backlog.md lines 365-366 record it.
- Suggestion, open: "Not in the art folders: ..." sentence built twice (EnemyCatalogPage.xaml.cs:109,
  PartyMemberCatalogViewModel.cs:78); one BattleArtChoices method could build it.
- Follow-up, open: PM4b's TakeImport must raise MissingArtNote after ArtChoices.Refresh; picking a name the box already
  holds raises no Changed, so the note stays stale.
