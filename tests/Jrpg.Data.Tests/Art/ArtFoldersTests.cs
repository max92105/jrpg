using Jrpg.Data.Art;

namespace Jrpg.Data.Tests.Art;

public sealed class ArtFoldersTests
{
    [Fact]
    public void FieldSpriteImagesIn_RepositoryArt_ListsPlayerAndPlaceholder()
    {
        var images = ArtFolders.FieldSpriteImagesIn(ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content")));

        Assert.Equal(["npc_placeholder.png", "player.png"], images);
    }

    [Fact]
    public void TilesheetImagesIn_RepositoryArt_ListsEveryPngSorted()
    {
        var images = ArtFolders.TilesheetImagesIn(ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content")));

        // 34 images; ordinal order puts every capitalised name before the lower-case tree sheets.
        Assert.Equal(34, images.Count);
        Assert.Equal("City.png", images[0]);
        Assert.Contains("Grass3.png", images);
        Assert.Equal("tree06_s_03_animation.png", images[^1]);
        Assert.Equal(images.Order(StringComparer.Ordinal), images);
    }
}
