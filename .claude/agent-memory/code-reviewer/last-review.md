---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice D1 (dialogue typing ticks). Round 1.
Verdict: Fix the Criticals, then commit. 1 Critical, 3 Warnings, 3 Suggestions, 1 Follow-up.

- Open (Critical): ExplorationScene.cs:84 AskAll(_presenter.Sounds) has no test; TextTyping is read only in EventPresenterSoundTests; deleted, the game is silent and all tests pass
- Open (Warning): at Normal (30 ms) and 60 Hz, multiples fall 3 or 4 frames apart, the 50 ms ones are dropped; ticks at chars 2,4,8,12,14,18,22, every 100 ms on average, slower than Slow (90) and Fast (75); developer's call on decision 422's 60 ms
- Open (Warning): TypingSound's clock and EventPresenter's _typing.Clear() are untested past the first tick; removing the reset, the += or the Clear passes; one Fast-speed test on the updates between ticks catches all three
- Open (Warning): TypingTicks.cs:22 `lastMultiple > 0` is dead: `lastMultiple > visibleBefore` with visibleBefore >= 0 already implies it
- Open (Suggestion): IsDue_SecondCharacterTyped_IsTrue and IsDue_GapReachedExactly_IsTrue make the same call; give the first a gap well above the minimum
- Open (Suggestion): EventPresenter.cs:51 voice param doc still says only "asked for"; it now also mutes the typing
- Open (Suggestion): docs/map.md Exploration row doesn't list TypingSound
- Follow-up: ExplorationScene.cs:117 the Menu press's confirm sound has no test (S3 code); not in the backlog
- Note: hand check is the developer's
- Fixed (from S4): PartyPage confirm asks and refusals tested; StatusPageSoundTests and QuestsPageSoundTests split, each with a two-update Clear test; InventoryPage rail and tab moves tested; IMenuPage.Sounds default gone; EquipmentPage `?.SelectedRow ?? 0` gone
- Dropped (from S4): InventoryPageSoundTests' reliance on the potion opening the list (a Suggestion, the slice is committed)
