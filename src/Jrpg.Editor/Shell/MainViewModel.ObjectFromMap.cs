using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Shell;

/// <summary>Objects > New object from map selection: the composer on the map's cells (decision 514).</summary>
internal sealed partial class MainViewModel
{
    /// <summary>
    /// Whether the menu entry is on: once the content loaded; a selection's problem goes to the status bar.
    /// </summary>
    public bool CanStartObjectFromMap => Composing is not null;

    /// <summary>
    /// The composer on the canvas the map selection makes around the layer chosen in the tool bar; null, with the
    /// reason in the status bar, when there is no selection or it can't make an object.
    /// </summary>
    public ObjectComposerViewModel? StartObjectFromMap()
    {
        if (Composing is null || CurrentMap is not { } shown)
        {
            return null;
        }

        if (Painting!.SelectedMapCells is not { } cells)
        {
            CursorStatus = "Ctrl + drag a rectangle of cells on the map first (Tiles tab).";
            return null;
        }

        var layout = shown.Map.Layout;
        var baseZ = Layer.Z;
        if (MapSelectionCanvas.ProblemOf(layout, cells, baseZ) is { } problem)
        {
            CursorStatus = problem;
            return null;
        }

        var warning = MapSelectionCanvas.DrawingWarningOf(MapSelectionCanvas.TakenLayersOf(layout, cells, baseZ), baseZ);
        return Composing.StartNewFromMap(MapSelectionCanvas.Of(layout, cells, baseZ, Catalog!.Tiles), warning);
    }
}
