using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentRootLocatorTests
{
    private static readonly string _repositoryContent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo", "content"));

    [Fact]
    public void Choose_ContentArgument_WinsOverBuiltInRoot()
    {
        var other = Path.Combine(Path.GetTempPath(), "other-content");

        var root = ContentRootLocator.Choose(["--fullscreen", "--content", other], _repositoryContent);

        Assert.Equal(Path.GetFullPath(other), root);
    }

    [Fact]
    public void Choose_NoArgument_UsesBuiltInRoot()
    {
        var root = ContentRootLocator.Choose([], _repositoryContent);

        Assert.Equal(_repositoryContent, root);
    }

    [Fact]
    public void ChooseArtRoot_BuiltInArtRoot_WinsOverContentArt()
    {
        var legacyAssets = Path.Combine(Path.GetTempPath(), "repo", "Assets");

        Assert.Equal(legacyAssets, ContentRootLocator.ChooseArtRoot(legacyAssets, _repositoryContent));
    }

    [Fact]
    public void ChooseArtRoot_NoBuiltInArtRoot_UsesArtUnderContent()
    {
        Assert.Equal(Path.Combine(_repositoryContent, "art"), ContentRootLocator.ChooseArtRoot(null, _repositoryContent));
    }

    [Fact]
    public void Find_NoArgumentNoMetadata_UsesBaseDirectory()
    {
        // This test assembly carries no ContentRoot metadata, like a published app.
        var root = ContentRootLocator.Find([], typeof(ContentRootLocatorTests).Assembly);

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "content"), root);
    }

    [Fact]
    public void Choose_ContentArgumentWithoutValue_IsIgnored()
    {
        var root = ContentRootLocator.Choose(["--content"], _repositoryContent);

        Assert.Equal(_repositoryContent, root);
    }
}
