# SW. A grant names a status record; `StatusKind` goes

Core + Data + Game + content, about 340 lines (about 130 of them deletions or mechanical fixture bodies, about 120
tests). Needs SC1, R1, R2, MS. Design: design.md, sections 2.2, 2.5 (scaling, kept by id, chance), 2.6 (replacement
mapping), 3.2 (the grants).

**Why one slice:** the five events, `StatusGrant` and `ActiveStatus` change type together, and Data (which builds the
grants), the Game (which reads the events) and the skills' grants (which must name records) don't compile or load
apart from them. Everything that could leave already did: the catalog (SC1), the test sites (R1, R2), the list (MS).
It has no split point inside; if it outgrows the estimate, the size is noted in the Done note, not cut.

## Goal
`skills.json`'s grants name records (`{ "status": "stun", "turns": 1, "chance": 60 }`) and the battle runs on them:
each status's parts act as today's kinds did, with today's numbers; the marks, names, tick tones and sounds come from
the record. Two changes are deliberate and named: statuses are kept by id (decision 800: Burning beside Poison, two
Guards add up), and every grant rolls its chance (decision 801; only the stuns are below 100 today). `StatusKind`,
`Game/Battle/StatusNames` and the six `status_*` texts are gone.

## Changes, Core
- `Statuses/StatusGrant`: `public sealed record StatusGrant(StatusDefinition Status, int Turns)` with
  `int Chance { get; init; } = 100` ("the percent it lands with before Intelligence and resistance, decision 798").
- `Statuses/ActiveStatus`: `ActiveStatus(StatusDefinition Status, int TurnsLeft, bool IsFresh)`; `Status` is "the
  record as it landed: its amounts raised by its granter (decision 799)".
- `Statuses/StatusDefinition`: `Strengthened(int permille)` (every part's amount through `StatFormula.Scaled`),
  `bool Has(StatusPartKind kind)`, `StatusPart? TickingPart` (its `damageEachTurn` or `healEachTurn`, or null).
- `Statuses/StatusSet`: kept by id. `Has(string statusId)` (the marks); `StatusDefinition? FirstWith(StatusPartKind
  kind)` (the rules); `Apply(StatusGrant grant)`: a new id is added fresh; the same id refreshes with the grant's turns
  and, part by part, the amount farther from zero (a private `Stronger(held, granted)`); `CountDown` and `Clear` return
  `IReadOnlyList<StatusDefinition>`.
- `Statuses/StatusModifiers`: `Apply` raises the resistances by the sum of `resistanceChange` amounts;
  `ExtraDamageTakenPercent` sums `damageTakenChange`; both through a private `SumOf(StatusSet, StatusPartKind)`.
- `Statuses/GrantScaling`: `Scaled(StatusGrant grant, int statusPermille, int extraTurns)` strengthens the record,
  scales the chance, and adds the extra turns unless the record has `skipTurn`; `ChancePermille(StatusGrant scaled,
  int crowdControlResistancePermille)` replaces `StunChancePermille` (the resistance subtracted for `skipTurn` only,
  within 0..1000). Summary rewritten from decisions 799 and 801.
- `Statuses/StatusKind.cs`: deleted.
- `Battle/EffectLanding.Grant`: every grant rolls `GrantScaling.ChancePermille(grant, target's crowd control
  resistance)`; a failed roll reports `StatusResisted(target, grant.Status)`; the summary's "a stun rolls" becomes
  "every grant rolls its chance".
- `Battle/SkillEmpowerment.EffectOf`: a status-only effect's grants become `grant with { Status =
  grant.Status.Strengthened(StatFormula.PermilleOf(100 + rule.BonusPercent)) }` (today's `DominanceFormula.Empowered`
  rounding, design.md, section 5).
- `Battle/TurnUpkeep.TickStatuses`: a status's `TickingPart` takes or restores its amount; the summary keeps
  "Poison isn't a hit" as "a tick isn't a hit".
- `Battle/TurnFlow.AdvanceToNextActor`: `while (turnOrder.Current.Statuses.FirstWith(StatusPartKind.SkipTurn) is { }
  stun)`, reporting `TurnSkipped(skipped, stun)`.
- `Battle/EnemyTargeting`: taunting members are those with `FirstWith(StatusPartKind.DrawAttacks)`.
- Events: `StatusApplied(Combatant Target, StatusDefinition Status, int Turns)`, `StatusEnded(Combatant,
  StatusDefinition Status)`, `StatusTicked(Combatant Holder, StatusDefinition Status, int Amount)`,
  `TurnSkipped(Combatant, StatusDefinition Status)`, `StatusResisted(Combatant Target, StatusDefinition Status)`;
  summaries say "a status" where they named Poison, Regen or Stun.

## Changes, Data and content
- `Content/ContentCatalogLoader`: `statuses.json` read before `skills.json` and `items.json`, which are read with
  `StrictWith(tierConverter, new CatalogReferenceConverter<StatusDefinition>(statusesById, status => status.Id,
  "status"))`.
- `Statuses/StatusGrantValidator`: turns at least 1; chance within 1..100 (`"effect.statuses[0].chance must be within
  1..100 (was 0)."`); the Stun power rule goes. `EffectValidator`'s "listed twice" compares status ids.
- `content/skills.json`: the 13 grants of design.md, section 3.2 (`"chance"` written only below 100).

## Changes, Game
- `Battle/StatusNames.cs`: deleted; `Ui/MessageIds.Battle`: the six `Status*` status-name constants go; `ui.json`:
  `status_stun` .. `status_regen` go.
- `Battle/ActionPlayback`: `StatusApplied`'s line is `applied.Status.Name`; the tick's tone is
  `FloatingNumberTone.Poison` for a `damageEachTurn` tick part, `Heal` otherwise (two lines changed in place; the file
  stays at 274).
- `Battle/BattleSounds.TickFileOf(StatusDefinition status)`: by `status.TickingPart?.Kind`: damage, the poison tick;
  heal, the regen tick; none, `InvalidOperationException` as today.
- `Battle/StatusMark`: `StatusMark(string statusId, bool isHarmful, MeasuredText letter)`; `StatusMarksView`: marks
  kept by status id (`_statuses.Has(mark.StatusId)`), the letter from `status.Name`, the rim from `mark.IsHarmful`;
  the static `IsHarmful(StatusKind)` goes.

## Changes, tests
- `Core.Tests/Builders/TestStatuses`: the bodies (design.md, section 2.6): each grant builds a one-part record (ids
  `poison`, `regen`, `guard`, `exposed`, `stun`, `taunt`); `Stun(chance, turns)` sets `Chance`; the ids become
  constants; `IdOf`, `AmountOf` and `ChanceOf` read the record.
- `Core.Tests/Statuses/GrantScalingTests`: the three `StunChancePermille_*` tests become `ChancePermille_*` (below).
- `Game.Tests/Battle/TestStatuses`: the three records. `BattleScenarios.WithSureStun` sets the grant's `Chance` to 100.
- `Game.Tests/Battle/StatusMarksViewTests`: the `IsHarmful` theory goes (below).
- `Data.Tests`: `ContentCatalogLoaderTests` line 186 asserts Shield Bash grants `stun` (no chance pinned);
  `EffectValidatorTests`' grant sites name a record built in the test.

## Tests
- `Core.Tests/Statuses/StatusSetTests` (63 lines):
  - `Apply_TwoIdsOfOneName_KeepsBoth` (`poison_1` and `poison_2` built in the test; decision 800).
  - `Apply_SameIdWeakerGrant_KeepsTheStrongerAmountPerPart` (a two-part record; one part stronger each side).
  - `Apply_SameIdNegativeAmounts_KeepsTheFartherFromZero`.
  - `FirstWith_SkipTurnHeld_ReturnsThatStatus`.
- `Core.Tests/Statuses/GrantScalingTests` (51 lines):
  - `ChancePermille_SkipTurnStatus_SubtractsCrowdControlResistance`.
  - `ChancePermille_OtherStatus_IgnoresCrowdControlResistance`.
  - `ChancePermille_AboveOneHundred_HeldAtOneThousand`.
  - `Scaled_SkipTurnStatus_GetsNoExtraTurns` (exists for Stun: kept, now on the part).
  - `Scaled_PartAmounts_RaisedByStatusPermille`.
- `Core.Tests/Battle/BattleStatusListTests` (from MS):
  - `Execute_PoisonAtSixtyPercent_RollsAndCanMiss` (`FixedSequenceRandom` drawing above 600: `StatusResisted`, no
    status; decision 801).
  - `Execute_SelfTauntWithCrowdControlResistance_AlwaysLands` (a member with 50 % resistance taunting itself).
  - `Execute_TwoGuardsOfDifferentIds_AddUp` (decision 800, the named behavior change).
- `Game.Tests/Battle/StatusMarksViewTests` (97 lines): `Draw_HarmfulRecord_MarkIsHarmful` and
  `Draw_HelpfulRecord_MarkIsHelpful` through a seam the view already has, else `Refresh_NewStatus_MarkTakesItsHarm`
  reading `Marks[0].IsHarmful`; `Refresh_NewStatus_LetterIsTheNamesFirst` (a record named "Burning": "B").
- `Game.Tests/Battle/BattleSoundsTests` (192 lines): `Sounds_HealTickOfAnyRecord_PlaysRegenTick` (a record named
  otherwise with `healEachTurn`).
- `Game.Tests/Battle/ActionPlaybackStatusTests` (new: `ActionPlaybackTests` is at 243):
  `Start_StatusApplied_ShowsTheRecordsName` ("Burning", not a message id); `Start_DamageTickOfAnyRecord_UsesPoisonTone`.

## Verify by hand
1. Run the simulations (`Jrpg.Simulations.exe -showLiveOutput`): every bound holds; paste the Chief's line in the Done
   note.
2. Run the game, Title > New game, choose Ragnir (the fighter), walk the forest until the goblins attack: Shield Bash
   stuns sometimes (an "S" mark, the goblin's turn skipped, "Stun" shown on the target), or the goblin resists.
3. New game with Omer: Fire Blast leaves a "B" mark named Burning that ticks at the end of the goblin's turn.
4. Decision 800's named change (two Guards adding up) is left to `Execute_TwoGuardsOfDifferentIds_AddUp`: in play it
   needs Teelle with 10 Conviction for both auras, more than a forest fight gives.
