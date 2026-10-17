using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentCatalogLoaderTests
{
    [Fact]
    public void Load_RepositoryContent_Succeeds()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        // Assets/Maps/start.json already refers to "potion", and ids never change once used.
        Assert.Contains("potion", catalog.Items.Keys);
    }
}
