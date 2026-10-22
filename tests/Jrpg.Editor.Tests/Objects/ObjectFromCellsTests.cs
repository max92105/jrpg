using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectFromCellsTests
{
    // Cells (4, 1) to (6, 2) of forest; each cell's tile id is 100 + 10 x column + row.
    private static readonly SheetCellRectangle _cells = new("forest", 4, 1, 3, 2);

    [Fact]
    public void Build_ThreeByTwoCells_PlacesOneTilePerCell()
    {
        var definition = ObjectFromCells.Build("tree_4", "General", _cells, cell => 100 + (10 * cell.Column) + cell.Row);

        Assert.Equal(("tree_4", "General", 3, 2), (definition.Id, definition.Category, definition.Width, definition.Height));
        Assert.Equal(
            [new ObjectTile(0, 0, 141), new ObjectTile(1, 0, 151), new ObjectTile(2, 0, 161), new ObjectTile(0, 1, 142), new ObjectTile(1, 1, 152), new ObjectTile(2, 1, 162)],
            definition.Tiles);
    }

    [Fact]
    public void Build_AnyRectangle_HasNoOutlineAndNoZones()
    {
        var definition = ObjectFromCells.Build("tree_4", "General", _cells, _ => 1);

        Assert.Empty(definition.CollisionPolygon);
        Assert.Empty(definition.AbovePlayerZones);
    }
}
