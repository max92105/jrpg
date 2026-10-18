# 0008. Game shell structure

Status: accepted
Date: 2026-10-18

## Context
Part 3a starts `src/Jrpg.Game`: the MonoGame window that will show exploration, then (part 3b) battles, menus, the title screen, save/load, defeat and the intro cutscene. Each is a screen; some stack (a menu over the map, a dialogue box over exploration), and moving between them (map -> battle -> map, title -> new game, defeat -> load) is where the legacy demo tangled itself: static `GameSystems`, a `SceneManager` and a `GameManager` that every screen called into, so screens knew each other and the rules leaked into the UI.

Constraints: the Game is thin, input to Core commands and Core state to pixels, with no rules (CLAUDE.md, `game.md`). Feature folders must not form cycles (ADR 0001 practice). Methods stay under the CA1502 complexity limit. No allocation in `Update`/`Draw`. Core stays deterministic: randomness and time come in from the Game (ADR 0007, decisions 35 and 43). Most Game code can't be unit-tested through a window, so whatever can be must be separable.

## Options
### A. A scene stack; scenes return requests that small transition handlers apply
`IScene` (`Update`, `Draw`) on a `SceneStack` (drawn bottom-up, only the top updates). A scene never calls another: when it wants a change (start a battle, open the menu, load a slot) it returns a `SceneRequest` record. `GameFlow` owns the stack and the one `GameSession`, and hands each request to the handler for its group (`SessionTransitions`: new game, load, title, quit; `BattleTransitions`: start, finish, defeat; `MenuTransitions`), each switching over only its own few request types.
- Pros: dependencies point one way (screens -> flow never; flow -> screens), so no folder cycles. Each handler stays small. The flow's update path can run headless with scripted input, so one test can walk a seeded new game into a battle. Screen logic (menus, cursors, layouts) lives in pure presenters testable without a window.
- Cons: a new request type means touching a record, a handler and the scene; a little indirection for simple jumps. The request records arrive in part 3b; 3a has only the new-game path.

### B. Scenes call a shared scene-manager service (the legacy shape)
- Pros: the most direct code; a scene just says "go to battle".
- Cons: every scene depends on the manager and, through it, on the others: the folder cycles and hidden coupling the legacy demo had. Hard to test without the whole game.

### C. One state machine in `JrpgGame`
- Pros: all transitions visible in one place.
- Cons: one switch over every state and event, past CA1502 within part 3b; the class becomes the new `GameManager`.

## Decision
**A** (developer, 2026-10-18), with these rules:
- One `GameSession` at a time, owned by `GameFlow`; scenes get what they read through their constructors.
- The flow's update path never touches `GraphicsDevice` or textures; drawing is separate, so the flow runs headless in tests.
- The Game seeds `SeededRandom` from the clock and writes the seed to the debug output, so a run can be replayed; it passes `DateTimeOffset.Now` to `GameSession.Capture` and every update's delta to `AddPlayTime` (decision 35).
- Until text can be drawn (slice 1c), startup errors go to the console and the debug output, naming the file.
- No allocation in `Update` or `Draw` (`game.md`).

## Consequences
- 3a builds `IScene`, `SceneStack`, `GameFlow` and `SessionTransitions` with the new game only; `SceneRequest` and the other handlers come with their first user in 3b.
- The folder dependency table in the plan is checked at each slice's review; a scene needing another scene's type is a sign it should return a request instead.
- 3b adds a headless test that drives a seeded new game into the forest until a battle is requested and finished.
