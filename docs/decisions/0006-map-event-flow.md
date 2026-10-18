# 0006. How a map event runs

Status: accepted
Date: 2026-10-17

## Context
Slice 3a builds map events: a chest that shows a line, gives an item and swaps to an opened chest; a door that warps; a sign that talks. An event is a list of steps (dialogue, give item, swap object, warp). Some steps must wait for the player (read a message, see "Obtained Potion"), and a warp must wait for the Game's fade-out before the map changes.

Legacy ran events in `GameManager.RunEventAsync`, an `async` loop in the WPF game: the UI applied each step itself (inventory, world swaps, warps), so the rules lived in the Game. It also had a bug: a found item only showed a message box and wasn't added (fixed in the plan, decision 20).

Constraints: rules live in Core, never in the Game (CLAUDE.md); no `async` in Core's simulation (core.md); Core must run headless in tests; results over side effects (ADR 0004 reports actual amounts). Later features will need branching (a yes/no choice in dialogue) and event-started battles (question 30, open).

## Options
### A. A step cursor in Core, advanced by the Game
`EventRun` holds the current step. Steps that don't need the player (swap) apply and move on at once; steps the player must see wait as `Current` until the Game calls `Continue()`. A give-item step applies when it becomes current and exposes an `ItemReceipt` (item, added, lost) so the message tells what really happened.
- Pros: all rules in Core, testable headless one step at a time. The Game only shows `Current` and calls `Continue()` after the player presses a button or the fade-out ends. Branching and battles fit later: they are just more waiting steps.
- Cons: Core holds a little in-flight state (`EventRun`) the Game must not drop; saving mid-event needs a rule (5a saves only between events). Two calls per step instead of one.

### B. Run the whole event at once and return field events (like ADR 0004)
`Run(event)` applies every step and returns `DialogueShown`, `ItemReceived`, `ObjectSwapped`, `WarpRequested`… for the Game to play back.
- Pros: same shape as battle; no in-flight state in Core.
- Cons: everything after a warp would run before the player even sees the new map, and a swap after a warp would hit the wrong map. A yes/no choice can't work, because the outcome is needed before the rest runs. The Game must replay events faithfully, or the screen and the state disagree.

### C. The Game iterates the steps and applies them (legacy)
- Pros: no new Core types; easiest to port.
- Cons: inventory and world rules end up in the Game, against the architecture; untestable without the Game; the legacy item bug lived exactly here.

## Decision
**A** (developer, 2026-10-17), with these rules:
- `EventRun` starts by marking a "once" event done (legacy did the same, so a warp or battle mid-event can't make it run twice).
- Swaps apply without waiting. Dialogue, give-item and warp steps wait for `Continue()`. A warp completes on `Continue()`, after the Game's fade-out.
- A give-item step applies when it becomes current; extra copies beyond `MaxStack` are lost (decision 29, placeholder) and the receipt says how many.
- `Continue()` after the last step throws; the Game checks `IsFinished`.
- Event-started battles stay out until question 30 is answered; they would be another waiting step.

## Consequences
- The Game's job per frame: if an event runs, show `Current` (or the receipt) and call `Continue()` on confirm; no walking meanwhile.
- Saves happen only between events, so `EventRun` is never saved.
- New step kinds pick "applies at once" or "waits" and get a test each; branching adds a step whose `Continue` takes the player's choice.
