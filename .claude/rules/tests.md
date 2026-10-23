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
