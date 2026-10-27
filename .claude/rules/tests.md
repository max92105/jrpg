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
- Tests on the repository's content assert the entries they need (an id is there, a named layer exists), never totals of a whole catalog or map: content changes in the editor every day.
- A test that a value is refreshed or cached starts from a state where the old value differs from the expected one, and checks a value the runtime doesn't already share (not an enum name or a string literal).
- A timing or presentation step is tested through to the step that follows it in real content (stills then a warp, a still then a dialogue), and a hardware probe is checked against the library's source, not assumed.
- A rule with two sides (an `||`, a lower and an upper bound, a rise and a drop) gets one test per side; the class-mechanics reviews found the untested side four times.
- A test that needs a member without something (no skills, no lasting resource) builds one with a test class; it never picks a repository member that happens to lack it today, since content changes (the "actor without skills" test moved member three times).
- A value copied from one record into another (a sheet stat into `CombatStats`, a definition's number into a battle start) gets a test whose fixture differs from the destination's default: when the neutral test rules give the default, deleting the copy line passes every test (the H1 and H2 reviews).
- A new list or dictionary in a save or content record gets a null-entry test: the strict serializer lets a null element or value through, and the null surfaces as a crash later (five slices, last G1b).
