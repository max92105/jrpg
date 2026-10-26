# TS. Menu pages advance with the menu's time

Part of [the UI second pass plan](../README.md). Decision 262. Design: [design.md](../design.md), "Key decisions" (pages get time through `Advance`). After MO1.

- **TS. Menu pages advance with the menu's time** (Game, about 130 lines)
  - **Goal:** a menu page can move over time (glide its selection, bob its pointer), like the tabs, without changing how pages read input: `GameMenuScene` gives the open page the update's time before its input.
  - **Changes (Game):**
    - `Menus/IMenuPage`: `void Advance(TimeSpan delta)`: "the time since the last update, before `Update`; a page with nothing moving does nothing". `Update(InputFrame)` keeps its signature, so the 170-odd test calls don't change.
    - Every page implements it: `PartyPage`, `StatusPage`, `EquipmentPage`, `InventoryPage`, `SavePage`, `LoadPage`, `OptionsPage`, `SystemPage` (an empty body for now, with no comment needed beyond the interface's), and the test pages (`CountingMenuPage`, `TestSystemPages`).
    - `Menus/GameMenuScene.Update`: calls the open page's `Advance(delta)` before its `Update`, during the page transition too.
    - `Battle/PointerBob` moves to `Ui/PointerBob` (the next slices bob the list pointer; `Ui` can't read `Battle`). It takes the distance as a parameter: `OffsetAt(TimeSpan elapsed, float distance)`; the target pointer passes its 8 px. Its tests move with it (`PointerBobTests`, if any; else none).
  - **Tests** (`GameMenuSceneTests`): `Update_PageOpen_AdvancesThePageBeforeItsInput` (`CountingMenuPage` records the order of calls).
  - **Verify by hand:** nothing visible changes; the battle's target pointer still bobs 8 px.
