# FE2. A found item, a recruit and a quest's start, advance or completion sound as their line shows

Part of [the sound pass plan](../README.md). After FE1 and Phase 2. Decisions 421, 430, 432, 433, 437. Design: [design.md](../design.md), "Key decisions" 4 and 7.

- **FE2. A found item, a recruit and a quest's start, advance or completion sound as their line shows** (Game + content, about 235 lines; adds no view type)
  - **Goal:** the event steps that show a line about what happened sound on the update their line appears: a found item (`GiveItemStep`), a member joining (`RecruitStep`), a quest started or advanced (`StartQuestStep`, `AdvanceQuestStep`: one sound) and a quest completed (`CompleteQuestStep`: its own, decision 433). A found item whose receipt added nothing (`Added == 0`, the "couldn't carry it" line) is silent: nothing went through (decision 421). A dialogue and a rest's line add nothing (the typing and FE3's rest sound cover them).
  - **New slots (brief, longest length):** non-melodic single hits, never a tune (decision 432), heard over the field's track.
    - `effects/PlaceholderItemGot.wav`: a bright pluck or pop (a plucked string, a cork pop), "you got something"; not a chime, which is the heal's. 0.5 s.
    - `effects/PlaceholderRecruit.wav`: a warm single hit, a hand clasp or a soft low drum with a ring, a companion joining. 0.8 s.
    - `effects/PlaceholderQuestUpdated.wav`: a light notification: a page turned, or one soft bell strike. 0.6 s.
    - `effects/PlaceholderQuestCompleted.wav`: a fuller single strike than the update (a bell or gong hit with a longer ring), more resolved. 0.8 s.
  - **Changes:**
    - `Ui/UiSoundFiles`: a fourth group, `Field` ("in `AudioFolders.Effects`: what the field's events sound like"), with `ItemGot`, `Recruit`, `QuestUpdated`, `QuestCompleted`.
    - `Exploration/EventPresenter` (about 255 → 272): when `ShowPages` starts a step's reading, it asks the step's sound through a private switch over the page steps (design "Key decisions" 4; Dialogue and Rest give none; a give-item step only when `CurrentItemReceipt.Added` is above 0). The class summary gains the sentence.
    - `tests/Jrpg.Game.Tests/Ui/UiSoundFilesTests`: an `InlineData(typeof(UiSoundFiles.Field), AudioFolders.Effects)` row; `EveryGroup_IsCheckedAbove` expects 4.
    - Content: four manifest entries, the WAVs, the credits.
  - **Tests** (`EventPresenterStepSoundTests`, new; `EventPresenterTests` is 305; reuse the event builders of `EventPresenterQuestTests`):
    - `Update_GiveItemStepShows_AsksTheItemSound`
    - `Update_GiveItemNothingCarried_AsksNothing` (a full stack: the receipt's `Added` is 0)
    - `Update_GiveItemLineStillShowing_AsksNothing` (the update after: the per-update clear)
    - `Update_RecruitStepShows_AsksTheRecruitSound`
    - `Update_StartQuestStepShows_AsksTheQuestUpdatedSound`
    - `Update_AdvanceQuestStepShows_AsksTheQuestUpdatedSound`
    - `Update_CompleteQuestStepShows_AsksTheQuestCompletedSound`
    - `Update_DialogueStepShows_AsksNothing`
    - `ExplorationSceneSoundTests`: `Update_ChestEventShows_SceneAsksTheItemSound` (through what the flow reads; the typing test covers the ticks' branch)
  - **Report:** the four chosen pages, any CC-BY flagged.
  - **Verify by hand:** heard at Phase 4's listening (FE3's hand check). Before committing: each printed length within its brief; open the start town's chest: the game asks the sound as the "found" line appears.
