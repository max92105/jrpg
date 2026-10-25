# Class mechanics: design

Part of [the class mechanics plan](README.md).

### Tunables and placeholders

Every value here is a placeholder (decision 196). Rules behind them are the README's open questions. Content values live in `content/`; screen values are named constants in their view.

**Resources** (`classes.json`, each class's `resources`; ADR 0019). A gain of 0 is left out of the file.

| Class | Resource id, name | Max | Starts at | Gains | Special | Slice |
|---|---|---|---|---|---|---|
| mage | `mana`, Mana | 10 | 3 | +2 per turn | start lowered by missing Clarity | M1b, LR3 |
| mage | `clarity`, Clarity | 3 | 3 | −1 per battle fought | lasting | LR3 |
| fighter | `stamina`, Stamina | 6 | 3 | +1 per turn | | FT1 |
| barbarian | `ferocity`, Ferocity | 10 | 0 | +2 per hit taken | | BB1 |
| paladin | `conviction`, Conviction | 10 | 0 | +3 per enemy fallen; +2 per heavy hit (20 %) | | PL1 |
| rogue | `readiness`, Readiness | from speed | = max | | max = speed ÷ 10, at least 1 | RG1 |
| warlock | `instability`, Instability | 10 | 0 | −1 per turn | backfire 5 % per point, 10 % of max HP | WL1, WL2 |
| druid | `growth`, Growth | 10 | 0 | | dominance over `decay`, +50 % | DR2 |
| druid | `decay`, Decay | 10 | 0 | | dominance over `growth`, +50 % | DR2 |
| cleric | `faith`, Faith | 5 | 5 | −1 per ally fallen; −1 per heavy hit (20 %) | never spent; lasting from LR3 | CL1, PL1, LR3 |
| monk | `chakra`, Chakra | 10 | 0 | +1 per hit landed | | MO1 |

**Speed** (`classes.json` `base.speed` and `growth.speed`, RG1; question 213): fighter 12/+1, mage 10/+1, cleric 10/+1, rogue 30/+1, barbarian 11/+1, paladin 9/+1, monk 18/+1, warlock 11/+1, druid 12/+1. Only the rogue's readiness reads it.

**Skills** (`skills.json`; class lists in `classes.json` `skillIds`, ADR 0020). "Change" is what a skill does to a resource: negative spends, positive gains. "Needs" is a minimum that isn't spent.

| Skill id | Class | Change | Needs | Effect | Status granted | Other | Slice |
|---|---|---|---|---|---|---|---|
| `fire_blast` (kept) | mage | mana −2 | | damage 15, single enemy | | | M2b |
| `fireball` (kept) | mage | mana −5 | | damage 20, all enemies | | | M2b |
| `meteor` | mage | mana −9 | | damage 60, single enemy | | | M2b |
| `heal` (kept) | cleric | | faith 1 | restore 15 HP, single ally | | | CL1 |
| `recovery` (kept) | cleric | | faith 2 | restore 30 HP, self | | | CL1 |
| `heal_all` (kept) | cleric | | faith 4 | restore 50 HP, all allies | | | CL1 |
| `power_strike` | fighter | stamina −2 | | damage 35, single enemy | | | FT1 |
| `shield_bash` | fighter | stamina −3 | | damage 15, single enemy | Stun, 1 turn | | FT1 |
| `taunt` | barbarian | | | grant status, self | Taunt, 2 turns | | BB1 |
| `rampage` | barbarian | ferocity −6 | | damage 70, single enemy | | | BB1 |
| `smite` | paladin | conviction −3 | | damage 40, single enemy | | | PL1 |
| `aura_of_protection` | paladin | conviction −4 | | grant status, allies in range 1 | Guarded +5, 3 turns | | PL2b |
| `wide_aura` | paladin | conviction −6 | | grant status, allies in range 2 | Guarded +3, 3 turns | | PL2b |
| `ready` | rogue | readiness −1 | | extra turn after one other combatant | | quick | RG4 |
| `expose` | rogue | | | grant status, single enemy | Exposed +50 %, 3 turns | | RG4 |
| `eldritch_bolt` | warlock | instability +2 | | damage 20, single enemy | | | WL1 |
| `void_rift` | warlock | instability +3 | instability 4 | damage 25, all enemies | | | WL1 |
| `chaos_lance` | warlock | instability +4 | instability 7 | damage 70, single enemy | | | WL1 |
| `rejuvenate` | druid | growth +2 | | grant status, single ally | Regen 10, 3 turns | | DR2 |
| `rot` | druid | decay +2 | | grant status, single enemy | Poison 8, 3 turns | | DR2 |
| `equilibrium` | druid | growth −3, decay −3 | | damage 45, all enemies | | | DR2 |
| `palm_strike` | monk | | | damage 10, single enemy | | 2 hits | MO1 |
| `flurry` | monk | chakra −3 | | damage 8, single enemy | | 4 hits | MO1 |

Content ids never change: `fire_blast`, `fireball`, `heal`, `recovery` and `heal_all` keep theirs, and MP1 removes their `mpCost`.

**Statuses** (ADR 0021; the power and turns come from the granting effect).

| Status | Rule (code) | Power means | Slice |
|---|---|---|---|
| Stun | The holder loses its next turn; the lost turn counts down the status | unused (0) | ST2 |
| Taunt | Enemy attacks go to a taunting party member (question 209) | unused (0) | BB1 |
| Guarded | Defense + power | defense added | PL2a |
| Exposed | Damage taken × (100 + power) % | percent added | RG4 |
| Poison | Loses power HP at the end of the holder's turn, ignoring defense; it can fell the holder | HP lost per turn | DR1 |
| Regen | Restores power HP at the end of the holder's turn | HP restored per turn | DR1 |

**Rest** (Phase 7; questions 219-222).

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Inn | `content/maps/start.json` | one placeholder NPC ("Innkeeper") with an Interact event: a line, then `rest` | RS2. |
| Rest item | `items.json` `camp_kit` | "Camp Kit", usable from the menu only, rests the active party, stack 9 | RS3. |
| What a rest does | Core `Party/PartyMember.Rest` | full HP, revives, refills lasting resources | Decision 230. |

**Screens** (1920 x 1080 virtual screen)

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Resource row | `Battle/PartyStatusView` (M3) | the MP row's style, 190 wide | One plain row per resource where MP was: name, amount over max, thin bar. |
| `PipsUpTo` | `Battle/ResourceGaugeView` (LK1b) | 10 | A resource whose max is at most this shows as pips (Octopath's BP); larger ones as a thin bar. |
| `PipSize`, `PipGap` | `Ui/PipRow` (LK1b) | 9, 4 | Diamond pips, lit or dim. |
| Resource colors | `classes.json` resource `color` (LK1a) | mana `#5B8CFF`, clarity `#B9C8FF`, stamina `#E8C25A`, ferocity `#E0533D`, conviction `#F2E6B0`, readiness `#9AD1C9`, instability `#A65BE0`, growth `#6BC46B`, decay `#7A5A8C`, faith `#FFF1C1`, chakra `#5FD3E0` | The pips' or bar's fill. |
| `StatusMarkSize` | `Battle/StatusMarksView` (LK2) | 22 | A status's small diamond mark with its turns left. |
| `SwapSlide` | `Battle/FormationSlide` (LK3) | 0.25 s | Two battlers trading places slide instead of jumping. |

## Design overview

### What exists, checked
- **The turn.** `Battle/BattleState.Execute(BattleCommand)` runs one turn and returns ordered events (ADR 0004): `ActionStarted`, `EffectApplied`, `CombatantDefeated`, `BattleEnded`. `EndTurn` checks defeat, then victory, then `TurnOrder.Advance()`. Nothing happens between turns. `Defend` does nothing.
- **Turn order.** `Battle/TurnOrder` is a cycle shuffled once at the start, skipping the fallen. There is no speed stat. Each combatant acts once per cycle.
- **Combatants.**
  - `Battle/Combatant` has `Stats` (`CombatStats(Attack, Defense)`, "fixed for the battle"), `Vitals` (HP and MP, shared with the member) and `Skills`.
  - Battle positions come from the order of `BattleState.Combatants`, which is fixed (`Game/Battle/BattleLayout.SpotsOf`, party in active order).
  - The member's class is private (`PartyMember._class`).
- **Skills and effects.**
  - `Skills/SkillDefinition`: `MpCost` and one `Effect(Kind, Power, Targeting)`. Skills come from `PartyMemberDefinition.SkillIds`; only Sarxian has any.
  - `EffectKind`: Damage, RestoreHp, RestoreMp (no content uses it), Revive.
  - `EffectResolver.Apply(effect, vitals, defense)` is shared by battle and field item use. `DamageFormula.Calculate(power, defense)` = max(1, power − defense).
  - `TargetRules.CanTarget(targeting, relation, vitals)` has no notion of position.
- **Enemy AI.** `Battle/EnemyTactics` attacks a random living party member, with one `NextBelow` per turn. It reads no stats.
- **MP everywhere.** About 185 mentions in 56 files:
  - Core: `Vitals` (Mp, MaxMp, `TrySpendMp`, `RestoreMp`, a 4-argument constructor), `ClassStats.MaxMp`, `ClassGrowth`, `EnemyDefinition.MaxMp`, `PartyMemberProgress.Mp`.
  - Data: `SaveGameRepair` clamps MP; the class and enemy validators check it.
  - Game: the battle panel's MP row (`PartyStatusPanel`, `VitalsText`), `FloatingNumbersView`'s RestoreMp color, the Status page, the sidebar card, the equipment sheet, the member card (`MemberVitalBars`), the leader profile's stats.
  - Content: `classes.json` (`maxMp` base and growth) and `enemies.json` (`maxMp`).
- **Saves.** `PartyMemberProgress(Id, Level, Experience, Hp, Mp, WeaponId, ArmorId)`, `SaveFile.CurrentFormatVersion = 1`. No migration exists yet: a newer version is rejected (`FileSaveStore.CheckFormatVersion`). Saves hold member ids, never class ids.
- **The class id `warrior`:** `classes.json`, Ragnir's `classId`, and test builders (`ClassBuilder` and `ContentCatalogBuilder` default to it) plus about 70 test lines. The barbarian's description says "A fierce warrior".
- **Editor.** It never edits skills, classes or items. It loads the catalog through Data, so new validation applies to it. Map event steps are authored in `Events/EventStepViewModel`.
- **Rest.** Nothing restores the party, and no item rests.

### Core changes
- **Resources** (new folder `Core/Resources`, ADR 0019, ADR 0024):
  - `ResourceDefinition`: one battle resource a class has, with its numbers, gains and optional rules.
  - `ResourceGains`: how much a resource gains on each trigger.
  - `ResourceChange`: what one skill does to one resource.
  - `ResourceRequirement`: a minimum of one resource a skill needs without spending it.
  - `ResourceMeters`: one combatant's current resource amounts in one battle, and which force of a pair dominates.
  - Formulas, each a pure static class:
    - `HeavyHitRule`: whether a hit is heavy;
    - `BackfireFormula`: a backfire's chance and damage;
    - `DominanceFormula`: a skill's power when its force dominates;
    - `ReadinessFormula`: charges from speed.
  - Rule records: `BackfireRule`, `DominanceRule`, `StrainRule`, each optional on a definition.
- **Statuses** (new folder `Core/Statuses`, ADR 0021):
  - `StatusKind`: the closed list of statuses.
  - `StatusGrant`: what an effect inflicts.
  - `ActiveStatus`: a status on a combatant now.
  - `StatusSet`: the statuses on one combatant.
  - `StatusModifiers`: what statuses do to stats and to damage taken.
- **Battle:**
  - `Combatant` gains `Resources`, `Statuses` and `CurrentStats`.
  - `Formation` (FM1): the party's line in battle and moving within it.
  - Internal helpers, each with one job:
    - `TurnUpkeep`: end-of-turn bookkeeping;
    - `ResourceTriggers`: the gains that battle happenings trigger;
    - `SkillPayment`: paying a skill's costs and checking its requirements;
    - `ResourceStarts`: a member's resource maxima and starting amounts for a battle (speed, lasting amounts, strain);
    - `LastingResourceCarry`: lasting amounts written back after a battle.
  - `TurnOrder` gains an extra turn placed after a combatant (RG2).
  - New commands: `MoveCommand` (FM1).
  - New events: `ResourceChanged`, `StatusApplied`, `StatusEnded`, `TurnSkipped`, `PlacesSwapped`, `ExtraTurnReadied`, `StatusTicked`, `Backfired`.
  - `EnemyTactics` attacks a taunting member when there is one.
- **Skills:** `SkillDefinition` loses `MpCost` and gains `ResourceChanges`, `Requirements` and `IsQuick`.
- **Effects:**
  - `EffectKind` loses `RestoreMp` and gains `GrantStatus`, `ReadyExtraTurn` and `Rest`;
  - `Effect` gains `Status` (`StatusGrant?`), `Hits` (default 1) and `Range` (for `AlliesInRange`);
  - `Targeting` gains `AlliesInRange` and `OtherCombatant`;
  - `DamageFormula` gains a damage-taken percent, defaulted to 100.
- **Stats:** `Vitals` loses MP (HP only).
- **Party:**
  - `ClassStats` loses `MaxMp` and gains `Speed`;
  - `ClassDefinition` gains `SkillIds` and `Resources`;
  - `PartyMemberDefinition` loses `SkillIds`;
  - `PartyMember` exposes `Class`, holds its lasting resource amounts and can `Rest()`;
  - `PartyMemberProgress` loses `Mp` and gains `LastingResources`.
- **Events (maps):** `RestStep`.

### Content format
`classes.json` (one class shown, after Phase 7):
```json
{
  "id": "mage", "name": "Mage", "description": "...",
  "base": { "maxHp": 150, "attack": 20, "defense": 4, "speed": 10 },
  "growth": { "maxHp": 12, "attack": 2, "defense": 1, "speed": 1 },
  "skillIds": ["fire_blast", "fireball", "meteor"],
  "resources": [
    { "id": "mana", "name": "Mana", "max": 10, "startsAt": 3, "gains": { "perTurn": 2 } },
    { "id": "clarity", "name": "Clarity", "max": 3, "startsAt": 3, "isLasting": true,
      "gains": { "perBattleFought": -1 }, "strain": { "lowersStartOf": "mana", "perMissingPoint": 1 } }
  ]
}
```
- Warlock: `"backfire": { "chancePerPointPercent": 5, "damagePercentOfMaxHp": 10 }` on `instability`.
- Druid: `"dominance": { "over": "decay", "bonusPercent": 50 }` on `growth`, and the mirror on `decay`.
- Paladin: `"gains": { "perHeavyHit": 2, "heavyHitPercent": 20, "perEnemyFallen": 3 }`.
- Rogue: `"maxFromSpeed": { "speedPerPoint": 10 }` on `readiness` instead of `max`/`startsAt`.
- LK1a adds `"color"`.

`skills.json` (two shown):
```json
{ "id": "shield_bash", "name": "Shield Bash", "description": "...", "iconFile": "",
  "resourceChanges": [ { "resourceId": "stamina", "amount": -3 } ],
  "effect": { "kind": "damage", "power": 15, "targeting": "singleEnemy",
              "status": { "kind": "stun", "power": 0, "turns": 1 } } },
{ "id": "aura_of_protection", "name": "Aura of Protection", "description": "...", "iconFile": "",
  "resourceChanges": [ { "resourceId": "conviction", "amount": -4 } ],
  "effect": { "kind": "grantStatus", "power": 0, "targeting": "alliesInRange", "range": 1,
              "status": { "kind": "guarded", "power": 5, "turns": 3 } } }
```
Optional fields may be left out when empty, false or 1 (`hits`). Nothing writes these files, so only reading matters.

**Validation (Data).** Each check names the class, skill or resource:
- A resource's id is unique in its class; max ≥ 1, unless the max comes from speed; 0 ≤ starts at ≤ max.
- Heavy-hit percent is 1..100.
- A dominance or a strain names another resource of the same class; a strain's source is lasting.
- A class's `skillIds` exist.
- Every skill a class lists changes or requires only that class's resources, and a requirement is at most that resource's max.
- `grantStatus` has a `status`; turns ≥ 1, power ≥ 0.
- `hits` ≥ 1. `range` ≥ 0, only for `alliesInRange`.
- A quick skill spends a resource.

Skills load before classes.

### Structure

Core folders:

| Folder | Depends on | Notes |
|---|---|---|
| Resources (new) | nothing | Definitions, meters, formulas, rule records. It knows nothing of combatants or members. |
| Statuses (new) | Stats | `StatusModifiers` maps `CombatStats`. |
| Effects | Stats, Statuses | `Effect.Status`. |
| Skills | Effects, Resources | `ResourceChanges`, `Requirements`. |
| Party | Resources, Skills, Stats, Items, Progression | `ClassDefinition.Resources`, lasting amounts on `PartyMember`. |
| Battle | Party, Skills, Effects, Statuses, Resources, Randomness | All runtime rules, including `Formation`. |
| Events | nothing new | `RestStep` is a record. |
| Exploration | Events, Party | Runs `RestStep` through `PartyMember.Rest()`. |

Checked edges: Resources and Statuses reference neither Battle, Party nor Skills, so there is no cycle. Party → Resources is new, and the reverse is not allowed: `ResourceMeters` takes definitions and starting amounts, never a member.

Data: `Resources/ResourceDefinitionValidator` (new), `Statuses/StatusGrantValidator` (new, called by `Effects/EffectValidator`), `Party/ClassReferences` (new). Saves get no migration before the first release (ADR 0022).

Game:

| Type | Folder | Depends on |
|---|---|---|
| `SkillCostText` (M2b) | Battle | Core Skills, Resources; `Ui/UiMessages` |
| `ResourceRowsText` (M3) | Battle | Core Battle, Resources |
| `StatusTagsText` (FT1) | Battle | Core Statuses; `Ui/UiMessages` |
| `ResourceGaugeView` (LK1b) | Battle | `Ui/PipRow`, `Ui/StatBar` |
| `PipRow` (LK1b) | Ui | `Ui/Diamond`, `UiTextures`; never Battle |
| `StatusMarksView` (LK2) | Battle | `Ui/Diamond`, `StatusTagsText` |
| `FormationSlide` (LK3) | Battle | Core Battle |

### When things happen in a turn (ADR 0019, ADR 0021)
`Execute(command)`:
1. `ActionStarted`.
2. The action.
   - A skill:
     - pays its costs, reporting `ResourceChanged` for each;
     - applies its effect to each target, once per hit, reporting `EffectApplied` (none for `GrantStatus`). Each hit's triggers run (hit taken, hit landed).
     - grants its status to a target still standing (`StatusApplied`, marked fresh);
     - on a fall: `CombatantDefeated`, the fallen's statuses cleared, and the fall's triggers (conviction, faith);
     - finally the backfire roll (`Backfired`).
   - Move: `PlacesSwapped`.
   - Ready: `ExtraTurnReadied`.
3. A quick skill keeps the turn while the battle goes on: no upkeep, no advance.
4. Otherwise, the upkeep of the actor's turn, skipped when the turn was an extra one:
   - status ticks (`StatusTicked`, maybe `CombatantDefeated`);
   - per-turn gains (`ResourceChanged`);
   - countdown (`StatusEnded`). A status marked fresh loses the mark instead of a turn.
5. Victory or defeat check (defeat first, as today).
6. Advance: a readied extra turn comes first when its anchor just acted. While the new actor is stunned: `TurnSkipped`, its upkeep, the check, advance again.

The Game's playback of each event (`ActionPlayback`, exhaustive switch):

| Event | Beat | Shows |
|---|---|---|
| `ResourceChanged` | none | The HUD reads the new amount at the next stage change. |
| `StatusApplied` | the impact | A cue with the status's name on the target. |
| `StatusEnded` | none | The tag goes at the next refresh. |
| `TurnSkipped` | `Skip` | A "Stunned" cue on the skipped combatant. |
| `PlacesSwapped` | `Banner` ("Move") | The battlers stand in their new spots at the next refresh (LK3 slides them). |
| `ExtraTurnReadied` | `Banner` ("Ready") | The turn-order strip shows the extra turn (it reads `UpcomingActors`). |
| `StatusTicked` | `Tick`, after the impact | A floating number on the holder. |
| `Backfired` | `Backfire`, after the impact | A floating number and a "Backfire" cue on the caster. |

### Key design decisions
1. **Mechanisms in code, numbers in content** (ADR 0019 A). Seven resources are a meter on the same four triggers. Backfire, dominance, readiness-from-speed and strain are switched on by optional fields; code never names a class.
2. **Upkeep at the end of the holder's own turn.** No events fall outside an `Execute`, so the constructor stays silent, and "3 turns" means three of the holder's turns. A status granted this turn isn't counted down this turn (the fresh mark), so a self-granted Taunt lasts its full 2 turns.
3. **Stun is skipped inside `Execute`**, not given a "pass" command. ADR 0004's one-call rule holds; the skipped turn's events come at the end of the previous list and play as a `Skip` beat.
4. **Formation is battle state** (ADR 0023). `BattleState` keeps the party's line, starting from the active order. `Combatants` stays the fixed roster, and positions are read from `Formation`. The party order on the map doesn't change (question 211).
5. **Ready inserts one extra turn into `TurnOrder`**, after the chosen combatant's next turn. The strip shows it with no Game rule, and the extra turn has no upkeep, so it can't double regeneration.
6. **Faith is a resource that skills require and nothing spends.** It reuses `ResourceRequirement` (CL1), which the warlock then shares (WL1).
7. **Sticky dominance lives in `ResourceMeters`.** After each change, a force strictly above its rival becomes dominant; a tie keeps the current one. `DominanceFormula` stays pure: power and a flag.
8. **Lasting resources** (ADR 0024) are on the member between battles. The combatant starts with the member's amount, and `GameSession.FinishBattle` writes the amount back and applies per-battle triggers. Saves store them by resource id.
9. **MP goes in four steps** (ADR 0022): skills, screens, saves, then members. Each step builds and passes alone, and the save format change lands before `Vitals` loses MP, so a save never refers to a field the model lacks.
10. **Resource colors in content** (LK1b). The other ways are a Game table keyed by content ids, or a palette by position that repeats colors.
11. **Text.** Resource names are content; status names and battle phrases are `ui.json` and `MessageIds` (ADR 0010).

### Extension points
- A new trigger is a `ResourceGains` field and one call in `ResourceTriggers`.
- A new status is a `StatusKind`, its rule in `StatusModifiers` or `TurnUpkeep`, and its name in `ui.json`.
- The other classes' lasting costs (question 220) are a lasting resource and a strain in content, plus a trigger only if a new one is needed.
- Speed-based turn order would change `TurnOrder` only.
- Enemy mechanics (question 224) would give `EnemyDefinition` resources and `EnemyTactics` skills.

### Replacement mapping

| Today | Becomes |
|---|---|
| class id `warrior` | `fighter` (FR1) |
| `PartyMemberDefinition.SkillIds` | `ClassDefinition.SkillIds` (CS1) |
| `BattleState.CanAfford` (MP) | `SkillPayment.CanPay`, behind the same public `CanAfford` (M2a; MP part removed in MP1) |
| `SkillDefinition.MpCost`, `Vitals.TrySpendMp`, `EffectKind.RestoreMp` | resource changes and requirements (MP1) |
| `BattleSkillChoice.IsSayingNotEnoughMp` | `IsSayingCannotAfford`, with the text from `SkillCostText` (M2b) |
| The battle panel's MP row | the resource rows (MP2 removes it, M3 adds them) |
| MP on the Status page, sidebar, equipment sheet, member card, leader profile | gone (MP2) |
| `PartyMemberProgress.Mp`, save format 1 | dropped in format 2, which refuses format 1 saves (MP3a) |
| `Vitals(hp, maxHp, mp, maxMp)`, `ClassStats.MaxMp`, `EnemyDefinition.MaxMp` | `Vitals(hp, maxHp)`; `ClassStats.Speed` (MP3b, RG1) |
| `Combatant.Stats` read by attacks | `Combatant.CurrentStats` (PL2a) |
| Battle positions from the `Combatants` order | positions from `BattleState.Formation` (FM2) |
| `EnemyTactics`' random target | random among taunters, else as today (BB1) |

## Risks
- **`BattleState` grows.** It is 264 lines. Upkeep, payment, triggers and the formation go to their own types, so that `Execute` stays a dispatcher. The reviewer checks CA1502 on `EndTurn` (ST2, RG2).
- **Seeded tests.**
  - With no taunter, `EnemyTactics` must make the same `IRandom` calls (BB1 pins it).
  - The backfire roll draws only when the caster's resource with a backfire rule is above 0 (WL2).
  - Sarxian, a warlock, is the Game tests' battler: after CS1 he has no skills until WL1.
- **Event order in one list.** Skipped turns, ticks and a readied turn arrive in the previous actor's list, so `ActionPlayback` splits beats by event kind (FT1, DR1 test this).
- **MP removal touches 56 files.** The four MP slices are ordered so each builds and passes alone. A slice that would leave a screen reading `Vitals.Mp` after MP3b fails the build, which is the guard.
- **Two save format changes** (MP3a, LR2). Each refuses the older format (ADR 0022) and gets a refusal test and a round trip; `SaveGameRepair` clamps the new lasting amounts.
- **Formation and layout.** `BattleLayout.SpotsOf` assumes the `Combatants` order. FM2 changes it to read the formation, and every Game test that checks spots is re-run.
- **Editor.** A map with a rest step can't open in an editor without it, so the placeholder inn lands in RS2 with the editor support.
- **Determinism of upkeep order.** Ticks, gains and countdowns run in a fixed order (statuses in the order they were applied).
- **Performance.** Nothing per frame in Core. The HUD formats resource and status text at stage changes only.
