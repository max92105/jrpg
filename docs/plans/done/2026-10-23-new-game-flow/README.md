# Game: the new-game flow
Status: done

First feature plan after the migration and the code-health plan.
- Decisions 156-178 and 185-187 are recorded below.
- Questions 179-183 are open; the next free number is 188.
- This plan writes ADRs 0016-0018; the next ADR is 0019.

Developer's request (2026-10-23, verbatim, typos kept): "OK let<s plan real new feature now and real game flow. update main menu i dont, want to see invalid option. not grayed jsut not there. continue should be the default selected when available else its new game. then new game now should move to character selection. there will be no customisation just a choice between. Ragnir - Warrior, Omer - Mage, Zanyr - Cleric, Lazarus - Rogue, Jazog - Barbarian, Kamelia - Paladin, Zarruk - Monk, Sarxian - Warlock. the selected character will be the leader 'main character' when hovering one i want be able to display information about the class and the summary of the character. When selecting we gon into a cinemtic that will show why this character is entengle in the story essentially it will be a voice over some still image that will appear and disappear. i have no budget and i have no other idea on how to make it for now. so lets start with that. i will need placeholder for now to test behavior and i will place in the repo the more official art and audio later. right now i just want this to work and run so i can test a full new game flow."

The developer approved the plan on 2026-10-23, answering the draft's questions (see "Decisions"). The devil's advocate's review reordered it so that the whole flow runs before any look work.

## Goal
At the end of this plan, a new game plays from the title to the map like this:
1. **Title.** Only usable entries show; nothing is greyed.
   - With a readable save: Continue, New Game, Load, Quit, with the cursor on Continue.
   - Without one: New Game and Quit, with the cursor on New Game.
   - The defeat screen also hides the loads it can't use.
2. **Character choice.** New Game opens a screen with a vertical list of nine characters on the left. The rest of the screen shows the highlighted character's information:
   - the class and its description;
   - the character's summary;
   - their level 1 stats, taken from the class.

   Confirm asks "Begin as X?"; Cancel goes back to the title.
3. **The leader.** The chosen character starts alone in the party, at level 1 with 0 XP. The others are recruited during play, through a recruit event on the map. For now, placeholder recruit points on the start map offer each of them.
4. **Classes like D&D's.** A class decides a character's stats at level 1 and how they grow with each level. Items have no class restriction.
5. **The intro.** After the choice, that character's intro plays: still images that fade in and out, a voice-over, and subtitles. Each still's time is set in content. Confirm or Cancel skips it. Then the map shows and play begins.
6. **Placeholders.** Every description, summary, still, caption, voice and class number is a placeholder:
   - The stills, the new characters' portraits and the voice are generated once by scripts in this plan.
   - The developer replaces them by dropping files into `content/` and editing JSON, with no code change.

## Order, and why
1. **Title (T1, T2).** Independent and small; the first thing every run shows.
2. **The flow, end to end and silent (L1, S1, S1b, ADR 0016, K1, K2a, K2b, K2c, K3).**
   - The leader in Core, then the choice screen and its confirmation, then the intro as a map cutscene (ADR 0016): an event that runs on arrival, the still step, the editor's support for both, the Game showing stills, and the intro map.
   - After K3, title → choice → intro → map runs with today's characters. This is the developer's "just make it run".
3. **Classes and characters (ADR 0017, C1a, C1b1, C1b2, C1c, C2, S2).**
   - The nine classes, then stats from the class, then growth on level-up.
   - Then the nine characters, then the choice screen showing what they are.
4. **Recruiting (R1a, R1b, R2).**
   - A map event can recruit a member (Core, Data and the game), then the editor can author it.
   - Then placeholder recruit points on the start map.
5. **The voice (ADR 0018, A1a1, A1a2, A1b), then a run-through (Z1).** Audio comes after the silent intro, so the intro can be tested without sound. Z1 is the developer's first full check.
6. **The look (S3a, S3b, K4), then the final check (Z).** Behavior first, then the look, per the code-health plan's rule.

## Guiding rules
- **Systems, not design.** Every value the developer hasn't given is a placeholder in content (design.md, "Tunables and placeholders") and, where it decides design, a numbered question. Nothing is balanced here beyond "a lone level 1 leader can win the start map's fights" (decision 187).
- **Content only in `content/`** (ADR 0011):
  - class stats and descriptions in `classes.json`;
  - summaries in `party-members.json`;
  - the choice list in `new-game.json`;
  - intros as arrival events on the `intro` map, one per leader (decision 185);
  - stills in `content/art/cinematics/`;
  - the voice in `content/audio/voices/`;
  - the game's own words in `content/text/ui.json` (ADR 0010).
- **Rules in Core.** Core decides who leads, who joins, what a class gives at a level and what recruiting does. The Game only passes the choice and plays requests.
- **Scenes return requests** (ADR 0008):
  - the choice returns `LeaderChosen` or `TitleRequested`;
  - the intro is a map event, shown by the exploration scene's `EventPresenter`;
  - each returns its request once.

  The update path never touches the graphics or audio device: scenes write the voice they want into a plain `VoiceChannel`, and the host plays it.
- **Pure presenter, thin draw.** `LeaderChoiceMenu`, the still step's timing and the class stat formula are tested headless; the views are checked by hand.
- **No allocation per frame.** Texts, layouts and wrapped summaries are built when the screen opens or the cursor moves, never per frame.
- **Look** (`.claude/rules/game.md`). Look at `docs/ui_ux_exemples/Menu6.webp` and `MainMenu.jpg` before S3a, S3b and K4. Painted art is shrunk or covered smoothly.
- **Keyboard and controller** (decision 50); no mouse.
- **No NuGet package.** Audio uses MonoGame's own `SoundEffect` (ADR 0018).
- **Placeholder files stay small.** There is one shared voice file, small generated stills and small generated portraits, because git keeps every version forever. The generator scripts are one-off PowerShell, run once and not added to the solution; each slice keeps the command it ran in its Done note.
- **Mechanical test edits.** When a slice changes many test call sites (L1, C1b), it records the solution's test count before and after.

## Out of scope (follow-ups)
- **More cutscene steps** (moves, camera, waits): the intro starts the cutscene system (ADR 0016), and later story scenes add the steps they need.
- **Class skills, subclasses and abilities.** A class gives stats and growth only. Skills stay on the member (`skillIds`) until a class-skills plan (question 181).
- **Music, sound effects and volume settings** (ADR 0018 sets where audio lives).
- **A map sprite per leader** (question 58).
- **The story.** Where the recruits really are, story flags and the opening event: the start map's recruit points are placeholders.
- **Editor support** for classes, characters and the new game: they are edited as JSON. The editor does get every new event step and activation (K2b, R1b, A1a2), because otherwise it can't open a map that uses one.
- **Title art.** The title keeps `StartMenu.png`.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the title
- [x] **T1. The title shows only usable entries; Continue first and selected with a save** (Game, about 170 lines) → [slices/T1.md](slices/T1.md)
- [x] **T2. The defeat screen hides the loads it can't use** (Game, about 80 lines) → [slices/T2.md](slices/T2.md)

### Phase 2: the flow, end to end
- [x] **L1. A new game starts with the chosen leader alone; the party may hold 5** (Core + Data + content, about 300 lines, plus about 50 one-argument call-site edits) → [slices/L1.md](slices/L1.md)
- [x] **S1. New Game opens the character choice** (Game, about 280 lines) → [slices/S1.md](slices/S1.md)
- [x] **S1b. Choosing asks "Begin as X?" first** (Game + content, about 90 lines) → [slices/S1b.md](slices/S1b.md)
- [x] **ADR 0016: Intro cinematics are map cutscenes** (accepted: option B) → [slices/ADR-0016.md](slices/ADR-0016.md)
- [x] **K1. An event can run when the player arrives on its map, only when a given character leads** (Core + Data, about 200 lines) → [slices/K1.md](slices/K1.md)
- [x] **K2a. The still step: show an image for a set time with a caption; skipping runs past presentation steps** (Core + Data + content, about 230 lines) → [slices/K2a.md](slices/K2a.md)
- [x] **K2b. The editor authors the arrival activation, the leader condition and the still step** (Editor, about 200 lines) → [slices/K2b.md](slices/K2b.md)
- [x] **K2c. The map shows a still step: covering the screen, fading in and out, Confirm or Cancel skipping** (Game, about 250 lines) → [slices/K2c.md](slices/K2c.md)
- [x] **K3. The intro map: a new game starts there, each leader's intro plays, then a warp to the start map** (content + Data + tests, about 150 lines) → [slices/K3.md](slices/K3.md)

### Phase 3: classes and characters
- [x] **ADR 0017: Stats come from the class** (before C1a) → [slices/ADR-0017.md](slices/ADR-0017.md)
- [x] **C1a. The nine classes, with descriptions; items lose their class restriction** (Core + Data + content + Game, about 220 lines) → [slices/C1a.md](slices/C1a.md)
- [x] **C1b1. Classes carry their base stats and growth per level; the one formula** (Core + Data + content, about 170 lines; split from C1b) → [slices/C1b1.md](slices/C1b1.md)
- [x] **C1b2. A member's stats come from their class at their level; members join at level 1** (Core + Data + content, about 260 lines; split from C1b) → [slices/C1b2.md](slices/C1b2.md)
- [x] **C1c. A level-up raises max HP, max MP, attack and defense by the class's growth** (Core + Game, about 150 lines) → [slices/C1c.md](slices/C1c.md)
- [x] **C2. The nine characters: Ragnir, Omer and Lazarus added, Kamilia removed, Teelle the Paladin, Thaali the Druid, summaries, generated portraits** (Core + Data + content, about 220 lines) → [slices/C2.md](slices/C2.md)
- [x] **S2. The choice shows the class, description, summary and level 1 stats** (Game, about 220 lines) → [slices/S2.md](slices/S2.md)

### Phase 4: recruiting
- [x] **R1a. A map event can recruit a member** (Core + Data + Game + content, about 230 lines) → [slices/R1a.md](slices/R1a.md)
- [x] **R1b. The editor authors the recruit step** (Editor, about 120 lines) → [slices/R1b.md](slices/R1b.md)
- [x] **R2. Placeholder recruit points on the start map** (content + tests, about 100 lines) → [slices/R2.md](slices/R2.md)

### Phase 5: the voice
- [x] **ADR 0018: Audio files and playback** (accepted: option A) → [slices/ADR-0018.md](slices/ADR-0018.md)
- [x] **A1a1. The voice step: an event starts a voice file, checked at startup; one generated placeholder voice** (Core + Data + Game startup + content, about 280 lines; split from A1a) → [slices/A1a1.md](slices/A1a1.md)
- [x] **A1a2. The editor authors the voice step; every intro starts with the placeholder voice** (Editor + content, about 60 lines; split from A1a) → [slices/A1a2.md](slices/A1a2.md)
- [x] **A1b. The intro speaks; with no sound device the game plays on silently** (Game, about 220 lines) → [slices/A1b.md](slices/A1b.md)
- [x] **Z1. Run-through: the full new-game flow** (manual) → [slices/Z1.md](slices/Z1.md)

### Phase 6: the look
- [x] **S3a. Choice screen look: the list on the left, header and footer bars** (Game, about 170 lines) → [slices/S3a.md](slices/S3a.md)
- [x] **S3b. Choice screen look: the information area, portrait and stats** (Game, about 200 lines) → [slices/S3b.md](slices/S3b.md)
- [x] **K4. Intro look: lead-in and lead-out, caption band, skip hint** (Game, about 170 lines) → [slices/K4.md](slices/K4.md)
- [x] **Z. Play-through check** (manual) → [slices/Z.md](slices/Z.md)

## Decisions
Questions and decisions share numbers.

From the request (2026-10-23):
- **156. Title entries:** an entry that can't be used is left out, not greyed. Continue is selected when it shows; otherwise New Game. This replaces decision 80's greyed Continue and Load.
- **157. Character choice:** New Game opens a choice between fixed characters, with no customisation. The final list is in decision 162.
- **158. The leader:** the chosen character is the leader, the main character. While the cursor is on a character, the screen shows their class information and summary.
- **159. The intro:** choosing plays an intro showing why that character is caught up in the story: a voice-over over still images that appear and disappear.
- **160. Placeholders:** placeholder art, text and audio now, so that the full new-game flow runs and can be tested. The developer adds the real files later.

The developer's answers to the draft's questions (2026-10-23):
- **161. Spellings:** "take the one from content". The shown names are Kamilia and Zaruk.
- **162. The characters:** "Replace Kamilia by Teelle as the paladin, Put Thaali in as a Druid."
  - The choice lists nine, each with a class:
    - Ragnir, Warrior; Omer, Mage; Zanyr, Cleric; Lazarus, Rogue; Jazog, Barbarian;
    - Teelle, Paladin; Zaruk, Monk; Sarxian, Warlock; Thaali, Druid.
  - *Read by Claude, to confirm:* Kamilia leaves the game (her content entry goes; old saves drop her through `SaveGameRepair`, as they drop any unknown member), and Thaali is a ninth choice of a ninth class, Druid. Thaali is listed last (question 182).
- **163. Who starts:** "Nobody else in party except leader at the start, the other will join later in the game. be able to recruit them."
  - The new game starts with the leader alone.
  - A map event can recruit a member (R1a).
  - *For testing:* placeholder recruit points on the start map, one per character (R2).
- **164. A class:** "think of a class like D&D has class". A class decides the character's stats at level 1 and their growth per level (ADR 0017). Skills and abilities by class come later (question 181).
- **165. Equipment:** "right now just don't have restriction for class in item". Items lose `allowedClassIds`, and anyone can wear anything (C1a).
- **166. Placeholder art:** "if you are able to do placeholder yourself do it please". The slices generate small placeholder stills, portraits and a voice with one-off scripts.
- **167. Wording:** the placeholder texts are fine.
- **168. What the choice shows:** everyone starts at level 1 with 0 XP, and the rest depends on the class. The information area shows the class, its description, the summary and the level 1 stats.
- **169. Layout:** a vertical list on the left; the information about the highlighted character uses most of the rest of the screen.
- **170. Confirming:** choosing asks "Begin as X?" before the intro (S1b).
- **171. Skipping the intro:** allowed. Confirm or Cancel skips it.
- **172. Subtitles:** yes, under the stills.
- **173. Still timing:** each still's time is set in content, to match the voice.
- **174. Play time:** the intro counts (the session starts at the choice).
- **175. The defeat screen:** it also hides the loads it can't use (T2). "not a fan of grayed out if it's not available don't show."
- **176. Party size:** 5. `rules.json`'s `maxActiveMembers` becomes 5 (L1). This answers question 150.
- **177. Title order:** Continue comes first when there's a readable save: Continue, New Game, Load, Quit.
- **178. Cancel on the choice** goes back to the title.
- **185. Each leader's intro:** every new game starts on one `intro` map, which holds one arrival event per leader, each running only when that character leads (`onlyWhenLeaderId`). One place to edit, and the condition serves later story scenes (ADR 0016).
- **186. A save repaired onto the intro map** (its map left the content) plays the leader's intro again, then warps to the start map: intro events aren't "once" (2026-10-24, from the K3 review). Only a new game and that repair reach the intro map.
- **187. The start fight is balanced on the goblin, not the classes** (2026-10-24, from C1b2's simulation; answers question 184): the goblin goes from 20 attack and 40 HP to 16 and 30, so a lone level 1 leader of every class wins the goblin trio attacking only. Raising the weak classes instead would have given the mage more defense than the paladin.

Earlier decisions that apply:
- 35: play time counts everything since the playthrough began.
- 43: a fresh seed per playthrough.
- 50: controls.
- 58: one map sprite for everyone (still open).
- 65: the defeat menu, now without greyed entries (T2).
- 82: UI sizes are placeholders.
- 107: the leader never leaves the active party.
- 149: the official party is 5.

## Open design questions (developer decides)
Each slice uses the placeholder until answered.
- **179. A recruit's level.** Placeholder: a recruit joins at level 1 with 0 XP, like the leader, whatever the party's level. The alternative is to join at the leader's level, or at a level set per recruit point.
- **180. The class numbers.** Each class's level 1 stats and growth per level, in `classes.json`. Placeholder: D&D-flavored values (design.md), chosen only so a lone level 1 leader can win the start map's fights.
- **181. Skills.** Placeholder: members keep their own `skillIds`. Should skills come from the class, and from which level?
- **182. Thaali's place in the list.** Placeholder: last, after the request's eight.
- **183. Kamilia's art files** (`Kamilia.png` in portraits and battlers). Placeholder: kept in the repository unused, since only the developer can decide to delete art.

Still open from earlier plans: 92, 112, 147, 151-155.

## ADRs
- **0016 Intro cinematics are map cutscenes** (accepted, option B, `docs/decisions/0016-intro-cinematics.md`). The intro is a map event: an arrival activation, a still step and a voice step, run by `EventRun`, ending with a warp to where play starts. The slideshow content type (A) and video (C) were declined.
- **0017 Stats come from the class** (before C1a). Where do a member's max HP and MP, attack and defense come from?
  - **A. The class at the member's level (recommended).** Base at level 1 plus growth per level, as D&D classes give hit dice and features by level. The member keeps only who they are: name, art, summary, skills.
  - **B. Per member, as today, plus a per-member growth.** Every character tuned by hand: the opposite of "think of a class like D&D".
  - **C. Class base plus a per-member modifier.** More tuning knobs than placeholders need. It can be added to A later.
  - This supersedes the part of ADR 0007 that takes max HP and MP and base stats from the member's definition. Saves still hold only level, XP, HP, MP and equipment, so no format change.
- **0018 Audio files and playback** (accepted 2026-10-24: A).
  - **A. WAV through MonoGame's `SoundEffect` (recommended).**
  - **B. OGG through `Song`/`MediaPlayer`.**
  - **C. A package**, which needs the developer's approval and isn't needed here.
- **None needed for:**
  - `leaderChoices` (content, validated by Data);
  - the recruit step (a new `EventStep`, ADR 0006's pattern);
  - removing the item class lists (decision 165);
  - the save format (unchanged).
