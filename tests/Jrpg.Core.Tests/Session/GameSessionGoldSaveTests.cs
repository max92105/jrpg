using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// The purse travels through the save (format 6); the shops' stocks are written empty until shops roll stocks.
public sealed class GameSessionGoldSaveTests
{
    private static readonly DateTimeOffset _savedAt = new(2026, 10, 29, 20, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Capture_PurseHoldsGold_SaveKeepsIt()
    {
        var session = StartNew();
        session.Party.Purse.Add(250);

        var save = session.Capture(_savedAt);

        Assert.Equal(250, save.Gold);
        Assert.Empty(save.ShopStocks);
    }

    [Fact]
    public void Restore_SaveWithGold_PurseHoldsIt()
    {
        var session = StartNew();
        session.Party.Purse.Add(250);
        var save = session.Capture(_savedAt);

        var restored = GameSession.Restore(save, Catalog(), new FixedSequenceRandom());

        Assert.Equal(250, restored.Party.Purse.Gold);
    }

    private static GameSession StartNew() => GameSession.StartNew(Catalog(), "sarxian", new FixedSequenceRandom());

    private static ContentCatalog Catalog() => new ContentCatalogBuilder()
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithMaps(new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field" })
        .WithNewGame(new NewGameDefinition
        {
            LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
            StartingItems = [],
            StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
            FieldSpriteFile = "player.png",
        })
        .Build();
}
