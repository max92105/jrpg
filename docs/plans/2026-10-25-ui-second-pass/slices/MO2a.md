# MO2a. The selection glides and the pointer bobs: tabs and title

Part of [the UI second pass plan](../README.md). Decision 262. Design: [design.md](../design.md), "Tunables and placeholders" (`SelectionGlide`, pointer bob). After TS and T3.

Developer: "make the animation smooth."

- **MO2a. The selection glides and the pointer bobs: the menu tabs and the title** (Game, about 210 lines)
  - **Goal:**
    - Moving the cursor on the game menu's tabs or the title's entries, the highlight (the selected tab's glow and line; the title's selection band and pointer) glides to the new row over 0.08 s, easing out, instead of jumping. Labels change color at once.
    - The list pointer bobs gently, 4 px left and back over 0.9 s, everywhere `MenuPointer` is drawn with a time (this slice: the title; MO2b: the pages).
  - **New types:** Game `Ui/SelectionGlide` (internal sealed): where a list's highlight is while it glides from the row it left to the selected one.
    - `void Select(int row)` (from where the highlight is now, so a quick second move glides on from mid-way), `void Advance(TimeSpan delta)`, `float Position` (in rows).
  - **Changes (Game):**
    - `Ui/MenuPointer.Draw` takes a bob offset (`PointerBob.OffsetAt(elapsed, 4)`), computed by the caller from its own clock; callers without a clock yet pass 0.
    - `Menus/GameMenuScene`: a `SelectionGlide` follows `_tabs.Selected`; `DrawTab` draws the selected tab's texture and line at the glide's position.
    - `Title/TitleScene`: the band and pointer at the glide's position; the pointer bobs.
    - A move that wraps (last to first) jumps rather than gliding through the list.
  - **Tests** (`SelectionGlideTests`):
    - `Position_AtStart_IsTheFirstSelectedRow`
    - `Position_RightAfterSelect_IsThePreviousRow`
    - `Position_AfterTheGlide_IsTheSelectedRow`
    - `Position_HalfwayThroughTheGlide_IsBetweenTheRows`
    - `Select_DuringAGlide_StartsFromTheCurrentPosition`
    - `JumpTo_DuringAGlide_IsAtTheRowAtOnce` (`void JumpTo(int row)`, which the callers use when the cursor wraps; two methods, no flag argument)
  - **Verify by hand:** hold Down on the tabs and on the title: the glow travels smoothly, never lags more than a row; the title's diamond bobs gently.
