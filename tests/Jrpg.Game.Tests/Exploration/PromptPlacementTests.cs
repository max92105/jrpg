using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class PromptPlacementTests
{
    private static readonly Point _boxSize = new(60, 30);

    [Fact]
    public void BoxFor_TwoCellObject_CentersOnItsWidth()
    {
        var box = PromptPlacement.BoxFor(PlacedAt(1, 3), DefinitionOfWidth(2), Vector2.Zero, _boxSize);

        Assert.Equal(48 + 48 - 30, box.X);
        Assert.Equal(_boxSize, box.Size);
    }

    [Fact]
    public void BoxFor_CameraMoved_ShiftsByTheOrigin()
    {
        var atRest = PromptPlacement.BoxFor(PlacedAt(1, 3), DefinitionOfWidth(1), Vector2.Zero, _boxSize);

        var moved = PromptPlacement.BoxFor(PlacedAt(1, 3), DefinitionOfWidth(1), new Vector2(20, 10), _boxSize);

        Assert.Equal(atRest.Location - new Point(20, 10), moved.Location);
    }

    [Fact]
    public void BoxFor_ObjectAtRow3_BottomIs6PixelsAboveItsTop()
    {
        var box = PromptPlacement.BoxFor(PlacedAt(1, 3), DefinitionOfWidth(1), Vector2.Zero, _boxSize);

        Assert.Equal((3 * MapView.CellPixels) - 6, box.Bottom);
    }

    private static PlacedObject PlacedAt(double x, double y) =>
        new() { Uid = "thing", ObjectId = "thing", Position = new Point2D(x, y), Z = MapLayers.PlayerZ, Events = [] };

    private static ObjectDefinition DefinitionOfWidth(int width) => new()
    {
        Id = "thing",
        Width = width,
        Height = 1,
        CollisionPolygon = [],
        Category = "test",
        Tiles = [],
        AbovePlayerZones = [],
    };
}
