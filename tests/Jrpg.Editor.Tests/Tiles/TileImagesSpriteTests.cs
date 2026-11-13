using System.Windows;
using System.Windows.Media.Imaging;
using Jrpg.Core.Content;
using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>
/// An NPC's standing frame cut as its sheet's record says (ADR 0043), on the test world's placeholder sprite (384 x 288
/// px, 48 px frames, down on row 0).
/// </summary>
public sealed class TileImagesSpriteTests
{
    private const string Placeholder = "npc_placeholder.png";

    private static readonly string _root = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_root);

    [Fact]
    public void FieldSpriteFrameOf_PlaceholderSheet_IsItsStandingFrame()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        var frame = Assert.IsType<CroppedBitmap>(images.FieldSpriteFrameOf(Placeholder));

        Assert.Equal(new Int32Rect(0, 0, 48, 48), frame.SourceRect);
    }

    // The same image read through another record: 16 x 24 frames, down on row 2, so the crop moves and shrinks.
    [Fact]
    public void FieldSpriteFrameOf_RecordWithOtherFrames_CropsItsSize()
    {
        var catalog = WithPlaceholder(16, 24, downRow: 2);
        var images = new TileImages(new CatalogDraft(catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        var frame = Assert.IsType<CroppedBitmap>(images.FieldSpriteFrameOf(Placeholder));

        Assert.Equal(new Int32Rect(0, 48, 16, 24), frame.SourceRect);
    }

    [Fact]
    public void FieldSpriteFrameOf_FileWithoutRecord_IsTheMark()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        Assert.Same(MissingArtMark.Picture, images.FieldSpriteFrameOf("nobody.png"));
    }

    // A frame one pixel taller than the image: the standing frame would be cropped past the edge.
    [Fact]
    public void FieldSpriteFrameOf_FramePastTheImage_IsTheMark()
    {
        var imageHeight = PngSize.Read(Path.Combine(ArtFolders.RootIn(_root), ArtFolders.FieldSprites, Placeholder)).Height;
        var catalog = WithPlaceholder(48, imageHeight + 1, downRow: 0);
        var images = new TileImages(new CatalogDraft(catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        Assert.Same(MissingArtMark.Picture, images.FieldSpriteFrameOf(Placeholder));
    }

    // An import replacing the file: the frame cropped before is dropped, so the next draw crops the new image.
    [Fact]
    public void Forget_SpriteFile_CropsAnew()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);
        var before = images.FieldSpriteFrameOf(Placeholder);

        images.Forget("NPC_PLACEHOLDER.png");

        Assert.NotSame(before, images.FieldSpriteFrameOf(Placeholder));
    }

    private static ContentCatalog WithPlaceholder(int frameWidth, int frameHeight, int downRow)
    {
        var sheet = _catalog.FieldSprites[Placeholder];
        var changed = sheet with
        {
            FrameWidth = frameWidth,
            FrameHeight = frameHeight,
            Walk = sheet.Walk with { Rows = sheet.Walk.Rows with { Down = downRow } },
        };
        var sheets = new Dictionary<string, FieldSpriteSheet>(_catalog.FieldSprites, StringComparer.Ordinal);
        sheets[Placeholder] = changed;
        return _catalog with { FieldSprites = sheets };
    }
}
