---
name: sound-ask-coverage
description: Screens gaining sounds leave Ask lines and Clear calls unasserted; map each one to a test
metadata:
  type: project
---

When a screen gains sounds, tests tend to assert the slice's listed cases, leaving other `_sounds.Ask` lines and `Clear` untested (SavePage in S3, PartyPage, StatusPage and QuestsPage in S4).

**Why:** a deleted Ask is a silent action, and a deleted Clear replays a sound every frame, and neither fails a test.

**How to apply:** for each `_sounds.Ask` in the diff name the test asserting it; for each `Clear`, a test with two updates. The remaining slices D1, B4 and B5 add more of these.
