# Editor push: design

The README holds the roadmap, the decisions and the open question. This file holds the reasoning: the challenge of
each idea in the developer's two documents against what Core, Data and the editor already do (sections 2 to 4), the
target data model for skills and effects (section 5), the design of sub-plan 1, the editor's safety net (sections 1
and 6), and the risks (section 7). Sections 3 to 5 are **direction to confirm** when each later sub-plan is planned
(decisions 675-681), updated with the developer's answers of 2026-11-15 (decisions 692-702).

## 1. Tunables and placeholders (sub-plan 1)

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Report folder | `Hosting/App` | `%LocalAppData%\Jrpg\Editor\Errors\` | Where each unexpected error's report goes (decision 671); the game's captures already live under `%LocalAppData%\Jrpg\`. |
| Report file name | `Hosting/UnexpectedErrorLog` | `editor-error-yyyy-MM-dd-HHmmss-fff.txt`, `-2`, `-3` on a clash | One file per error, sorted by time in Explorer. |
| Box title | `Hosting/UnexpectedErrorQuestion` | "Unexpected error" | The message box's caption; what `open-editor-windows.ps1` looks for (NET1). |
| Same error | `Hosting/UnexpectedErrors` | the exception's type and `TargetSite` (the method that threw) | What makes an error "the same again" (decision 690); the message is left out, since it often holds changing values. |
| Test entry | `Shell/MainWindow.xaml` | Help > "Raise a test error", in every build | Throws an `InvalidOperationException` so the net is checked by hand and by the smoke script (decision 674). |

## 2. Where the editor stands (2026-11-15)

- **One main window, map first.** The side panel's tabs (`Shell/EditingMode`: Tiles, Objects, Events) choose what a
  click on the map does. Everything else opens from the menu bar as a modal window: Database > Quests, Shops, Drop
  tables, Effect strips, Cinematics, Enemies; NPCs > Character sprites; Map > Settings. Each Database window's session
  is one undo step (ADR 0045), applied with OK.
- **Written by the editor:** maps, tiles, sheets, objects, field sprites, quests, shops, enemies (numbers,
  resistances, rewards, art; not tactics), drop tables, effect strips, cinematics.
- **Hand-written JSON only:** `skills.json`, `items.json`, `affixes.json`, `classes.json`, `party-members.json`,
  `encounters.json`, `rules.json`, `new-game.json`, and an enemy's `tactics` and `phases`.
- **No crash net.** `Hosting/App` wires no `DispatcherUnhandledException`: any throw in a window closes the editor and
  the unsaved draft is gone (backlog, "From the character sprites and shops plans"; the two rules in
  `.claude/rules/editor.md` that end "the editor has no unhandled-exception handler"). The last hand check found one.

## 3. The challenge, idea by idea

Each idea: what exists, what the idea adds, the verdict, and a simpler path where one exists.

### 3.1 "Multiple modes of edition"

- **Exists:** one map-centred window with three side tabs and ten modal windows behind menus.
- **Challenge:** "modes" can be three things of very different cost: (a) **workspaces**, a mode bar (World, Database,
  later Art) where Database is a non-modal page with a record-type list on the left and the editor on the right; (b) a
  launcher over the modal windows; (c) more menu entries. (a) breaks one invariant: today a window's session is one
  undo step and nothing else edits while it is open; a non-modal page lives beside the map editor's Ctrl+Z, so an undo
  can pull a record out from under the page's typed rows.
- **The developer's answer (decision 693):** the mode bar, for its screen space: "mode bar to use more of screen space
  would be very good". He did not ask for it first, so it sits at rank 11, after the content editors; he may move it
  earlier.
- **Consequence (decision 694):** the new editors (rows 6 to 10) are modal windows on the existing catalog-window
  pattern (ADR 0045); the workspaces row converts them all with the six existing ones (about eleven windows: mostly
  XAML roots, and OK/Cancel becoming Apply/Revert in a shared bar). Its ADR fixes the undo rule of a live page before
  any window moves: a page holds a session, Apply is one undo step, and a draft change under an open session (an undo,
  a nested apply) reloads the page, asking first only when it has typed changes.

### 3.2 "The event window is cluttered and hard to use"

- **Exists, measured:** `Events/ObjectEventsWindow.xaml` is 820 x 560. Its left column (300 px) stacks nine preset
  buttons and a quest box, wrapping to four rows, above the event list. Its right column always shows five condition
  rows (activation, prompt, object, leader, quest with kind and stage) and the once box, then the step list, then the
  chosen step's fields squeezed under it, then **fourteen** "Add" buttons wrapping to three rows.
- **Verdict:** agreed, and cheap because no format changes: presets into one "Preset" drop-down button; the conditions
  folded under a one-line summary ("Interact, once, while quest *find_ragnir* is active") that opens on click; one "Add
  step" drop-down grouped by purpose (Talk: dialogue, voice, cinematic; World: give item, swap object, warp, move NPC;
  Party: recruit, rest; Screens: forge, shop; Quests: start, advance, complete); the step list left and its fields
  right instead of stacked; keys for add, remove and move. The cell event window gets the same parts. Its plan starts
  by asking him which of these hurts most. `Events/EventStepViewModel.cs` (260) moves `Label` and `Problem` out first.

### 3.3 Map background setup, animated backgrounds from GIFs

- **Exists:** a battle background is per encounter (`encounters.json` `backgroundFile`, no editor). GIFs become strips
  at import for battlers and effects (ADR 0047); the game is PNG-only.
- **The developer's answer (decision 700):** "map background" is the battle background.
- **Challenge:** an animated full-screen background can't be a horizontal strip: frames are 1920 px wide and
  `TextureLimit.MaxSide` is 4096, so two frames at most. It needs frames in rows (a grid sheet), a lower-resolution
  strip scaled up, or a folder of frames: a format decision for its plan, with the game still PNG-only (decision 680).
- **Verdict:** an Encounters window (enemies, background, music), and a map's random battles naming their background
  there or in Map settings, planned in the encounters row.

### 3.4 "Always a preview, and be able to tweak frame rate"

- **Exists:** previews in the Effect strips window (pace, hit frame), the Enemies window's Art tab (idle loop), the
  Character sprites window (walk), the Animate panel (tiles). The GIF import dialog (`GifImport/GifImportWindow`)
  previews nothing: the GIF's pace is decided (most common delay, decision 632) before he sees it.
- **Verdict:** agreed: one shared preview control (frames, a pace box, play and pause, a frame line), built where it
  is first needed (decision 701): with "Make strip from sheet cells" in the skills editors row, where picked cells must
  play before a strip is written, then the GIF import showing the GIF playing with its pace editable before it writes.
  The older windows switch to it when next touched. Per-frame times (backlog, decision 632) only if he asks.

### 3.5 Flexible sheet sequences ("choose a sequence from the sheet")

- **Exists:** three animation shapes, all "N frames rightward from column 0": a tile (ADR 0039), a field sprite's walk
  (`Sprites/FacingCycle`: a row per facing, ADR 0043), an enemy's idle loop (ADR 0047). An effect strip is a whole
  file (ADR 0048). Evidence of the pain: `field-sprites.json`'s `npc merchant A v00.png` lists row 2 for both Up and
  Right; the record can't say "Right is Left mirrored" or "frames 0, 1, 2, 1".
- **Challenge:** "flexible everywhere" suggests one runtime frame-list format for everything. Two different needs hide
  in it. (1) **A sheet used in place** (a bought NPC sheet holding four facings): the record must name cells. (2) **A
  sheet holding several separate things** (six skill effects on one image): each effect is its own asset with its own
  pace record, and the game only needs a strip per effect.
- **Simpler path for (2):** an editor tool, "Make strip from sheet cells": pick cells in any order on the sheet, see
  them play, name the strip; it writes a PNG strip into `effects/` (or `battlers/`) and its pace record. The game, ADR
  0048 and every effect lookup stay as they are.
- **For (1):** a field sprite facing becomes a list of cells (any column, any row) plus "mirror of another facing";
  `FrameCount` rightward stays the default form so existing records load unchanged; read by the game's
  `FieldSpriteFrame` and the editor's `TileImages`. ADR amending 0043.
- **Placement (decision 701):** (1) is its own small row on the story side (NPCs); (2) goes with the skills editors.

### 3.6 Monsters: portrait, image, stats, skills, attack strip

- **Exists:** portrait, still or animated battle image (GIF import), stats, resistances, rewards and drop table are all
  in Database > Enemies. An enemy's skills are the ones its `tactics` and `phases` name (ADR 0034); its basic attack
  plays `attackEffectFile`.
- **Challenge:** a "skills list" separate from tactics would say the same thing twice and could disagree (a skill
  listed but never chosen). "The attack strip linked to the skills": every damaging skill plays the attacker's strip
  today (decision 657); the backlog holds the fix (decision 665): a skill names its own strip.
- **Verdict:** the enemy window shows the skills its tactics name; a skill gets an optional `effectFile` whose pace,
  hit frame and swing come from its `effect-strips.json` record (decision 678, roadmap row 2). "A skill as simple as a
  slash" is a skill with `effectFile: "Slash.png"` and a damage effect.

### 3.7 Monster behavior

- **Exists:** ADR 0034: weighted actions, each a skill or the attack, with a condition from a closed list (own HP
  below, ally HP below, ally fallen), and phases replacing the list below an HP share. **His first example already
  works**: the goblin chief's `field_dressing` has `allyHpBelow 50` and heals the lowest-HP ally the condition found
  (decision 401). Nothing edits it in the editor.
- **Challenge:** a rules language or behavior tree in data would be the most expensive thing on this list (an
  interpreter, a validator, an editor for nested logic, tests for every combination) and hard to debug in a turn-based
  battle.
- **The developer's answer (decision 698):** closed lists of common triggers, skills, targets and conditions, as in
  "when critically hit - skill X - lowest HP enemy". Two things this adds to ADR 0034: **triggers that react to battle
  events** (being critically hit, an ally falling, a status landing), not only state conditions read at the enemy's
  turn; and **a target per action** chosen from a list ("lowest HP enemy", random, the condition's subject, self, the
  last attacker). A reaction raises the question of when it acts (at once, out of turn, or on the enemy's next turn,
  as a priority): the enemy-behavior plan proposes a set of each list from the genre's common cases for him to trim.
  A boss trick no list expresses becomes a named rule in Core with a test.

### 3.8 The skill brainstorm against Core's model

| His sketch | Core today | Verdict |
|---|---|---|
| Skill `Cost` (a number) | `ResourceChanges` and `Requirements` on the class's own resources (ADR 0019); MP removed (ADR 0022) | **Keep Core's.** A single cost number would bring MP back. The editor shows the changes as a "Cost" line. |
| `TargetType` Enemy, Enemy_all, Self | `Effects/Targeting`: nine values | **Keep**: a superset; one target choice per skill (decision 697). |
| `Description` | `SkillDefinition.Description` | Exists. |
| Skill -> `Array[Effect]` | One `Effect`: kind, power, targeting, damage type, hits, one optional `StatusGrant` | **Change in part.** Every example fits *one instant effect plus statuses*: Cone of Cold is ice damage plus Slowed; Rest is a heal on self; Apply Poison is a status grant. What is missing is **several statuses**, not several instant effects (decision 675; extension point in section 5). |
| Instant_Effect "Fire Damage", "Heal" | `EffectKind.Damage` with `DamageType.Fire`; `RestoreHp` | Exists. |
| EffectType `Resource_Recovery` | A skill's `ResourceChanges` on its user only | Add a `restoreResource` kind only when a skill or item gives an ally a resource. |
| `DamageTier` | Nothing | Refused at first ("`power` is the tier"); **overturned by decision 704**: tiered effect records (fire_1 to fire_5) multiply the user's Power, per the developer's Damage Test sheet. |
| "modifier 2", `Power` | `power` scaled by the user's Power (`EffectScaling`) and dominance (`SkillEmpowerment`) | **Kept (decision 696)**: "skill with power, weapon gives power": the skill has a power, the weapon adds to the wielder's Power through its bonuses; no multiple of the weapon's damage. |
| Status_Effect `Regen_1`, `Poisoned` | `StatusKind`: Stun, Taunt, Guarded, Exposed, Poison, Regen; power and turns on the grant (ADR 0021) | **Change**: statuses become records with a name, icon and parts (decision 676), one strength each (decision 702). |
| `Trigger` Start_Of_Turn, End_Of_Turn | Ticks at the end of the holder's own turn | **Change (decision 695)**: "I want to be able to choose": each status says whether it acts at the start or the end of its holder's turn. |
| `Duration` | `StatusGrant.Turns` | Exists. |
| Component `lower_stats`, "Lower Damage" | Nothing lowers a stat; Guarded raises resistances, Exposed raises damage taken | **Add** a stat change part (Slowed, Weakened). |
| `deal_damage_component`, `heal_component`, `hp_modifier_component` | Poison and Regen's rules | Become parts: damage each turn (with a damage type, so Burning is fire and meets fire resistance) and heal each turn. |
| Ongoing_Effect with `Array[Instant_Effect]` | Poison and Regen tick in `TurnUpkeep` | **Refused (decision 677).** An instant effect needs an attacker: its Power, accuracy, crit and the hit roll. At tick time the granter may have fallen; running `EffectLanding` again would need a fake attacker and flags to skip the rolls. A tick is its own small rule. |
| Flow: Click Skill -> Targeting -> Confirm -> Execute | `CommandMenu` -> `BattleTargetCursor` -> `ActionWindUp` -> `BattleState.Execute` | Exists, end to end. |

`fireball` and `fire_blast` already exist in `skills.json` with his targets; Fire Blast already leaves a 2-turn burn.
What blocks a real fire skill isn't the model: a skill can't name its own strip (`art-src/gifs/FireExplosion.gif`
waits for it, roadmap row 2), and statuses can't be named (row 6).

### 3.9 Items, rarities, uniques, affixes, prices

- **Exists:** base items (`items.json`: consumable effect, or equipment kind, damage, damage type and stat bonuses;
  materials; price; stack). Rolled tiers Common, Magic, Rare with weights and affix counts (`rules.json` `loot`, ADR
  0030); the affix pool (`affixes.json`: stat, range, gear kinds, weight, gold value); a piece's price is its base
  plus its affixes' gold values; selling pays a share set once in `rules.json` `trade` (ADR 0031); the Forge rerolls.
  `Rarity.Unique` is reserved and never rolls (ADR 0029). No editor for any of it.
- **Uniques:** a unique is its own item record with fixed affixes, which saves exactly as a piece does today (base id,
  rarity, affix ids and values), so no format change (decision 681); it drops from a drop table by its id. The
  developer's answer (decision 699): "I think affix should hold their values": an affix carries its own values; on a
  unique they are fixed and never rerolled (the Forge refuses), and `GearRepair`'s clamp to the pool's range must not
  touch them.
- **Prices: still open (question 688).** One sale share for every item (ADR 0031) or a per-item sale price. Settled in
  the items plan.
- **Affixes:** the model exists; the editor is what is missing. One affix raising two stats would break the save's
  "affix id and one value" shape: out of scope until he asks.

### 3.10 Cinematics: a point to zoom toward

- **Exists:** `Cinematics/StillZoom` scales about the still's centre; the pan toward a point is backlog (decision 618).
- **Verdict:** small and safe: a focus point per still (a share of the picture's width and height), Core's zoom scaling
  about it for the game and the editor's preview alike, set by clicking the still's picture. No save change.

### 3.11 Classes and progression (last, as he said)

- **Exists:** `classes.json` (base and growth stats, skill ids, resources with rules: Will scaling, strain, lasting,
  dominance, backfire), a member's stats from class and level (ADR 0017), every class skill known from level 1 (ADR
  0020), `ExperienceTable`.
- **Why last:** a class names skills and resources; the skill and status model must settle first, and the resource
  rules are the most intricate form in the game. Progression (skills learned at a level, an XP curve editor) changes
  nothing saved: saves keep level and experience, skills are derived.

## 4. Simpler alternatives, collected

- Multi-effect sheets: strips cut in the editor instead of a frame-list format in the game (decision 680).
- Monster skills list: derived from tactics instead of a second list (decision 679).
- Behavior rules: closed lists and named Core rules instead of a rules language (decisions 679, 698).
- Uniques: an item record instead of a template plus a save bump (decision 681).
- Prices: one sale share instead of per-item sell prices (question 688, open).
- Animated backgrounds: a sheet like every other animation, never GIF in the game (decision 680).
- Workspaces: new editors as modal windows now, converted together later (decision 694).

## 5. Recommended target model for skills and effects (row 6, direction to confirm)

Statuses move to their own file and are named by id; a skill keeps one instant effect and one target choice; an effect
grants any number of statuses. Field names are a proposal the model plan confirms.

```json
// statuses.json (new): what a status does while held, from a closed list of parts (amends ADR 0021)
[
  { "id": "burning", "name": "Burning", "iconFile": "Burning.png", "isHarmful": true, "actsAt": "endOfTurn",
    "description": "Loses HP to fire at the end of each turn.",
    "parts": [ { "kind": "damageEachTurn", "damageType": "fire", "power": 5 } ] },
  { "id": "slowed", "name": "Slowed", "iconFile": "Slowed.png", "isHarmful": true,
    "parts": [ { "kind": "statChange", "stat": "speed", "percent": -30 } ] },
  { "id": "stunned", "name": "Stunned", "iconFile": "Stunned.png", "isHarmful": true,
    "parts": [ { "kind": "skipTurn" } ] }
]

// skills.json: the fields that change
{ "id": "cone_of_cold", "name": "Cone of Cold",
  "resourceChanges": [ { "resourceId": "mana", "amount": -4 } ],
  "effectFile": "ConeOfCold.png",
  "effect": { "kind": "damage", "damageType": "ice", "power": 20, "targeting": "singleEnemy",
              "statuses": [ { "statusId": "slowed", "turns": 2, "chance": 100 } ] } }
```

- **Status parts, closed in Core** (each a rule with a test, as ADR 0021's kinds are): `damageEachTurn` (type, power;
  Poison and Burning), `healEachTurn` (Regen), `statChange` (a stat by a percent; Slowed, Weakened), `skipTurn` (Stun),
  `drawAttacks` (Taunt), `resistanceChange` (Guarded), `damageTakenChange` (Exposed).
- **When it acts (decision 695):** `actsAt`, the start or the end of its holder's turn, for the parts that act each
  turn (damage, heal); the countdown keeps ADR 0021's rule unless the plan finds a reason to move it with them.
- **One strength per status (decision 702):** "one per status": Poison is Poison everywhere, its numbers on the
  record. **Today's content is not a mechanical conversion:** `skills.json` sets strengths per skill: Guarded 20, 12
  and 20; Exposed 50 and 20; Poison 5 and 8; Regen 6 and 10; Stun 60, 30 and 40, where Stun's power is its landing
  chance, scaled by the user's Intelligence (`GrantScaling`) and cut by the target's crowd control resistance. Each
  such pair becomes either distinct records ("poisoned" and "poisoned_strong") or one record with a rebalance: the
  model plan's **first design question**, asked per status.
- **The grant (decision 676):** a status id, turns and a landing **chance** in percent (100 when absent). The chance
  replaces Stun's "power as its chance": it is now a field of every grant (Apply Poison at 60 %), Intelligence keeps
  scaling it as it scales Stun's today, and the target's crowd control resistance keeps cutting it for the
  crowd-control parts (skip turn, draw attacks) only. Intelligence keeps scaling turns and the parts' numbers, never a
  skip turn's turns.
- **The set:** keyed by status id, not by kind, so Burning and Poisoned run together; the same id refreshes (ADR
  0021's "refresh, not stack").
- **The game:** a status mark's icon and name come from the record (today `Game/Battle/StatusNames` and
  `StatusMarksView` switch over `StatusKind`), the tick's number tone from its part.
- **Extension point:** if a skill ever needs two instant effects, `SkillDefinition.Effect` becomes `Effects` (a list,
  one targeting on the skill); every caller reads the first today (33 source uses, 111 test uses), so that is its own
  plan, not a side change.
- **No save change:** statuses end with the battle (ADR 0021), skills come from classes (ADR 0020); saves name neither.

## 6. Sub-plan 1 design: the editor's safety net

### What exists
- `Hosting/App.OnStartup` finds the content root and shows `Shell/MainWindow`; `App.xaml` sets
  `ShutdownMode="OnMainWindowClose"`. No handler: an exception nothing catches ends the process.
- `Shell/MainViewModel.Saving`: `ConfirmClose` asks the close question (`SaveChangesWindow`: Save all, Don't save,
  Cancel) through the private `UnsavedChangesQuestion()`, "Unsaved changes in map 'forest' and the tiles and objects.".
- `.claude/scripts/open-editor-windows.ps1` opens windows through their menus and calls a window a crash when the
  process exited; it prints the error output at the end.
- No editor code uses threads or tasks (grep `async`, `Task.Run`, `new Thread`: none), so the UI thread's dispatcher
  sees every exception there is. `AppDomain.UnhandledException` waits until a thread exists (decision 673).

### Structure (every type in `Hosting/`, decision 691)

| Type | Responsibility |
|---|---|
| `UnexpectedErrors` | Decides, for each exception nothing caught, whether it is written and whether the developer is asked. |
| `UnexpectedErrorLog` | Writes one exception's report to a new file and to the error output. |
| `UnexpectedErrorReport` | One unexpected error and the file its report went to, if any. |
| `UnexpectedErrorAnswer` | The developer's answer: keep working, or close the editor. |
| `UnexpectedErrorQuestion` | The message box's text: the error, its report and what is unsaved. |

Folder dependencies: `Hosting` -> `Shell` (already: `App` builds `MainWindow`; now it also reads
`MainViewModel.UnsavedChanges`). Nothing in `Shell` reads `Hosting`. The optional NET2 window lives in `Hosting` too,
so the edge never turns around.

### Key decisions (README, decisions 669-674, 690, 692)
1. `App` handles `DispatcherUnhandledException` (`Handled = true`) once the main window has loaded; before that (a
   startup throw), the error is written and the editor ends as today, since there is nothing to keep (decision 673).
2. **The same error again** (decision 690): an exception whose type and `TargetSite` match one already reported (being
   asked about, or answered) is counted, never asked about and never written as a new file. When the editor exits,
   each count is added to its error's report ("Came back 37 more times."). A render tick throwing every frame thus
   costs one question and one file, and Save all stays reachable. This widens the review's "already answered" to
   "already reported" on purpose: while the box is open the render tick keeps running in its message loop, and
   "answered" alone would write a file per frame until he answers.
3. A different error while a question is open is written and not asked about (decision 670).
4. No (the box's "close the editor") posts `MainWindow.Close()` to the dispatcher, so the handler returns first, and
   goes through `ConfirmClose`: the existing close question offers Save all, which refuses content the game would
   reject. There is no emergency write of an invalid draft (decision 672).
5. The report goes to the error output too (decision 671): the smoke script prints it under the entry that raised it.

### Extension points
- The custom error window (NET2), only if he asks after using the box: Show report, Copy details, the details folded.

### Replacement mapping
- `.claude/rules/editor.md`'s two "the editor has no unhandled-exception handler" endings are reworded (NET1): the rule
  stays (a command never throws on typed input), its reason becomes "the throw stops the command halfway and opens the
  unexpected-error box".
- The backlog line on `DispatcherUnhandledException` is removed (NET1).

## 7. Risks

### Program-level
- **Schema churn before content exists.** Skills, statuses and items are placeholders today ("Placeholder: the
  developer writes the real skill"). This is the cheapest moment to change their shape and the most expensive moment
  to build forms on them: an editor written on today's `StatusKind` would be rewritten. Hence model (row 6) before
  forms (row 7). Row 2 (a skill's own strip) is the one battle change before the model, because it touches neither.
- **Test churn of the status swap:** `StatusKind.` and `new StatusGrant(` appear 150 times in 37 files (Core battle
  tests, Game playback and sound tests). The model plan keeps the most-called shapes (a test helper building a record
  per old kind) and re-pins in a tests-only slice before the removal slice, as the stats plan did; decision 702 adds a
  content rebalance or new records to that plan.
- **Save format:** none of the recommended changes touch saves (statuses and skills unsaved; uniques as item records;
  an affix's values already saved). The ones that would (a unique template, a multi-stat affix) are refused for now.
- **A second format:** GIFs in the game, a runtime frame-list format for effects, a behavior script, or an editor
  project file beside `content/` would each be a second source of truth (ADR 0011). Every recommendation keeps one:
  content JSON plus PNGs.
- **Workspaces late:** converting about eleven windows in row 11 instead of six now; accepted (decision 694), since he
  asked for content tools first. Its ADR must fix the live page's undo rule before any window moves.
- **Battle reactions** (decision 698): an enemy acting on "critically hit" is the first behavior triggered by an
  event rather than read at the enemy's turn; it touches `TurnFlow` and `ActionPlayback` (at its complexity limit,
  backlog). The behavior plan sizes that before promising reactions.
- **Hot files:** `Shell/MainWindow.xaml` 292 lines (NET1 adds one menu line, 293; the backlog's "next control in its
  own user control" is about controls, and a menu item sits in the existing Help menu); `Shell/EditorTemplates.xaml`
  297; `Shell/MainViewModel.cs` 285; `Events/EventStepViewModel.cs` 260.

### Sub-plan 1
- **NET1's size:** the review set it as one slice so the script is never blind to the box; its estimate is about 420
  lines with tests, past the ~300 guide. If `/slice` must split it, the split is named in NET1's file (the box and the
  policy with the script first, the report file and the counts second): the script keys on the box's title, so it
  stays sighted in both halves.
- **State after Keep working:** the command that threw stopped halfway. Catalog and map edits are whole steps (a new
  catalog computed, then assigned; an `IMapEdit` applied whole), so the draft is consistent; a view may show stale
  marks. The box tells him to save and restart if something looks wrong.
- **An error inside a modal Database window:** the window stays open with its typed session; closing the editor from
  the box closes the main window, which closes the owned window and drops its unapplied session (the close question
  names only what reached the draft). The box's text says so.
- **The same method throwing two different ways** (an index error and a null in one method) counts as one error under
  the type-and-`TargetSite` rule only when the types match; two types are two errors, each asked once.
