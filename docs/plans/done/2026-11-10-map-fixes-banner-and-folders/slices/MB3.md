# MB3. The banner's look

Game, about 140 lines raw, about 225 at 1.6x. Builds on MB2 and `.claude/rules/game.md`, "UI look". Look at `docs/ui-reference/` first (no shot shows an area banner: take the vibe of `battle-victory.jpg`'s serif title and `battle-party-panel.webp`'s spaced-caps labels beside large numbers).

## Goal
The banner looks like the reference's UI: the map's name in the title serif, under it a hairline and the level as a small spaced-caps "LV" beside a large number, on a dark band that fades out toward the left (no hard box), sliding in a little from the right while it fades in. A map without a level shows the name and hairline alone.

## Changes
- `Game/Exploration/MapBannerView`:
  - fonts from `text.Fonts.TitleFontAt(UiTheme.MapBannerTitlePixels)` and `FontAt(...)` for the label and number (ADR 0025 roles);
  - the label from `ui.json`: `status_level_short` ("Lv") drawn in spaced caps as `SheetRowLook` draws its labels (reuse it, or the spaced-caps helper the Status page uses; no new copy of the spacing rule); MB2's `map_banner_level` line goes if no longer read (remove the id and its constant, grep it);
  - the band: `UiTextures`' horizontal fade (the one `ColumnPanelView` or `SectionTitle` uses), right-aligned to the screen edge, sized from the measured text once per map;
  - the slide: the offset eased with `Ui/Easing` from `UiTheme.MapBannerSlide` to 0 over the fade in, read from a new `MapBanner.FadeInShare` (0 to 1, staying 1 after the fade in: `Opacity` alone can't tell fading in from fading out).
- `Ui/UiTheme`: `MapBannerTitlePixels`, `MapBannerLabelPixels`, `MapBannerNumberPixels`, `MapBannerSlide`, the band's color and padding (design.md tunables).

## Tests
- `MapBannerTests` (MB1's file): `FadeInShare_HalfwayThroughTheFadeIn_IsHalf`, `FadeInShare_DuringTheHold_IsOne`, `FadeInShare_DuringTheFadeOut_IsOne` (the slide doesn't run backwards on the way out).
- `MapBannerViewTests`: `Update_LongName_BandFitsTheWiderLine` (the band's width from the name when wider than the level line), through an internal `BandWidth` accessor.
- The look itself is the hand check (draw-only; F12 captures before and after).

- **Done, with deviations:** the label is `status_level_short` in spaced caps (upper-cased, `SheetRowLook.LabelSpacing`), so MB2's `map_banner_level` line, its id and `MapBannerTextPixels` are gone; the band is a `UiTextures.Gradient` of `UiTheme.MapBannerBand` (transparent at the left to dark at the right), running from the text's padding to the screen's right edge, so the view takes `UiTextures` and `Draw` takes the textures. `MapBanner.FadeInShare` reads `Easing.ShareOf`. The view's test accessors are `NameLine`, `LevelNumber` and `TextWidth` (the width the band's text part and the hairline cover; the band itself runs to the screen edge). From the review: the hairline is drawn with or without a level, the slide is rounded to whole pixels so the text and the lines move together. Not captured: the machine's freedom for captures wasn't settled this session; the look is the hand check's, captures welcome in this note.

## Verify by hand
1. F5, enter a map with an area level: F12 capture while it holds; compare with `docs/ui-reference/battle-victory.jpg` (serif title) and `battle-party-panel.webp` (label beside number): the name in the serif, "L V" in small spaced caps beside a large "3", the band fading to the left, nothing boxed.
2. A town without a level: the name and the hairline alone, the band narrower.
3. The longest display name in content ("Primeval Growth Crossroad"): fits on one line, the band wide enough.
4. The slide: the banner comes in from the right over its fade in and leaves in place.
5. Captures go to the slice's Done note (paths in `docs/captures`).
