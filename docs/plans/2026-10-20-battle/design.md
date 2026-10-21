# Battle: design

Part of [the battle plan](README.md): the tunables, the design overview and the risks. Slices point here for the details they build on.

### Tunables and placeholders
Legacy values unless said otherwise. "Where" is where the value lives, so it changes in one place.

| Name | Where | Placeholder | Meaning | Slice |
|---|---|---|---|---|
| encounter rate | `content/maps/forest.json` | 1 in 25 steps, 3 grace steps | Content (part 2), not tuned here. | - |
| battle start and end fade | `Flow/GameFlow` | 0.5 s out, 0.15 s black, 0.5 s in | The classic screen-change fade (decision 98). | B1b |
| battler size | `Battle/BattlerScaling` | painted art (source over 512 px): smoothly shrunk to 96 px. Pixel art: point-scaled by 1/2 (the goblin: 128 px), feet on the spot's bottom. | Decision 100. | B2b |
| formation | `Battle/BattleLayout` | rows 136 px apart, shifted 50 px per row; enemies from x 396, party from x 1424; bottom row 120 px above the screen's bottom | Legacy `BattleUI` layout constants. | B2a |
| vignette, top shade | `Ui/UiTheme` | radial `#00000000` at 0.45 to `#B3000000`; top 320 px `#99000000` to clear | Legacy `Brush.BattleVignette`, `BattleTopShade`. | B2a |
| party status | `Battle/PartyStatusView` | 400 px wide at the top right, margins 24 and 40 px. HP 30 px, MP 24 px, max values 17 and 15 px. Bars 3 px; the HP bar is 250 px, the MP bar 190 px. 64 px diamond portrait. The acting member's panel slides 36 px left. A fallen member's panel shows at 55 % with HP in red. | Legacy `CharacterPanel`. | B3a |
| enemy tag | `Battle/EnemyTagView` | 160 px wide, name 16 px, 4 px red bar, 38 px under the sprite; letters A, B, C... when several enemies share a name | Legacy `EnemyTag`; decision 99. | B3b |
| battle bar colors | `Ui/UiTheme` | HP `#3E7F5A` to `#7FD39A`, MP `#3E5C9A` to `#8FB4F0`, enemy HP `#8A2A2A` to `#E0524A`, track `#80000000`, EXP `#E3C17E` | Legacy `BattleStyles.xaml`. | B3a, B7b1 |
| turn order | `Battle/TurnOrderView` | current 140 px diamond, then the next 8 of the repeating cycle at 66 px, touching (-6 px), with a "NEXT" label and a 760 px line. Enemies have a red lower half. Non-targets dim to 35 % while targeting. | Legacy `TurnOrderBar`; decision 104. | B3c, B4c |
| command panel | `Battle/CommandPanelView` | 360 px wide; header tab 300 x 32; rows 46 px, icons 24 px, text 20 px, hint 16 px. 40 px left of the actor; top at least 260 and at most 1080 - 420. | Legacy `CommandPanel`. | B4c |
| target pointer | `Battle/PointerBob` | 44 px, bobs 8 px over 0.45 s each way (sine); code-drawn if `TargetArrow.png` isn't transparent | Decision 56. | B4c |
| step | `Battle/BattlePacing` | 0.2 s forward, 0.2 s back, 50 px | Legacy `StepDuration`. | B5a |
| beats | `Battle/BattlePacing` | an impact beat holds 1.0 s; a defeat in it extends the hold to 2.0 s; Defend and Run have no hold | Legacy delays, made event-driven (ADR 0013). | B5b |
| banner | `Battle/BattleBanner` | 0.15 s in, 1.2 s hold, 0.3 s out, 190 px from the top | Legacy `BattleBanner`. | B5b |
| floating number | `Battle/FloatingNumber` | pops from 60 % to 100 % in 0.18 s, rises 50 px over 1.6 s, fades over the second half; white damage, green HP and revive, blue MP | Legacy `FloatingText`. | B5c |
| defeated enemy | `Battle/ActionPlayback` | white tint for 1.0 s, then a 1.0 s fade | Legacy `AnimateDeath` (no shader). | B5c |
| effect strip | `Battle/EffectStrip` | 0.08 s per frame, 96 px, 30 px below the target's top; damage only | Decisions 57, 101. | B5d |
| placeholder cue | `Battle/PlaceholderCue` | the action's name over each target: 0.15 s in, 0.3 s shown, 0.15 s out | Decision 101; question 106. | B5d |
| victory | `Battle/VictoryScreen` | the title slides 60 px in 0.45 s; the reward bar comes after 0.25 s; the first card after 0.45 s, then 0.1 s apart; a 0.5 s pause. A full bar fills in 1.1 s, with 0.2 s minimum per level. "Level Up!" pops to 1.5x over 0.35 s. | Legacy `VictoryScreen`, `ExpGainRow`. | B7a, B7b1, B7b2 |
| victory layout | `Battle/VictoryScreenView` | "Victory!" 96 px at (104, 112), line at y 226; one reward bar per reward, 800 x 64 from (88, 268); cards in 2 columns, 880 px wide at (104, 420); 100 px footer | Legacy `VictoryScreen.xaml`. | B7b1 |
| escaped | `Battle/EscapedNotice` | "Escaped" at 64 px on the banner for 1.5 s, or until Confirm | Legacy `EscapedScreen`. | B8a |
| defeat | `Battle/DefeatScreen` | red "Defeat" fades in over 0.6 s; choices appear after 0.7 s | Legacy `DefeatScreen`; decision 65. | B8b |


## Design overview

### What exists, checked
- **Core (part 1).**
  - `Battle/BattleState`:
    - one `Execute(BattleCommand)` per turn, returning ordered `BattleEvent`s (ADR 0004): `ActionStarted`, `EffectApplied` (amount actually applied), `CombatantDefeated`, `BattleEnded`;
    - `Phase`, `CurrentActor`, `Combatants` (party in active order, then enemies in encounter order);
    - `LivingOn`, `CanUse(skill)`, `CanUse(item)`;
    - `GrantVictoryExperience`, which returns `ExperienceGain`s with `LevelProgressStep`s.
  - The constructor throws when a side has no living combatant.
  - Commands are closed records (`private protected` constructor).
    - Defend does nothing.
    - Flee always succeeds.
  - `TurnOrder` (internal): a shuffled cycle that skips the fallen. It has no "upcoming" query.
  - `EnemyTactics` (internal): attacks a random living member.
  - `BattleRewards` (internal): an even split, remainder dropped (decision 13).
  - Targets: there is no battle `TargetChoiceFor`, and `CanUse(item)` doesn't check that a legal target exists.
  - `Combatant` shares a member's `Vitals` and exposes `Member` or `Enemy`.
- **Core (part 2).**
  - `ExplorationState`:
    - rolls encounters per step and sets `PendingEncounter`;
    - refuses `Walk`/`TryInteract` while one is pending;
    - has `FinishEncounter()`, which resets the grace steps.
  - `GameSession.Capture` refuses while an encounter is pending. `GameSession` keeps neither the catalog nor its `IRandom`.
  - `PartyState.TryMoveToReserve` allows leaving only fallen members active, so starting a battle then throws (known crash path until C4).
- **Data.**
  - `ArtReferenceCheck` covers tilesheets, the field sprite, portraits and item icons, through `File.Exists`, which ignores case on Windows.
  - It covers no battle art, and `ArtFolders` has no `Battlers` or `Effects`.
  - Skills have no `description`.
- **Content.**
  - The forest table rolls 1 in 25 steps, with 3 grace steps, for `goblin_trio` (background `Forest.png`).
  - The new game's active party is Sarxian alone, who has all five skills.
  - Battlers:
    - party battlers are 1024 px paintings;
    - `Goblin.png` is about 256 px pixel art.
  - `goblin`'s `portraitFile` is `goblin.png`, but the file is `Goblin.png`.
  - Command icons exist; `TargetArrow.png` may have an opaque grey background.
  - Three skills have an empty `iconFile`.
  - The GIF effects are in `content/art/effects-source/`.
- **Game.**
  - `GameFlow` plays `EncounterFreeContent.Of(catalog)`.
  - A request returned while the flow's fade plays would throw (`ScreenFade.Start`; T1a follow-up).
  - `SceneStack` draws every scene bottom to top and updates the top one.
  - `MenuTransitions` pushes and pops.
  - Reusable pieces:
    - `Ui/StatBar`, `Diamond`, `UiTextures`;
    - `ListCursor`, `CoverFit`, `GradientStrip`, `SquareShrink`;
    - `Rendering/CanopyMask.Build`, `Scenes/ScreenFade`;
    - `Saving/SaveFilesPanel.ChooseToLoad`, `SaveSlots.MostRecent`;
    - the blurred-text effect.
- **Legacy** (`legacy/JRPG/UI/Battle/**`, `Managers/SceneManager.cs`, `TriggerHandler.cs`).
  - `BattleUI` callbacks: step forward, then execute; effects and a 1 or 2 s hold; step back, then advance.
  - Enemy targets come from `new Random()`.
  - End of battle: victory shares the experience; "Escaped" shows for 1.5 s; defeat offers Load a save or Return to title.
  - The encounter is three hardcoded goblins.

### Core changes
| Member | Why | Slice |
|---|---|---|
| `Battle/VictoryRewards` (public record); `GrantVictoryExperience` returns it | The victory screen shows the total experience; gold and drops become new members later (decision 103). | C1 |
| `GameSession`: keeps the catalog and the `IRandom`; `EnemyTactics`, `ActiveBattle`, `StartPendingBattle()`, `FinishBattle()` | One place starts and ends battles. | C1 |
| `EnemyTactics` public | The Game asks it for enemy commands (ADR 0004). | C1 |
| `TurnOrder.Upcoming(int count)`; `BattleState.UpcomingActors(int count)` | The turn-order bar. | B3c |
| `BattleState.TargetChoiceForAttack()`; private `ChoiceFor(Targeting)` shared with `ThrowIfIllegalTargets` | The pointer only offers legal targets, and the offer and the check can't drift. | C2 |
| `SkillDefinition.Description` (required) | Skill hints come from content, like items (decision 102). | C3 |
| `BattleState.TargetChoiceFor(SkillDefinition)`, `CanAfford(SkillDefinition)`; `CanUse(skill)` also needs a legal target | Skills in battle. | B6a |
| `BattleState.TargetChoiceFor(ItemDefinition)`; `CanUse(item)` also needs a legal target | Items in battle (a revive with nobody fallen is greyed). | B6b |
| `BattleState` with nobody standing in the party starts as `Defeat` | A battle that starts with the whole party down is lost at once (decision 97). | C4 |

**Core folder edges.** `Session` -> `Battle` becomes direct; it was transitive through `Exploration`. Nothing in `Battle` points to `Session`.

### Game structure added by this plan
```
Jrpg.Game/
  Scenes/   BattleRequested, BattleFinished (B1b)
  Flow/     BattleTransitions (B1b); EncounterFreeContent deleted (B1b)
  Battle/   BattleTurnSequence, BattleScene (B1b); BattleLayout (B2a); BattlerArt, BattlerScaling (B2b);
            VitalsText, PartyStatusView (B3a); EnemyLabels, EnemyTagView (B3b); TurnOrderStrip, TurnOrderView (B3c);
            CommandEntry, CommandMenu, BattleTargetCursor (B4a); CommandListView (B4b);
            CommandPanelView, CommandPanelPlacement, CommandHeaderShape, PointerBob (B4c);
            TurnStage, BattlePacing (B5a); ActionPlayback, PlaybackBeat, BattleBanner (B5b); FloatingNumber (B5c);
            EffectStrip, PlaceholderCue (B5d); ExperienceBarAnimation, VictoryScreen (B7a);
            VictoryScreenView, PointedBarShape (B7b1); VictoryEntrance (B7b2); EscapedNotice (B8a); DefeatEntry, DefeatMenu, DefeatScreen (B8b)
  Ui/       Vignette; UiTextures.BattleVignette, TopShade (B2a); UiTextures.Battler (B2b); UiTheme battle tokens (B3a)
```
No type is named after its folder. `Jrpg.Game.Battle` sits beside `Jrpg.Core.Battle`, as `Exploration` already does.

**Folder dependencies, checked edge by edge (acyclic):**
| Folder | Depends on |
|---|---|
| `Hosting`, `Input`, `Rendering` | Core, Data only |
| `Ui` | Rendering, Input |
| `Scenes` | Input, Rendering |
| `Dialogue` | Ui, Rendering, Input |
| `Saving` | Hosting, Ui, Rendering, Input |
| `Exploration` | Scenes, Dialogue, Hosting, Ui, Rendering, Input |
| `Menus` | Scenes, Saving, Hosting, Ui, Rendering, Input |
| `Title` | Scenes, Saving, Ui, Rendering, Input |
| **`Battle`** (new) | Scenes, **Saving** (defeat's save list, B8b), Ui, Rendering, Input. Not Menus, Exploration or Title. |
| `Flow` | all of the above, now with Battle; only `JrpgGame` and `Program` depend on `Flow` |

### Key design decisions
1. **The battle stacks over the map (ADR 0013).** `Flow/BattleTransitions` works like `MenuTransitions`:
   - `Start` calls `session.StartPendingBattle()` and pushes `factory.CreateBattle(session)`;
   - `Finish` pops it.
   Both happen behind the flow's fade (decision 98). The map scene stays underneath, untouched. It resumes on its own once `PendingEncounter` is null (`FinishBattle` cleared it), so there is no rebuild, and the camera and animation are as left.
2. **Requests are returned once, by every scene; the flow waits for its fade.**
   - A scene returns a given request on one update, then null until its state changes. The map's `BattleRequested` comes only on the update a step starts the encounter. The battle's outcome request comes once.
   - `GameFlow` holds a request returned while its fade isn't idle in one slot, and starts it when the fade ends (fixes the T1a follow-up). A second waiting request throws, since no scene sends two.
3. **The turn sequence (ADR 0013).** `Battle/BattleTurnSequence` (pure) runs each turn:
   1. step the acting combatant forward;
   2. on a party turn, wait for the command menu; on an enemy turn, ask `session.EnemyTactics`;
   3. `Execute` the command at the moment of impact;
   4. play the returned events as beats;
   5. step back;
   6. move to the next turn, or end.
   - **The HUD reads live Core values** (option A): `Vitals`, the inventory, `UpcomingActors`.
   - **Refresh points:** the turn order refreshes when StepForward starts, and the vitals text when Holding starts. Text is never formatted per frame.
   - **The on-screen actor is `BattleTurnSequence.Actor`**, never `battle.CurrentActor`, which moves on inside `Execute`.
4. **Playback is event-driven beats, exhaustive over `BattleEvent`.** `ActionPlayback` turns one turn's events into a sequence of beats, and the hold lasts as long as its beats:
   - a banner beat (skill or item name, "{name} defends.");
   - an impact beat for the effects landing together (numbers, effect strip or placeholder cue, a 1.0 s hold);
   - a defeat beat (the hold extends to 2.0 s).
   Defend and Run produce no hold. A future between-turn event (poison) gets its own beat. Any event type it doesn't know throws `InvalidOperationException` naming it, so a new mechanic can't be shown silently wrong.
5. **The end of a battle is the scene's.** When the outcome's beats end, the scene calls `session.FinishBattle()` once (experience on a victory; the encounter cleared). It then shows the matching screen over the frozen battle:
   - **Victory:** `VictoryScreen`, then `BattleFinished`.
   - **Escape:** `EscapedNotice`, then `BattleFinished`.
   - **Defeat:** `DefeatScreen`, then `LoadRequested` or `TitleRequested`. `SessionTransitions` ends the session and replaces every scene, the map included (decision 31).
6. **Art from content, prepared once, named exactly.**
   - `Battle/BattlerArt.Of(Combatant)` is the one place that maps a combatant to its files.
   - `Battle/BattlerScaling` decides how a battler image reaches the screen: painted art is shrunk smoothly once (`UiTextures.Battler`), pixel art is point-scaled by a whole factor.
   - Battle art checks compare file names exactly (ordinal, case included).
7. **One place per tunable.** Layout in `BattleLayout`, timing in `BattlePacing` and the timing types, colors in `UiTheme`, words in `ui.json` (ADR 0010). Content names come from content. Enemy letters ("Goblin A") are display only, in `EnemyLabels`.
8. **Input in battle.**
   - Up/Down move in lists.
   - Up/Down/Left/Right move between targets in formation order (decision 56).
   - Confirm chooses, and Cancel goes back one level.
   - Menu does nothing in battle.
   - Confirm skips the victory animation, then continues.

### Extension points
**Left open by this plan.** Each is a single place to change:
- **New commands:** a `BattleCommand` record and its `Execute` case, plus a `CommandEntry` row.
- **New effects and status effects:** `EffectKind`, `EffectResolver`, and a new `BattleEvent` when the mechanic needs one (ADR 0004). `ActionPlayback` gives it a beat, and its exhaustive switch shows where.
- **Enemy behavior:** `EnemyTactics` is the one place enemies decide. A second behavior is the moment for per-enemy tactics in content.
- **Turn order:** `TurnOrder` stays internal behind `UpcomingActors`.
- **Targeting:** `TargetChoiceFor*`; the pointer only walks `Candidates`.
- **Battle types:** a battle is "start the pending encounter" (event battles: decision 30).
- **Rewards:** `VictoryRewards` gains gold and drops; the victory view draws one bar per reward row.
- **HUD source:** moving from live values to event-derived values (ADR 0013 option B) touches only `VitalsText` and `ActionPlayback`.
- **Look:** presenters are separate from views.

**Deliberately not built** (no current user): an `IEnemyTactics` interface, a timeline engine (ADR 0013 option C), per-skill animation fields, status/speed/row stats, a battle-speed setting, gold or drops, and a shared generic target cursor.

### Legacy mapping
| Legacy | New | Slice |
|---|---|---|
| `GameManager.TryRandomEncounter` | Core `RandomEncounters` (part 2); `ExplorationScene` returns `BattleRequested` | B1b |
| `TriggerHandler` `TriggerBattle` | `encounters.json`; event battles are decision 30 | - |
| `SceneManager.StartBattleAsync`, `EndBattleAsync`, `BattleExit` | `Flow/BattleTransitions` (push/pop behind the fade); `BattleFinished`, `LoadRequested`, `TitleRequested` | B1b, B8b |
| `BattleManager.ShareExperience`, `TotalExpReward` | `GameSession.FinishBattle` -> `VictoryRewards` | C1 |
| `BattleUI` turn flow | `Battle/BattleScene` + `BattleTurnSequence` | B1b, B5a |
| `BattleUI` layout, `DrawSprites`, `CreateSprite` | `Battle/BattleLayout`, `BattlerArt`, `BattlerScaling`, `UiTextures.Battler` | B2a, B2b |
| `Brush.BattleVignette`, `BattleTopShade` | `Ui/Vignette`, `UiTextures.BattleVignette`, `TopShade` | B2a |
| `CharacterPanel` | `Battle/PartyStatusView`, `VitalsText` | B3a |
| `EnemyTag` | `Battle/EnemyTagView`, `EnemyLabels` | B3b |
| `TurnOrderBar`, `TurnPortrait`, `TurnQueue` | `Battle/TurnOrderStrip`, `TurnOrderView`, Core `UpcomingActors` | B3c |
| `CommandPanel` | `Battle/CommandMenu`, `CommandListView`, `CommandPanelView` | B4a-B4c, B6a, B6b |
| `CommandPanel.DescribeSkill` | `skills.json` `description` (decision 102) | C3 |
| `EnterTargetingMode`, `AddTargets` | Core `TargetChoiceFor*` + `Battle/BattleTargetCursor` | C2, B4a |
| `ShowTargetPointer`, `ApplyPulsingGlow` | pointer with `PointerBob`; the glow becomes the turn order's highlight | B4c |
| `AnimateStepForward`, `AnimateStepBack`, delays | `BattleTurnSequence` stages, `BattlePacing`, `ActionPlayback` beats | B5a, B5b |
| `BattleBanner`, `FloatingText`, `AnimateDeath` | `Battle/BattleBanner`, `FloatingNumber`, `ActionPlayback` | B5b, B5c |
| `PlayVFX` (GIF) | `Battle/EffectStrip` (damage only), `PlaceholderCue` | B5d |
| `VictoryScreen`, `ExpGainRow` | `Battle/VictoryScreen`, `ExperienceBarAnimation`, `VictoryScreenView` | B7a, B7b1, B7b2 |
| `ShowEscaped` | `Battle/EscapedNotice` | B8a |
| `DefeatScreen` | `Battle/DefeatMenu`, `DefeatScreen` over `Saving/SaveFilesPanel` | B8b |


## Risks
- **Many slices (25).** Each ends playable; from B1b on, the forest always has battles that can at least be run from.
- **The map drawn under the battle.** Stacking means `SceneStack` draws the map every frame behind a full-screen battle. It's cheap at this map size and keeps the map untouched (ADR 0013). If profiling ever shows a cost, the stack can skip scenes under an opaque one.
- **`BattleScene` growth.** The scene only routes; the turn sequence, menu, playback, HUD and end screens keep their own state. If CA1502 or the review flags it, extract a `Battle/BattleEnding` at that slice's review.
- **Determinism.**
  - One `IRandom` per session feeds encounters, turn order and tactics, so the draw order matters.
  - The flow seeds from the clock, so the headless flow test (B1b) is built to be seed-independent.
  - Battle tests build seeded sessions and use test catalogs where the outcome matters, not repository balance (B-2).
- **Per-frame allocation.** Playback beats are built once per turn, and floating numbers use a fixed pool. `UpcomingActors` and `LivingOn` are called only at stage changes. Texts are formatted on change. Big art is shrunk once.
- **Changed tests:**
  - `EncounterFreeContentTests` deleted (B1b);
  - the reward tests read `VictoryRewards.Gains` (C1);
  - the B1b flow test presses Up to reach Run (B4b);
  - B1b's sequence tests gain deltas (B5a);
  - the old "both sides need a living combatant" test splits: no living enemy still throws, nobody standing becomes a defeat (C4).
  Each is a requested behavior change named in its slice; no assertion is weakened.
- **Known crash path until C4:** the Party page can leave only fallen members active, and the next battle then throws.
- **Battles in existing tests.** Tests walking in the forest could now roll a battle; current ones only warp or check fades. New ones set the table explicitly.
- **Art:**
  - the effect strips are a manual conversion (B5d);
  - `goblin.png` is caught by B3c's exact-name check and fixed in content there;
  - painted portraits at 96 px may read poorly (decision 100, follow-up plan).
- **Content format:** `skills.json` gains a required `description` (C3). Content is the only source (ADR 0011), so every file is updated in the same slice.
- **Save format:** no change. Battles are never saved: `Capture` refuses while an encounter is pending.
- **Look without shaders:** blurred-text outlines, gradients and dimming stand in for blur, glow and drop shadows.

