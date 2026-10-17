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
