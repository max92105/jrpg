using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class GameFlowTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Constructor_RepositoryContent_StartsANewGameOnTheStartMap()
    {
        var flow = new GameFlow(_catalog);

        Assert.Equal("start", flow.Session.Exploration.Location.MapId);
    }

    [Fact]
    public void Update_TwoDeltas_AddsBothToPlayTime()
    {
        var flow = new GameFlow(_catalog);

        flow.Update(TimeSpan.FromSeconds(0.5), default);
        flow.Update(TimeSpan.FromSeconds(0.25), default);

        Assert.Equal(TimeSpan.FromSeconds(0.75), flow.Session.PlayTime);
    }
}
