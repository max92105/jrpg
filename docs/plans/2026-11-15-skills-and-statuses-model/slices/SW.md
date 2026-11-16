# SW. A grant names a status record; `StatusKind` goes

Core + Data + Game + content, about 260 lines (about 110 of them deletions or mechanical fixture bodies, about 90
tests). Needs SR1, R1, R2, MS, CH. Design: design.md, sections 2.2, 2.5 (scaling, kept by family), 2.6 (replacement
mapping), 3.2 (the grants).

**Why one slice:** the five events, `StatusGrant` and `ActiveStatus` change type together, and Data (which builds the
grants), the Game (which reads the events) and the skills' grants (which must name records) don't compile or load
apart from them. Everything that could leave already did: the catalog (SR1), the test sites (R1, R2), the list (MS),
the chance (CH). It has no split point inside; if it outgrows the estimate, the size is noted in the Done note.

## Goal
`skills.json`'s grants name records (`{ "status": "stun", "turns": 1, "chance": 60 }`) and the battle runs on them:
each status's parts act as today's kinds did, with today's numbers; the marks, names, tick tones and sounds come from
the record. A holder keeps one status per family (decision 800): two Guards keep the stronger as today, and the one
named change is that Burning and Poison, two families, both tick on one target. `StatusKind`,
`Game/Battle/StatusNames` and the six `status_*` texts are gone.

## Changes, Core
- `Statuses/StatusGrant`: `public sealed record StatusGrant(StatusDefinition Status, int Turns)`, `Chance` kept from
  CH.
- `Statuses/ActiveStatus`: `ActiveStatus(StatusDefinition Status, int TurnsLeft, bool IsFresh)`; `Status` is "the
  record as it landed: its amounts raised by its granter (decision 799)".
- `Statuses/StatusDefinition`: `Strengthened(int permille)` (every part's amount through `StatFormula.Scaled`),
  `bool Has(StatusPartKind kind)`, `StatusPart? TickingPart` (its `damageEachTurn` or `healEachTurn`, or null).
- `Statuses/StatusSet`: one status per family. `Has(string familyKey)` (the marks); `StatusDefinition?
  FirstWith(StatusPartKind kind)` (the rules); `Apply(StatusGrant grant)`: a new family is added fresh; a held family
  becomes the granted record with the grant's turns and, part by part, the amount farther from zero of the two (a
  private `Stronger(StatusDefinition held, StatusDefinition granted)`; SR1's rule guarantees the parts line up);
  `CountDown` and `Clear` return `IReadOnlyList<StatusDefinition>`.
- `Statuses/StatusModifiers`: `Apply` raises the resistances by the sum of `resistanceChange` amounts;
  `ExtraDamageTakenPercent` sums `damageTakenChange`; both through a private `SumOf(StatusSet, StatusPartKind)`.
- `Statuses/GrantScaling`: `Scaled` strengthens the record (`Status = grant.Status.Strengthened(statusPermille)`) and
  adds the extra turns unless the record has `skipTurn`; `ChancePermille` subtracts the resistance for a record with
  `skipTurn` (it read `Kind == Stun`).
- `Statuses/StatusKind.cs`: deleted.
- `Battle/SkillEmpowerment.EffectOf`: a status-only effect's grants become `grant with { Status =
  grant.Status.Strengthened(StatFormula.PermilleOf(100 + rule.BonusPercent)) }` (today's `DominanceFormula.Empowered`
  rounding, design.md, section 5).
- `Battle/EffectLanding.Grant`: `StatusApplied(target, grant.Status, grant.Turns)`, `StatusResisted(target,
  grant.Status)`.
- `Battle/TurnUpkeep.TickStatuses`: a status's `TickingPart` takes or restores its amount; the summary's "Poison isn't
  a hit" becomes "a tick isn't a hit".
- `Battle/TurnFlow.AdvanceToNextActor`: `while (turnOrder.Current.Statuses.FirstWith(StatusPartKind.SkipTurn) is { }
  stun)`, reporting `TurnSkipped(skipped, stun)`.
- `Battle/EnemyTargeting`: taunting members are those with `FirstWith(StatusPartKind.DrawAttacks)`.
- Events: `StatusApplied(Combatant Target, StatusDefinition Status, int Turns)`, `StatusEnded(Combatant,
  StatusDefinition Status)`, `StatusTicked(Combatant Holder, StatusDefinition Status, int Amount)`,
  `TurnSkipped(Combatant, StatusDefinition Status)`, `StatusResisted(Combatant Target, StatusDefinition Status)`;
  summaries say "a status" where they named Poison, Regen or Stun.

## Changes, Data and content
- `Content/ContentCatalogLoader`: `statuses.json` is already read before `skills.json` and `items.json` (SR1); they are
  now read with `StrictWith(tieredEffectConverter, new CatalogReferenceConverter<StatusDefinition>(statusesById,
  status => status.Id, "status"))`.
- `Statuses/StatusGrantValidator`: the power rule goes with the field; `EffectValidator`'s "listed twice" compares
  status ids.
- `content/skills.json`: the 13 grants of design.md, section 3.2 (`"status"` names the record; `"power"` and `"kind"`
  go; `"chance"` stays where CH wrote it).

## Changes, Game
- `Battle/StatusNames.cs`: deleted; `Ui/MessageIds.Battle`: the six status-name constants go; `ui.json`:
  `status_stun` .. `status_regen` go.
- `Battle/ActionPlayback`: `StatusApplied`'s line is `applied.Status.Name`; the tick's tone is
  `FloatingNumberTone.Poison` for a `damageEachTurn` tick part, `Heal` otherwise (two lines changed in place; the file
  stays at 274).
- `Battle/BattleSounds.TickFileOf(StatusDefinition status)`: by `status.TickingPart?.Kind`: damage, the poison tick;
  heal, the regen tick; none, `InvalidOperationException` as today.
- `Battle/StatusMark`: `StatusMark(string familyKey, bool isHarmful, MeasuredText letter)`; `StatusMarksView`: one
  mark per family (`_statuses.Has(mark.FamilyKey)`), the letter from `status.Name`, the rim from `mark.IsHarmful`; the
  static `IsHarmful(StatusKind)` goes.

## Changes, tests
- `Core.Tests/Builders/TestStatuses`: the bodies (design.md, section 2.6): each grant builds a one-part record (ids
  `poison`, `regen`, `guard`, `exposed`, `stun`, `taunt`, no family); `Stun(chance, turns)` keeps its `Chance`; the
  ids become constants; `IdOf`, `AmountOf` read the record.
- `Game.Tests/Battle/TestStatuses`: the three records.
- `Game.Tests/Battle/StatusMarksViewTests`: the `IsHarmful` theory goes (below).
- `Data.Tests`: `ContentCatalogLoaderTests` line 186 asserts Shield Bash grants `stun`; `EffectValidatorTests`' grant
  sites name a record built in the test.

## Tests
- `Core.Tests/Statuses/StatusSetTests` (63 lines, about 120 after; records built in the test):
  - `Apply_TwoRecordsOfOneFamily_KeepsOne` (`poison_1` then `poison_2`: one status).
  - `Apply_WeakerRecordOfAHeldFamily_KeepsTheStrongerAmountPerPart` (a two-part family; one part stronger each side).
  - `Apply_NegativeAmountsOfOneFamily_KeepsTheFartherFromZero`.
  - `Apply_TwoFamilies_KeepsBoth` (Burning beside Poison).
  - `FirstWith_SkipTurnHeld_ReturnsThatStatus`.
- `Core.Tests/Statuses/GrantScalingTests`: `Scaled_PartAmounts_RaisedByStatusPermille`,
  `ChancePermille_SkipTurnRecord_SubtractsCrowdControlResistance` (now on the part).
- `Core.Tests/Battle/BattleStatusListTests` (from MS): `Execute_BurningThenPoison_BothTickAtTheEnd` (the named change)
  and `Execute_ExposedFiftyThenTwenty_KeepsFifty` (decision 800, no stacking to 70).
- `Game.Tests/Battle/StatusMarksViewTests` (97 lines): `Refresh_HarmfulRecord_MarkIsHarmful`,
  `Refresh_HelpfulRecord_MarkIsHelpful`, `Refresh_NewStatus_LetterIsTheNamesFirst` (a record named "Burning": "B"),
  `Refresh_TwoRecordsOfOneFamily_ShowsOneMark`.
- `Game.Tests/Battle/BattleSoundsTests` (192 lines): `Sounds_HealTickOfAnyRecord_PlaysRegenTick` (a record named
  otherwise with `healEachTurn`).
- `Game.Tests/Battle/ActionPlaybackStatusTests` (new: `ActionPlaybackTests` is at 243):
  `Start_StatusApplied_ShowsTheRecordsName` ("Burning", not a message id); `Start_DamageTickOfAnyRecord_UsesPoisonTone`.

## Verify by hand
1. Run the simulations (`Jrpg.Simulations.exe -showLiveOutput`): every bound holds; paste the Chief's line in the Done
   note.
2. Run the game, Title > New game, choose Ragnir (the fighter), walk the forest until the goblins attack: Shield Bash
   stuns sometimes (an "S" mark, the goblin's turn skipped, "Stun" shown on the target), or the goblin resists.
3. New game with Omer: Fire Blast leaves a "B" mark named Burning that ticks at the end of the goblin's turn; cast it
   twice on one goblin: still one mark, its turns refreshed.

- **Note from CH's review:** a dominant force raises a status-only skill's grants by their power, so since CH it no
  longer raises a stun's chance (its power is 0); SW's `Strengthened` raises the record's parts, never the chance, as
  decision 799 says. No status-only stun skill exists in content.
