using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class MapDrawListTests
{
    private static readonly Dictionary<string, ObjectDefinition> _objects = new()
    {
        ["tree"] = new()
        {
            Id = "tree",
            Width = 4,
            Height = 4,
            CollisionPolygon = [],
            Category = "General",
            Tiles = [new ObjectTile(0, 0, 1)],
            AbovePlayerZones = [],
        },
    };

    private readonly MapDrawList _drawList = new();

    [Fact]
    public void Rebuild_SpriteLowerOnScreen_DrawnLater()
    {
        // The second tree is placed first but stands lower on screen, so it's drawn over the first.
        var layout = Layout(layers: [], Tree(y: 6, z: 1), Tree(y: 2, z: 1));

        _drawList.Rebuild(layout, _objects, new Point2D(0, 20));

        Assert.Equal(
            [(DrawableKind.Object, 1), (DrawableKind.Object, 0), (DrawableKind.Player, 0)],
            _drawList.Drawables.Select(drawable => (drawable.Kind, drawable.Index)));
    }

    [Fact]
    public void Rebuild_PlayerBetweenObjectsOnItsLayer_SortsByFeet()
    {
        // The player's feet are at 7.5: in front of the first tree's base (2 + 4), behind the second's (6 + 4).
        var layout = Layout(layers: [], Tree(y: 2, z: MapLayers.PlayerZ), Tree(y: 6, z: MapLayers.PlayerZ));

        _drawList.Rebuild(layout, _objects, new Point2D(3, 7));

        Assert.Equal(
            [(DrawableKind.Object, 0), (DrawableKind.Player, 0), (DrawableKind.Object, 1)],
            _drawList.Drawables.Select(drawable => (drawable.Kind, drawable.Index)));
    }

    [Fact]
    public void Rebuild_PlayerFeetOnObjectBase_PlayerDrawnInFront()
    {
        // The tree's base is -0.5 + 4 = 3.5, exactly the player's feet; the tie must not depend on the unstable sort.
        var layout = Layout(layers: [], Tree(y: -0.5, z: MapLayers.PlayerZ));

        _drawList.Rebuild(layout, _objects, new Point2D(0, 3));

        Assert.Equal(
            [(DrawableKind.Object, 0), (DrawableKind.Player, 0)],
            _drawList.Drawables.Select(drawable => (drawable.Kind, drawable.Index)));
    }

    [Fact]
    public void Rebuild_LayersAndObjects_FollowZ()
    {
        // The legacy start map: ground at -5 and -4, objects at -3, so the player stands over them; a roof layer at 5 covers all.
        var layout = Layout(layers: [-5, -4, 5], Tree(y: 0, z: -3));

        _drawList.Rebuild(layout, _objects, new Point2D(0, 0));

        Assert.Equal(
            [(DrawableKind.Layer, 0), (DrawableKind.Layer, 1), (DrawableKind.Object, 0), (DrawableKind.Player, 0), (DrawableKind.Layer, 2)],
            _drawList.Drawables.Select(drawable => (drawable.Kind, drawable.Index)));
    }

    [Fact]
    public void Rebuild_Twice_KeepsOneEntryPerThing()
    {
        var layout = Layout(layers: [-5], Tree(y: 0, z: -3));

        _drawList.Rebuild(layout, _objects, new Point2D(0, 0));
        _drawList.Rebuild(layout, _objects, new Point2D(0, 1));

        Assert.Equal(3, _drawList.Drawables.Count);
    }

    private static PlacedObject Tree(double y, int z) =>
        new() { Uid = $"tree@{y}", ObjectId = "tree", Position = new Point2D(0, y), Z = z };

    private static MapLayout Layout(int[] layers, params PlacedObject[] objects) => new()
    {
        Id = "test_map",
        Width = 10,
        Height = 20,
        Layers = [.. layers.Select(z => new TileLayer { Z = z, Rows = [] })],
        Objects = objects,
    };
}
