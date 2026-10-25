# 0021. Status effects

Status: accepted
Date: 2026-10-24

## Context
Several class mechanics put a lasting condition on a combatant for a few turns:

| Status | Class | What it does |
|---|---|---|
| Stun | Fighter's bash (decision 200) | The holder loses its next turn. |
| Taunt | Barbarian (decision 199) | Draws the enemies' attacks. |
| Guarded | Paladin's aura (decision 201) | Raises defense. |
| Exposed | Rogue (decision 202) | Every hit on the holder lands harder. |
| Poison, Regen | Druid's Decay and Growth (decision 195) | HP lost or restored at the end of each of the holder's turns. |

Two questions need one answer for all of them. What is a status, and where do its rule and its numbers live? And when does it act and count down?

Constraints:
- **Numbers in content:** no numbers in C#, and the developer tunes powers and turns while play-testing (ADR 0019's "mechanisms in code, numbers in content").
- **Determinism:** rules live in Core and are deterministic (ADR 0001).
- **Events:** every change comes out of the one `Execute` call as events (ADR 0004). The screen plays them, and its playback throws on an event it doesn't know.
- **Battle only:** statuses are battle state. Lasting effects belong to ADR 0024.

## Options
### A. A closed list of kinds in Core, with the numbers from the granting effect
`Statuses/StatusKind` lists the kinds. Each kind's rule is code with a test (`StatusModifiers`, `TurnUpkeep`, `EnemyTactics`). An effect may carry one `StatusGrant(kind, power, turns)` from content.
- Pros:
  - Stun, Taunt and Exposed are rules, not numbers, so code is where they belong.
  - Powers and turns stay in `skills.json`, where the developer tunes them.
  - Data can validate a grant, and each rule has its own test.
- Cons: a new kind of status is a code change, plus a name in `ui.json`.

### B. Statuses as content definitions (`statuses.json`)
Each status is a record with its numbers, and skills grant one by id.
- Pros: names and numbers sit in one file.
- Cons:
  - A file can't describe Stun, Taunt or Exposed, so each kind still needs code.
  - B adds a catalog without removing that code.

### C. Stat modifiers only
- Pros: the smallest change, since a status is just a change to stats.
- Cons: no stun, taunt, poison or regeneration, so four class mechanics are lost.

## Decision
A (developer, 2026-10-24): a closed list of status kinds in Core, each rule in code with a test, granted by an effect with a power and a number of turns from content.

## Consequences
- **Granting:** an effect may carry one `StatusGrant` (kind, power, turns), applied to each target still standing after the effect. `EffectKind.GrantStatus` grants without changing HP, so it reports no `EffectApplied`.
- **Countdown:** a status counts down at the end of its holder's own turns, starting from the turn after the one it was granted on (a "fresh" mark). A self-granted two-turn Taunt therefore lasts two full turns. A lost turn (Stun) counts too.
- **Re-applying:** granting a status the holder already has refreshes its turns and keeps the stronger power. It never stacks.
- **Ending:** a fall clears the fallen's statuses. Every status ends with the battle and is never saved.
- **Events:** `StatusApplied`, `StatusEnded`, `TurnSkipped` and `StatusTicked`. Each is added together with its mechanic (ADR 0004).
- **Text:** status names are player-facing text in `ui.json` (ADR 0010).
- **New code:** a `Core/Statuses` folder that references neither Battle, Party nor Skills: `StatusKind`, `StatusGrant`, `ActiveStatus`, `StatusSet` and `StatusModifiers`.
- **A new kind** is a `StatusKind` member, plus its rule and a test, plus its name in `ui.json`.
