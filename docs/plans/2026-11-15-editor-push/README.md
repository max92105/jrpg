# Editor push: the roadmap, and sub-plan 1, the editor's safety net
Status: approved

Drafted on 2026-11-15 from the developer's two documents ("Game Editor" and "Skill brainstorm", summarized in
[design.md](design.md), section 3) and his words after the battle turn presentation's hand check: "my issues are with
the editor, I need it to be way better than it is right now"; "push the editor a big step forward so I can actually
start implementing the game faster... the goal is to support the game development all in the editor in the end. The
only thing left after that is class edition and progression." He asked for his ideas to be challenged, not only
planned: the challenge is design.md, sections 3 to 5.

Approved on 2026-11-15 with the devil's advocate's review (the same error asked once, decision 690; sub-plan 1 in two
slices, NET2 optional, decision 691; the roadmap reordered, decision 701; new editors as modal windows until the mode
bar, decision 694; the status strengths stated, design.md section 5) and the developer's answers (decisions 692-702):
the editor stays open after an error, "yes always try to save progress"; the mode bar for screen space, late; each
status chooses start or end of turn; skill power with the weapon giving power; one target choice per skill; enemy
behavior from closed lists of triggers, skills, targets and conditions; an affix holds its values; the map background
is the battle background; "we need to do both before I start building again"; one strength per status. The sell price
stays open for the items plan (question 688).

This folder is a **program**: the roadmap of twelve sub-plans below, and the slices of the first one. Each later
sub-plan gets its own `/plan-feature` folder when its turn comes, starting from its roadmap line and the design.md
section it names. Decisions 675-681 are direction to confirm in those plans.

- Numbers: decisions 668-681 and 690-702, question 688 (open), ADR 0050. Questions 682-687 and 689 were answered.
- Sub-plan 1 is Editor only (`Hosting/`, one menu line in `Shell/`), plus the smoke script and two rule lines. No
  Core, Data, content or save change.

## Goal
1. **A roadmap** that gets the developer to "the whole game made in the editor", ordered so both the story side and
   the battle side are ready before he builds again (decision 701), each idea challenged against what already exists.
2. **Sub-plan 1, the safety net:** an exception in the editor no longer closes it and loses the draft. The editor
   writes a report, shows a box naming the error, the report and what is unsaved, and stays open; the same error again
   never asks twice, so a loop can't trap him; closing goes through the usual close question with its Save all.

## Roadmap

Rank, sub-plan, goal, and why it sits there. Sizes are rough slice counts for planning, not commitments.

| # | Side | Sub-plan | Goal | Why at this rank |
|---|---|---|---|---|
| 1 | both | **Editor safety net** (this folder, NET1; NET2 optional) | An unexpected error shows a box and keeps the editor open with its draft; each error leaves one report; the same error never asks twice. | Protects every hour spent in every later row; the last hand check lost a draft. No format change. |
| 2 | battle | **A skill's own effect strip** (about 2 slices) | Decision 678 alone: a skill or item names its effects strip, its pace, hit frame and swing from `effect-strips.json`; the first fire skill plays `art-src/gifs/FireExplosion.gif` imported through the existing GIF import (design 3.6). | The one battle change that needs no model change; unblocks the fire skill waiting since the battle presentation plan. |
| 3 | story | **Event window cleanup** (about 5 slices) | Presets and the 14 step kinds in grouped drop-downs, conditions folded under a summary, step list beside its fields, keys for add, remove and move (design 3.2). | Every quest, door, chest and NPC goes through it. No format change. |
| 4 | story | **Cinematic focus point** (about 3 slices) | A still zooms toward a point clicked on its picture, game and preview alike (design 3.10). | Small; the intros are being authored now. |
| 5 | story | **Character sprite cells and mirrors** (about 3 slices) | A field sprite facing as any cells of its sheet, or the mirror of another facing; the walk preview shows it (design 3.5, need 1). ADR amending 0043. | NPCs from bought sheets whose rows don't fit the fixed layout (`npc merchant A v00.png` reads row 2 for Up and Right). |
| 6 | battle | **Skills and statuses model** (about 9 slices) | `statuses.json` records with parts from a closed Core list, each acting at the start or end of its holder's turn; several statuses per effect with a chance; Slowed and Weakened (design 3.8, 5). ADR amending 0021. **First design question: one strength per status (decision 702) means today's per-skill strengths (Poison 5 and 8, Stun 60, 30 and 40...) become distinct records or a rebalance, status by status.** | The schema settles before forms are built on it, while skills are placeholders. |
| 7 | battle | **Skills and statuses editors** (about 9 slices) | Database windows for statuses and skills (cost as resource rows, effect, statuses, strip); the read-only side-by-side table of skills, classes and enemies (ADR 0045's first balancing tool); "Make strip from sheet cells" with the shared preview-and-pace control it first needs, then the GIF import's preview (design 3.4, 3.5 need 2). | Right after the model. Modal windows (decision 694). |
| 8 | battle | **Enemy behavior** (about 7 slices) | The tactics and phases form in the Enemies window; closed lists of triggers (battle-event reactions such as being critically hit included), skills, targets ("lowest HP enemy" included) and conditions, a proposed set for him to trim (decisions 679, 698; design 3.7). ADR amending 0034. | Needs skills editable (7) to pick from; makes complex fights content, not JSON. |
| 9 | battle | **Items, uniques and affixes** (about 8 slices) | Windows for base items (by kind) and affixes; uniques as item records with fixed affix values that drop by id and never reroll (decisions 681, 699; design 3.9). Question 688 (sell price) first. ADR amending 0029. | Loot and shops work today from JSON; fights come first, then what they drop. |
| 10 | battle | **Encounters and battle backgrounds** (about 6 slices) | An Encounters window (enemies, background, music); a map's battles naming their background; animated backgrounds as sheets the game loads (decision 700; design 3.3). | Needs enemies finished (8); the animation sheet shape is its ADR. |
| 11 | both | **Workspaces: the mode bar** (about 9 slices) | "Mode bar to use more of screen space": World and Database modes, the Database mode a non-modal page using the whole window; the six existing Database windows and the ones rows 6-10 add (about eleven) move in. ADR on the undo rule of a live page (design 3.1). | Asked for, not asked for first (decision 693); he may move it earlier. Every window it converts is built on one pattern, so the move is mechanical. |
| 12 | both | **Classes and progression** (to plan) | Class windows or pages (stats, growth, skills, resources), skills learned by level, the experience table (design 3.11). | The developer's own "last"; needs the skill model (6) and editors (7). |

## Order, and why (sub-plan 1)
1. **The ADR first** (0050): what the editor does with an unexpected error had real alternatives; the developer chose.
2. **NET1 is the whole net in one slice**: the policy with its repeat rule, the report, the box and the script that
   watches for it, so the script is never blind to the box (decision 691).
3. **NET2 only if he asks** after using the box: a dark custom window with Show report and Copy details.

## Guiding rules
- **Never lose the draft:** an exception after the main window has loaded is handled; the editor stays open with its
  maps, catalogs and undo history. Closing goes through `ConfirmClose`, so nothing closes without the close question
  while something is unsaved.
- **Never trap him:** the same error again (type and `TargetSite`) is counted, not asked about and not written again,
  so Save all stays reachable even when a render tick throws every frame (decision 690).
- **No emergency write of content the game would refuse** (decision 672): Save all stays the only way to disk.
- **The rule against throwing stays:** `.claude/rules/editor.md` still forbids a command that throws on typed input;
  the net is for the bugs that slip through, never a reason to throw.
- **Headless first:** the policy, the log and the box's text are tested without a window; `App` only wires them.
- **Sizes:** Editor slices about 200 lines, tests at about 10 lines each; NET1 is larger by the review's choice (about
  420 lines) and names its split point.
- **Files near 300** (checked 2026-11-15): `Shell/MainWindow.xaml` 292 (+1: the Help menu's test entry);
  `Shell/MainWindow.xaml.cs` 262 (+4: its click); `Shell/MainViewModel.cs` 285 (untouched: NET1 changes the `.Saving`
  partial, 191).
- **Test files, globbed on 2026-11-15:** no `tests/Jrpg.Editor.Tests/Hosting/` folder and no file named
  `*UnexpectedError*` in `src/` or `tests/`; NET1 creates `Hosting/UnexpectedErrorsTests`,
  `Hosting/UnexpectedErrorLogTests` and `Hosting/UnexpectedErrorQuestionTests`, and extends
  `Shell/MainViewModelSavingTests` (186 lines, about 210 after).

## Out of scope
- An autosave, or a recovery copy of an invalid draft (decision 672).
- Errors on threads other than the UI's: the editor has none; `AppDomain.UnhandledException` comes with the first one.
- Errors in the game (`Jrpg.Game` already names a broken content file at startup).
- Everything in roadmap rows 2 to 12: each is its own plan.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: the net
- [ ] ADR 0050, unexpected errors in the editor. Write it from decisions 669-674, 690 and 692 (the options were keep
  open, Save all then close, keep open with an autosave; the developer chose keep open).
- [ ] **NET1. An unexpected error keeps the editor open: the policy with the same-error rule, the report file, a box
  naming the error, the report and what is unsaved, and the smoke script watching for it** (Editor + script, about
  420 lines) → [slices/NET1.md](slices/NET1.md)
- [ ] **NET2 (optional, only if the developer asks after using the box). A custom error window: Keep working, Close
  editor..., Show report, Copy details** (Editor, about 200 lines) → [slices/NET2.md](slices/NET2.md)

## Decisions

Sub-plan 1:
- **668.** The editor push is a program: this folder holds the roadmap and sub-plan 1; each later row gets its own
  `/plan-feature` folder in the roadmap's order, which the developer may change.
- **669.** An exception nothing caught on the editor's UI thread, once the main window has loaded, is handled: it is
  written, the developer is asked, and the editor stays open with its draft.
- **670.** While the box is open, a different error is written and not asked about.
- **671.** Each unexpected error's report goes to a new file in `%LocalAppData%\Jrpg\Editor\Errors\` and to the error
  output.
- **672.** Closing from the box goes through the close question (Save all, Don't save, Cancel); there is no emergency
  write of a draft the game would refuse, and no autosave.
- **673.** An error before the main window has loaded is written and still ends the editor. Errors on other threads
  aren't handled until the editor has a thread.
- **674.** Help > Raise a test error, written in `MainWindow.xaml` and kept in every build, throws for the hand checks
  and the smoke script.
- **690.** The same error again (the same exception type and `TargetSite` as one already reported, asked about or
  answered) is counted, never asked about and never written as a new file; on exit its count is added to its report.
- **691.** Sub-plan 1 is two slices: NET1, the whole net with the box and the script; NET2, a custom window, only if the
  developer asks after using the box. Every sub-plan 1 type lives in `Hosting/`.
- **692.** (Question 682.) After an unexpected error the editor stays open so he can save: "yes always try to save
  progress."

For the later sub-plans (direction to confirm when each is planned; reasons in design.md):
- **675.** A skill keeps one instant effect; a list of instant effects waits for a skill that needs two.
- **676.** Statuses become content records in `statuses.json` (name, icon, description, when they act, parts from a
  closed Core list); an effect grants any number of them, each with turns and a landing chance in percent. The chance
  replaces Stun's "power as its chance": every grant has one, Intelligence scales it as it scales Stun's today, and
  crowd control resistance cuts it for the crowd-control parts only (amends ADR 0021; design 5).
- **677.** A status's turn effects are their own rule in `TurnUpkeep`, never an instant effect run again (the
  brainstorm's "Ongoing_Effect with Array[Instant_Effect]" is refused).
- **678.** A skill or item may name its own effects strip; its pace, hit frame and swing come from that strip's
  `effect-strips.json` record (ADR 0048); the attacker's strip stays the default.
- **679.** Enemy behavior stays data over closed lists (ADR 0034), extended by named Core rules, never a scripting
  language; an enemy's skills are the ones its tactics and phases name, not a second list.
- **680.** The game stays PNG-only: GIFs and multi-effect sheets become strips or sheets at import (ADR 0047 kept);
  only field sprites read a list of cells in place.
- **681.** A unique item is its own item record with fixed affixes, dropped by its id; saves keep it as any piece
  (base id, rarity, affixes), so no format change.

The developer's answers and the review, for the later sub-plans:
- **693.** (Question 683.) The mode bar comes, for its screen space: "mode bar to use more of screen space would be
  very good". It sits at rank 11; he may move it earlier.
- **694.** Until the mode bar, new editors (rows 6-10) are modal windows on the catalog-window pattern (ADR 0045); the
  mode bar row converts them with the six existing windows, its ADR and cost in its row.
- **695.** (Question 684.) Each status chooses whether it acts at the start or the end of its holder's turn: "I want
  to be able to choose."
- **696.** (Question 685.) Skill damage stays a power on the skill; the weapon contributes through the wielder's Power:
  "skill with power, weapon gives power". No multiple of the weapon's damage.
- **697.** (Question 686.) One target choice per skill: every effect and status lands on the chosen targets.
- **698.** (Question 687.) Enemy behavior is built from closed lists of common triggers, skills, targets and
  conditions, as in "when critically hit - skill X - lowest HP enemy": the triggers include reactions to battle events
  (being critically hit), the targets include "lowest HP enemy". The enemy behavior plan proposes each list from the
  genre's common cases for him to trim.
- **699.** (Question 688, the affix half.) "I think affix should hold their values": an affix carries its own values;
  on a unique they are fixed, never rerolled.
- **700.** (Question 689.) A map's "background" is the battle background its battles use.
- **701.** Both the story side and the battle side come before he builds again ("we need to do both before I start
  building again"): the roadmap's order above, with the animation work split so each part lands beside the content
  that needs it (sprite cells with the story rows; strips from sheets and the shared preview with the skills editors).
- **702.** One strength per status ("one per status"): Poison is Poison everywhere, its numbers on its record; today's
  per-skill strengths become distinct records or a rebalance, the model plan's first design question.

## Open design questions (developer decides)
- **688. The sell price** (for the items plan, row 9): (a) one sale share for every item, as `rules.json` `trade`
  sets today (ADR 0031); (b) the same, plus an optional per-item sale price. **Recommendation: (a)**; (b) is one
  optional field if a share feels wrong in play.
- For row 6: per status, distinct records or a rebalance (decision 702); asked at that plan's start.

## ADRs needed
- **0050, unexpected errors in the editor** (this sub-plan): written from the README's decisions, the developer having
  chosen.
- Later, each in its own sub-plan: a skill's own strip (amends 0048, row 2); field sprite cells and mirrors (amends
  0043, row 5); statuses as records with their timing and chance (amends 0021, row 6); enemy triggers and target rules
  (amends 0034, row 8); unique items with fixed affix values (amends 0029, row 9); animated battle backgrounds' sheet
  shape (row 10); the mode bar and the undo rule of a live page (row 11).
