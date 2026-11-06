using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Objects > New object from map selection: the composer on the map's cells (decision 514), and the cells replaced by
/// the object on Save (decision 515).
/// </summary>
internal sealed partial class MainViewModel
{
    private MapSelectionSource? _mapSelectionSource;

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

        var taken = MapSelectionCanvas.TakenLayersOf(layout, cells, baseZ);
        _mapSelectionSource = new MapSelectionSource(cells, baseZ, taken);
        return Composing.StartNewFromMap(MapSelectionCanvas.Of(layout, cells, baseZ, Catalog!.Tiles), MapSelectionCanvas.DrawingWarningOf(taken, baseZ));
    }

    /// <summary>
    /// The composer's Save from the map: the object as one catalog step, then, when ticked, the cells it kept erased
    /// from the base layer up and the object placed there, as one map step, and the object selected in Objects mode.
    /// </summary>
    /// <returns>False, nothing changed, while the composer has a problem.</returns>
    public bool TryApplyObjectFromMap(ObjectComposerViewModel composer)
    {
        if (!Composing!.TryApply(composer))
        {
            return false;
        }

        if (composer.IsReplacingMapCells && _mapSelectionSource is { } source && CurrentMap is { } map)
        {
            var definition = Catalog!.Objects[composer.Naming.Id];
            var placed = new PlacedObject
            {
                Uid = PlacedObjectUids.Next(definition.Id, [.. map.Map.Layout.Objects.Select(onMap => onMap.Uid)]),
                ObjectId = definition.Id,
                Position = new Point2D(source.Cells.TopLeft.X, source.Cells.TopLeft.Y),
                Z = source.BaseZ,
            };
            map.Do(new ReplaceCellsWithObjectEdit(ErasedCells(map.Map.Layout, source, definition), placed));
            Placing!.Select([placed.Uid]);
            Mode = EditingMode.Objects;
            Painting!.ClearMapSelection();
        }

        return true;
    }

    // From the base layer up only, so the ground under the base never leaves a hole (decision 515). The base z chosen
    // in the tool bar may have no layer on the map yet (a z nothing is painted on), so only the map's layers are read.
    private List<SetTilesEdit> ErasedCells(MapLayout layout, MapSelectionSource source, ObjectDefinition definition)
    {
        var kept = definition.Tiles.Select(tile => (new CellOnLevel(new GridCell(tile.X, tile.Y), tile.Level), SheetCell.Of(Catalog!.Tiles[tile.TileId]))).ToHashSet();
        var levels = source.TakenLayers.Where(taken => taken.Level >= 0).ToDictionary(taken => taken.Z, taken => taken.Level);
        return
        [
            .. layout.Layers
                .Where(layer => levels.ContainsKey(layer.Z))
                .Select(layer => new SetTilesEdit(layer.Z, ErasedOn(layer, levels[layer.Z], source.Cells, kept)))
                .Where(erase => erase.Changes.Count > 0),
        ];
    }

    // A map cell goes only where the saved object still holds its sheet cell on that layer's level, so a cell erased or
    // repainted in the composer stays on the map.
    private List<TileChange> ErasedOn(TileLayer layer, int level, MapCellRectangle cells, HashSet<(CellOnLevel, SheetCell)> kept) =>
        [.. cells.Cells
            .Select(cell => (Cell: cell, TileId: layer.TileIdAt(cell)))
            .Where(onMap => onMap.TileId != TileLayer.EmptyTileId)
            .Where(onMap => kept.Contains((new CellOnLevel(new GridCell(onMap.Cell.X - cells.TopLeft.X, onMap.Cell.Y - cells.TopLeft.Y), level), SheetCell.Of(Catalog!.Tiles[onMap.TileId]))))
            .Select(onMap => new TileChange(onMap.Cell, onMap.TileId, TileLayer.EmptyTileId))];

    // What the composer opened from: the replacement reads it on Save, so the levels are the ones the canvas had.
    private sealed record MapSelectionSource(MapCellRectangle Cells, int BaseZ, IReadOnlyList<(int Z, int Level)> TakenLayers);
}
