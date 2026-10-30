using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.Trade;
using static Jrpg.Core.Tests.Builders.ShopCatalog;

namespace Jrpg.Core.Tests.Session;

// A save keeps every opened shop's stock (decision 385): a load shows the same pieces and draws nothing until the
// leader gains a level. ShopCatalog's store: the sword draws 1 (3 Power), the ring 2 (4 Agility).
public sealed class GameSessionShopSaveTests
{
    private static readonly DateTimeOffset _savedAt = new(2026, 10, 29, 21, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Capture_OpenedShop_SavesItsPiecesAndLevel()
    {
        var session = GameSession.StartNew(Build(), "sarxian", new FixedSequenceRandom([1, 2]));
        session.ShopFor("store");

        var save = session.Capture(_savedAt);

        var expected = new SavedShopStock("store", RolledAtLevel: 1, [Magic(Sword, Power, 3).ToSaved(), Magic(Ring, Agility, 4).ToSaved()]);
        Assert.Equal([expected], save.ShopStocks);
    }

    [Fact]
    public void Capture_NoShopOpened_SavesNoStock()
    {
        var session = GameSession.StartNew(Build(), "sarxian", new FixedSequenceRandom());

        Assert.Empty(session.Capture(_savedAt).ShopStocks);
    }

    // The restored session's random holds nothing: a draw on reopening would throw.
    [Fact]
    public void Restore_SavedStock_ShopShowsTheSamePiecesWithoutDrawing()
    {
        var catalog = Build();
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom([1, 2]));
        var pieces = session.ShopFor("store").Stock.Pieces.ToList();

        var restored = GameSession.Restore(session.Capture(_savedAt), catalog, new FixedSequenceRandom());

        Assert.Equal(pieces, restored.ShopFor("store").Stock.Pieces);
    }

    [Fact]
    public void Restore_LeaderAboveTheSavedLevel_RollsAgainOnOpening()
    {
        var catalog = Build();
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom([1, 2]));
        session.ShopFor("store");
        var save = session.Capture(_savedAt);
        var leveled = save with { Members = [save.Members[0] with { Level = 2 }] };

        var restored = GameSession.Restore(leveled, catalog, new FixedSequenceRandom([3, 0]));

        GearPiece[] expected = [Magic(Sword, Power, 5), Magic(Ring, Agility, 2)];
        Assert.Equal(expected, restored.ShopFor("store").Stock.Pieces);
    }
}
