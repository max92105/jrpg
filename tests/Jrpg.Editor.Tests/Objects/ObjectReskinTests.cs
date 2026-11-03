using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectReskinTests
{
    // A 1 x 2 chest of the tests' own, with an outline and a zone, on tiles 1 and 2.
    private static readonly ObjectDefinition _chest = new()
    {
        Id = "chest",
        Width = 1,
        Height = 2,
        Category = "Village",
        CollisionPolygon = [new Point2D(0.1, 0.5), new Point2D(0.9, 0.5), new Point2D(0.9, 1), new Point2D(0.1, 1)],
        Tiles = [new ObjectTile(0, 0, 1), new ObjectTile(0, 1, 2)],
        AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5)]],
    };

    [Fact]
    public void Reskin_SameSize_ReplacesTheTilesKeepingIdAndCategory()
    {
        var cells = SheetCellRectangle.Between(new SheetCell("autumn", 3, 4), new SheetCell("autumn", 3, 5));

        var reskinned = ObjectReskin.Reskin(_chest, cells, cell => 100 + cell.Row);

        Assert.Equal([new ObjectTile(0, 0, 104), new ObjectTile(0, 1, 105)], reskinned.Tiles);
        Assert.Equal(("chest", "Village"), (reskinned.Id, reskinned.Category));
    }

    [Fact]
    public void Reskin_SameSize_KeepsTheOutlineAndZones()
    {
        var cells = SheetCellRectangle.Between(new SheetCell("autumn", 0, 0), new SheetCell("autumn", 0, 1));

        var reskinned = ObjectReskin.Reskin(_chest, cells, _ => 100);

        Assert.Same(_chest.CollisionPolygon, reskinned.CollisionPolygon);
        Assert.Same(_chest.AbovePlayerZones, reskinned.AbovePlayerZones);
    }

    [Fact]
    public void Reskin_OtherSize_TakesTheRectanglesWidthAndHeight()
    {
        var cells = SheetCellRectangle.Between(new SheetCell("autumn", 2, 2), new SheetCell("autumn", 4, 3));

        var reskinned = ObjectReskin.Reskin(_chest, cells, cell => (cell.Row * 10) + cell.Column);

        Assert.Equal((3, 2), (reskinned.Width, reskinned.Height));
        Assert.Equal(6, reskinned.Tiles.Count);
        Assert.Contains(new ObjectTile(2, 1, 34), reskinned.Tiles);
    }
}
