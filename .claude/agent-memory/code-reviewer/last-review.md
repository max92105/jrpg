---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F2a (ForgeAction, ForgeActionRow, ForgeActionChoice, ForgeActionFormat,
MaterialListText, ForgeRefusalNames; ForgeScene actions; ForgeDetailView Actions band). Round 1.
Verdict: Fix the Criticals. verify.ps1 all pass (2894 tests).

- Critical: MessageIds.cs lines 598-631, six doc comments hold a literal `\x27` for an apostrophe (perl/sed escape left by an edit; verify's shell-quote check misses it) - open
- Suggestion: ForgeRefusalNames has three of six refusals tested (NoYield, StackFull, NoAffixLeft not reached); a theory over every ForgeRefusal value with distinct ids - open
- Suggestion: docs/map.md ForgeScene entry still lists only Up/Down and Cancel; add "Confirm opens a piece's actions, Cancel closes them first" - open
- Follow-up: verify.ps1's "no shell quote" check could also catch `\x27` - open
