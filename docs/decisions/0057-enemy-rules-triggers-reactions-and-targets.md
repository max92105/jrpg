# 0057. Enemy rules: triggers, reactions and targets

Status: accepted
Date: 2026-11-16

Amends ADR 0034: an enemy's action becomes a rule that may name its target, answer what was done to it since its last
turn, win before the draw and be used once per battle.

## Context
ADR 0034 gave an enemy a weighted list of actions (a skill or the attack, at most one condition: its HP below a share,
an ally's HP below a share, an ally fallen) and phases that replace the list. Every action aims as its skill does (a
random foe by the taunt rule; all, or itself; a heal on the lowest share among those its condition found, decisions
400, 401). The developer wants complex fights built in the editor: "when ally is 50 percent hp - use x skill - on that
ally", "when critically hit - skill x - lowest hp enemy", from closed lists of common triggers, skills and targets,
reactions to battle events included (decisions 679, 698). Constraints: no scripting language and no nested logic
(decision 679); every existing enemy keeps its targets and its seeded draws; Core keeps no threat; the Game changes
nothing. The developer delegated the plan's approval, so its game-design choices were taken while he was away and are
marked below.

## Options
### 1. When a reaction acts (decision 844)
**A. On the enemy's next turn, remembered until then (chosen).**
- Pros: no new event and no Game change; one note at the end of `BattleState.Execute`; a reaction competes with the
  enemy's other rules, or wins by priority.
- Cons: no instant counter; a stunned enemy's reaction waits; the window spans a whole round, so "it was hit" with
  priority holds nearly every turn for an enemy the party attacks (tuned with weights instead, or once per battle).

**B. At once, as a counter out of turn, on the readied extra turns.** Core already plays them (a rogue's Ready):
`TurnOrder.Advance` takes a readied turn anchored on the combatant who just acted, `BattleTurnSequence` asks the
tactics for it as for any turn, and the turn strip marks it `IsExtra`. A reaction rule flagged "at once" would, at the
end of `Execute`, ready the enemy's extra turn anchored on the attacker, and that turn's choice would take the rule.
- Pros: an instant counter; no Game change.
- Cons: about two slices (Core: the flag, the readying in `TacticMemoryNotes`, one readied turn per combatant
  (question 214), no counter to a counter; Data and the editor's check box); every counter adds a turn the party
  didn't plan for.

### 2. How a rule is chosen (decision 841)
**A. Weights with optional priority rules (chosen).**
- Pros: ADR 0034's draw for everything not marked; a priority rule makes a reaction certain.
- Cons: the list's order matters, among priority rules only.

**B. Reactions always first.**
- Pros: less to set.
- Cons: less control: a "when hit" rule meant to fire sometimes can't.

**C. An ordered list, the first match wins (gambits).**
- Cons: refused by ADR 0034 (its option B): sameness without conditions.

### 3. Where a target lives
**A. On the rule (chosen).** Decision 697's "one target choice per skill" stays the skill's *aim*; the rule picks *who*
within it.
- Pros: "heal that ally" and "the lowest HP foe" are each one rule's choice.
- Cons: a target fits some aims only, so Core needs a fit table and Data a check (decision 840).

**B. A target per skill.**
- Cons: an enemy's heal and a member's would aim alike, losing "heal that ally".

## Decision
Option A on each question (decisions 832-848, 855, 856).
- **A rule (832)** is ADR 0034's action extended: *when* (one trigger, or always), *use* (a skill or the attack), *on*
  (a target rule, or automatic), a weight, and two optional flags, *priority* and *once per battle*. The code keeps
  `EnemyAction` and `TacticCondition`; "rule", "when", "use" and "on" are the editor's words.
- **One trigger per rule (833).** (default taken while the developer was away; his to overturn) No "and": a boss trick
  two triggers would need becomes a phase, a named trigger or a named rule in Core with a test.
- **The triggers (843).** (default taken while the developer was away; his to overturn) Today's three (its HP below a
  share, an ally's HP below a share, an ally has fallen), two more states read at its turn (a foe's HP below a share,
  every Nth turn of its own) and three reactions (it was hit, critically hit, hit on a weakness). Cut: "an ally fell",
  "a harmful status landed", "it stands alone".
- **Every Nth turn (847).** (default taken while the developer was away; his to overturn) It counts the turns the
  enemy acted (a skipped turn doesn't count) and holds on its Nth, 2Nth...; N from 2 to 10.
- **The targets (834).** (default taken while the developer was away; his to overturn) Automatic, that one, itself,
  the foe with the lowest HP, the foe with the highest HP, the lowest HP on its side (itself included), and a foe
  without the skill's statuses. A random foe or ally is what automatic already does for a single-foe or single-ally
  skill (decision 400). His "x skill on y enemy" is the lowest or highest HP foe; one named monster would need an enemy
  id on the rule.
- **Automatic (835).** A rule without `on` aims as today. The heal's narrowing to those its condition found applies
  only when the trigger is `ownHpBelow` or names allies; under any other trigger it takes the lowest share among all
  its legal targets, since a trigger naming foes, the fallen or only itself would leave nobody.
- **Eligibility (836).** A rule is eligible only when its target rule finds a legal target now; deciding eligibility
  never draws. The draws (the taunt's roll, a random pick) come after the pick, for the chosen rule only.
- **Lowest and highest HP (837).** (default taken while the developer was away; his to overturn) HP shares (HP over
  max HP), cross-multiplied so nothing rounds, a tie going to the first in battle order (decision 401). Not taken:
  absolute HP.
- **The taunt (838).** (default taken while the developer was away; his to overturn) For every rule aiming at one foe,
  "that one" included: while any of the skill's legal foes taunts and the enemy doesn't shrug it off (decision 324's
  roll, drawn only when a taunter is among them), the rule picks among the taunting foes only, whoever it would have
  named.
- **That one (839)** is whom the trigger names: the allies below the share, the fallen allies, itself, the foes below,
  the foes who hit it; among several still legal, the lowest HP share, without a draw.
- **The fit (840).** A target fits some aims only (Core's `TacticTargetFit`); Data refuses a misfit naming the enemy,
  the rule's place, its skill and the skill's aim, and the editor shows that message.
- **Priority (841).** (default taken while the developer was away; his to overturn) When eligible rules are marked
  priority, the first of them in the list is chosen, with no draw; otherwise the weighted draw among the eligible
  rules, as in ADR 0034.
- **Once per battle (842).** (default taken while the developer was away; his to overturn) A rule marked once isn't
  eligible after the enemy used its skill in this battle, through any rule; the attack can't be marked once.
- **Reactions (844).** (default taken while the developer was away; his to overturn) Answered on the enemy's next
  turn, never out of turn: what happened is remembered from the moment it happens until the end of the next turn the
  enemy acts (a skipped turn keeps it), then forgotten, whether a rule answered it or not.
- **A hit (845)** is each damage hit a foe's action landed on the enemy, a crit and a weakness hit counted as such. A
  miss and a status tick aren't hits; a blocked hit is one; a skill hitting every foe notes it on every enemy it lands
  on.
- **The memory (846)** (`TacticMemory`: the reactions since its last turn, the skills it used, the turns it acted) is
  noted at one place, the end of `BattleState.Execute`, from the call's events, as phases are, so every path that hits
  is seen.
- **The file (848).** An action's `on`, `isPriority`, `isOncePerBattle` and a condition's `turns` are optional and
  left out at their defaults (null, false, false, 0).

## Consequences
- Every existing enemy keeps its behavior and its seeded draws: a rule without `on` takes today's path, and
  eligibility and the memory draw nothing.
- Every enemy the editor writes reads like today's, and hand-written files keep loading. No save format change:
  enemies and battles aren't saved. No Game file changes.
- A new trigger is an enum member, its row in Core's shape table, its arm (or its note for a reaction), its Data rule,
  its editor words and a test that the automatic heal still finds someone; a new target is a member, its arm, its fit
  and its words.
- A reaction rule with priority makes an enemy predictable; the developer tunes it with weights or once per battle.
- "Once per battle" leaves ADR 0034's later work. "Highest threat" waits: Core keeps no threat, and "that one" after
  "it was hit" stands in for it (decision 855).
- Later work, unchanged from ADR 0034 (decision 856): cooldowns, two triggers on one rule, acting out of turn (option
  1B, the first extension), phases that change stats, resistances or art. A phase still replaces the whole rule list,
  so a boss's reaction rule is typed again in each phase's list; a rule for every list is later work.
