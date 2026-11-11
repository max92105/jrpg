using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

// A cell's picture at a time in the editor's animation (ADR 0039), on a test-world forest tile given 4 frames of 140
// ms.
public sealed class TileImagesAnimationTests
{
    private static readonly string _root = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_root);
    private static readonly TimeSpan _secondFrame = TimeSpan.FromMilliseconds(140);

    private readonly CatalogDraft _draft = new(_catalog);
    private readonly TileImages _images;
    private readonly SheetCell _animatedCell;

    public TileImagesAnimationTests()
    {
        var tile = _catalog.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall" && tile.Column + 4 <= 32);
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [tile.Id] = new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 } });
        _images = new TileImages(_draft, ArtFolders.RootIn(_root), () => MissingArt.None);
        _animatedCell = SheetCell.Of(tile);
    }

    [Fact]
    public void CellImageAt_AnimatedTilesCellAtTheFrameTime_IsTheNextCellsImage() =>
        Assert.Same(_images.CellImageOf(NextTo(_animatedCell)), _images.CellImageAt(_animatedCell, _secondFrame));

    [Fact]
    public void CellImageAt_StillCell_IsItsOwnImage()
    {
        var still = _animatedCell with { Row = _animatedCell.Row + 1 };

        Assert.Same(_images.CellImageOf(still), _images.CellImageAt(still, _secondFrame));
    }

    // The second frame's cell isn't animated itself: painted somewhere, it stays still.
    [Fact]
    public void CellImageAt_AnimationFrameOfAnotherTile_IsItsOwnImage()
    {
        var frame = NextTo(_animatedCell);

        Assert.Same(_images.CellImageOf(frame), _images.CellImageAt(frame, _secondFrame));
    }

    private static SheetCell NextTo(SheetCell cell) => cell with { Column = cell.Column + 1 };
}
