using Jrpg.Data.Art;

namespace Jrpg.Data.Tests.Art;

public sealed class ArtFoldersTests
{
    // The developer imports NPC sheets into this folder: only the two the code relies on are pinned, and the order.
    [Fact]
    public void ImagesIn_RepositoryFieldSprites_ListsPlayerAndPlaceholder()
    {
        var artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

        var images = ArtFolders.ImagesIn(artRoot, ArtFolders.FieldSprites);

        Assert.Contains("npc_placeholder.png", images);
        Assert.Contains("player.png", images);
        Assert.Equal(images.Order(StringComparer.Ordinal), images);
    }

    // Any art folder by name (decision 589): PNGs only, in ordinal order, whatever order the disk gives them.
    [Fact]
    public void ImagesIn_FolderWithPngsAndOtherFiles_ListsPngsSorted()
    {
        var artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");
        var portraits = Path.Combine(artRoot, ArtFolders.Portraits);
        Directory.CreateDirectory(portraits);
        try
        {
            File.WriteAllBytes(Path.Combine(portraits, "b.png"), []);
            File.WriteAllBytes(Path.Combine(portraits, "a.png"), []);
            File.WriteAllText(Path.Combine(portraits, "notes.txt"), "not a picture");

            Assert.Equal(["a.png", "b.png"], ArtFolders.ImagesIn(artRoot, ArtFolders.Portraits));
        }
        finally
        {
            Directory.Delete(artRoot, recursive: true);
        }
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

    // The cinematics' pictures (ADR 0046): one folder per cinematic, named with it; PNGs only, sorted.
    [Fact]
    public void ImagesUnder_PictureInSubfolder_NamesItWithItsFolder()
    {
        var artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");
        var intro = Path.Combine(artRoot, ArtFolders.Cinematics, "intro");
        Directory.CreateDirectory(intro);
        try
        {
            File.WriteAllBytes(Path.Combine(intro, "2_road.png"), []);
            File.WriteAllBytes(Path.Combine(intro, "1_home.png"), []);
            File.WriteAllText(Path.Combine(intro, "notes.txt"), "not a picture");

            Assert.Equal(["intro/1_home.png", "intro/2_road.png"], ArtFolders.ImagesUnder(artRoot, ArtFolders.Cinematics));
        }
        finally
        {
            Directory.Delete(artRoot, recursive: true);
        }
    }

    [Fact]
    public void ImagesUnder_NoFolder_IsEmpty()
    {
        var artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");

        Assert.Empty(ArtFolders.ImagesUnder(artRoot, ArtFolders.Cinematics));
    }
}
