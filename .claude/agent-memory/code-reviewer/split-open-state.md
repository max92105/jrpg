---
name: split-open-state
description: Game-side deal/dialog owners keep "what is open" and "what to do on Yes" in separate fields guarded by ?? throw
metadata:
  type: feedback
---

Seen in the shops plan twice: T2 `Purchase` (_picked + Picker) and T3 `ShopCounter` (Picker/Prompt + _settle), each with a `?? throw` guard for a state the code never reaches.

**Why:** two fields that must agree invite a stale one (T2's _picked stayed set after Cancel) and the guard is defensive code for an impossible state.

**How to apply:** when a scene or counter opens a picker/prompt, check whether the open UI and its continuation are one value; suggest pairing them (Suggestion, not Warning, unless a concrete stale-state bug exists). See [[tautological-tests]] for the companion test smell.
