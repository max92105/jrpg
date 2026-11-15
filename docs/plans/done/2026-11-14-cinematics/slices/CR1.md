# CR1. The eight placeholder intros become cinematics

Content + tests, about 240 lines, mostly content. Needs CG3. Decision 621; design "Replacement mapping".

## Goal
Every leader's intro plays a cinematic: the eight placeholder intros (Omer, Zanyr, Lazarus, Jazog, Teelle, Zaruk,
Sarxian, Thaali) each get a cinematic `<member>_intro` holding what they show today, and their events become `[voice
Placeholder.wav, cinematic <member>_intro, warp]`: the `voice` step stays (decision 621), and since these cinematics
have no track and no voiced still, the voice plays on over them as today (decision 622). Nothing the player sees changes
but the skip (a 2 s hold now, decision 623).

## Steps
Mechanical; the slice writes it, no hand step.
1. `content/cinematics.json`: eight records `jazog_intro`, `lazarus_intro`, ..., each with the three stills
   `Placeholder1.png` to `Placeholder3.png`, 4 s each, today's captions ("Intro of Omer, still 1 (placeholder)." and so
   on), no voice, no track, no zoom; laid out as the writer writes them, sorted by id among `ragnir_intro`.
2. `content/map-layouts/intro.json`: in each of the eight cell events, the three `still` steps become one `cinematic`
   step naming its record; the `voice` step before it, the gates, cells and warps unchanged.
3. `RepositoryNewGameTests`: `..._EveryIntroPlaysAVoiceOrACinematic` (CG3) becomes
   `StartNew_RepositoryContent_EveryIntroPlaysItsCinematic` (each leader's intro holds a `CinematicStep` whose id the
   catalog has); `StartNew_ZanyrLeader_...` asserts the current step after the voice is a `CinematicStep` whose
   cinematic's first caption names Zanyr; `Restore_SaveLandingOnTheIntroMap_PlaysTheIntroAgain` asserts a
   `CinematicStep`.

## Tests
- The three re-pinned tests above.
- `ContentWriterRepositoryTests`: the identical-bytes test proves the hand-written records' layout.

## Verify by hand
New game with Sarxian, then with Thaali: black, three placeholder stills with their captions under the placeholder
voice, black, the start map; holding E for 2 s skips. Ragnir's intro is unchanged.

- **Done:** as planned, written by a script from today's still steps (each record's three stills, seconds and captions as the event held them), sorted by id with `ragnir_intro`; `ContentWriterRepositoryTests`' identical-bytes test passes on the new `cinematics.json`.
  CR1's review: the every-intro test checks each leader's cell plays `<member>_intro`, not any cinematic the catalog has.
