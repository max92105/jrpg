using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game.Flow;

namespace Jrpg.Game.Tests.Flow;

public sealed class EncounterFreeContentTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Of_RepositoryContent_LeavesNoRandomBattleTable()
    {
        Assert.Contains(_catalog.Maps.Values, map => map.Settings.RandomEncounters is not null);

        var played = EncounterFreeContent.Of(_catalog);

        Assert.All(played.Maps.Values, map => Assert.Null(map.Settings.RandomEncounters));
        Assert.Equal(_catalog.Maps.Keys.Order(), played.Maps.Keys.Order());
    }
}
