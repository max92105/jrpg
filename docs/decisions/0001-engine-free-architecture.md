# 0001. MonoGame for the game, WPF for editors, pure Core library

Status: accepted
Date: 2026-10-17

## Context
A from-scratch 2D JRPG in C#, no game engine. A first demo was built in WPF. WPF is a retained-mode UI framework: no fixed-step game loop, UI-oriented input, slow when drawing many sprites as controls, extra work for crisp pixel art, Windows only. The project also needs content tools (map, database, dialogue editors). We want game logic that is testable headless so AI-assisted QA and balance simulations are possible.

## Options
### A. Stay fully in WPF
- Pros: known technology, demo already exists.
- Cons: fighting the framework for game loop, input, rendering performance; logic tends to leak into views.

### B. MonoGame for the game, WPF for tools, shared Core
- Pros: MonoGame is a framework (loop, input, sprite batching, audio), not an engine, so the "no engine" goal holds. WPF skills are reused where WPF excels (form-heavy editors). Core is pure .NET and fully testable.
- Cons: two UI stacks; the WPF demo's rendering code is rewritten.

### C. Lower-level (SDL3 / Silk.NET / Raylib-cs)
- Pros: maximum control / minimal API.
- Cons: more plumbing (C) or smaller ecosystem; little benefit for a 2D JRPG.

## Decision
Option B. `Jrpg.Core` holds all game rules with no rendering references. `Jrpg.Game` (MonoGame) and `Jrpg.Editor` (WPF) are thin shells on top.

## Consequences
- Demo logic moves into Core; demo rendering is rewritten in MonoGame.
- Every gameplay rule must be reachable from a unit test with a fixed seed.
- Content is JSON shared by game and editors; the editor validates with the same code the game loads with.
