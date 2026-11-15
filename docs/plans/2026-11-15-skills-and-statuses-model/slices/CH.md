# CH. Every grant has a landing chance

Core + Data + content, about 110 lines (about 70 of them tests, the `GrantScalingTests` rewrite included), plus three
grants of `skills.json`. Needs MS (and R1, R2). Design: README, decisions 798, 801, 802 and 819; design.md, section
2.5 ("Chance").

## Goal
Every grant carries a landing chance in percent (100 when left out), raised by its user's Intelligence; a Stun's chance
moves from its power to the grant's `chance`; every grant rolls it, drawing only when uncertain, and one that fails
reports `StatusResisted`. Crowd control resistance still cuts only a Stun's chance; a self-taunt always lands. Grants
are still `StatusGrant(Kind, Power, Turns)` until SW. Two behaviors change and are named: a non-stun grant below 100
can now miss (none in content), and Intelligence raises every grant's chance, not only Stun's (decision 801, a default
to confirm).

## Changes
- `Core/Statuses/StatusGrant`: `int Chance { get; init; } = 100` ("the percent it lands with before Intelligence and
  resistance, decision 798"); `Power`'s doc loses "Stun's chance in percent".
- `Core/Statuses/GrantScaling`:
  - `Scaled` also scales `Chance` by the status percent (`StatFormula.Scaled`), for every kind.
  - `ChancePermille(StatusGrant scaled, int crowdControlResistancePermille)` replaces `StunChancePermille`: the scaled
    chance in permille, less the resistance for a Stun only, within 0..1000. Summary rewritten from decisions 801 and
    324.
- `Core/Battle/EffectLanding.Grant`: every grant rolls `ChancePermille(grant, target's crowd control resistance)`; a
  failed roll reports `StatusResisted(target, grant.Kind)`; the class summary's "a stun rolls" becomes "every grant
  rolls its chance".
- `Data/Statuses/StatusGrantValidator`: chance within 1..100 (`"effect.statuses[0].chance must be within 1..100 (was
  0)."`); the Stun power rule goes (a Stun's power is unread from now on; the swap removes powers).
- `content/skills.json`: Shield Bash, Palm Strike and Crushing Blow's stun grants get `"chance": 60`, `30`, `40` and
  `"power": 0`.
- `tests/Jrpg.Core.Tests/Builders/TestStatuses`: `Stun(chance, turns)` is `new StatusGrant(StatusKind.Stun, 0, turns)
  { Chance = chance }`; `ChanceOf` reads `Chance`.
- `tests/Jrpg.Game.Tests/Battle/BattleScenarios.WithSureStun`: sets the grant's `Chance` to 100.
- `Data.Tests/Content/ContentCatalogLoaderTests` line 186: asserts Shield Bash's grant is a Stun with a chance below
  100 (no number pinned).

## Tests
- `Core.Tests/Statuses/GrantScalingTests` (51 lines): the three `StunChancePermille_*` become
  `ChancePermille_StunAgainstResistance_SubtractsIt`, `ChancePermille_OtherStatusAgainstResistance_IgnoresIt`,
  `ChancePermille_AboveOneHundred_HeldAtOneThousand`; plus `Scaled_PoisonChance_RaisedByStatusPermille` (decision
  801's new side) and `Scaled_StunTurns_GetNoExtraTurns` (kept).
- `Core.Tests/Battle/BattleStatusListTests` (from MS):
  - `Execute_PoisonAtSixtyPercent_RollsAndCanMiss` (`FixedSequenceRandom` drawing above 600: `StatusResisted`, no
    status).
  - `Execute_PoisonAtSixtyPercent_LandsOnALowDraw` (the other side).
  - `Execute_SelfTauntWithCrowdControlResistance_AlwaysLands` (a member with 50 % resistance taunting itself).
- `Data.Tests/Effects/EffectValidatorTests`: rows (chance 0; chance 101) and `FindProblem_ChanceOneHundred_Accepted`,
  `FindProblem_ChanceOne_Accepted`.

## Verify by hand
- Run the game, Title > New game, choose Ragnir, fight the goblins: Shield Bash stuns sometimes and is resisted
  sometimes, as before.
