using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
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

// The cinematic step waits with its record until the Game has shown it, and a skip runs past it (ADR 0046), on an event
// of a cinematic, a potion given and a dialogue.
public sealed class EventRunCinematicTests
{
    private static readonly ObjectDefinition _sign =
        new() { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };

    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").WithMaxStack(99).Build();
    private static readonly CinematicDefinition _cinematic = new()
    {
        Id = "test_cinematic",
        Stills =
        [
            new CinematicStill { ImageFile = "test/1.png", Seconds = 4 },
            new CinematicStill { ImageFile = "test/2.png", Seconds = 3 },
        ],
    };

    private static readonly ContentCatalog _catalog = new ContentCatalogBuilder().WithItems(_potion).Build() with
    {
        Cinematics = new Dictionary<string, CinematicDefinition> { [_cinematic.Id] = _cinematic },
    };

    private static readonly CinematicStep _playCinematic = new("test_cinematic");
    private static readonly GiveItemStep _givePotions = new("potion", 2);
    private static readonly DialogueStep _afterwards = new(new Speech(["So it began."]));
    private static readonly MapEvent _event = new MapEventBuilder().WithSteps(_playCinematic, _givePotions, _afterwards).Build();

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11").WithObject(_sign, new Point2D(0, 0)).Build();
    private readonly Inventory _inventory = new();

    [Fact]
    public void Start_CinematicStep_WaitsWithItsRecord()
    {
        var run = Start();

        Assert.Equal(_playCinematic, run.Current);
        Assert.Same(_cinematic, run.CurrentCinematic);
    }

    [Fact]
    public void Continue_PastCinematic_ClearsCurrentCinematic()
    {
        var run = Start();

        run.Continue();

        Assert.Null(run.CurrentCinematic);
        Assert.Equal(_givePotions, run.Current);
    }

    [Fact]
    public void SkipPresentation_OnCinematic_AppliesTheGiveItemAndStopsAtIt()
    {
        var run = Start();

        run.SkipPresentation();

        Assert.Equal(_givePotions, run.Current);
        Assert.Equal(2, _inventory.QuantityOf("potion"));
    }

    private EventRun Start() => new(
        new TriggeredEvent(_event, _map.Definition.Layout.Objects[0]),
        _map,
        new ExplorationContext(_catalog, _inventory, new WorldState(), new FixedSequenceRandom()),
        TestParties.LedBy("sarxian"));
}
