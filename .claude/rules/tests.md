---
paths:
  - "tests/**/*.cs"
---

# Rules for tests

- xUnit. One behavior per test. Arrange / Act / Assert, separated by a blank line.
- Name: `Method_Scenario_ExpectedResult`.
- Always seed randomness. Never `Thread.Sleep` or real time.
- Test through public APIs. If you need to reach into internals, the design probably needs a seam.
- Prefer small builders (`new PartyBuilder().WithHero(level: 5).Build()`) over large fixture files.
- Simulations in `tests/Jrpg.Simulations` assert on aggregate outcomes (win rate within a range, average turns), and print a short summary table.
- Never weaken or delete an existing assertion to make a test pass. If the expected value is wrong, say so and explain why.
- Every guard that turns an input into a no-op (a greyed entry, input before an entrance ends) has a test that presses it and asserts nothing happened.
- UI tests pick an entry by what it is (a helper that moves until `Selected` is the wanted entry), not by a fixed count of presses.
- `ListCursor` wraps: a "move until" helper fails when it comes back to its start row, never on "the cursor stopped moving" (the gold plan's T3 found two helpers that would have looped forever).
- Tests on the repository's content assert the entries they need (an id is there, a named layer exists), never totals of a whole catalog or map: content changes in the editor every day.
- A test that a value is refreshed or cached starts from a state where the old value differs from the expected one, and checks a value the runtime doesn't already share (not an enum name or a string literal).
- A timing or presentation step is tested through to the step that follows it in real content (stills then a warp, a still then a dialogue), and a hardware probe is checked against the library's source, not assumed.
- A rule with two sides (an `||`, a lower and an upper bound, a rise and a drop) gets one test per side; the class-mechanics reviews found the untested side four times.
- A validator or an editor `Problem` gets a test with an accepted value at each bound, not only the rejected ones: with 0 and max + 1 alone, a check refusing every value passes (the gold plan's I1).
- A test of which line, page or value was chosen asserts that value (its id, a number only it has), never that something is shown: "a message shows" can't tell the refusal from the rest (the gold plan's I2b, L2b).
- A test that needs a member without something (no skills, no lasting resource) builds one with a test class; it never picks a repository member that happens to lack it today, since content changes (the "actor without skills" test moved member three times).
- A value copied from one record into another (a sheet stat into `CombatStats`, a definition's number into a battle start) gets a test whose fixture differs from the destination's default: when the neutral test rules give the default, deleting the copy line passes every test (the H1 and H2 reviews).
- A new list or dictionary in a save or content record gets a null-entry test: the strict serializer lets a null element or value through, and the null surfaces as a crash later (five slices, last G1b).
- A test over a catalog's entries builds its own definitions (`TestQuests.Errand`, the builders) or picks a repository entry by id, never by position or because the repository holds one of them today (quests G1, G2, QE2, QE3).
- A line that passes something up to the host (a part's sounds to its scene, a scene's cue to the flow) gets a test through what the host reads, each parallel branch its own; a per-update clear gets a test with a second update (the music plan's S3, S4, D1, B4, B5).
- A test that runs a presenter through a fade or a hold steps at the game's fixed 1/60 s (`StartMapScenarios.OneUpdate`), never a bigger step to save updates: a step longer than a phase jumps over it (the sound pass's FE3).
- A filter clause inside a check or a lookup (a `.Where` that skips what goes with the change, a readable-file test, an open map's unsaved state) gets a test with the filtered item present: the Mana Seed reset's E1b, X2, E3b and E3c reviews each found one untested.
- When tests change their data source (the repository's content to the test world), their names and summaries change with it: grep the old source's word in test names and in the fixture's users, not only in comments (the reset's T2a, T2b and T2c reviews).
- A relayed `PropertyChanged` arm and each `[NotifyCanExecuteChangedFor]` that greys a button gets a test asserting the event is raised; a `CanExecute` that reads the catalog is re-raised on every catalog change and tested with an undo while the panel is open (the composer plan's CO2, CO3, AE2).
- A sort or draw-order test sets its items up in the opposite order (the overlay painted before the base): a fixture already in order passes with the sort deleted (quests J1, the levels plan's OL3 Critical and OL4).
