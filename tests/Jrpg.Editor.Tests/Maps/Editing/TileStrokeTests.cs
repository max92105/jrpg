using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class TileStrokeTests
{
    private const int GroundZ = -5;
    private const int Grass = 1;
    private const int Sand = 2;

    // A 3x3 field of grass on one ground layer.
    private readonly MapDraft _draft = MapDraft.From(new MapDefinition(
        new MapLayout
        {
            Id = "field",
            Width = 3,
            Height = 3,
            Layers = [new TileLayer { Z = GroundZ, Rows = [[Grass, Grass, Grass], [Grass, Grass, Grass], [Grass, Grass, Grass]] }],
            Objects = [],
        },
        new MapSettings { Id = "field", DisplayName = "Field" }));

    [Fact]
    public void MoveTo_AtMapEdge_PaintsOnlyCellsOnTheMap()
    {
        var stroke = new TileStroke(_draft, new TileBrush(GroundZ, Sand, Size: 3));

        stroke.MoveTo(new GridCell(0, 0));

        Assert.Equal(4, stroke.ToEdit()!.Changes.Count);
        Assert.Equal(Sand, _draft.TileIdAt(GroundZ, new GridCell(1, 1)));
        Assert.Equal(Grass, _draft.TileIdAt(GroundZ, new GridCell(2, 2)));
    }

    [Fact]
    public void MoveTo_FastDrag_PaintsTheCellsInBetween()
    {
        var stroke = new TileStroke(_draft, new TileBrush(GroundZ, Sand, Size: 1));

        stroke.MoveTo(new GridCell(0, 1));
        stroke.MoveTo(new GridCell(2, 1));

        Assert.Equal(Sand, _draft.TileIdAt(GroundZ, new GridCell(1, 1)));
    }

    [Fact]
    public void ToEdit_CellCrossedTwice_RecordsItOnceWithItsOriginalTile()
    {
        var stroke = new TileStroke(_draft, new TileBrush(GroundZ, Sand, Size: 1));
        stroke.MoveTo(new GridCell(0, 0));
        stroke.MoveTo(new GridCell(1, 0));
        stroke.MoveTo(new GridCell(0, 0));

        var edit = stroke.ToEdit()!;

        Assert.Equal([new TileChange(new GridCell(0, 0), Grass, Sand), new TileChange(new GridCell(1, 0), Grass, Sand)], edit.Changes);
    }

    [Fact]
    public void MoveTo_EraserOnLayerWithoutTiles_ChangesNothing()
    {
        var stroke = new TileStroke(_draft, new TileBrush(-3, TileLayer.EmptyTileId, Size: 1));

        Assert.False(stroke.MoveTo(new GridCell(1, 1)));
        Assert.Null(stroke.ToEdit());
    }

    [Fact]
    public void MoveTo_Eraser_EmptiesTheCell()
    {
        var stroke = new TileStroke(_draft, new TileBrush(GroundZ, TileLayer.EmptyTileId, Size: 1));

        stroke.MoveTo(new GridCell(1, 1));

        Assert.Equal(TileLayer.EmptyTileId, _draft.TileIdAt(GroundZ, new GridCell(1, 1)));
    }
}
