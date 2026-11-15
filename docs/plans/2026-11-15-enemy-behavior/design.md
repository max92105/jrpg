# Enemy behavior: design

Read with the [README](README.md). Sections: 1 tunables, 2 design overview, 3 the lists, 4 the file and the editor, 5
ADR 0057's options, 6 risks.

## 1. Tunables and placeholders

| Name | Where | Value | Meaning |
|---|---|---|---|
| Percent bounds | Data, `EnemyTacticsValidator` (today's rule) | 1..99 | An HP share a trigger reads (ADR 0034). |
| Every Nth turn's bounds | Data, `EnemyTacticsValidator.MostTurnsBetween` | 2..10 (placeholder) | 1 would be "always"; past 10 a fight rarely reaches it (decision 847). |
| A new rule's boxes | Editor, `TacticRuleRow.New` | always, the attack, automatic, weight 1 | Decision 859. |
| A new phase's threshold | Editor, `EnemyTacticsViewModel.NewThreshold` | 50 for the first phase, else half the last's, at least 1 | Decision 859; typed over anyway. |

No Core constant: every rule number is content.

## 2. Design overview

### 2.1 What exists (2026-11-15, with row 6's SW and the mode bar's WS7 assumed in)
- **Core (ADR 0034):** `Battle/EnemyDefinition.Tactics` and `.Phases` (`EnemyPhase`: `BelowHpPercent`, `Tactics`);
  `EnemyAction` (`SkillId` or null for the attack, `Weight`, `When`); `TacticCondition(Kind, Percent)` over
  `TacticConditionKind` (OwnHpBelow, AllyHpBelow, AllyFallen); `TacticConditions.CombatantsMeeting` (who meets a
  condition now) and `Holds`; `EnemyTactics.ChooseCommand` (eligible = condition holds and `BattleState.CanUse`; one
  weighted draw, the attack with nothing eligible); `EnemyTargeting` (a random living member by the taunt rule; all or
  itself as the skill aims; a single-ally heal at the lowest share among those its condition found, decision 401, its
  `HealCandidates` intersecting the legal targets with `CombatantsMeeting` for every kind but `AllyFallen`);
  `EnemyPhases.EnterReached` at the end of `BattleState.Execute`. `EnemyDefinition.NamesSkill` is how an enemy knows a
  skill (`Combatant.Knows`). After SW, a taunting member is one with `Statuses.FirstWith(StatusPartKind.DrawAttacks)`.
- **Turn order:** `TurnOrder` already plays readied extra turns (a rogue's Ready): `Advance` takes a readied turn
  anchored on the combatant who just acted (`TurnOrder.cs` line 84), `BattleTurnSequence` asks the tactics for it as
  for any turn (line 132), and the turn strip marks it `IsExtra`. ADR 0057's option B would build on it.
- **Events Core already reports:** `ActionStarted(Actor, Command)` (the first event of every `Execute`),
  `EffectApplied(Target, Kind, Amount, Affinity, IsCritical)` (a blocked hit still reports one, after `HitBlocked`),
  `HitMissed`, `CombatantDefeated`, `StatusApplied`. Enough to tell, from one call's events, who hit whom and how.
- **Data:** `Battle/EnemyTacticsValidator` (weights, conditions' percents, phases descending, a skill an enemy can use:
  no class resource, no formation aim, no readied turn); `AbsentWhenNullFilter` leaves out `skillId` and `when`,
  `AbsentWhenZeroFilter` a condition's `percent`, `AbsentWhenEmptyFilter` `tactics` and `phases`. The editor already
  writes `enemies.json` (`ContentWriterEnemyTests`).
- **Editor:** the Enemies page (WS7's `Battle/EnemyCatalogPage`, the former window's layout) edits everything but the
  rules; `EnemyViewModel` carries `Tactics` and `Phases` from the record it came from (`_original with {...}`) and
  shows `TacticsSummary`. `CatalogDraft.SetEnemies` compares enemies by content, tactics and phases included
  (`IsSameEnemy`), so a session that changed nothing records nothing.
- **Game:** `BattleScene` and `BattleTurnSequence` call `EnemyTactics.ChooseCommand`; nothing in the Game reads a rule.
  This plan changes no Game file.

### 2.2 Core changes, by slice
| Slice | Change |
|---|---|
| EB1 | `Battle/TacticTarget` (Subject, Self, LowestHpFoe, HighestHpFoe, LowestHpOnItsSide); `EnemyAction.On`; `Battle/TacticTargetCandidates` (who a target rule may aim at, no draw); `EnemyTargeting.ForRule` (the pick: the taunt among the legal foes, lowest or highest share); `EnemyTactics`: a rule with `On` is eligible only with a candidate, and aims by it. |
| EB2 | `Battle/TacticConditionKinds` (`ShapeOf(kind)`), `TacticConditionShape`, `TacticSubject`, `TacticConditionNumber` (None, Percent); `Battle/TacticTargetFit` (`Fits(action, skill)`); `Battle/EnemySkillUse` and `EnemySkillRefusal`, moved from Data's validator. |
| EB8 | `EnemyAction.IsPriority`; `EnemyTactics`: the first eligible priority rule wins before any draw. |
| EB9 | `TacticConditionKind.WasHit`, `.WasCriticallyHit`, `.WasHitOnWeakness`; `TacticConditionShape.IsReaction`; `Battle/TacticMemory` (on `Combatant.TacticMemory`: reactions with their subjects); `Battle/TacticMemoryNotes.Note(events)` at the end of `Execute` beside `EnemyPhases.EnterReached`; one arm in `CombatantsMeeting` for every reaction kind; `EnemyTargeting.HealCandidates` narrows only for `OwnHpBelow` or an allies-naming trigger (decision 835). |
| EB10 | `EnemyAction.IsOncePerBattle`; `TacticMemory.HasUsed` and its note. |
| EB11 | `TacticConditionKind.FoeHpBelow`, `.EveryNthTurn`; `TacticCondition.Turns`; `TacticConditionNumber.Turns`; `TacticMemory.TurnsActed`. |
| EB12 | `TacticTarget.UnafflictedFoe`. |

### 2.3 New types (one line each)
Core `Battle/` (namespace `Jrpg.Core.Battle`):
- `TacticTarget` (EB1): the closed list of whom a rule aims at; null on an action is automatic.
- `TacticTargetCandidates` (EB1, internal static): whom a rule's target may aim at among the skill's legal targets now,
  with no draw.
- `TacticConditionShape` (EB2): what a trigger kind names as "that one" and which number it reads.
- `TacticConditionKinds` (EB2, public static): every trigger kind's shape, one table.
- `TacticSubject` (EB2): who a trigger names: itself, allies, fallen allies or foes (always names no one: no kind).
- `TacticConditionNumber` (EB2): the number a trigger reads: none, a percent, turns (EB11).
- `TacticTargetFit` (EB2, public static): whether a rule's target can aim its skill (or the attack) after its trigger.
- `EnemySkillUse` (EB2, public static): whether an enemy can use a skill, and if not, why.
- `EnemySkillRefusal` (EB2): why an enemy can't use a skill.
- `TacticMemory` (EB9, internal): what an enemy's rules remember of this battle so far.
- `TacticMemoryNotes` (EB9, internal static): writes one `Execute`'s events into the enemies' memories.

Data: no new type. `Battle/EnemyTacticsValidator` gains the target, flag and turns checks and reads
`TacticConditionKinds` instead of switching on kinds.

Editor `Battle/` (namespace `Jrpg.Editor.Battle`):
- `TacticConditionChoice` (EB3): one trigger as the When box offers it, with its words.
- `TacticTargetChoice` (EB3): one target rule as the On box offers it, with its words.
- `TacticSkillChoice` (EB3): one skill (or the attack) as the Use box offers it, by name.
- `TacticRuleRow` (EB3): one rule as typed.
- `TacticListViewModel` (EB4): one list of rules as edited, the enemy's from the start or a phase's with its
  threshold.
- `EnemyTacticsViewModel` (EB5): an enemy's lists as edited, phases added and removed.
- `EnemyTacticsView` (EB7): the Tactics tab, a user control on the Enemies page.

Tests: `Core.Tests/Battle/EnemyTacticsReplayTests` (EB1, step 0): today's choices for three seeds, recorded.

### 2.4 Folder dependencies
| From | To | Why | New? |
|---|---|---|---|
| Core `Battle` | `Effects` | `Targeting`, `HitAffinity`, `EffectKind` | existing |
| Core `Battle` | `Skills` | `SkillDefinition` (fit, use) | existing |
| Core `Battle` | `Statuses` | `StatusSet.FirstWith` (the taunt), `StatusDefinition.IsHarmful` and family (EB12) | existing |
| Data `Battle` | Core `Battle` | `TacticConditionKinds`, `TacticTargetFit`, `EnemySkillUse` | existing folder edge |
| Editor `Battle` | Core `Battle`, Data `Content`, Editor `Catalogs`, `Events` | the choices and checks; `CatalogProblems.OfEnemies`; `TypedNumber`, `ListItemWatch`; `ListOrder` | existing edges |

No new folder edge; Core `Battle` reads no folder that reads it.

### 2.5 Key decisions (README numbers)
- **Eligibility never draws (836):** `TacticTargetCandidates` lists candidates without a draw; only the chosen rule's
  pick draws (`EnemyTargeting.ForRule`: the taunt roll when a taunter is among the skill's legal foes, then the
  random pick for `unafflictedFoe` only). A rule without `On` takes today's path (835), so existing draws hold: EB1's
  replay test proves it (860).
- **The pick (841, 842):** eligible = trigger holds, `BattleState.CanUse(skill)` (or the attack), not a used once-rule,
  and, with `On`, a candidate. Then the first eligible priority rule in list order; else `WeightedPick` over the
  eligible ones; else the attack.
- **The taunt (838):** `ForRule(enemy, target, candidates, legal)`: for a foe-aimed target, the taunting foes among
  `legal` (the skill's legal foes) replace the candidates when any taunts and the enemy doesn't shrug it off; the
  target's own pick then runs over them.
- **The automatic heal (835):** `HealCandidates` intersects the legal allies with `CombatantsMeeting` only when the
  kind is `OwnHpBelow` or its shape's subject is `Allies`; otherwise the legal allies. Without the change, the first
  foe-naming trigger (EB9's `wasHit`) would hand `LowestShareOf` an empty set and its `Aggregate` would throw.
- **Reactions remembered, not acted at once (844, 845, 846):** `TacticMemoryNotes.Note` runs at the end of `Execute`
  while the battle goes on. For the call's actor, if an enemy: forget its reactions (EB9), note a skill used (EB10),
  count the turn (EB11). Then each damage `EffectApplied` on an enemy from a foe actor notes `WasHit` (and
  `WasCriticallyHit` when `IsCritical`, `WasHitOnWeakness` when `Affinity` is weak) with the actor as subject.
- **One trigger's subject (839):** `TacticConditions.CombatantsMeeting` returns whom a trigger names; `Holds` stays "at
  least one". Every Nth turn names the enemy itself, so `Holds` needs no special case; a reaction names the noted
  subjects, fallen or standing (the target rule then keeps the legal ones).
- **The shapes are one table (EB2):** `TacticConditionKinds.ShapeOf(kind)` reads a dictionary `{ kind → (subject,
  number, isReaction) }`, tested to hold every kind. Data's number check, the fit, the automatic heal and the editor's
  number box read it; no switch over kinds outside `CombatantsMeeting`.

### 2.6 Extension points
- **A trigger:** an enum member, its shape row, its arm in `CombatantsMeeting` (or a note in `TacticMemoryNotes` for a
  reaction), a `TacticConditionChoice` line, the automatic heal's test, tests. The cut rows (`allyFell`,
  `harmfulStatusLanded`, `lastStanding`) come back this way.
- **A target:** an enum member, its arm in `TacticTargetCandidates` and `EnemyTargeting.ForRule`'s grouping, its fit
  in `TacticTargetFit`, a `TacticTargetChoice` line.
- **Threat (decision 855):** a per-enemy damage-taken tally in `TacticMemory` (noted from the same events) and a
  `HighestThreatFoe` target; about one slice.
- **A named monster as the target (Defaults to confirm, 10):** an enemy id on the rule and a target "that monster";
  about one slice.
- **Acting out of turn (decision 844's alternative):** section 5's option B, about two slices.
- **Two triggers on one rule; a rule for every phase's list:** `EnemyAction.AlsoWhen`; an `EnemyDefinition.AlwaysRules`
  list joined to whichever list is current.

### 2.7 Replacement mapping
| Today | Becomes | Slice |
|---|---|---|
| `EnemyTargeting.HealCandidates`' "every kind but `AllyFallen`" | `OwnHpBelow` or a trigger naming allies | EB9 |
| `TacticConditions`' three-arm switch | the same switch, more arms; every reaction kind through one arm to `TacticMemory.Noted` | EB9, EB11 |
| Data's `FindConditionProblem` switch on `AllyFallen` | a check on `TacticConditionKinds.ShapeOf(kind).Number` | EB2 |
| Data's `FindUsabilityProblem` (the three refusals) | `Core/Battle/EnemySkillUse.RefusalOf`, Data mapping each `EnemySkillRefusal` to today's message | EB2 |
| `EnemyViewModel.TacticsSummary` and its line on the Stats tab | the Tactics tab | EB6 |
| `EnemyViewModel.ToEnemy`'s carried `Tactics` and `Phases` | `EnemyTacticsViewModel.ToTactics()` / `ToPhases()` | EB6 |
| The draft's "simulations' lines identical" hand steps | `EnemyTacticsReplayTests` | EB1 |

## 3. The lists

### 3.1 Triggers ("when", decision 843; trim freely)
| Kind (`when.kind`) | Editor words | Read | Number | "That one" (subject) | Since |
|---|---|---|---|---|---|
| (left out) | always | - | - | no one | today |
| `ownHpBelow` | its HP is below | at its turn | percent 1..99 | itself | today |
| `allyHpBelow` | an ally's HP is below | at its turn | percent | the living allies below | today |
| `allyFallen` | an ally has fallen | at its turn | - | the fallen allies | today |
| `wasHit` | it was hit | since its last turn | - | the foes who hit it | EB9 |
| `wasCriticallyHit` | it was critically hit | since its last turn | - | the foes who crit it | EB9 |
| `wasHitOnWeakness` | it was hit on a weakness | since its last turn | - | the foes who did | EB9 |
| `foeHpBelow` | a foe's HP is below | at its turn | percent | the living foes below | EB11 |
| `everyNthTurn` | every Nth turn of its own | at its turn | turns 2..10 | itself | EB11 |

Cut at review (decision 843): "an ally fell" (`allyFallen` with "that one" already revives the fallen), "a harmful
status landed on it" (Core has no cleanse; `wasHit` covers hitting back), "it stands alone" (for size). Left out from
the start: "its HP is above" (phases cover the other direction), "a foe is casting" (no cast time), "turn number at
least N" (phases and every Nth turn cover it), "highest threat" (decision 855).

### 3.2 Targets ("on", decision 834; trim freely)
| Target (`on`) | Editor words | Whom | Draws | Since |
|---|---|---|---|---|
| (left out) | automatic | today's rule (decisions 400, 401, 835) | as today | today |
| `subject` | that one | the trigger's subjects still legal; the lowest HP share among several | the taunt's roll for foes | EB1 |
| `self` | itself | the enemy | none | EB1 |
| `lowestHpFoe` | the foe with the lowest HP | the lowest HP share among legal foes | the taunt's roll | EB1 |
| `highestHpFoe` | the foe with the highest HP | the highest share | the taunt's roll | EB1 |
| `lowestHpOnItsSide` | the lowest HP on its side (itself included) | the lowest share among its legal allies and itself | none | EB1 |
| `unafflictedFoe` | a foe without its statuses | a random legal foe holding none of the skill's harmful statuses' families | the taunt's roll, one draw | EB12 |

The taunt (decision 838), one wording everywhere: a taunt narrows among the skill's legal foes for every foe-aimed
rule. While any of them taunts and the enemy doesn't shrug it off (one roll, only then), the rule picks among the
taunting foes, whoever it would have named.

### 3.3 Which target fits which aim (`TacticTargetFit`, decision 840)
The attack aims as `AttackEffect.Targeting` (one foe).

| Target | Fits |
|---|---|
| automatic | every aim |
| `self` | `self`, `singleAlly` |
| `lowestHpFoe`, `highestHpFoe` | `singleEnemy`, the attack |
| `lowestHpOnItsSide` | `singleAlly` |
| `unafflictedFoe` | a `singleEnemy` skill granting at least one harmful status |
| `subject` | by the trigger's subject: itself → `self`, `singleAlly`; allies → `singleAlly`; fallen allies → `singleFallenAlly`; foes → `singleEnemy`, the attack; always (no trigger) → nothing |

A skill hitting all, or aimed along the formation, fits automatic only.

## 4. The file and the editor

### 4.1 `enemies.json`
His first and third examples, as the editor writes them:

```json
{
  "skillId": "field_dressing",
  "weight": 3,
  "when": {
    "kind": "allyHpBelow",
    "percent": 50
  },
  "on": "subject"
},
{
  "skillId": "crushing_blow",
  "weight": 1,
  "when": {
    "kind": "wasCriticallyHit"
  },
  "on": "lowestHpFoe",
  "isPriority": true
}
```

Messages, through `ContentListValidator` ("enemies.json: enemy 'goblin_chief': ..."):
- `tactics[1] (cleave).on: lowestHpFoe doesn't fit cleave, which aims at allEnemies.`
- `phases[0].tactics[1] (attack).on: subject needs a when that names someone.`
- `tactics[0] (attack).isOncePerBattle: the attack can't be once per battle.`
- `tactics[3].when.turns must be within 2..10 (was 1).`
- `tactics[1].when.percent must be left out of wasHit (was 50).`

### 4.2 The Tactics tab (decision 849)
```
+-------------------------+-------------------------------------------------------------+
| Lists                   | Rules                                                       |
| > From the start        | 0. Always: attack on automatic, weight 4                    |
|   Below 50 % HP         | 1. Always: cleave on automatic, weight 2                    |
|                         | 2. When an ally's HP is below 50 %: field_dressing on that  |
| Threshold [50] %        |    one, weight 3                                            |
| [Add phase] [Remove]    | [Add] [Remove] [Up] [Down]                                  |
|                         | When  [an ally's HP is below    v] [50] %                   |
| Uses: cleave,           | Use   [Field Dressing           v]                          |
|  field_dressing, ...    | On    [that one                 v]                          |
|                         | Weight [3]   [ ] Priority (EB8)   [ ] Once per battle (EB10) |
+-------------------------+-------------------------------------------------------------+
```
Numbers are Data's indexes (`tactics[2]`), so the problem line points at a row the eye finds.

## 5. ADR 0057's options, honestly

**When a reaction acts (decision 844):**
- **A. On the enemy's next turn, remembered until then (chosen).** Pros: no new event, no Game change, one note at the
  end of `Execute`; a reaction competes with the enemy's other rules or wins by priority. Cons: no instant counter; a
  stunned enemy's reaction waits; the window spans a whole round, so "it was hit" with priority holds nearly every turn
  for an enemy the party attacks (the developer tunes it with weights instead, or with once per battle).
- **B. At once, as a counter out of turn, on the readied extra turns.** Core already plays readied extra turns:
  `TurnOrder.Advance` takes a readied turn anchored on the combatant who just acted, `BattleTurnSequence` asks the
  tactics for it as for any turn, and the turn strip marks it `IsExtra`. A reaction rule flagged "at once" would, at
  the end of `Execute`, ready the enemy's extra turn anchored on the attacker, and that turn's choice would take the
  rule. Cost: about two slices (Core: the flag, the readying in `TacticMemoryNotes`, one readied turn per combatant
  (question 214), no counter to a counter; Data and the editor's check box), no Game change. Not chosen as the default
  because every counter adds a turn the party didn't plan for; it stays the first extension.

**How a rule is chosen (decision 841):**
- **A. Weights with optional priority rules (chosen).** Keeps ADR 0034's draw for everything not marked; a priority rule
  makes a reaction certain.
- **B. Reactions always first.** Less to set, less control (a "when hit" rule meant to fire sometimes can't).
- **C. An ordered list, the first match wins (gambits).** Refused by ADR 0034: sameness without conditions.

**Where targets live:** on the rule (chosen; decision 697's "one target choice per skill" stays the skill's *aim*, and
the rule picks *who* within it) against a target per skill (an enemy's heal and a member's would aim alike, losing
"heal that ally").

**Phases (unchanged, stated):** a phase replaces the whole rule list, so a boss's reaction rule is typed again in each
phase's list; a rule shared by every list is later work.

## 6. Risks
- **Determinism:** eligibility draws nothing and a rule without `on` takes today's path; `EnemyTacticsReplayTests`
  (EB1, step 0) pins three seeds' full choices of two fights built in the test and runs in every later slice.
- **A crash under new triggers (the review's B1):** the automatic heal's narrowing would empty under a foe-naming
  trigger; decision 835 changes it in EB9, the first such trigger, and each trigger slice tests the heal.
- **Hot files:** `BattleState.cs` 283 (+1 line, EB9). The Enemies page's XAML after WS7 (re-read in EB7; the tab is a
  user control, +3). `EnemyViewModel.cs` 188 plus EP (a partial takes the tactics, and `TacticsSummary` leaves, -6).
- **Complexity (CA1502):** `TacticConditions.CombatantsMeeting` reaches 6 arms and a default at EB11 (every reaction
  through one arm); `EnemyTargeting.ForRule` groups targets by how they pick (by share, by draw, itself) and stays at 4
  arms; `EnemyTactics.IsEligible` is four named predicates joined; Data's checks read the shape table.
- **Test churn:** `EnemyViewModelTests` asserts `Assert.Same(Chief.Tactics, enemy.Tactics)` (lines 40-42) and the
  summary line (152-164): EB6 re-pins the first to content equality and deletes the second with the line it tested.
- **Equality:** `EnemyAction` and `TacticCondition` are records without lists; their new fields join equality, so
  `CatalogDraft.SetEnemies`'s "nothing changed" check holds for an untouched enemy rebuilt from rows
  (`CatalogDraftEnemyTests.SetEnemies_EqualByContent_RecordsNothing` guards it; EB6 adds the page's own test).
- **Other plans:** the mode bar's WS7 rewrites the Enemies window as a page; EB6 and EB7 re-read it. Row 6's EP edits
  `EnemyViewModel.cs` and the Stats tab; the tactics go in a new partial and a new tab, so the edits don't overlap.
- **Save format:** none (enemies and battles aren't saved).
- **Balance:** none by itself (no content change); a reaction rule with priority makes an enemy predictable, the
  developer's to tune.
