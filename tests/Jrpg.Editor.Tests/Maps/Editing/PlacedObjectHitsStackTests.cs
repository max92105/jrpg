using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

/// <summary>The objects under a point, topmost first, and the pick cycling through them (decision 523).</summary>
public sealed class PlacedObjectHitsStackTests
{
    // A 1x1 crate; a 3x3 house whose roof zone is its top third; a villager, an NPC drawn from a field sprite.
    private static readonly Dictionary<string, ObjectDefinition> _definitions = new()
    {
        ["crate"] = Definition("crate", 1, 1, [new ObjectTile(0, 0, 8)]),
        ["house"] = Definition("house", 3, 3, [.. Enumerable.Range(0, 9).Select(cell => new ObjectTile(cell % 3, cell / 3, 10 + cell))]) with
        {
            AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.34), new Point2D(0, 0.34)]],
        },
        ["villager"] = Definition("villager", 1, 1, []) with { FieldSpriteFile = "npc_placeholder.png" },
    };

    private static readonly PlacedObject _house = Placed("house_1", "house", 0, 0, z: -4);

    [Fact]
    public void AllAt_ThreeOverlapping_TopmostFirst()
    {
        var (bottom, middle, top) = (Placed("crate_1", "crate", 2, 2), Placed("crate_2", "crate", 2, 2), Placed("crate_3", "crate", 2, 2));

        var hits = PlacedObjectHits.AllAt(Layout(bottom, middle, top), new Point2D(2.5, 2.5), _definitions);

        Assert.Equal([top, middle, bottom], hits);
    }

    // The lower layer's crate has the lower base, which alone would put it on top.
    [Fact]
    public void AllAt_HigherLayerListedFirst_ComesFirst()
    {
        var higherLayer = Placed("crate_1", "crate", 2, 2, z: 2);
        var lowerLayer = Placed("crate_2", "crate", 2, 2.4, z: 1);

        var hits = PlacedObjectHits.AllAt(Layout(higherLayer, lowerLayer), new Point2D(2.5, 2.8), _definitions);

        Assert.Equal([higherLayer, lowerLayer], hits);
    }

    // The roof's zone is drawn over the player's layer, so over the villager, whatever the house's own layer.
    [Fact]
    public void AllAt_VillagerOnLayer1UnderAMinus4HousesRoof_HouseFirst()
    {
        var villager = Placed("villager_1", "villager", 1, 0, z: MapLayers.PlayerZ);

        var hits = PlacedObjectHits.AllAt(Layout(_house, villager), new Point2D(1.5, 0.5), _definitions);

        Assert.Equal([_house, villager], hits);
    }

    [Fact]
    public void AllAt_VillagerOnLayer1BesideTheRoofZone_VillagerFirst()
    {
        var villager = Placed("villager_1", "villager", 1, 2, z: MapLayers.PlayerZ);

        var hits = PlacedObjectHits.AllAt(Layout(_house, villager), new Point2D(1.5, 2.5), _definitions);

        Assert.Equal([villager, _house], hits);
    }

    [Fact]
    public void NextUnder_SelectedTop_GivesTheSecond()
    {
        var (top, middle, bottom) = ThreeCrates();

        Assert.Equal(middle, PlacedObjectHits.NextUnder([top, middle, bottom], top.Uid));
    }

    [Fact]
    public void NextUnder_SelectedLast_WrapsToTheTop()
    {
        var (top, middle, bottom) = ThreeCrates();

        Assert.Equal(top, PlacedObjectHits.NextUnder([top, middle, bottom], bottom.Uid));
    }

    [Fact]
    public void NextUnder_SelectedNotThere_GivesTheTop()
    {
        var (top, middle, bottom) = ThreeCrates();

        Assert.Equal(top, PlacedObjectHits.NextUnder([top, middle, bottom], "crate_9"));
        Assert.Equal(top, PlacedObjectHits.NextUnder([top, middle, bottom], null));
    }

    [Fact]
    public void NextUnder_NothingThere_IsNull() => Assert.Null(PlacedObjectHits.NextUnder([], "crate_1"));

    private static (PlacedObject Top, PlacedObject Middle, PlacedObject Bottom) ThreeCrates() =>
        (Placed("crate_3", "crate", 2, 2), Placed("crate_2", "crate", 2, 2), Placed("crate_1", "crate", 2, 2));

    private static MapLayout Layout(params PlacedObject[] objects) => new() { Id = "glade", Width = 10, Height = 10, Layers = [], Objects = objects };

    private static PlacedObject Placed(string uid, string objectId, double x, double y, int z = 1) =>
        new() { Uid = uid, ObjectId = objectId, Position = new Point2D(x, y), Z = z };

    private static ObjectDefinition Definition(string id, int width, int height, IReadOnlyList<ObjectTile> tiles) => new()
    {
        Id = id,
        Width = width,
        Height = height,
        CollisionPolygon = [],
        Category = "General",
        Tiles = tiles,
        AbovePlayerZones = [],
    };
}
