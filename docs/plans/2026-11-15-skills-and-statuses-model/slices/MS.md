# MS. An effect grants several statuses

Core + Data + content, about 200 lines (about 110 of them tests), plus 13 grants of `skills.json` rewritten. Needs R1.
Design: README, decision 809; design.md, sections 2.2 and 3.2.

## Goal
An effect's `statuses` list replaces its single `status`: every grant lands, in list order, on each target still
standing after the effect landed (a damage effect that missed every hit grants none, decision 326). Today's grants are
still `StatusGrant(Kind, Power, Turns)`; the swap (SW) turns each into a record. Data refuses a null entry, the same
kind twice and a bad grant, naming its index. `skills.json` writes `"statuses": [ ... ]`.

## Changes
- `Core/Effects/Effect`: the positional `StatusGrant? Status` goes; `IReadOnlyList<StatusGrant> Statuses { get; init; }
  = []` ("granted in order to each target still standing after the effect (ADR 0021, decision 809)");
  `Equals(Effect?)` and `GetHashCode` compare the statuses in order (as `StatusDefinition`, SR1). Grep
  `new Effect(` in `src/` and `tests/` for a call passing a fifth or later argument positionally (range, hits): each
  gets its name.
- `Core/Battle/EffectLanding.LandOn`: `foreach (var grant in effect.Statuses)` where it read `effect.Status`; the
  summary says "the statuses it grants".
- `Core/Battle/EffectScaling.ScaledFor`: every grant through `GrantScaling.Scaled`.
- `Core/Battle/SkillEmpowerment.EffectOf`: a status-only effect raises every grant's power.
- `Data/Effects/EffectValidator`: `FindStatusesProblem`: no null entry (`"effect.statuses must not contain a null
  entry."`), the same kind not twice (`"effect.statuses lists poison twice."`), then each grant through
  `StatusGrantValidator` with its index (`"effect.statuses[1].turns must be at least 1 (was 0)."`); the
  `grantStatus` rule reads "at least one status". `StatusGrantValidator.FindProblem(StatusGrant grant, int index)`.
- `content/skills.json`: the 13 `"status": { ... }` become `"statuses": [ { ... } ]`, numbers unchanged.
- `tests/Jrpg.Core.Tests/Builders/TestStatuses.Granting`: `effect with { Statuses = [grant] }`.
- `tests/Jrpg.Game.Tests/Battle/BattleScenarios.WithSureStun`: reads `Statuses[0]` and writes the list back with its
  power at 100.
- `Data.Tests`: `ContentCatalogLoaderTests` lines 186 and 259 read `Statuses` (still the grant's kind and turns, no new
  numbers pinned); `EffectValidatorTests`' four `new StatusGrant(` sites build their effects with the list.

## Tests
- `Core.Tests/Battle/BattleStatusListTests` (new: `BattleStatusTests` is at 246; skills built in the test with
  `TestStatuses`):
  - `Execute_EffectWithTwoStatuses_GrantsBothInListOrder` (two `StatusApplied` events, Exposed then Poison).
  - `Execute_DamageThatMissedEveryHit_GrantsNoneOfItsStatuses` (`FixedSequenceRandom` drawing the miss).
  - `Execute_StatusOnlySkillWithDominantForce_RaisesEveryGrant` (two grants, both raised by the bonus).
- `Core.Tests/Effects/EffectTests` (new): `Equals_SameStatusesInOrder_IsTrue`, `Equals_StatusesInAnotherOrder_IsFalse`.
- `Data.Tests/Effects/EffectValidatorTests`: rows of its invalid theory (a null entry; Poison twice; the second grant
  with 0 turns, naming `statuses[1]`) and `FindProblem_TwoDifferentStatuses_Accepted`.

## Verify by hand
- Run the game, New game, choose Omer, fight the goblins: Fire Blast still leaves its two-turn mark on the goblin (the
  same behavior as before).

- **Note from R1's review:** `EffectScalingTests`'s `Assert.Equal(poison, scaled)` compares whole `Effect` records;
  once `Statuses` is a list it compares by reference, so compare the list's contents there.
