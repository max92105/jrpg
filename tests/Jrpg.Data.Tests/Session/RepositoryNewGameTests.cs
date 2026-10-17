using Jrpg.Core.Session;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Session;

public sealed class RepositoryNewGameTests
{
    [Fact]
    public void StartNew_RepositoryContent_LeaderIsFirstActiveMember()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var session = GameSession.StartNew(catalog);

        Assert.Equal("sarxian", session.Party.Leader.Id);
        Assert.Equal(["sarxian"], session.Party.ActiveMembers.Select(member => member.Id));
        Assert.Equal(7, session.Party.Members.Count);
        Assert.Equal(5, session.Inventory.QuantityOf("potion"));
    }
}
