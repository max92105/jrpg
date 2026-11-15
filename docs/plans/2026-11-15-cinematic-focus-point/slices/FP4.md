# FP4. Click to set, marker, Clear focus, Play about it

Editor, about 140 lines (about 40 of them tests; the rest XAML, code-behind and the converter). Needs FP3 (and FP2 for
step 6 of the hand check). Decisions 747, 753, 755. Design: "Key decisions", "Replacement mapping", "Risks" (XAML).

## Goal
In Database > Cinematics..., a click on the chosen still's picture sets its focus where the click lands; a ring with a
cross marks it over the picture; the line under the zoom boxes names it, with **Clear focus** beside it; **Play** zooms
each still about its focus, as the game does. While Play runs, a click on the picture does nothing.

## New types
- `Editor/Cinematics/FocusMarkerMarginConverter` (internal sealed, `IValueConverter`): a still's focus to the marker's
  margin in the preview frame.
  - `public const double MarkerSize = 16;` (a double: the XAML binds the marker's `Width` and `Height` to it with
    `x:Static`, and WPF refuses an int there as the window opens).
  - `Convert`: a `StillFocus` gives `new Thickness(x share * CinematicCatalogWindow.PreviewWidth - MarkerSize / 2,
    y share * PreviewHeight - MarkerSize / 2, 0, 0)` (the shares from `StillFocus.Share`); anything else gives
    `new Thickness()` (the marker is hidden then through `HasFocus`). `ConvertBack` throws `NotSupportedException`, as
    `CinematicPictureConverter`'s does.

## Changes
- `Editor/Cinematics/CinematicCatalogWindow.xaml` (156 lines, about 186 after):
  - `Window.Resources`: `<local:FocusMarkerMarginConverter x:Key="FocusMarkerMargin" />`.
  - Under the "Zoom from % / to %" row, a `FieldRow`: a muted `TextBlock` bound to `FocusText`, then a **Clear focus**
    button (`Command="{Binding ClearFocusCommand}"`, `ToolTip="The zoom heads to the center again"`).
  - In the 480 x 270 frame, the chosen still's layer becomes a `Grid` (`Background="Transparent"` so an empty frame
    takes the click too, `Cursor="Cross"`, `MouseLeftButtonDown="OnStillPictureClicked"`,
    `ToolTip="Click to choose where the zoom heads"`) holding today's "No picture to show" text and `Image`, and the
    marker: a `Grid` of `MarkerSize` with `HorizontalAlignment="Left"`, `VerticalAlignment="Top"`,
    `Margin="{Binding Focus, Converter={StaticResource FocusMarkerMargin}}"`, `Visibility` from `HasFocus` through
    `VisibleWhenTrue`, `IsHitTestVisible="False"` (a click on it moves the point), drawn as an `Ellipse` and two short
    `Line`s in `FocusMarkBrush` over the same in `WindowBackgroundBrush`, 1 px wider (decision 755).
  - Play's layer (the existing black `Grid` near line 132, which fills the frame exactly): no new grid. The
    `RenderTransformOrigin="{Binding ZoomOrigin}"` and the `ScaleTransform` (`ScaleX`, `ScaleY` bound to `Scale`) go
    on that black `Grid`; the `Image`'s own `RenderTransformOrigin` and `Image.RenderTransform` are removed (the image
    can be larger than the frame under `UniformToFill`, so its own origin isn't the frame's). Scaling the black grid
    at 100 % or more still covers the clipped frame. Play's layer stays over the still's and takes any click while it
    shows, so no handler runs during Play (decision 753).
  - The comment over Play's layer: "faded and zoomed about its focus by Core's rules, as the game draws it".
- `Editor/Cinematics/CinematicCatalogWindow.xaml.cs` (78 lines): `OnStillPictureClicked(object sender,
  MouseButtonEventArgs e)`: the position in the clicked grid divided by `PreviewWidth` and `PreviewHeight`, passed to
  the chosen still's `CinematicStillRow.SetFocusAt` (the grid's `DataContext`; the still panel is disabled with no
  still chosen, so a click can't arrive then: a cast, no guard). The class summary names the click.
- `Editor/Shell/EditorTheme.xaml`: `<SolidColorBrush x:Key="FocusMarkBrush" Color="#FFD45A" />` beside the other
  role brushes.
- `docs/map.md`, the Editor `Cinematics` row: a click on the chosen still's picture sets its focus, marked by a ring
  (`FocusMarkerMarginConverter`), **Clear focus** removes it, Play zooms about it (`CinematicPreview.ZoomOrigin`).
- `Editor/Shell/ControlsHelpWindow.xaml` (the F1 list, 114 lines; it has no Cinematics entry yet): a "Cinematics
  window" heading after "Shapes window", in the same `Heading` and `Lines` styles, with one line: "Click the chosen
  still's picture: its zoom heads to that point (a gold ring marks it). Clear focus: back to the center. Play shows the
  zoom as the game does."

## Tests
`FocusMarkerMarginConverterTests` (new, `Editor.Tests/Cinematics`; Glob found none), foci built in the test:
- `Convert_FocusAtTheCenter_CentersTheMarker` ((50, 50) gives a margin of 232, 127).
- `Convert_FocusAtTheTopLeft_CentersTheMarkerOnTheCorner` ((0, 0) gives -8, -8).
- `Convert_FocusAtTheBottomRight_CentersTheMarkerOnTheCorner` ((100, 100) gives 472, 262).
- `Convert_NoFocus_IsNoMargin`.

No window test (editor tests never open a window): the XAML is the hand check's, and
`.claude/scripts/open-editor-windows.ps1` opens the Cinematics window (already in its default `-Windows` list; no new
window to add) with no `ERROR` line.

## Verify by hand
**The implementer** runs step 8 (`open-editor-windows.ps1`, no `ERROR` line) and may take a screenshot of the
Cinematics window with the window open on a still; it never clicks **OK** with a typed zoom or point on the real intro
and never edits `content/cinematics.json`. **Steps 1-7 are the developer's**, left unticked with a Done note saying
so.

1. `dotnet run --project src/Jrpg.Editor`. **Database > Cinematics...**; choose **ragnir_intro** in the left list, then
   **1. ragnir_intro/1_happy_life.png, 4 s** in **Stills, in order**.
2. The line under the zoom boxes reads "Zoom heads to the center: click the picture to choose a point"; **Clear focus**
   is greyed; the pointer over the picture is a cross.
3. Type **100** in **Zoom from %** and **150** in **to %**. Click the father's face in the picture: a gold ring with a
   cross sits where you clicked; the line reads "Zoom heads to 2x %, 2x %" (about 25 %, 25 %); **Clear focus** is lit.
   Click elsewhere (on the ring itself too): the ring moves there.
4. Click **Play**: the still grows toward the point, the point staying where it is in the frame, no black edge. While
   it plays, click the picture: nothing changes (the ring and the line keep their place once it stops). Click **Stop**.
5. Click **Clear focus**: the ring leaves, the line says the center; **Play** grows from the center as before.
6. Set a point again, click **OK**, **File > Save all**; run the game as FP2's hand check does (New Game, Ragnir): the
   first still grows toward the same point as in the preview. Then keep it (it is your intro) or set it as you want
   it; to drop it, reopen the window, **Clear focus**, **OK** and save, or `git checkout content/cinematics.json`.
7. Reopen the window, set a point on a still, press **Cancel**: reopened, the still has no point (the session dropped).
8. After a build, `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1`: no
   `ERROR` line for the Cinematics window.
