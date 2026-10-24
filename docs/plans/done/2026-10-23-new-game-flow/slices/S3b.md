# S3b. Choice screen look: the information area

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (screens).

Look at `docs/ui_ux_exemples/Menu6.webp` first: large art on the right, a name over rows of class information, descriptions in small text.

- **S3b. Choice screen look: the information area, portrait and stats** (Game, about 200 lines; decisions 158, 168, 169)
  - **Goal:** right of the list, using most of the screen:
    - the portrait, large, at the right, fading out toward the text;
    - the name, large;
    - the class in spaced caps under a section line, then the class description;
    - a hairline, then the summary;
    - the level 1 stats as small labels (`LV`, `HP`, `MP`, `ATK`, `DEF`) beside large numbers, as the Status page shows them.

    Moving the cursor fades the new information in over `ProfileFade`.
  - **New types (Game):** `NewGame/LeaderProfileView`, which draws the highlighted character's information.
    - It is built once, with every choice's texts measured and laid out, indexed like `LeaderChoiceMenu.Choices`. It takes over S2's prepared texts.
    - `Show(int choiceIndex)` restarts the fade.
    - `Update(TimeSpan delta)`.
    - `Draw(SpriteBatch batch, TextureLibrary textures)`.
  - **Changes:**
    - `Ui/UiTextures.Portrait(TextureLibrary textures, string portraitFile, int size)`: a portrait shrunk smoothly once per file and size, through the existing `ShrunkOnce`.
    - `NewGame/LeaderChoiceScene` hands the information area to the view and calls `Show` when the selection changes. S2's plain text goes.
    - `SectionTitle` and the section line are reused for the class heading.
  - **Tests (`LeaderProfileViewTests`; the fade is the view's only logic, read through a small internal `Opacity`):**
    - `Opacity_RightAfterShow_IsZero`
    - `Opacity_AfterProfileFade_IsOne`
  - **Verify by hand:**
    - Each character shows their own portrait, smooth, not pixelated (the three generated ones are flat placeholders).
    - Text never runs under the portrait.
    - Moving fast leaves no flicker.
    - Lots of empty space, with the art showing through.
- **Done, with deviations:**
  - The portrait's left 300 px fade in over 30 thin strips of rising opacity, drawn from the one smoothly resized texture (`UiTextures.Portrait`), so no second texture is baked.
  - The class heading draws its caps over the section line texture: `SectionTitle` takes a fixed message, and the class is per character. The info area runs from x 540 to 1160, and the portrait from 1200.
  - The stat labels are now the Status page's (Level, HP, MP, Attack, Defense) rather than S2's "Max HP" and "Max MP". `LeaderProfile` gains `PortraitFile`.
  - No screenshots (privacy rule): the look is the developer's hand check. The review asked for a test that moving the cursor shows the next character: `ShownProfile` (index and fade) is now read by `Update_Down_ShowsTheNextProfileFadingIn`. Tests: 1770 before, 1773 after.
