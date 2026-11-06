using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

/// <summary>
/// Map cells replaced by an object as one step (decision 515), on a 2 x 1 map of two layers built in code.
/// </summary>
public sealed class ReplaceCellsWithObjectEditTests
{
    private static readonly PlacedObject _box = new() { Uid = "box_1", ObjectId = "box", Position = new Point2D(0, 0), Z = 1 };

    private readonly MapDraft _draft = MapDraft.From(new MapDefinition(
        new MapLayout { Id = "glade", Width = 2, Height = 1, Layers = [new TileLayer { Z = 0, Rows = [[1, 1]] }, new TileLayer { Z = 1, Rows = [[2, 3]] }], Objects = [] },
        new MapSettings { Id = "glade", DisplayName = "Glade" }));

    private readonly ReplaceCellsWithObjectEdit _edit = new(
        [new SetTilesEdit(0, [new TileChange(new GridCell(0, 0), 1, 0)]), new SetTilesEdit(1, [new TileChange(new GridCell(0, 0), 2, 0), new TileChange(new GridCell(1, 0), 3, 0)])],
        _box);

    // Layer 1 left empty is dropped from the saved map, as any empty layer.
    [Fact]
    public void Apply_TwoLayers_ErasesTheirCellsAndPlacesTheObject()
    {
        _edit.Apply(_draft);

        var map = _draft.ToDefinition().Layout;
        Assert.Equal([[0, 1]], map.Layers[0].Rows);
        Assert.Equal([0], map.Layers.Select(layer => layer.Z));
        Assert.Equal(["box_1"], map.Objects.Select(placed => placed.Uid));
    }

    [Fact]
    public void Revert_PutsTheCellsBackAndRemovesTheObject()
    {
        _edit.Apply(_draft);

        _edit.Revert(_draft);

        var map = _draft.ToDefinition().Layout;
        Assert.Equal([[1, 1]], map.Layers[0].Rows);
        Assert.Equal([[2, 3]], map.Layers[1].Rows);
        Assert.Empty(map.Objects);
    }
}
