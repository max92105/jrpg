---
name: sound-ask-coverage
description: Screens gaining sounds leave Ask lines, Clear calls and the scene's forward unasserted; map each one to a test
metadata:
  type: project
---

When a screen gains sounds, tests tend to assert the slice's listed cases, leaving other `_sounds.Ask` lines, `Clear` and the line forwarding a part's sounds to its scene untested (SavePage and LoadPage in S3, PartyPage, StatusPage and QuestsPage in S4, ExplorationScene's AskAll and EventPresenter's Clear in D1).

**Why:** a deleted Ask or forward is a silent action, and a deleted Clear replays a sound every frame, and neither fails a test. `Assert.Contains` over many updates can't see a repeat.

**How to apply:** for each `_sounds.Ask` and `AskAll` in the diff name the test asserting it, through the scene the host reads; for each `Clear`, a test with two updates. The same goes for a value crossing from a scene to the flow: B4 tested the cue and the selection apart, never the flow line joining them. B5 again: the skill list's confirm went unasserted while the item list's had a test, so check parallel branches one by one.
