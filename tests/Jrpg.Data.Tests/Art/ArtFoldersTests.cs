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
    public void TilesheetImagesIn_TestWorldArt_ListsEveryPngSorted()
    {
        var artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "test-world"));

        var images = ArtFolders.TilesheetImagesIn(artRoot);

        // The frozen world's eleven images (ADR 0037); ordinal order puts every capitalised name before the tree sheet.
        Assert.Equal(
            ["City.png", "FG_Forest_Fall.png", "Grass1.png", "Grass2.png", "Grass3.png", "Ground.png", "RA_Ground_Tiles.png", "RA_Jungle.png", "Water1.png", "Water2.png", "tree01_s_01_animation.png"],
            images);
    }
}
