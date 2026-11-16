# Enemy behavior (editor push, roadmap row 8)
Status: approved, in progress

Drafted on 2026-11-15 from roadmap row 8 of [the editor push](../2026-11-15-editor-push/README.md), its design.md
section 3.7, decisions 679 and 698 there, and the developer's words: "behavior - way to use these skills in combat. I
want to be able to make complex fights... would be cool if I can edit behavior in editor. like: when ally is 50 percent
hp - use x skill - on that ally; or when ally is 50 percent hp - use x skill - on y enemy", and "list common trigger -
skills - targets condition (ex. when critically hit - skill x - lowest hp enemy)". Approved on 2026-11-15 by delegation
from the developer, with the devil's advocate's review applied: the automatic heal no longer empties under a foe-naming
trigger (B1, decision 835); the counter out of turn costed honestly (C1, decision 844); one taunt rule (C2, 838); a
shorter plan with his three examples done by EB9 (C3, C4: the choices folded into EB2 and EB3, reactions right after
priority, `allyFell`, `harmfulStatusLanded` and `lastStanding` cut); the whole plan after row 6's SW (C5, 857); a
replay test instead of pasted simulation lines (C6, 860); his second example mapped (C7, 834); the hit rules tested
(C8, 845). Every game-design choice taken while he was away is marked "(default taken while the developer was away;
his to overturn)" and listed in "Defaults to confirm".

**Before this plan starts:**
- Row 6 ([skills and statuses model](../2026-11-15-skills-and-statuses-model/README.md)) is done through SW at least:
  `EnemyTargeting` finds taunting members with SW's `Statuses.FirstWith(StatusPartKind.DrawAttacks)`, Core tests build
  Stun and Taunt as SW's records (`Builders/TestStatuses`), and EB12 reads a status record's `IsHarmful` and family.
  Row 6's T3 and EP (an enemy's Power and its box) are in `EnemyDefinition` and the Enemies page.
- The mode bar's stage 1 ([mode bar](../2026-11-15-mode-bar/README.md), WS7) has made the Enemies window a page of the
  Database mode (`Battle/EnemyCatalogPage`, hosted in `Workspaces/CatalogDialog`): EB6 and EB7 edit the page, not
  `EnemyCatalogWindow.xaml` (deleted by WS7), and each re-reads it first.

## Goal
1. **"When / use / on" rules for enemies, as content and in the editor:** an enemy's tactics and each phase's are a
   list of rules, each *when* (a trigger from a closed Core list, or always), *use* (one of the enemy's skills, or the
   attack), *on* (a target rule from a closed Core list, or the skill's automatic aim), with a weight, and optionally
   *priority* (chosen before any draw) and *once per battle*. His three examples, all done by EB9:
   - "when an ally is at 50 % HP: x skill on that ally": `allyHpBelow 50`, `on: subject` (EB1, EB2; the tab, EB7);
   - "when an ally is at 50 % HP: x skill on y enemy": `allyHpBelow 50`, `on: lowestHpFoe` or `highestHpFoe` (EB1; if
     "y" means one named monster, see "Defaults to confirm", 10);
   - "when critically hit: skill x on the lowest HP enemy": `wasCriticallyHit`, `on: lowestHpFoe`, priority (EB8,
     EB9).
2. **Triggers that react to battle events** (being hit, critically hit, hit on a weakness), not only states read at the
   enemy's turn (decision 698).
3. **The Tactics tab of the Enemies page:** the lists (from the start, then each phase with its threshold), the rules
   as sentences, the chosen rule's boxes; add, remove, move up and down; add and remove phases. Today's "Tactics and
   phases: edited in enemies.json" line goes.
4. **Every existing enemy keeps its behavior and its seeded draws,** guarded by a recorded replay test (EB1).

## Order, and why
1. **ADR 0057 first** (amends ADR 0034): reactions' timing and the priority rule had real alternatives.
2. **Targets first (EB1, EB2):** his first two examples are target rules on a trigger that exists today. EB1 starts by
   recording today's choices as a test, before `EnemyTactics` changes. EB2 brings the file and the Core rules the
   editor's boxes read (`TacticConditionKinds`, `TacticTargetFit`, `EnemySkillUse`).
3. **The tab next (EB3-EB7),** on today's three triggers plus the targets, so he edits behavior in the editor before
   the lists grow: one rule with the boxes' choices, one list, the enemy's lists, the wiring into the enemy, then the
   tab (an editor panel's rules before its view).
4. **Priority, then reactions (EB8, EB9):** his third example, and the memory every later option reads.
5. **A trimmable last phase (EB10-EB12):** once per battle, two more state triggers, the foe without its statuses.
   Each goes through Core, Data and the editor's words in one slice; striking a row removes its part or its slice.

## Guiding rules
- **Closed lists, named rules, no scripting** (decision 679): a new trigger or target is an enum member, its row in
  Core's shape table, its arm, its Data rule, its editor words and its tests.
- **Nothing changes for an enemy that doesn't use the new fields:** a rule without `on` aims as today (decisions 400,
  401; the one change, decision 835, only reaches triggers that don't exist yet), deciding eligibility never draws,
  and the memory draws nothing. EB1's replay test (decision 860) stays asserted through every later slice.
- **One place sees the battle's history:** the end of `BattleState.Execute`, from the call's events (ADR 0034's rule for
  phases), writes what an enemy's rules remember (`TacticMemory`, from EB9).
- **Every switch over a kind has room:** Core's `TacticConditions.CombatantsMeeting` (EB9 sends every reaction kind
  through one arm to `TacticMemory.Noted`), `EnemyTargeting.ForRule` (targets grouped by how they pick), Data's
  condition check (reads Core's shape table, no switch), the editor's words (`TacticConditionChoice.All`, a list
  guarded by an every-kind test). Each slice says where its arm goes.
- **Each new trigger proves the automatic heal still finds someone** (decision 835): its slice adds
  `ChooseCommand_AutomaticSingleAllyHealOn<Kind>_HealsTheLowestShare`.
- **Sizes:** Core and Data slices about 300 lines with tests at about 13 lines each; Editor slices about 200, tests at
  about 10 lines each. EB3 is above (about 280, the review folded the choices into it) and names its split point.
- **Files near 300 (checked 2026-11-15):** `Core/Battle/BattleState.cs` 283 (EB9 adds one line, the memory's note
  beside the phases'; nothing else adds to it). `Editor/Battle/EnemyViewModel.cs` 188 plus row 6's EP: the tactics
  live in a new partial `EnemyViewModel.Tactics.cs` (EB6). The Enemies page's XAML (WS7's `EnemyCatalogPage.xaml`,
  about 200): the tab is a user control of its own, `EnemyTacticsView.xaml` (EB7), so the page gains about 3 lines.
  `Data/Battle/EnemyTacticsValidator.cs` 113, about 160 after EB12.
- **Test files, globbed on 2026-11-15:** `Data.Tests/Battle/EnemyTacticsValidatorTests` is at 233: new Data tests go to
  `EnemyTacticRuleValidatorTests` (EB2, EB10: `on` and the flags) or `EnemyTacticConditionValidatorTests` (EB9, EB11:
  the new kinds), both new. `Editor.Tests/Battle/EnemyCatalogViewModelTests` is at 160, about 190 after WS7: EB3 adds
  two tests. No test file named `*TacticTarget*`, `*TacticRule*`, `*TacticList*`, `*TacticChoice*`,
  `*EnemySkillUse*`, `*Reaction*`, `*TacticMemory*`, `*Replay*` or `*OncePerBattle*` exists; each slice names the file
  it creates or extends.
- **Tests build their enemies** (`EnemyDefinitionBuilder`, records built in the test, the test world's enemies by id
  with `with`), never today's values of `content/enemies.json`: the test world's non-map side is the live content.

## Out of scope
- **Acting out of turn** (an immediate counterattack): reactions are answered on the enemy's next turn (decision 844);
  the ADR states the out-of-turn option's cost (about two slices on the readied extra turns).
- **Threat or aggro** ("highest threat"): Core keeps no threat; "that one" after "it was hit" is the stand-in (decision
  855).
- **Two triggers on one rule**, a rule shared by every phase's list, cooldowns, phases that change stats, resistances or
  art (decision 856).
- **The cut rows:** `allyFell`, `harmfulStatusLanded`, `lastStanding` (decision 843).
- **Repository content:** no enemy gets new rules from this plan (decision 854); the developer writes them in the
  editor.
- The skill and status editors (row 7), encounters (row 10).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: a rule names its target
- [x] ADR 0057, enemy rules: triggers, reactions and targets (amends ADR 0034). Write it from decisions 832-848 and
  855-856 with design.md, section 5's options and costs; the developer delegated approval, so the defaults are marked
  in it and no question is asked.
- [x] **EB1. Today's choices recorded; a rule names its target: itself, the lowest or highest HP foe, the lowest HP on
  its side, or whom its condition names** (Core, about 270 lines plus a 70-line replay test first) →
  [slices/EB1.md](slices/EB1.md)
- [x] **EB2. `on` in `enemies.json`: loaded, checked against the skill's aim and the trigger, written; which skills an
  enemy can use moves to Core** (Core + Data, about 290 lines) → [slices/EB2.md](slices/EB2.md)

### Phase 2: the Tactics tab
- [x] **EB3. One rule as typed, and what its boxes offer** (Editor, about 280 lines; split point named) →
  [slices/EB3.md](slices/EB3.md)
- [x] **EB4. One list of rules: add after the chosen one, remove, move; a phase's threshold** (Editor, about 200
  lines) → [slices/EB4.md](slices/EB4.md)
- [x] **EB5. An enemy's lists: from the start and each phase, phases added and removed** (Editor, about 170 lines) →
  [slices/EB5.md](slices/EB5.md)
- [x] **EB6. The Enemies page keeps and applies an enemy's rules; Duplicate copies them** (Editor, about 150 lines) →
  [slices/EB6.md](slices/EB6.md)
- [x] **EB7. The Tactics tab** (Editor, about 190 lines, mostly XAML) → [slices/EB7.md](slices/EB7.md)

### Phase 3: priority and reactions
- [x] **EB8. Priority: an eligible priority rule is chosen before any draw** (Core + Data + Editor, about 160 lines) →
  [slices/EB8.md](slices/EB8.md)
- [x] **EB9. Reactions: it was hit, critically hit, hit on a weakness; the enemy's tactic memory** (Core + Data +
  Editor, about 300 lines) → [slices/EB9.md](slices/EB9.md)

### Phase 4: more ways to choose (trimmable)
- [ ] **EB10. Once per battle** (Core + Data + Editor, about 180 lines) → [slices/EB10.md](slices/EB10.md)
- [ ] **EB11. Triggers: a foe's HP is below, every Nth turn of its own** (Core + Data + Editor, about 290 lines) →
  [slices/EB11.md](slices/EB11.md)
- [ ] **EB12. Target: a foe without the skill's statuses** (Core + Data + Editor, about 130 lines) →
  [slices/EB12.md](slices/EB12.md)

`slices/EB13.md` and `slices/EB14.md` are superseded stubs from the draft: delete them.

## Decisions

Numbers 831-860. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **831.** One plan, four phases: targets (EB1, EB2), the Tactics tab (EB3-EB7), priority and reactions (EB8, EB9), a
  trimmable last phase (EB10-EB12); ADR 0057 before EB1. It runs after row 6's SW and the mode bar's WS7.

The model (ADR 0057, amending ADR 0034):
- **832.** An enemy's rule is ADR 0034's action extended: *when* (one trigger, or always), *use* (a skill or the
  attack), *on* (a target rule, or automatic), a weight, and two optional flags, *priority* and *once per battle*. The
  code keeps `EnemyAction` and `TacticCondition` (113 uses); "rule", "when", "use" and "on" are the editor's words.
  No scripting language, no nested logic (decision 679).
- **833.** (default taken while the developer was away; his to overturn) One trigger per rule: no "and". A boss trick
  two triggers would need becomes a phase, a named trigger or a named rule in Core with a test.
- **834.** (default taken while the developer was away; his to overturn) The target rules are design.md, section 3.2's
  table: automatic, that one (whom the trigger names), itself, the foe with the lowest HP, the foe with the highest
  HP, the lowest HP on its side (itself included), and from EB12 a foe without the skill's statuses. A random foe and
  a random ally are what automatic already does for a single-foe or single-ally skill (decision 400), so they get no
  entry. His second example, "when an ally is at 50 % HP: x skill on y enemy", is `allyHpBelow 50` with `lowestHpFoe`
  or `highestHpFoe`; if "y" means one named monster, no target covers it (it would need an enemy id on the rule):
  listed in "Defaults to confirm".
- **835.** A rule without `on` aims as today (decisions 400, 401: a random foe by the taunt rule; all, or itself, as
  the skill aims; a heal on one ally at the lowest HP share among those its condition found). The heal's narrowing
  applies only when the trigger is `ownHpBelow` or names allies (`TacticConditionKinds.ShapeOf(kind).Subject ==
  Allies`); under any other trigger it takes the lowest share among all its legal targets, since a trigger naming
  foes, the fallen or only itself would leave nobody and the pick would throw (`EnemyTargeting.HealCandidates` today
  intersects for every kind but `allyFallen`). Every existing enemy keeps its targets and its draws.
- **836.** A rule is eligible only when its target rule finds a legal target now (the skill's own legal candidates);
  deciding eligibility never draws. The draws (the taunt's roll, a random pick) come after the pick, only for the
  chosen rule.
- **837.** (default taken while the developer was away; his to overturn) "Lowest HP" and "highest HP" compare HP shares
  (HP over max HP), cross-multiplied so nothing rounds, a tie going to the first in battle order, as decision 401's
  heal does. Not taken: absolute HP (a 30 HP goblin "lower" than a 200 HP knight at 20 %).
- **838.** (default taken while the developer was away; his to overturn) A taunt narrows among the skill's legal foes
  for every rule that aims at one foe, "that one" included: while any of the skill's legal foes taunts and the enemy
  doesn't shrug it off (decision 324's roll, drawn only when a taunter is among the legal foes), the rule picks among
  the taunting foes only (the lowest or highest share among them, or, for "that one", the lowest share among them),
  whoever the rule would have named.
- **839.** "That one" (`subject`) is whom the trigger names (design.md, section 3.1's subject column): the allies below
  the share, the fallen allies, itself, the foes below, the foes who hit it. Among several still legal, the lowest HP
  share, without a draw (decision 401's rule widened).
- **840.** A target rule fits some aims only (design.md, section 3.3): Core's `TacticTargetFit` says which; Data
  refuses a misfit naming the enemy, the rule's place, its skill and the skill's aim; the editor shows that message.
- **841.** (default taken while the developer was away; his to overturn) Priority: when one or more eligible rules are
  marked priority, the first of them in the list is chosen, with no draw; otherwise the weighted draw among the
  eligible rules, as today. The list's order matters only among priority rules. Not taken: every reaction taking
  priority by itself (less control), or a pure ordered list (ADR 0034's option B, refused there for its sameness).
- **842.** (default taken while the developer was away; his to overturn) Once per battle: a rule marked once isn't
  eligible after the enemy used its skill in this battle, through any rule; the attack can't be marked once.
- **843.** (default taken while the developer was away; his to overturn) The triggers are design.md, section 3.1's
  table: today's three (its HP below a share, an ally's HP below a share, an ally has fallen), two more states read at
  its turn (a foe's HP below a share, every Nth turn of its own) and three reactions (it was hit, critically hit, hit
  on a weakness). Cut at review: "an ally fell" (near "an ally has fallen", whose "that one" already revives the
  fallen), "a harmful status landed" (Core has no cleanse; "it was hit" covers hitting back), "it stands alone" (for
  size; one row and a few lines later).
- **844.** (default taken while the developer was away; his to overturn) A reaction is answered on the enemy's next
  turn, never out of turn: what happened is remembered from the moment it happens until the end of the next turn the
  enemy acts (a skipped turn keeps it), then forgotten, whether a rule answered it or not. That window spans a whole
  round, so "it was hit" with priority holds nearly every turn for an enemy the party attacks. Not taken: an immediate
  counter out of turn, about two slices on Core's readied extra turns (ADR 0057; design.md, section 5).
- **845.** A reaction counts what a foe's action did to the enemy: each damage hit landed on it (`EffectApplied` of
  kind damage after the actor's `ActionStarted`), a crit, a weakness hit (`HitAffinity.Weak`). A miss isn't a hit, a
  status tick isn't one (decision 803), a blocked hit is one (it still lands; no enemy can block today, as no enemy has
  a block chance, so the rule is set for when one can), and a skill hitting every foe notes it on every enemy it lands
  on.
- **846.** What an enemy's rules remember (`TacticMemory`: the reactions since its last turn, the skills it used, the
  turns it acted) is noted at one place, the end of `BattleState.Execute`, from the call's events, as phases are (ADR
  0034), so every path that hits is seen.
- **847.** (default taken while the developer was away; his to overturn) Every Nth turn counts the turns the enemy
  acted (a skipped turn doesn't count) and holds on its Nth, 2Nth...; N from 2 to 10.
- **848.** The file: an action's `on`, `isPriority`, `isOncePerBattle` and a condition's `turns` are optional and left
  out at their defaults (null, false, false, 0), so every enemy the editor writes reads like today's and hand-written
  files keep loading. Enemies aren't saved: no save format change.

The editor:
- **849.** The Enemies page gets a Tactics tab: the lists left ("From the start", then "Below 50 % HP" per phase, its
  threshold box under the list, Add phase and Remove phase), the chosen list's rules right as numbered sentences ("2.
  When an ally's HP is below 50 %: field_dressing on that one, weight 3"), Add (after the chosen rule), Remove, Up and
  Down under them, the chosen rule's boxes below (When with its number, Use, On, Weight, and the flags from EB8 and
  EB10). The page's apply stays one undo step (decision 591).
- **850.** The When and On boxes offer every trigger and target, always; a combination that doesn't fit shows on the
  problem line (Data's message) instead of narrowing the boxes, so a two-way selection is never lost to a replaced
  list (drop tables plan's lesson).
- **851.** The Use box offers the attack, then the draft's skills an enemy can use (Core's `EnemySkillUse`: no class
  resource, no formation aim, no readied turn), by name. A line under the lists says "Uses: cleave, field_dressing":
  the skills its rules name, which are its skills (decision 679).
- **852.** The tab lives on the Enemies page of the Database mode (the mode bar's WS7, `Battle/EnemyCatalogPage`), so it
  has the main window's width; EB7 re-reads the page first and checks the two columns fit.
- **853.** A rule's sentence names its skill by id: the rows don't hold the catalog, so `EnemyViewModel.From(enemy)`
  keeps its signature (45 test sites).

Content, scope and other rows:
- **854.** (default taken while the developer was away; his to overturn) No repository enemy gets new rules in this
  plan; the developer writes them in the editor.
- **855.** "Highest threat" waits: Core keeps no threat. "That one" after "it was hit" (the foes who hit it) is the
  stand-in; a threat table is an extension point (design.md, section 2.6).
- **856.** Later work, unchanged from ADR 0034: cooldowns, two triggers on one rule, acting out of turn, phases that
  change stats, resistances or art. A phase still replaces the whole rule list: a boss's reaction rule is typed again
  in each phase's list; "a rule for every list" is later work.
- **857.** The plan runs after row 6's SW: `EnemyTargeting`'s taunt reads SW's `FirstWith(StatusPartKind.DrawAttacks)`,
  Core tests build Stun and Taunt as SW's records, and EB12 reads a status record's `IsHarmful` and family.
- **858.** "Foe" is a combatant on the enemy's other side (a party member), in code (`FoeHpBelow`, `LowestHpFoe`) and
  in the editor's words; "enemy" stays the record kind (`EnemyDefinition`) so the two never mix.
- **859.** A new rule starts as always, the attack, automatic, weight 1; a new phase's threshold box starts at 50 for
  the first phase, else half the last phase's threshold rounded down, at least 1 (tunables, design.md, section 1).
- **860.** Before `EnemyTactics` changes, `Core.Tests/Battle/EnemyTacticsReplayTests` records, for three fixed seeds,
  the full sequence of `ActionStarted` actors, commands and targets of a fight with a Chief-like boss and one with
  forest-like goblins, both built in the test with today's kinds of rules; it stays asserted for the whole plan and
  replaces the draft's "paste identical simulation lines" steps.

## Defaults to confirm
Taken while the developer was away, each his to overturn; the loud ones first:
1. **844. Reactions act on the enemy's next turn,** not as an immediate counter. The window spans a round, so "it was
   hit" with priority answers nearly every turn. A counter out of turn would cost about two slices on Core's readied
   extra turns, no Game change (ADR 0057).
2. **841. Priority rules:** an eligible priority rule is chosen first, no draw; without one, the weighted draw as today.
   This is what makes "when critically hit - skill X" reliable.
3. **843 and 834. The lists** (design.md, sections 3.1 and 3.2): 8 triggers (3 today, 5 new) and 6 targets plus
   automatic; `allyFell`, `harmfulStatusLanded` and `lastStanding` cut. EB10, EB11 and EB12 can go whole.
4. **838. Taunt beats every single-foe rule,** "that one" included.
5. **837. "Lowest HP" means the lowest share,** not the lowest number.
6. **842. Once per battle,** by skill; never on the attack.
7. **833. One trigger per rule.**
8. **847. Every Nth turn** counts turns acted, N from 2 to 10.
9. **854. No repository enemy gets new rules.**
10. **834. "On y enemy"** is read as the lowest or highest HP foe. If he meant one named monster of the fight, a rule
    would need an enemy id and a target "that monster" (about one slice).

## Open design questions (developer decides)
- None blocking: the defaults above are the questions. For later: threat (decision 855), a counter out of turn
  (decision 844's alternative), two triggers on one rule, a rule shared by every phase.

## Notes for other rows
- **Row 6:** the plan starts after SW (decision 857); EP's Power box sits on the Stats tab, unaffected by the Tactics
  tab.
- **Row 7 (skills editor):** a skill an enemy's rule names is "used" by that enemy; the Skills window's removal guard
  (if it has one) reads `EnemyDefinition.NamesSkill`, which this plan keeps.
- **Row 11 (mode bar):** its WS7 lands first; the Tactics tab is a user control (`EnemyTacticsView`) added to the
  Enemies page.

## ADRs needed
- **0057, enemy rules: triggers, reactions and targets** (before EB1), amending ADR 0034: decisions 832-848, 855, 856;
  options and costs in design.md, section 5. Written from the decisions without a question: the developer delegated
  approval.
