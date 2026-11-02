# FE3. A warp from the map and a rest sound as their fade starts

Part of [the sound pass plan](../README.md). After FE2. Decisions 421, 430, 432, 434, 435, 437. Design: [design.md](../design.md), "Key decisions" 4 and 7.

- **FE3. A warp from the map and a rest sound as their fade starts** (Game + content, about 170 lines; adds no view type)
  - **Goal:**
    - **Warp:** a warp started from the map (a door, a map's edge) sounds as its fade to black starts (decision 434); a warp out of a cutscene's black (`StayBlackIntoAWarp`: the intro's nine) is silent.
    - **Rest:** a rest sounds as its fade to black starts: a free rest on its step's first update, an inn's stay on the Yes that paid (decision 435). The inn's question opening, its No and a Yes short of gold (the refusal's line) are silent (decision 421).
  - **New slots (brief, longest length):**
    - `effects/PlaceholderWarp.wav`: a soft "step through": a door's creak and close, or a muffled whoosh, neutral enough for a map's edge. 0.6 s (the fade out is 0.25 s, the black 0.08 s: it may ring into the new map).
    - `effects/PlaceholderRest.wav`: one calm, non-melodic hit with a long soft tail (a singing bowl, a low chime struck once), the night passing (decision 432). 0.8 s.
  - **Changes:**
    - `Ui/UiSoundFiles.Field`: `Warp`, `Rest`.
    - `Exploration/EventPresenter` (about 272 → 282): `ShowWarp` asks `Field.Warp` where it starts the fade; `ShowRest` compares `_rest.IsFading` before and after `Start`/`Answer` and asks `Field.Rest` when it turned true. The class summary gains the two sounds.
    - Content: two manifest entries, the WAVs, the credits.
  - **Tests** (`EventPresenterStepSoundTests`):
    - `Update_WarpFromTheMap_AsksTheWarpSound`
    - `Update_WarpHolding_AsksNothing` (a later update of the same warp)
    - `Update_WarpAfterACutscene_AsksNothing` (a still then a warp, as the intro)
    - `Update_FreeRestStarts_AsksTheRestSound`
    - `Update_InnQuestionOpens_AsksNothing`
    - `Update_YesAtTheInn_AsksTheRestSound`
    - `Update_YesAtTheInnShortOfGold_AsksNothing`
    - `Update_NoAtTheInn_AsksNothing`
  - **Report:** the two chosen pages, any CC-BY flagged.
  - **Verify by hand:** Phase 4's listening (decision 437): open the start town's chest (the pluck as the "found" line appears, not on the press); recruit a member (the start town's recruits); start and complete `placeholder_errand` (the completion reads as a bigger moment than the start; no repository event advances a quest, so the advance's sound is checked by its test only); a plain dialogue only ticks. Walk through the town's door into the house and back out, then off the map's edge into the forest: each warp sounds as the screen darkens. Start a new game: the intro's cutscene ends into the town without a warp sound. Rest at the town's inn (20 gold) with enough gold (the hit under the black), without (silent, the refusal line), and decline (silent). Each vetoed sound gets a shortlist of three page links; the picks replace them before Phase 5.
