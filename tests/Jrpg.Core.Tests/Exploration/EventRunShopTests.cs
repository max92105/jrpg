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

// The shop step waits while the Shop is open, as the Forge's step does (decision 353).
public sealed class EventRunShopTests
{
    private static readonly ObjectDefinition _counter = new() { Id = "counter", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly DialogueStep _farewell = new(new Speech(["Come again."]));

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11").WithObject(_counter, new Point2D(0, 0)).Build();

    [Fact]
    public void Advance_OpenShopStep_WaitsForContinue()
    {
        var run = Start(new MapEventBuilder().WithSteps(new OpenShopStep("general_store"), _farewell).Build());
        Assert.Equal(new OpenShopStep("general_store"), run.Current);

        run.Continue();

        Assert.Equal(_farewell, run.Current);
    }

    private EventRun Start(MapEvent mapEvent) => new(
        new TriggeredEvent(mapEvent, _map.Definition.Layout.Objects[0]),
        _map,
        new ExplorationContext(new ContentCatalogBuilder().Build(), new Inventory(), new WorldState(), new FixedSequenceRandom()),
        TestParties.LedBy("sarxian"));
}
