using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

// The Forge step waits while the Forge is open, as a dialogue does (decision 353).
public sealed class EventRunForgeTests
{
    private static readonly ObjectDefinition _anvil = new() { Id = "anvil", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly DialogueStep _farewell = new(new Speech(["Come back with more scrap."]));

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11").WithObject(_anvil, new Point2D(0, 0)).Build();

    [Fact]
    public void Advance_OpenForgeStep_WaitsForContinue()
    {
        var run = Start(new MapEventBuilder().WithSteps(new OpenForgeStep(), _farewell).Build());
        Assert.Equal(new OpenForgeStep(), run.Current);

        run.Continue();

        Assert.Equal(_farewell, run.Current);
    }

    private EventRun Start(MapEvent mapEvent) => new(
        new TriggeredEvent(mapEvent, _map.Definition.Layout.Objects[0]),
        _map,
        new ExplorationContext(new ContentCatalogBuilder().Build(), new Inventory(), new WorldState(), new FixedSequenceRandom()),
        TestParties.LedBy("sarxian"));
}
