using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The inn in the repository's content (the gold plan, I3): the start map's innkeeper charges for the rest (decision
// 381).
public sealed class RepositoryInnTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void StartMap_Innkeeper_ChargesForRest()
    {
        var innkeeper = _catalog.Maps["start"].Layout.Objects.Single(placed => placed.Uid == "innkeeper");

        var rest = Assert.Single(innkeeper.Events.SelectMany(mapEvent => mapEvent.Steps).OfType<RestStep>());
        Assert.NotNull(rest.Price);
    }
}
