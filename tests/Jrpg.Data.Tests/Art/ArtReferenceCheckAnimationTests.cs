using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// An animated tile's last frame against its image (ADR 0039), on the test world's forest sheet: 512 x 384 px cut in 16
// px cells, so its columns are 0 to 31.
public sealed class ArtReferenceCheckAnimationTests
{
    private const int LastColumn = 31;

    private static readonly string _artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly TileDefinition _forestTile = _catalog.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall");

    [Fact]
    public void FindProblems_LastFrameOutsideTheImage_NamesTheTile()
    {
        var tile = ThreeFramesFrom(LastColumn - 1);

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(WithOnly(tile), _artRoot));

        Assert.Equal($"tilesheets/FG_Forest_Fall.png: tile {tile.Id}'s last animation frame ({LastColumn + 1}, {tile.Row}) lies outside the 512x384 image.", problem);
    }

    [Fact]
    public void FindProblems_LastFrameOnTheImagesLastColumn_FindsNothing() =>
        Assert.Empty(ArtReferenceCheck.FindProblems(WithOnly(ThreeFramesFrom(LastColumn - 2)), _artRoot));

    [Fact]
    public void FirstTileOutside_OnlyTheLastFrameOutside_ReturnsTheTile()
    {
        var tile = ThreeFramesFrom(LastColumn - 1);

        Assert.Equal(tile, ArtReferenceCheck.FirstTileOutside(WithOnly(tile), "FG_Forest_Fall.png", (512, 384)));
    }

    private static TileDefinition ThreeFramesFrom(int column) =>
        _forestTile with { Column = column, Animation = new TileAnimation { FrameCount = 3, FrameMilliseconds = 100 } };

    private static ContentCatalog WithOnly(TileDefinition tile) =>
        _catalog with { Tiles = new Dictionary<int, TileDefinition> { [tile.Id] = tile } };
}
