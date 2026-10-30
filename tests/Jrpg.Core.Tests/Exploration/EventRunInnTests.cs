using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

// A priced rest is an inn's stay (decision 382): it waits for the Game to pay and rests nobody until then; a free rest
// is given at once, as before.
public sealed class EventRunInnTests
{
    private const int Price = 20;
    private static readonly ObjectDefinition _bed = new() { Id = "bed", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly ContentCatalog _catalog = new ContentCatalogBuilder().Build();
    private static readonly DialogueStep _goodNight = new(new Speech(["Sleep well."]));

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11").WithObject(_bed, new Point2D(0, 0)).Build();
    private readonly PartyState _party = TestParties.LedBy("sarxian");

    public EventRunInnTests() => _party.Leader.Vitals.TakeDamage(10);

    [Fact]
    public void Advance_PricedRest_WaitsWithoutResting()
    {
        var run = Start(new RestStep { Price = Price });

        Assert.IsType<RestStep>(run.Current);
        Assert.Equal(Price, run.CurrentInnStay!.Price);
        Assert.Equal(_party.Leader.Vitals.MaxHp - 10, _party.Leader.Vitals.Hp);
    }

    [Fact]
    public void Advance_FreeRest_RestsAtOnceWithNoInnStay()
    {
        var run = Start(new RestStep());

        Assert.Null(run.CurrentInnStay);
        Assert.Equal(_party.Leader.Vitals.MaxHp, _party.Leader.Vitals.Hp);
    }

    [Fact]
    public void Continue_WithoutPaying_LeavesThePartyUnrested()
    {
        var run = Start(new RestStep { Price = Price }, _goodNight);

        run.Continue();

        Assert.Equal(_goodNight, run.Current);
        Assert.Null(run.CurrentInnStay);
        Assert.Equal(_party.Leader.Vitals.MaxHp - 10, _party.Leader.Vitals.Hp);
    }

    [Fact]
    public void TryPay_OnTheWaitingStay_RestsThroughTheRun()
    {
        _party.Purse.Add(Price);
        var run = Start(new RestStep { Price = Price });

        Assert.True(run.CurrentInnStay!.TryPay());

        Assert.Equal(0, _party.Purse.Gold);
        Assert.Equal(_party.Leader.Vitals.MaxHp, _party.Leader.Vitals.Hp);
    }

    private EventRun Start(params EventStep[] steps)
    {
        var mapEvent = new MapEventBuilder().WithSteps(steps).Build();
        var context = new ExplorationContext(_catalog, new Inventory(), new WorldState(), new FixedSequenceRandom());
        return new EventRun(new TriggeredEvent(mapEvent, _map.Definition.Layout.Objects[0]), _map, context, _party);
    }
}
