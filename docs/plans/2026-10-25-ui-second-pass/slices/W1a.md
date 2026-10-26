# W1a. The untitled framed window, first used by the battle's action name

Part of [the UI second pass plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (untitled window, window colors, action name box). After C1. Look at `docs/ui-reference/battle-damage-numbers.webp` (the "Attack" box), `dialog-confirm.webp` (the frame), the index's "shared pieces" section and S0's battle captures first.

The titled variant and its separator are W1b's (the devil's advocate's split).

- **W1a. The untitled framed window, first used by the battle's action name** (Game, about 200 lines with the 1.6x for a new view)
  - **Goal:** a shared window in the reference's frame language (a dark translucent body; an outer hairline and an inner one inset with open corners), and the battle's action name shown in a small one instead of the band.
  - **New types:**
    - Game `Ui/FramedWindowLayout` (internal readonly record struct): where a framed window's frame and body sit around a body of a given size.
      - `static FramedWindowLayout Untitled(Point center, Point bodySize)`; `Rectangle Frame`, `Rectangle InnerFrame`, `Rectangle Body`.
    - Game `Ui/FramedWindowView` (internal sealed): draws a framed window from its layout: body, outer hairline (`Ui/Outline`), the inner one as four lines stopping short of the corners. The caller writes its text into `Body`. Nothing new is baked.
  - **Changes (Game):**
    - `Ui/UiTheme` (shared part): `WindowBody`, `WindowLine`.
    - `Battle/BattleBannerView`: the message in an untitled window, centered 150 px from the top, at least 300 px wide; the layout is rebuilt only when the text changes (it is measured then).
    - `docs/plans/backlog.md`: the "banner band drawn twice" line now says only `BattleScene.DrawEscaped` draws the band, for the map banner plan.
    - `docs/map.md`, Ui line: `FramedWindowLayout`, `FramedWindowView`.
  - **Tests** (`FramedWindowLayoutTests`):
    - `Untitled_Frame_IsTheBodyPlusPaddingAroundTheCenter`
    - `Untitled_Body_IsInsideTheInnerFrame`
    - `Untitled_InnerFrame_IsInsetFromTheFrame`
  - **Verify by hand:** in a battle, each action's name shows in a small framed box near the top, fading like the old band; long names widen it; it doesn't cover the turn order. F12, compare with the before capture and `battle-damage-numbers.webp`.
