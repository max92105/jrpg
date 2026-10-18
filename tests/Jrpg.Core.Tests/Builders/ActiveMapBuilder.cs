using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Builders;

/// <summary>A map to walk on: tile 1 is walkable and tile 2 blocks; objects are placed by id.</summary>
internal sealed class ActiveMapBuilder
{
    public const int Floor = 1;
    public const int Wall = 2;

    private static readonly Dictionary<int, TileDefinition> _tiles = new()
    {
        [Floor] = TestTiles.Walkable(Floor),
        [Wall] = TestTiles.Blocking(Wall),
    };

    private readonly Dictionary<string, ObjectDefinition> _objects = [];
    private readonly List<PlacedObject> _placed = [];
    private string[] _rows = ["1"];
    private WorldState _world = new();

    /// <summary>Each string is one map row of single-digit tile ids.</summary>
    public ActiveMapBuilder WithRows(params string[] rows)
    {
        _rows = rows;
        return this;
    }

    public ActiveMapBuilder WithObject(ObjectDefinition definition, Point2D position, params MapEvent[] events)
    {
        _objects[definition.Id] = definition;
        _placed.Add(new PlacedObject { Uid = $"{definition.Id}_{_placed.Count}", ObjectId = definition.Id, Position = position, Events = events });
        return this;
    }

    /// <summary>Makes an object known to the map without placing it, as a swap target.</summary>
    public ActiveMapBuilder WithKnownObject(ObjectDefinition definition)
    {
        _objects[definition.Id] = definition;
        return this;
    }

    /// <summary>Shares the world state with other maps or asserts on it afterwards.</summary>
    public ActiveMapBuilder WithWorld(WorldState world)
    {
        _world = world;
        return this;
    }

    public ActiveMap Build()
    {
        var layout = new MapLayoutBuilder().WithLayer(_rows).Build() with { Objects = [.. _placed] };
        var definition = new MapDefinition(layout, new MapSettings { Id = layout.Id, DisplayName = "Test Map" });
        return new ActiveMap(definition, _objects, WalkabilityGrid.Build(layout, _tiles), _world);
    }
}
