using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
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

        var frame = Assert.IsType<CroppedBitmap>(images.FieldSpriteFrameOf(Placeholder, Facing.Down));

        Assert.Equal(new Int32Rect(0, 0, 48, 48), frame.SourceRect);
    }

    // The same image read through another record: 16 x 24 frames, down on row 2, so the crop moves and shrinks.
    [Fact]
    public void FieldSpriteFrameOf_RecordWithOtherFrames_CropsItsSize()
    {
        var catalog = WithPlaceholder(16, 24, downRow: 2);
        var images = new TileImages(new CatalogDraft(catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        var frame = Assert.IsType<CroppedBitmap>(images.FieldSpriteFrameOf(Placeholder, Facing.Down));

        Assert.Equal(new Int32Rect(0, 48, 16, 24), frame.SourceRect);
    }

    // A placed facing (decision 572) reads its own row: left is row 1 on the placeholder's record.
    [Fact]
    public void FieldSpriteFrameOf_FacingLeft_CropsItsRow()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        var frame = Assert.IsType<CroppedBitmap>(images.FieldSpriteFrameOf(Placeholder, Facing.Left));

        Assert.Equal(new Int32Rect(0, 48, 48, 48), frame.SourceRect);
    }

    [Fact]
    public void FieldSpriteFrameOf_FileWithoutRecord_IsTheMark()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        Assert.Same(MissingArtMark.Picture, images.FieldSpriteFrameOf("nobody.png", Facing.Down));
    }

    // A frame one pixel taller than the image: the standing frame would be cropped past the edge.
    [Fact]
    public void FieldSpriteFrameOf_FramePastTheImage_IsTheMark()
    {
        var imageHeight = PngSize.Read(Path.Combine(ArtFolders.RootIn(_root), ArtFolders.FieldSprites, Placeholder)).Height;
        var catalog = WithPlaceholder(48, imageHeight + 1, downRow: 0);
        var images = new TileImages(new CatalogDraft(catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        Assert.Same(MissingArtMark.Picture, images.FieldSpriteFrameOf(Placeholder, Facing.Down));
    }

    // An import replacing the file: the frame cropped before is dropped, so the next draw crops the new image.
    [Fact]
    public void Forget_SpriteFile_CropsAnew()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => MissingArt.None);
        var before = images.FieldSpriteFrameOf(Placeholder, Facing.Down);

        images.Forget("NPC_PLACEHOLDER.png");

        Assert.NotSame(before, images.FieldSpriteFrameOf(Placeholder, Facing.Down));
    }

    // Right mirrors Left (row 1): the standing frame is Left's, flipped (decision 766).
    [Fact]
    public void FieldSpriteFrameOf_MirrorFacing_IsTheMirroredFacingsFrameFlipped()
    {
        var sheet = _catalog.FieldSprites[Placeholder];
        var mirrored = sheet with
        {
            Walk = sheet.Walk with { Sequences = new FacingSequences { Right = new FacingSequence { MirrorOf = Facing.Left } } },
        };
        var sheets = new Dictionary<string, FieldSpriteSheet>(_catalog.FieldSprites, StringComparer.Ordinal)
        {
            [Placeholder] = mirrored,
        };
        var draft = new CatalogDraft(_catalog with { FieldSprites = sheets });
        var images = new TileImages(draft, ArtFolders.RootIn(_root), () => MissingArt.None);

        var frame = Assert.IsType<TransformedBitmap>(images.FieldSpriteFrameOf(Placeholder, Facing.Right));

        Assert.Equal(new Int32Rect(0, 48, 48, 48), Assert.IsType<CroppedBitmap>(frame.Source).SourceRect);
        Assert.Equal(-1, Assert.IsType<ScaleTransform>(frame.Transform).ScaleX);
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
