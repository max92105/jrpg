# QE4. The Quests window

Part of [the quests plan](../README.md). After QE3. Decision 394. `.claude/rules/editor.md` (MVVM, the dark theme's `BasedOn` styles, a visible control for every feature). Design: [design.md](../design.md), "Structure and folder dependencies" (Editor `Shell`).

- **QE4. The Quests window** (Editor, about 160 lines)
  - **Goal:** a "Quests..." entry in the editor's menu bar opens a window: the quests on the left with a New quest button; on the right the chosen quest's id (editable while new), title, description (several lines), kind, and its stages with Add, Remove, Up and Down and the chosen stage's id and text; the problem line under them; OK applies (refused while there is a problem), Cancel drops the session. Ctrl+Z undoes the whole session; Ctrl+S saves `quests.json` with the map and the other catalogs.
  - **New types:**
    - `Editor/Quests/QuestCatalogWindow` (XAML and code-behind): the window over a `QuestCatalogViewModel`; `EditorWindow` style, the theme's combo box and scroll bar templates; code-behind only for OK and Cancel, as `ObjectEventsWindow`.
  - **Changes:**
    - `Editor/Shell/MainWindow.xaml`: the menu entry.
    - `Editor/Shell/MainWindow.QuestEditing.cs` (new partial, as `.EventEditing` and `.ObjectEditing`; `MainWindow.xaml.cs` is at 547): opens the window from `QuestEditing.Start()` and calls `Apply` on OK.
    - `ControlsHelpWindow`: no new gesture (a menu entry and buttons), nothing to add.
  - **Tests:** none new (window code); the rules are QE2's and QE3's. The hand check is the test.
  - **Verify by hand:** open Quests..., add a quest with two stages, OK: open an object's events, the new quest and its stages are in the step and condition boxes without a restart; Ctrl+Z: the quest is gone from the boxes; Ctrl+Y, use it in an event, Ctrl+Z on the quest: refused, naming the map; Ctrl+S: `quests.json` holds it, the game starts with it. A repeated id or an empty stage text: OK refuses and the line says why. Capture the window.
