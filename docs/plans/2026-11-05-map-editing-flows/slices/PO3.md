# PO3. The Placed object panel

Builds on design.md, "Key decisions" 10; PO1 and PO2's `PlacedObjectEditing`; decision 513.

## Goal
In the Objects tab, under the object list, a **Placed object** panel shows while exactly one placed object is selected (Ctrl + click): its uid (read only), an Object box listing every object, X, Y and Layer boxes, the problem line in red, the warning in amber, **Apply** (one Ctrl+Z step) and **Edit events...**, which opens the object's event dialog as a click in Events mode does. With none or several selected, the panel is hidden.

## New types
- `Editor/Objects/PlacedObjectPanel` (`.xaml` + `.xaml.cs`, a `UserControl`): the panel showing `PlacedObjectEditing` under the Objects panel's list.
  - A titled `GroupBox` ("Placed object"), `Visibility` bound to `HasObject`; rows: Uid (`TextBlock`), Object (`ComboBox`, `IsEditable="False"`, `ItemsSource="{Binding ObjectIds}"`, `SelectedItem="{Binding ObjectId}"`), X and Y (`TextBox`es, `UpdateSourceTrigger=PropertyChanged`), Layer (`TextBox`), the problem and warning lines (the main window's `ErrorLine` and `WarningLine` styles, from `MainWindowStyles.xaml`, MS0), a button row: Apply (`Command="{Binding ApplyCommand}"`) and Edit events... (raises a bubbling routed event `EditEventsRequested`, as WP3's step editor does). Enter in a box applies through `<UserControl.InputBindings><KeyBinding Key="Enter" Command="{Binding ApplyCommand}" /></UserControl.InputBindings>` on the panel, not `IsDefault` (a default button would answer Enter anywhere in the main window).
  - Code-behind: the routed event only.

## Changes
- `Shell/MainWindow.xaml`, the Objects tab: a `DockPanel` holding `<objects:PlacedObjectPanel DockPanel.Dock="Bottom" DataContext="{Binding Placing.Editing}" objects:PlacedObjectPanel.EditEventsRequested="OnPlacedObjectEditEvents" />` over the `ObjectBrowserView` (+4 lines, ends about 290).
- `Events/EventEditingViewModel`: `public EventListViewModel? StartEditing(string uid)`: the event dialog's state for the placed object with that uid on the open map; null when it isn't there (shares `StartEditingAt`'s construction through a private `EventListFor(PlacedObject)`).
- `Shell/MainWindow.EventEditing.cs`: `OnPlacedObjectEditEvents` opens the dialog for `Placing.Editing.Uid` (the same window and Apply as a click in Events mode, through the method the click uses, so WP4's Go to works from it too).
- `Shell/ControlsHelpWindow.xaml`, Objects: "Placed object (under the list, with one object selected): change its object, X, Y or layer and press Apply (one Ctrl+Z step); its uid and events stay. Edit events... opens its events."

## Tests
- `EventEditingViewModelTests: StartEditing_UidOnTheMap_StartsOnItsEvents`.
- `EventEditingViewModelTests: StartEditing_UnknownUid_IsNull`.

Size: XAML about 55, code-behind 10, main window 12, the event editing method 10; 2 tests (about 25): about 115.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `PlacedObjectPanel` (the selected placed object's object, position, layer and events, under the Objects panel's list).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Objects tab. With nothing selected, the list fills the tab as before.
2. Ctrl + click a chest on the map: the **Placed object** panel appears under the list with its uid, object, X, Y and layer.
3. Type a new X (say 10.5) and press Enter (or **Apply**): the chest jumps there. Press Enter in the map list's filter box afterwards: it opens a map, the panel applies nothing. Ctrl+Z: it goes back, and the panel shows the old X.
4. In Object, choose the opened chest; press Apply: the picture changes, the uid stays; click **Edit events...**: the same events are there. If one of them waits for the closed chest, the amber line said so before Apply.
5. Type `abc` in Y: the red line says what's wrong and Apply is greyed.
6. Type the z of another layer in Layer (the layer list shows each z) and Apply; choose that layer in the tool bar's Layer box and tick **Dim other layers**: the chest stays bright with that layer.
7. Ctrl + drag over two objects: the panel hides.
8. Ctrl+S, start the game: the chest is where the panel put it, as the object chosen.
