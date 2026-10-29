# F1a. A Forge screen that opens and closes

Part of [the forge plan](../README.md). Decisions 353 (a screen a forge master opens, not a menu tab), 370. After M4. Was the first part of the draft's F1 (split on 2026-10-29 so the forge is reachable by hand from N1 on). ADR 0008 (scenes return requests). Design: [design.md](../design.md), "Structure and folder dependencies".

- **F1a. A Forge screen that opens over the map and Cancel closes** (Game + text, raw 90, about 145 lines at 1.6x: adds a view type)
  - **Goal:** a Forge screen exists: drawn over the map (the stack draws the map under it) as a dark layer with its title; Cancel asks to close it, once. Nothing lists yet (F1b), and nothing opens it in play until E1 (known gap: tests only).
  - **New types:**
    - `Game/Crafting/ForgeScene`: the Forge screen's input and its close request (an `IScene`); returns `ForgeClosed` on the first Cancel and nothing after. Takes the text (F1b adds the session).
    - `Game/Crafting/ForgeView`: draws the Forge screen; here a translucent dark layer over the map and the title (plain: the look is F1d).
    - `Game/Scenes/ForgeClosed`: the request the Forge returns to close.
  - **Changes:** text `forge_title` ("Forge"); `MessageIds.ForgeTitle`.
  - **Tests** (`ForgeSceneTests`, new file):
    - `Update_Cancel_RequestsCloseOnce` (two Cancels: one `ForgeClosed`, then null)
    - `Update_Confirm_RequestsNothing`
  - **Watch:** CA1812 (an internal class nothing in its assembly builds): `SceneFactory.CreateForge` lands in E1, its first caller; if the build flags `ForgeScene` here, it lands in this slice instead (design.md, "Risks").
  - **Verify by hand:** none until E1 (no way in yet).
