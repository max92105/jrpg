using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// quests.json is loaded with the catalog: the placeholder errand (decision 397) is there with its stages in order, and
// a copy of the content with an unknown quest kind proves the loader refuses the file by name.
public sealed class ContentCatalogLoaderQuestTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("quests", "test-world");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_RepositoryContent_HasThePlaceholderErrandWithItsStagesInOrder()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);

        var errand = catalog.Quests["placeholder_errand"];
        Assert.Equal(QuestKind.Side, errand.Kind);
        Assert.Equal(["ask_innkeeper", "return_to_giver"], errand.Stages.Select(stage => stage.Id));
        Assert.All(errand.Stages, stage => Assert.NotEmpty(stage.Text));
    }

    [Fact]
    public void Load_UnknownQuestKind_ThrowsNamingTheFile()
    {
        _content.Replace("quests.json", "\"side\"", "\"epic\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("quests.json", exception.Message, StringComparison.Ordinal);
    }
}
