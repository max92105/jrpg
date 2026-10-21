# Game: battles
Status: approved, in progress

Second plan of part 3b (game parity). It takes from the part 3b notes at the end of `2026-10-18-shell-cutover-and-map-editing.md`:
- the battle seams in Core (note 4a);
- the battle screen, playback, skills and items, victory and escape (4b-4e);
- the defeat flow (6d).

Names, tunables and question numbers are reused from there where still valid (30, 52, 56, 57, 65).

Revised after a devil's-advocate review and the developer's answers (2026-10-20).

Developer's request (2026-10-20): "the last big gap with what i had in the legacy project and this one is the battle obviously. i need you to plan the clean implementation of the battle system. bring it to where we where before but ready to grow and change."

## Goal
At the end of this plan, walking in the forest starts random battles again, as legacy did. They play like legacy's, in the reference look (`docs/ui_ux_exemples/`, Octopath Traveler II):
1. **Into the battle.** The screen fades to the encounter's background, over the map. The enemies stand on the left ("Goblin A/B/C"), the active party on the right.
2. **The HUD.**
   - The turn order sits at the top left: who acts now in a big diamond, then who acts next.
   - Each party member's name, HP and MP sit at the top right.
   - Each enemy has its name and HP bar under it.
3. **Commands.** On a party member's turn, a command panel opens beside them: Attack, Skills, Items, Defend, Run.
   - Targets are chosen with a pointer.
   - Skills and items only offer what Core allows; the rest are greyed.
4. **Playback.** Each action plays back as event-driven beats:
   - the actor steps forward;
   - damage plays the attacker's effect strip;
   - other effects show a placeholder cue;
   - floating numbers pop and the HUD updates;
   - defeated enemies fade out;
   - the actor steps back.
   Enemy turns play the same way.
5. **Victory.** The victory screen shows the experience earned, then each member's bar filling level by level, with "Level Up!".
6. **Escape.** Run shows "Escaped" and returns to the map.
7. **Defeat.** The defeat screen offers:
   - "Load last save" (selected first);
   - "Load" (the save list);
   - "Main menu".
   A battle that starts with nobody standing is lost at once.

The design leaves seams in Core (commands, events, targeting, tactics, turn order, rewards) and in the Game (pure presenters, one place per tunable). New skills, effects, enemy behaviors, battle types, rewards and UI changes extend those seams instead of rewriting them. `Flow/EncounterFreeContent` is deleted.

## Order, and why
1. **ADR 0013, then the session's battle API (C1).** C1 comes right before B1, its first user.
2. **The flow handles a request during its fade (B1a), then random battles end to end (B1b).**
   - The battle stacks over the map with a bare screen whose only command is Run.
   - From B1b on, the forest has battles again, and every later slice is checked by walking into one.
3. **The stage and the HUD (B2a-B3c).** Backdrop, battlers, party status, enemy tags, turn order. Core's turn queue arrives with the turn order (B3c), its first user.
4. **Attack targets (C2), then commands and targeting (B4a-B4c).** Battles can now be won and lost; the end screens are known gaps until B7a and B8b.
5. **Playback (B5a-B5d).**
   1. timing;
   2. event-driven beats and the banner;
   3. numbers and fading enemies;
   4. effect strips and placeholder cues, with the GIF conversion (decision 57).
6. **Skills and items (C3, B6a, B6b).** Skill descriptions in content first. Each Core target rule arrives with its first user.
7. **End screens (B7a-B8b).** Victory, escape, then C4 (nobody standing loses at once) right before the defeat screen.
8. **Play-through check (Z).**

## Guiding rules
- **Systems, not balance.** Port mechanisms, not legacy design. The developer, 2026-10-20: "balance is not done yet; we're implementing systems and testing them."
  - Content values (enemy stats, encounter rate) stay as they are.
  - Tests that need a battle to end a certain way use a test catalog, not repository balance.
  - Legacy values become placeholders (table below). Each undecided choice is a numbered question.
- **Core owns every rule.** The Game asks Core for:
  - the turn queue and the legal targets;
  - whether a skill or item can be used, and whether a skill is affordable;
  - what an enemy does;
  - the outcome and the rewards.
  It never filters, rolls or decides. An `if` that decides gameplay goes to Core with a test.
- **Look** (`.claude/rules/game.md`, "UI look").
  - Look at `docs/ui_ux_exemples/Combat1-9` and `Victory_Menu.jpg` before each UI slice.
  - The legacy XAML (`legacy/JRPG/UI/Styles/BattleStyles.xaml`, `UI/Battle/**`) gives the starting values; lean toward the examples where legacy is plainer.
  - Legacy sizes were WPF units on 1920 x 1080, so they carry over 1:1.
  - **Pixel art is never smoothed:**
    - pixel-art battlers are point-scaled by a whole factor;
    - painted art (the party's 1024 px portraits) shrinks smoothly.
- **Thin shell (ADR 0008).**
  - The battle is a scene that returns `SceneRequest`s; `Flow/BattleTransitions` applies them behind the flow's fade.
  - A scene never builds another scene.
  - **Every scene returns a given request once**, then null until its state changes (key decision 2).
- **Pure presenter + thin draw.** Each part of the screen has:
  - a pure part (turn stages, beats, timings, cursors, layout, the request it returns), tested in `tests/Jrpg.Game.Tests`;
  - a draw part, checked by hand with the slice's checklist.
- **No allocation in `Update`/`Draw`.**
  - Text is formatted when values change.
  - Per-turn lists are built once per turn, and floating numbers live in a fixed pool.
  - Views loop over `battle.Combatants` checking `IsAlive`. They never call `LivingOn` or `UpcomingActors` per frame; those allocate and are called only at stage changes.
- **Keyboard and controller** (decision 50); no mouse. In battle, Esc is Cancel only: there is no menu in battle, and Cancel on the command list does nothing.
- **Seeds.** Battles draw from the session's one `IRandom`: turn order, enemy tactics and encounter rolls (ADR 0008, decision 43). The Game never rolls. Tests use seeded sessions.

## Out of scope (follow-ups)
- **Balance.** Enemy stats, experience and encounter rates are not tuned here (guiding rule).
- **Gold and item drops** (decision 103), its own plan:
  - content: enemy gold and drop tables;
  - Core: gold in the session, drops into the inventory;
  - the save format changes (gold), so an ADR;
  - the victory screen gains reward bars. `VictoryRewards` and the victory view are built so this is an addition (B7b).
- **Battle sprites with animation frames** (decision 100), its own plan. Pixel-art sprites at a whole multiple of their native size, idle and attack frames on a sheet. Needs a format decision (ADR).
- **Battles started by events** (decision 30). `BattleRequested` and `GameSession.StartPendingBattle` are the entry such a step would use.
- **New mechanics.** None of these are built:
  - defend mitigation (decision 94) and escape chance (decision 95);
  - status effects;
  - speed-based order;
  - enemy skills and behaviors;
  - weaknesses, break, boost;
  - rows;
  - per-skill effect animations (decision 101).
  Each would extend a seam below.
- Audio (question 76), a battle-speed option, auto-battle, a blurred victory background (decision 52), a swirl transition (decision 98).
- **Merging `Menus/ItemTargetPicker` with `Battle/BattleTargetCursor`.** They move differently: the menu lists every member and skips illegal ones, while battle only visits candidates.
- **Editor:** no battle editing (part 3c).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the session and the shell

- [x] **ADR 0013: Battle presentation and playback** (before B1b) → [slices/ADR-0013.md](slices/ADR-0013.md)
- [x] **C1. Battles in the session** (Core, about 230 lines) → [slices/C1.md](slices/C1.md)
- [x] **B1a. A request during the flow's fade waits for it** (Game, about 90 lines; devil's advocate B-1) → [slices/B1a.md](slices/B1a.md)
- [x] **B1b. Random battles start; Run ends them** (Game, about 270 lines) → [slices/B1b.md](slices/B1b.md)

### Phase 2: the stage and the HUD

- [x] **B2a. Backdrop, formation and background check** (Data + Game, about 200 lines) → [slices/B2a.md](slices/B2a.md)
- [x] **B2b. Battlers** (Data + Game, about 220 lines) → [slices/B2b.md](slices/B2b.md)
- [x] **B3a. Party status** (Game, about 240 lines) → [slices/B3a.md](slices/B3a.md)
- [x] **B3b. Enemy tags and letters** (Game, about 160 lines; decision 99) → [slices/B3b.md](slices/B3b.md)
- [x] **B3c. Turn order bar** (Core + Data + content + Game, about 280 lines) → [slices/B3c.md](slices/B3c.md)

### Phase 3: commands

- [x] **C2. Attack targets in Core** (Core, about 120 lines) → [slices/C2.md](slices/C2.md)
- [x] **B4a. Command menu and target cursor** (Game, about 230 lines) → [slices/B4a.md](slices/B4a.md)
- [x] **B4b. Fighting with Attack, Defend and Run** (Game, about 170 lines) → [slices/B4b.md](slices/B4b.md)
- [x] **B4c. Command panel and target pointer look** (Game, about 260 lines) → [slices/B4c.md](slices/B4c.md)

### Phase 4: playback

- [x] **B5a. Turn pacing: step forward and back** (Game, about 270 lines) → [slices/B5a.md](slices/B5a.md)
- [x] **B5b. Event-driven beats and the banner** (Game, about 260 lines) → [slices/B5b.md](slices/B5b.md)
- [x] **B5c. Floating numbers and fading enemies** (Game, about 220 lines) → [slices/B5c.md](slices/B5c.md)
- [x] **B5d. Effect strips and placeholder cues** (content + Data + Game, about 230 lines; decisions 57, 101) → [slices/B5d.md](slices/B5d.md)

### Phase 5: skills and items

- [x] **C3. Skill descriptions in content** (Core + Data + content, about 130 lines; decision 102) → [slices/C3.md](slices/C3.md)
- [ ] **B6a. Skills in battle** (Core + Data + Game, about 290 lines) → [slices/B6a.md](slices/B6a.md)
- [ ] **B6b. Items in battle** (Core + Game, about 290 lines) → [slices/B6b.md](slices/B6b.md)

### Phase 6: end screens

- [ ] **B7a. Victory: experience fill** (Game, about 270 lines) → [slices/B7a.md](slices/B7a.md)
- [ ] **B7b. Victory look** (Game, about 260 lines) → [slices/B7b.md](slices/B7b.md)
- [ ] **B8a. Escaped** (Game, about 110 lines) → [slices/B8a.md](slices/B8a.md)
- [ ] **C4. A battle with nobody standing is lost at once** (Core, about 90 lines; decision 97) → [slices/C4.md](slices/C4.md)
- [ ] **B8b. Defeat** (Game, about 290 lines; decision 65) → [slices/B8b.md](slices/B8b.md)
- [ ] **Z. Play-through check** (manual) → [slices/Z.md](slices/Z.md)

## Decisions (developer, 2026-10-20)
Questions and decisions share numbers.
- **30. Battles started by events:** stay out of scope (their own plan).
- **52. Blur:** the victory screen dims the battle for now; blur maybe later.
- **56. Battle layout, pacing and input:**
  - legacy layout and pacing as placeholders;
  - arrow targeting in formation order, starting on the last living enemy or on the actor;
  - no icon for skills without one;
  - a code-drawn pointer if `TargetArrow.png` isn't transparent.
- **57. Attack effects:** the two GIFs are converted to PNG strips outside the repository, at 0.08 s per frame, and `effects-source/` is deleted afterwards (git keeps them).
- **65. After a defeat:** three choices:
  - "Load last save" (the default, loads `SaveSlots.MostRecent` at once);
  - "Load" (the save list);
  - "Main menu".
  With no readable save, both loads are greyed.
- **94. Defend:** does nothing but pass the turn. Placeholder; mitigation is not implemented yet.
- **95. Run:** always succeeds (placeholder).
- **96. Fallen members after a battle:** stay at 0 HP (placeholder, subject to change).
- **97. Nobody standing:** no Party-page guard. A whole party down is a defeat, in battle or when a battle starts with nobody standing: the defeat screen with its choices (Core rule in C4).
- **98. Battle transition:** the flow's classic screen-change fade.
- **99. Enemy names:** "Goblin A/B/C" when several enemies share a name; display only, in the Game.
- **100. Battler art:** placeholders for now. Party portraits are shrunk smoothly to 96 px, and the goblin is point-scaled to 128 px. What 2D JRPGs commonly do:
  - pixel-art battle sprites at a whole multiple of their native size, with idle and attack frames on a sheet;
  - painted portraits kept for menus and dialogue.
  Proper battle sprites are a follow-up plan, which needs a format decision (ADR).
- **101. Effects:** the attack effect plays only on damage. An action with no effect animation shows a short placeholder text over the target, so trigger and timing can be tested.
- **102. Skill hints:** a `description` field in `skills.json`, like items (content format change, no generator). The existing skills get placeholder wording to review.
- **103. Victory:** "Level Up!" wording is fine. Gold and drops will exist: this plan stays with experience, but `VictoryRewards` and the victory view take them as an addition. Follow-up under "Out of scope".
- **104. Turn order:** the current actor plus the next eight of the repeating cycle.
- **105. Unavailable commands:** greyed; a greyed skill says "Not enough MP." on Confirm.
- **Font:** Lato for "Victory!" and "Defeat" until a serif is chosen.

Earlier decisions that apply:
- 13: the experience split.
- 26: encounters per step.
- 31: what follows a defeat is the Game's.
- 35: play time counts battles.
- 43: fresh seed on load.
- 50: controls.
- 51: icons.
- 82: look.
- 79: random battles off. This plan ends it at B1b.

## Open design questions (developer decides)
Each slice uses the placeholder until answered.
- **Font:** which serif for "Victory!", "Defeat" and big titles (Lato until then).
- **102 follow-up:** review the placeholder skill descriptions written in C3.
- **106. Placeholder cue:** the action's name over each target, shown 0.6 s in all (placeholder), or a neutral mark such as a small burst shape?

## ADRs needed
- **0013 Battle presentation and playback** (checkbox step before B1b). The main question: **where the battle HUD reads its values.**
  - **A. Live Core state (recommended).** The turn sequence steps the actor forward, then calls `Execute` at the moment of impact, and the HUD reads `Vitals`, the inventory and `UpcomingActors`.
    - Pros: the HUD can't disagree with Core; one path for party and enemies; legacy timing.
    - Cons: a multi-hit or chained action shows its final values from its first beat.
  - **B. Values derived from the events.** The screen keeps "shown" vitals and updates them beat by beat. Events would carry more data (MP spent, values before and after).
    - Pros: any sequencing.
    - Cons: a second vitals model, and event data no mechanic needs yet (ADR 0004).
  - Moving from A to B later only touches `VitalsText` and `ActionPlayback`: the beats already follow the events.
  - **C. A generic timeline/tween engine.** Speculative.
  The ADR also records:
  - **Stacking:** the battle stacks over the map (push/pop like the menu; the map resumes once the encounter is cleared), rather than replacing the map and rebuilding it on return.
  - **Beats:** `ActionPlayback` paces by event-driven beats. Defend and Run don't hold; a future between-turn event (poison) gets its own beat. Its switch is exhaustive and throws on unknown events.
  - **The outcome:** the scene calls `FinishBattle` once and returns its outcome request once.
- **Future, not in this plan:**
  - gold and drops (a save format change, decision 103);
  - a battle sprite sheet format (decision 100).
- **None needed for** the effect strips (ADR 0009), text (ADR 0010), the skill `description` field (a routine content addition validated by Data) or the save format here.
