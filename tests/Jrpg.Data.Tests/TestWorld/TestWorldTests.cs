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

    // The frozen copy of field-sprites.json (ADR 0043) must list every sprite the frozen objects and new game name.
    [Fact]
    public void Load_TestWorld_FieldSpritesListEveryNamedSprite()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);

        var objectSprites = catalog.Objects.Values.Select(definition => definition.FieldSpriteFile).OfType<string>();
        var named = objectSprites.Append(catalog.NewGame.FieldSpriteFile);

        Assert.Equal(named.Distinct().Order(), catalog.FieldSprites.Keys.Order());
    }

    // The frozen cinematics.json (ADR 0046), never the developer's: Ragnir's intro shows the placeholder stills.
    [Fact]
    public void Load_TestWorld_PlaysItsOwnFrozenCinematics()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);

        Assert.Equal("Placeholder1.png", catalog.Cinematics["ragnir_intro"].Stills[0].ImageFile);
    }

    [Fact]
    public void TestWorld_TilesheetFolder_HoldsTheUnsetUpImage()
    {
        Assert.Contains("Grass3.png", ArtFolders.TilesheetImagesIn(ArtFolders.RootIn(_testWorldRoot)));
    }
}
