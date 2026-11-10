using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class MapDrawOrderTests
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
        ["canopy_tree"] = new()
        {
            Id = "canopy_tree",
            Width = 4,
            Height = 4,
            CollisionPolygon = [],
            Category = "General",
            Tiles = [new ObjectTile(0, 0, 1)],
            AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(0.5, 0.5)]],
        },
    };

    private readonly List<MapDrawItem> _items = [];

    [Fact]
    public void FillWithPlayer_SpriteLowerOnScreen_DrawnLater()
    {
        // The second tree is placed first but stands lower on screen, so it's drawn over the first.
        var layout = Layout(layers: [], Tree(y: 6, z: 1), Tree(y: 2, z: 1));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(0, 20));

        Assert.Equal([(MapDrawItemKind.ObjectBody, 1), (MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0)], KindsAndIndexes());
    }

    [Fact]
    public void FillWithPlayer_PlayerBetweenObjectsOnItsLayer_SortsByFeet()
    {
        // The player's feet are at 7.5: in front of the first tree's base (2 + 4), behind the second's (6 + 4).
        var layout = Layout(layers: [], Tree(y: 2, z: MapLayers.PlayerZ), Tree(y: 6, z: MapLayers.PlayerZ));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(3, 7));

        Assert.Equal([(MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0), (MapDrawItemKind.ObjectBody, 1)], KindsAndIndexes());
    }

    [Fact]
    public void FillWithPlayer_PlayerFeetOnObjectBase_PlayerDrawnInFront()
    {
        // The tree's base is -0.5 + 4 = 3.5, exactly the player's feet; the tie must not depend on the unstable sort.
        var layout = Layout(layers: [], Tree(y: -0.5, z: MapLayers.PlayerZ));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(0, 3));

        Assert.Equal([(MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0)], KindsAndIndexes());
    }

    [Fact]
    public void FillWithPlayer_LayersAndObjects_FollowZ()
    {
        // The start map: ground at -5 and -4, objects at -3, so the player stands over them; a roof at 5 covers all.
        var layout = Layout(layers: [-5, -4, 5], Tree(y: 0, z: -3));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(0, 0));

        Assert.Equal(
            [(MapDrawItemKind.Layer, 0), (MapDrawItemKind.Layer, 1), (MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0), (MapDrawItemKind.Layer, 2)],
            KindsAndIndexes());
    }

    [Fact]
    public void FillWithPlayer_CanopyOfObjectBelowPlayer_DrawnAfterPlayer()
    {
        // The tree: its body at -3 stays under the player, its canopy goes over (MapLayers.CanopyZOf).
        var layout = Layout(layers: [-5], Tree(y: 0, z: -3) with { ObjectId = "canopy_tree" });

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(1, 1));

        Assert.Equal(
            [(MapDrawItemKind.Layer, 0), (MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0), (MapDrawItemKind.Canopy, 0)],
            KindsAndIndexes());
    }

    [Fact]
    public void FillWithPlayer_ObjectOnCanopyLayer_HasNoCanopy()
    {
        var layout = Layout(layers: [], Tree(y: 0, z: MapLayers.AbovePlayerZ) with { ObjectId = "canopy_tree" });

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(1, 1));

        Assert.DoesNotContain(_items, item => item.Kind == MapDrawItemKind.Canopy);
    }

    [Fact]
    public void FillWithPlayer_ObjectWithoutZones_HasNoCanopy()
    {
        var layout = Layout(layers: [], Tree(y: 0, z: -3));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(1, 1));

        Assert.DoesNotContain(_items, item => item.Kind == MapDrawItemKind.Canopy);
    }

    [Fact]
    public void FillWithPlayer_Twice_KeepsOneEntryPerThing()
    {
        var layout = Layout(layers: [-5], Tree(y: 0, z: -3));

        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(0, 0));
        MapDrawOrder.FillWithPlayer(_items, layout, PlacedDefinitionOf, new Point2D(0, 1));

        Assert.Equal(3, _items.Count);
    }

    [Fact]
    public void FillWithPlayer_SwappedObject_UsesCurrentDefinitionHeight()
    {
        // A 4-cell tree when placed (base 6, in front of the feet at 4.5), now a 1-cell stump (base 3, behind).
        var stump = _objects["tree"] with { Id = "stump", Height = 1 };
        var layout = Layout(layers: [], Tree(y: 2, z: MapLayers.PlayerZ));

        MapDrawOrder.FillWithPlayer(_items, layout, _ => stump, new Point2D(3, 4));

        Assert.Equal([(MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Player, 0)], KindsAndIndexes());
    }

    [Fact]
    public void Fill_NoPlayer_HasNoPlayerItem()
    {
        var layout = Layout(layers: [-5], Tree(y: 0, z: -3) with { ObjectId = "canopy_tree" });

        MapDrawOrder.Fill(_items, layout, PlacedDefinitionOf);

        Assert.Equal([(MapDrawItemKind.Layer, 0), (MapDrawItemKind.ObjectBody, 0), (MapDrawItemKind.Canopy, 0)], KindsAndIndexes());
    }

    [Fact]
    public void SpriteKeyOf_FartherUpTheScreen_SortsBefore()
    {
        var nearer = Tree(y: 6, z: 1);
        var farther = Tree(y: 2, z: 1);

        Assert.True(MapDrawOrder.SpriteKeyOf(farther, _objects["tree"], 1) < MapDrawOrder.SpriteKeyOf(nearer, _objects["tree"], 0));
    }

    private IEnumerable<(MapDrawItemKind Kind, int Index)> KindsAndIndexes() => _items.Select(item => (item.Kind, item.Index));

    private static ObjectDefinition PlacedDefinitionOf(PlacedObject placed) => _objects[placed.ObjectId];

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
