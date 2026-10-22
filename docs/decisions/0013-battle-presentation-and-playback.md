# 0013. Battle presentation and playback

Status: accepted
Date: 2026-10-20

## Context
The battle plan (`docs/plans/done/2026-10-20-battle/`) builds the battle screen in `src/Jrpg.Game`: a HUD (party vitals, enemy HP, turn order), a command panel, and each action played back over time (step forward, effects, floating numbers, step back). Core already runs a turn in one `Execute(BattleCommand)` call that applies everything and returns ordered `BattleEvent`s (ADR 0004), and the shell is a scene stack whose scenes return `SceneRequest`s (ADR 0008).

The main question is **where the HUD reads its values** while an action is being played back, since Core's state has already changed when `Execute` returns. Constraints: Core owns every rule and the Game never keeps a second copy of them; no allocation in `Update`/`Draw`; events carry only what a current mechanic needs (ADR 0004); a solo developer must keep the screen easy to change.

## Options
### A. Live Core state
The turn sequence steps the actor forward, then calls `Execute` at the moment of impact. The HUD reads `Vitals`, the inventory and `UpcomingActors` from Core, formatted when a stage starts.
- Pros: the HUD can't disagree with Core; one path for party and enemies; legacy's timing (it executed between step forward and the hold).
- Cons: a multi-hit or chained action shows its final values from its first beat.

### B. Values derived from the events
The screen keeps "shown" vitals and updates them beat by beat; events carry MP spent and values before and after.
- Pros: any sequencing, hit by hit.
- Cons: a second vitals model in the Game, and event data no mechanic needs yet.

### C. A generic timeline/tween engine
- Pros: any animation composes.
- Cons: speculative; nothing in the plan needs more than a few fixed stages.

## Decision
**A** (developer, 2026-10-20). The HUD reads live Core values, refreshed at stage changes, never formatted per frame. With it:
- **Stacking:** the battle scene is pushed over the map and popped at the end, like the menu, behind the flow's fade (`Flow/BattleTransitions`). The map stays untouched underneath and resumes once `FinishBattle` clears the pending encounter; it is not rebuilt.
- **Beats:** `ActionPlayback` turns one turn's events into beats (banner, impact, defeat), and the hold lasts as long as its beats. Defend and Run have no hold; a future between-turn event (poison) gets its own beat. Its switch over `BattleEvent` is exhaustive and throws `InvalidOperationException` on an unknown event, so a new mechanic can't be shown silently wrong.
- **The outcome:** the on-screen actor is the turn sequence's, never `battle.CurrentActor`. When the outcome's beats end, the scene calls `session.FinishBattle()` once and returns its outcome request once.

## Consequences
- The Game holds no vitals of its own; `VitalsText` reads `Vitals` when the hold starts, the turn order when step forward starts.
- Moving to B later touches only `VitalsText` and `ActionPlayback`, since the beats already follow the events.
- The map is drawn every frame under a full-screen battle. Cheap at this map size; if profiling shows a cost, the stack can skip scenes under an opaque one.
- No timeline engine: each timed piece (step, banner, floating number) is its own small pure type, tested in `tests/Jrpg.Game.Tests`.
