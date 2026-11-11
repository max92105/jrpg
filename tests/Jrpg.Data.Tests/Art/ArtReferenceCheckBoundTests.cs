using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// A tile's cell and its animation's last frame checked as counts (decision 542), so a hand-edited column or frame count
// near the int limit is refused rather than wrapping back inside the image; on the test world's forest sheet, 512 x 384
// px in 16 px cells.
public sealed class ArtReferenceCheckBoundTests
{
    private static readonly string _artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly TileDefinition _forestTile = _catalog.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall");

    [Fact]
    public void FindProblems_FrameCountAtIntMax_NamesTheTile()
    {
        var tile = FramesFromColumnTwo(int.MaxValue);

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(WithOnly(tile), _artRoot));

        Assert.Contains($"tile {tile.Id}'s last animation frame", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void FirstTileOutside_FrameCountAtIntMax_ReturnsTheTile()
    {
        var tile = FramesFromColumnTwo(int.MaxValue);

        Assert.Equal(tile, ArtReferenceCheck.FirstTileOutside(WithOnly(tile), "FG_Forest_Fall.png", (512, 384)));
    }

    // Column 2^28 times 16 px is 2^32, which wraps to 0 in int.
    [Fact]
    public void FindProblems_ColumnPastIntRange_NamesTheCell()
    {
        var tile = _forestTile with { Column = 1 << 28, Animation = null };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(WithOnly(tile), _artRoot));

        Assert.Equal($"tilesheets/FG_Forest_Fall.png: tile {tile.Id}'s cell (268435456, {tile.Row}) lies outside the 512x384 image.", problem);
    }

    // The row side of an import's check: 384 px in 16 px cells is rows 0 to 23.
    [Fact]
    public void FirstTileOutside_RowPastTheImage_ReturnsTheTile()
    {
        var tile = _forestTile with { Column = 0, Row = 24, Animation = null };

        Assert.Equal(tile, ArtReferenceCheck.FirstTileOutside(WithOnly(tile), "FG_Forest_Fall.png", (512, 384)));
    }

    private static TileDefinition FramesFromColumnTwo(int frameCount) =>
        _forestTile with { Column = 2, Animation = new TileAnimation { FrameCount = frameCount, FrameMilliseconds = 100 } };

    private static ContentCatalog WithOnly(TileDefinition tile) =>
        _catalog with { Tiles = new Dictionary<int, TileDefinition> { [tile.Id] = tile } };
}
