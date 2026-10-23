# New-game flow: design

Part of [the new-game flow plan](README.md).

### Tunables and placeholders

Every value here is a placeholder until the developer decides (README, open questions). Content values live in `content/`; screen values are named constants in their view.

**Content**

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Leader choices, in order | `new-game.json` `leaderChoices` | L1: today's members except Kamilia. C2: ragnir, omer, zanyr, lazarus, jazog, teelle, zaruk, sarxian, thaali | The list on the choice screen, top to bottom (decision 162, question 182). |
| Party at the start | none (`recruitedMemberIds` and `activeMemberIds` removed) | The leader alone | Decision 163. |
| Max active members | `rules.json` `party.maxActiveMembers` | 5 | Decision 176. |
| Classes | `classes.json` | The nine below, each with a one-sentence `description` | Decision 164; numbers are question 180. |
| Members' classes | `party-members.json` `classId` | The nine of decision 162 | Decision 162. |
| New characters | `party-members.json` | `ragnir`, `omer`, `lazarus`; no skills | C2. |
| New characters' art | `art/portraits/`, `art/battlers/` | `Ragnir.png`, `Omer.png`, `Lazarus.png`: 256 px squares, generated, a flat class color with the initial | Question 166's answer; small files, replaced by the developer's art. |
| Summaries | `party-members.json` `summary` | Two placeholder sentences per member | Decision 167. |
| Intros | `cinematics.json` | One per leader (`ragnir_intro` ...), three shots of 4 s, each caption naming the character and the shot number | Decisions 172, 173. |
| Stills | `content/art/cinematics/` | `Placeholder1.png` to `Placeholder3.png`: 640 x 360, generated, dark gradients with "Still n", shared by every intro | Covered smoothly to the screen. |
| Voice | `content/audio/voices/` | One `Placeholder.wav` shared by every intro, spoken by Windows' speech synthesizer ("Placeholder voice-over.") | A1a; replaced by per-intro files later. |
| Recruit points | `content/maps/start.json` | One NPC per character, each with a recruit event | R2; placeholders for the story's recruits. |

**Classes** (question 180). Stats at level 1, then the growth added per level after the first: `stat(level) = base + growth × (level − 1)`.

| Class | Max HP | Max MP | Attack | Defense | +HP | +MP | +Atk | +Def |
|---|---|---|---|---|---|---|---|---|
| warrior | 220 | 20 | 24 | 8 | 22 | 2 | 3 | 1 |
| barbarian | 250 | 10 | 26 | 6 | 26 | 1 | 3 | 1 |
| paladin | 210 | 40 | 22 | 9 | 20 | 4 | 2 | 2 |
| cleric | 180 | 60 | 18 | 7 | 16 | 6 | 2 | 1 |
| monk | 190 | 30 | 23 | 6 | 18 | 3 | 3 | 1 |
| rogue | 170 | 30 | 25 | 5 | 15 | 3 | 3 | 1 |
| druid | 175 | 60 | 19 | 6 | 16 | 6 | 2 | 1 |
| warlock | 160 | 70 | 22 | 4 | 14 | 7 | 3 | 1 |
| mage | 150 | 80 | 20 | 4 | 12 | 8 | 2 | 1 |

These numbers are a starting point for C1b's simulation (question 184). The forest's goblin trio deals 3 × (20 − defense) a round and has 3 × 40 HP at 2 defense. C1b adjusts the base HP and defense until a lone level 1 leader of each class wins with attacks alone, and records the win rates.

**Screens** (1920 x 1080 virtual screen)

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| `ListLeft`, `ListTop`, `ListWidth`, `RowPitch` | `NewGame/LeaderChoiceScene` (S3a) | 64, 140, 420, 88 | The vertical list on the left (decision 169). |
| `NameTextPixels`, `ClassTextPixels` | `NewGame/LeaderChoiceScene` (S3a) | 34, 20 | A row's name and, under it, its class in spaced caps. |
| `HeaderHeight`, `FooterHeight` | `NewGame/LeaderChoiceScene` (S3a) | 80, 64 | As the game menu's. |
| `InfoLeft` | `NewGame/LeaderProfileView` (S3b) | 540 | The information area starts here and runs to the screen's right edge. |
| `PortraitPixels` | `NewGame/LeaderProfileView` (S3b) | 720 | The portrait, shrunk smoothly, at the right of the information area. |
| `ProfileFade` | `NewGame/LeaderProfileView` (S3b) | 0.2 s | The information fades in when the cursor moves. |
| `ShotFade` | `Cinematics/CinematicPlayback` (K2) | 0.8 s, at most half the shot | Each still fades in and out over this ("appear and disappear"). |
| `LeadIn`, `LeadOut` | `Cinematics/CinematicPlayback` (K3) | 0.5 s, 0.5 s | Black before the first still and after the last. |
| `CaptionTextPixels`, `CaptionBandHeight` | `Cinematics/CinematicScene` (K3) | 32, 160 | The subtitle and the dark band behind it. |

## Design overview

### What exists, checked
- **Title.**
  - `Title/TitleMenu` holds a static `Entries` list of four and greys Continue and Load when there's no readable save (`IsEnabled`, decision 80).
  - `Title/TitleScene` draws greyed entries in `UiTheme.TextDisabled`.
  - `Battle/DefeatMenu` greys its two loads the same way (`IsEnabled`).
- **New game.**
  - `TitleScene` returns `NewGameRequested`.
  - `GameFlow.ApplyRequestOnceBlack` calls `SessionTransitions.StartNewGame()`, which calls `GameSession.StartNew(catalog, random)`.
  - `new-game.json` has `recruitedMemberIds` (seven) and `activeMemberIds` (`["sarxian"]`).
- **Party.**
  - `PartyState.Leader` is `activeOrder[0]`, fixed for the playthrough, and saves store the active order.
  - Members are added only at the start: there is no recruiting during play.
  - `PartyMember.Recruit` builds a member from its definition.
- **Stats.**
  - `PartyMemberDefinition` holds level, experience, HP and max HP, MP and max MP, attack and defense.
  - `PartyMember` takes `BaseStats` and the maxima from the definition, fixed for good.
  - `GainExperience` raises `Level` and nothing else: stats don't grow today.
  - `SaveGameRepair` clamps saved HP and MP to the definition's maxima (ADR 0007).
- **Classes.**
  - `ClassDefinition(Id, Name)`, validated by `ClassCatalogValidator`.
  - Items' `allowedClassIds` are checked by `EquipmentStats.CanBeWornBy`, in `PartyEquipment` (twice), `ItemCatalogValidator` and `SaveGameRepair.RepairWornItem`.
  - `StatusPage` and `MemberCardView` show the class name.
- **Events.**
  - `EventStep` subtypes: dialogue, give item, swap object, warp. They are run by `EventRun` (ADR 0006), loaded through `ContentTypeDiscriminators` and validated by `MapEventValidator`.
  - The editor's `EventStepViewModel` throws on a step type it doesn't know, so a new step needs an editor case.
- **Kamilia** is in `party-members.json`, `new-game.json` and `StatusPageTests`. Saves drop unknown members (`SaveGameRepair`).
- **Audio.** None. MonoGame DesktopGL 3.8.6 has `SoundEffect.FromFile` for WAV, with no package and no content pipeline.
- **Host.**
  - `Program` builds the `GameFlow` (or `StartupErrorScene`) and runs `JrpgGame`.
  - `GameFlow`'s and `SceneFactory`'s constructors are at four parameters.
- **Test churn.**
  - `GameSession.StartNew` has about 49 call sites in tests, in 24 files.
  - Many tests build members with explicit stats through `PartyMemberBuilder`.
  - Battle and Game tests on repository content assume Sarxian's numbers.

### Core changes
| Slice | Change |
|---|---|
| L1 | `Content/LeaderChoice` (`MemberId`). `NewGameDefinition` gains `LeaderChoices` and loses `RecruitedMemberIds` and `ActiveMemberIds`. `GameSession.StartNew(ContentCatalog catalog, string leaderId, IRandom random)`: the party is the leader alone; it throws `ArgumentException` for an id that isn't offered. |
| K1 | `Cinematics/CinematicDefinition`, `Cinematics/CinematicShot`; `ContentCatalog.Cinematics`; `LeaderChoice.IntroCinematicId`. |
| C1a | `ClassDefinition` gains `Description`. `EquipmentStats` loses `AllowedClassIds` and `CanBeWornBy`; `PartyEquipment` stops checking the class. |
| C1b | `ClassDefinition` gains `BaseStats` and `Growth` (`Party/ClassStats`: max HP, max MP, attack, defense). `Party/ClassGrowth.StatsAt(ClassDefinition, int level)` is the one formula. `PartyMemberDefinition` loses level, experience, HP, MP and the stats. `PartyMember` takes its class and computes its maxima and base stats from its level. `Recruit` joins at level 1, 0 XP, full HP and MP. |
| C1c | On a level-up, `PartyMember` raises its maxima and base stats, and adds the max HP and MP gained to its current HP and MP (`Vitals.RaiseMaxima`). |
| C2 | `PartyMemberDefinition` gains `Summary`. |
| R1a | `Events/RecruitStep(string MemberId)`. `EventRun` recruits through `PartyState.Recruit(PartyMember)`: into the active party when there's room, else the reserve. Recruiting someone already recruited does nothing. |
| A1a | `CinematicDefinition.VoiceFile` (required). |

Nothing in Core plays, times or draws a cinematic. The timeline is presentation, like `Typewriter`, and lives in the Game.

### Content format
`new-game.json` after K1:
```json
"leaderChoices": [
  { "memberId": "ragnir", "introCinematicId": "ragnir_intro" },
  ...
],
```
`classes.json` after C1b:
```json
{ "id": "warrior", "name": "Warrior", "description": "...",
  "base": { "maxHp": 220, "maxMp": 20, "attack": 24, "defense": 8 },
  "growth": { "maxHp": 22, "maxMp": 2, "attack": 3, "defense": 1 } }
```
`party-members.json` after C2: `id`, `name`, `classId`, `summary`, `portraitFile`, `battleSpriteFile`, `attackEffectFile` and `skillIds`, with no numbers.

`cinematics.json` after A1a:
```json
{ "id": "ragnir_intro", "voiceFile": "Placeholder.wav",
  "shots": [ { "imageFile": "Placeholder1.png", "seconds": 4.0, "caption": "Ragnir's intro, still 1 (placeholder)." }, ... ] }
```
A recruit step in a map event: `{ "type": "recruit", "memberId": "omer" }`.

Validation (Data):
- `leaderChoices`: at least one; no null; known, distinct member ids; known intro ids.
- Classes: `description` not blank; base max HP and every growth at least 0, base max HP at least 1, base attack and defense at least 0.
- Cinematics: unique ids; at least one shot; `imageFile` not blank; `seconds` above 0; `caption` null or not blank; `voiceFile` not blank.
- A recruit step names a known member.
- Startup checks:
  - stills by exact name in `art/cinematics/` (`ArtReferenceCheck`);
  - voices by exact name in `audio/voices/` (`AudioReferenceCheck`).

### Structure
| Project | Folder | Types |
|---|---|---|
| Core | Content | `LeaderChoice` (one character a new game may start led by, with the intro that follows) |
| Core | Party | `ClassStats` (max HP, max MP, attack, defense: a class's base or growth), `ClassGrowth` (the stats a class gives at a level: the one formula) |
| Core | Events | `RecruitStep` (adds a member to the party) |
| Core | Cinematics | `CinematicDefinition`, `CinematicShot` |
| Data | Cinematics | `CinematicCatalogValidator` |
| Data | Audio | `AudioFolders`, `AudioReferenceCheck` (every voice file named exists by exact name) |
| Data | Art | `ArtFolders.Cinematics`; `ArtReferenceCheck` covers stills |
| Game | Scenes | `LeaderChosen`, `CinematicFinished` |
| Game | NewGame | `LeaderChoiceMenu`, `LeaderChoiceScene`, `LeaderProfileView` (S3b) |
| Game | Cinematics | `CinematicPlayback`, `CinematicScene` |
| Game | Audio | `VoiceChannel`, `SoundLibrary`, `VoicePlayer` |
| Game | Ui | `HeaderBar` (S3a; the game menu becomes its second user) |
| Editor | Events | `EventStepKind.Recruit` and its fields (R1b) |

Game folder dependencies (none goes back up):

| Folder | Uses |
|---|---|
| NewGame | Scenes, Input, Ui, Rendering, Core Content and Party |
| Cinematics | Scenes, Input, Ui, Rendering, Audio (`VoiceChannel`), Data Art, Core Cinematics |
| Audio | Data Audio; MonoGame Audio (`SoundLibrary`, `VoicePlayer` only) |
| Flow | NewGame, Cinematics, Audio (through `SceneFactory`), plus what it used before |
| Ui | unchanged: no NewGame, Cinematics or Audio |

### The flow
```
Title --NewGameRequested--> [fade] LeaderChoiceScene (no session)
LeaderChoiceScene --TitleRequested--> [fade] Title
LeaderChoiceScene: Confirm -> "Begin as X?" (ConfirmPrompt); No closes it; Yes:
LeaderChoiceScene --LeaderChosen(choice)--> [fade, at black] Session = StartNew(choice.MemberId);
                                               map, with the intro pushed on top (from K2)
CinematicScene --CinematicFinished--> [fade, at black] pop the intro; the map takes input
```
- `SessionTransitions` gains `ShowLeaderChoice()` and `StartNewGame(LeaderChoice choice)`.
- `SceneFactory` gains `CreateLeaderChoice()` and `CreateIntro(CinematicDefinition)`.
- The intro stacks over the map as the battle does (ADR 0013), and the map isn't updated under it.
- **`GameFlow.ApplyRequestOnceBlack`** has 6 cases today. Whichever slice makes CA1502 fail first (S1 or K2) splits it into `ApplySessionRequest` (new game, leader chosen, load, title) and `ApplyScreenRequest` (battle, battle finished, cinematic finished).

### Recruiting
- `RecruitStep` runs at once, then shows "{name} joins the party." (a `ui.json` message) as a notice, as `GiveItemStep` shows what was received.
- Whether the member goes to the active party or the reserve is `PartyState`'s rule: active while there's room (`maxActiveMembers`), else reserve.
- A second run of the same event finds the member recruited and shows "{name} is already in the party." Since the leader's own recruit point exists on the start map, this is the normal case for one of the nine.
- The recruits saved are the members saved, so no format change.

### The voice-over path (ADR 0018)
- `SceneFactory` creates the one `VoiceChannel`, as it creates `UiTextures`, and hands it to each `CinematicScene`. `GameFlow.Voice` exposes it.
- `CinematicScene` calls `Voice.Play(file)` on its first update and `Voice.Stop()` when it ends or is skipped.
- After each update, `JrpgGame` calls `VoicePlayer.Follow(voice)`.
  - The player compares the channel's file with the one it last started: a different file, or null after a file, stops the old one and starts the new.
  - A stop followed by a replay of the same file reads as null, then the file, because `Follow` runs every update.
- `JrpgGame.LoadContent` touches the audio system once, so a missing device shows at startup, not mid-intro. A1b's hand check, run with the device disabled, finds which exception arrives, and only that one is caught. It is reported once, and the game runs silent.
- A voice file that fails to load is reported once, by name, and that intro plays silent.

### Key design decisions
1. **The intro is not the cutscene system** (ADR 0016).
2. **The choice is a list of records in `new-game.json`.** Which characters may lead, in what order and with which intro is new-game content.
3. **Core checks the leader.** `StartNew` accepts only an offered leader.
4. **A class gives stats by level, through one formula** (ADR 0017). A member is who they are, and the class is what they can do: linear growth is the simplest rule that makes levels matter. A curve, a per-member modifier or class skills can replace the formula later without touching saves.
5. **Recruiting is an event step**, because joining is something the story does on a map (ADR 0006's pattern). The start map's recruit points are content that the story replaces.
6. **Timing comes from content seconds, not the voice length** (decision 173). It is deterministic and testable.
7. **The session starts at the choice**, so the intro stacks over the map like a battle, and play time counts it (decision 174).
8. **Placeholders are generated, small and shared**, because git history keeps them forever.

### Replacement mapping
| Existing | Becomes |
|---|---|
| `TitleMenu.IsEnabled`, static `TitleMenu.Entries`, the greyed title entries | `TitleMenu.Entries` per instance, only usable entries, Continue first (T1) |
| `DefeatMenu.IsEnabled` and its greyed loads | `DefeatMenu.Entries` per instance, only usable entries (T2) |
| `NewGameRequested` starting the session | `NewGameRequested` showing the choice; `LeaderChosen` starting the session (S1) |
| `NewGameDefinition.RecruitedMemberIds`, `ActiveMemberIds` | `NewGameDefinition.LeaderChoices` (L1) |
| `GameSession.StartNew(catalog, random)` | `GameSession.StartNew(catalog, leaderId, random)` (L1) |
| `EquipmentStats.AllowedClassIds`, `CanBeWornBy` | removed (C1a) |
| Classes thief, healer, knight, ranger | the nine classes (C1a) |
| `PartyMemberDefinition` level, experience, HP, MP, attack, defense | `ClassDefinition.BaseStats` and `Growth`, through `ClassGrowth` (C1b) |
| `SaveGameRepair` clamping to the definition's maxima | clamping to the class's maxima at the saved level (C1b) |
| The game menu's two header-bar lines | `Ui/HeaderBar.Draw` (S3a) |
| `JrpgGame(rootScene, artRoot, settings)` | `JrpgGame(rootScene, contentRoot, settings, voice)` (A1b) |

## Risks
- **Test churn.**
  - L1 changes about 49 `StartNew` call sites. `ContentCatalogBuilder.WithLeader(id)` keeps each one a line.
  - C1b removes the stats from member definitions. `PartyMemberBuilder` keeps a way to set a member's level, and tests that need exact numbers give the member a test class with those numbers (`ClassBuilder`).
  - Repository-content tests that assume Sarxian's numbers are rewritten against his Warlock numbers.
  - Each of these slices records the test count before and after.
- **Difficulty.** A lone level 1 leader against the goblin trio may lose (question 184). C1b checks it in simulation.
- **Complexity.** `GameFlow`'s request switch is split in S1 or K2, whichever fails CA1502 first.
- **Constructor limits.** `GameFlow` and `SceneFactory` are at four parameters. The channel is created inside `SceneFactory`.
- **Audio hardware.** The exception for a missing device varies (`NoAudioHardwareException`, a missing OpenAL DLL, a failure inside `SoundEffect`), so A1b confirms by hand which one arrives.
- **Audio memory.** `SoundEffect` loads the whole file: fine for voice-overs.
- **Texture memory.** `TextureLibrary` keeps every still once loaded. That is small with the placeholders and large with real art (backlog line, added in K1).
- **Save format.** Unchanged: saves hold level, XP, HP, MP and equipment per member, plus the active order.
  - Old saves load. Members keep their saved level and get their class's stats at it.
  - Kamilia is dropped (`SaveGameRepair` reports it).
  - Saves that hold Ragnir, Omer or Lazarus need them to stay in content.
- **Content format.** `new-game.json`, `classes.json`, `party-members.json` and `items.json` change shape. Data's strict loading rejects old fields, so content and loader change in the same slice. The editor never writes these files.
