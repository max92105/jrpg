using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

public sealed class EventRunTests
{
    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly ObjectDefinition _openedChest = _chest with { Id = "chest_opened" };
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").WithMaxStack(99).Build();
    private static readonly Dictionary<string, ItemDefinition> _items = new() { [_potion.Id] = _potion };

    private static readonly DialogueStep _foundSomething = new(new Speech(["Something glints inside."]));
    private static readonly GiveItemStep _givePotions = new("potion", 3);
    private static readonly SwapObjectStep _openChest = new("chest_opened");

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11")
        .WithObject(_chest, new Point2D(0, 0))
        .WithKnownObject(_openedChest)
        .Build();

    private readonly Inventory _inventory = new();

    private PlacedObject Chest => _map.Definition.Layout.Objects[0];

    [Fact]
    public void Constructor_OnceEvent_MarksDoneBeforeFirstStep()
    {
        var chestEvent = new MapEventBuilder().AsOnce().WithSteps(_foundSomething).Build();

        var run = Start(chestEvent);

        Assert.True(_map.IsDone(chestEvent));
        Assert.Equal(_foundSomething, run.Current);
    }

    [Fact]
    public void Constructor_RepeatableEvent_IsNotMarkedDone()
    {
        var signEvent = new MapEventBuilder().WithSteps(_foundSomething).Build();

        Start(signEvent);

        Assert.False(_map.IsDone(signEvent));
    }

    [Fact]
    public void Constructor_SwapThenDialogue_AppliesSwapAndWaitsOnDialogue()
    {
        var run = Start(new MapEventBuilder().WithSteps(_openChest, _foundSomething).Build());

        Assert.Equal("chest_opened", _map.CurrentDefinitionOf(Chest).Id);
        Assert.Equal(_foundSomething, run.Current);
        Assert.False(run.IsFinished);
    }

    [Fact]
    public void Constructor_OnlySwapSteps_IsFinished()
    {
        var run = Start(new MapEventBuilder().WithSteps(_openChest).Build());

        Assert.True(run.IsFinished);
        Assert.Null(run.Current);
        Assert.Equal("chest_opened", _map.CurrentDefinitionOf(Chest).Id);
    }

    [Fact]
    public void Continue_ReachesGiveItem_AddsItemToInventory()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething, _givePotions).Build());
        Assert.Equal(0, _inventory.QuantityOf("potion"));

        run.Continue();

        Assert.Equal(_givePotions, run.Current);
        Assert.Equal(3, _inventory.QuantityOf("potion"));
        Assert.Equal(new ItemReceipt(_potion, Added: 3, Lost: 0), run.CurrentReceipt);
    }

    [Fact]
    public void Constructor_GiveItemBeyondMaxStack_ReceiptReportsLostCopies()
    {
        _inventory.Add(_potion, 98);

        var run = Start(new MapEventBuilder().WithSteps(_givePotions).Build());

        Assert.Equal(99, _inventory.QuantityOf("potion"));
        Assert.Equal(new ItemReceipt(_potion, Added: 1, Lost: 2), run.CurrentReceipt);
    }

    [Fact]
    public void Continue_PastGiveItem_ClearsTheReceipt()
    {
        var run = Start(new MapEventBuilder().WithSteps(_givePotions, _foundSomething).Build());

        run.Continue();

        Assert.Equal(_foundSomething, run.Current);
        Assert.Null(run.CurrentReceipt);
    }

    [Fact]
    public void Continue_WarpStep_WaitsOnIt()
    {
        var warp = new WarpStep("forest", new GridCell(1, 1));
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething, warp).Build());

        run.Continue();

        Assert.Equal(warp, run.Current);
    }

    [Fact]
    public void Continue_LastStep_Finishes()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething).Build());

        run.Continue();

        Assert.True(run.IsFinished);
        Assert.Null(run.Current);
    }

    [Fact]
    public void Continue_AfterFinished_Throws()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething).Build());
        run.Continue();

        Assert.Throws<InvalidOperationException>(run.Continue);
    }

    [Fact]
    public void Constructor_SwapInCellEvent_Throws()
    {
        var cellEvent = new MapEventBuilder().WithSteps(_openChest).Build();

        Assert.Throws<InvalidOperationException>(() => new EventRun(new TriggeredEvent(cellEvent, Source: null), _map, _inventory, _items));
    }

    private EventRun Start(MapEvent mapEvent) => new(new TriggeredEvent(mapEvent, Chest), _map, _inventory, _items);
}
