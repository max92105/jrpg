using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.TestWorld;

/// <summary>
/// The frozen test world (ADR 0037) loads whole: live systems, frozen maps, every named image present.
/// </summary>
public sealed class TestWorldTests
{
    private static readonly string _testWorldRoot = Path.Combine(AppContext.BaseDirectory, "test-world");

    [Fact]
    public void Load_TestWorld_HasTheFrozenStartMap()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);

        var villager = catalog.Maps["start"].Layout.Objects.Single(placed => placed.Uid == "villager_1");

        Assert.Equal((12, 4), (villager.Position.X, villager.Position.Y));
    }

    [Fact]
    public void FindProblems_TestWorld_FindsNone()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);

        Assert.Empty(ArtReferenceCheck.FindProblems(catalog, ArtFolders.RootIn(_testWorldRoot)));
    }

    [Fact]
    public void TestWorld_TilesheetFolder_HoldsTheUnsetUpImage()
    {
        Assert.Contains("Grass3.png", ArtFolders.TilesheetImagesIn(ArtFolders.RootIn(_testWorldRoot)));
    }
}
