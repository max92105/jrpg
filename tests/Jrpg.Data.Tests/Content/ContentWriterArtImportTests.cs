using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Art;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// One image import for every art folder but the tilesheets, each with its folder's own check: what the editor's
/// Character sprites page and plan 2's enemy art use.
/// </summary>
public sealed class ContentWriterArtImportTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-art-import", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterArtImportTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void ImportArtImage_NewFieldSprite_CopiesIt()
    {
        var source = PngHeaderFile.Write(ImportSource("Villager.png"), width: 128, height: 224);
        Assert.False(_writer.HasArtImage(ArtFolders.FieldSprites, "Villager.png"));

        var imported = _writer.ImportArtImage(ArtFolders.FieldSprites, source, _catalog);

        Assert.Equal("Villager.png", imported);
        Assert.True(_writer.HasArtImage(ArtFolders.FieldSprites, "Villager.png"));
    }

    // The placeholder's record needs 8 x 4 frames of 48 px: one frame of it is refused, the folder untouched.
    [Fact]
    public void ImportArtImage_FieldSpriteTooSmallForItsRecord_RefusesAndCopiesNothing()
    {
        var source = PngHeaderFile.Write(ImportSource("npc_placeholder.png"), width: 48, height: 48);
        var before = File.ReadAllBytes(ArtPath(ArtFolders.FieldSprites, "npc_placeholder.png"));

        var exception = Assert.Throws<ContentValidationException>(() => Import(ArtFolders.FieldSprites, source));

        Assert.Equal(
            "npc_placeholder.png: a 48x48 sprite doesn't hold the 8 x 4 frames of 48 x 48 px its record needs.",
            exception.Message);
        Assert.Equal(before, File.ReadAllBytes(ArtPath(ArtFolders.FieldSprites, "npc_placeholder.png")));
    }

    [Fact]
    public void ImportArtImage_EffectNotWholeFrames_Refuses()
    {
        var source = PngHeaderFile.Write(ImportSource("Slash.png"), width: 300, height: 128);

        var exception = Assert.Throws<ContentValidationException>(() => Import(ArtFolders.Effects, source));

        Assert.Equal("Slash.png: a 300x128 strip isn't a row of whole square frames.", exception.Message);
        Assert.False(_writer.HasArtImage(ArtFolders.Effects, "Slash.png"));
    }

    [Fact]
    public void ImportArtImage_Battler_CopiesIt()
    {
        var source = PngHeaderFile.Write(ImportSource("Wolf.png"), width: 200, height: 150);

        _writer.ImportArtImage(ArtFolders.Battlers, source, _catalog);

        Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(ArtPath(ArtFolders.Battlers, "Wolf.png")));
    }

    [Fact]
    public void ImportArtImage_BattlerWiderThan4096_IsRefusedNamingTheWidth()
    {
        var source = PngHeaderFile.Write(ImportSource("TooWideBattler.png"), width: 4097, height: 128);

        var exception = Assert.Throws<ContentValidationException>(() => Import(ArtFolders.Battlers, source));

        Assert.Equal(
            "TooWideBattler.png: a 4097 px wide image is wider than the game loads (4096 px); import it at a smaller height.",
            exception.Message);
        Assert.False(_writer.HasArtImage(ArtFolders.Battlers, "TooWideBattler.png"));
    }

    [Fact]
    public void ImportArtImage_Battler4096Wide_IsCopied()
    {
        var source = PngHeaderFile.Write(ImportSource("Wide.png"), width: 4096, height: 128);

        Import(ArtFolders.Battlers, source);

        Assert.True(_writer.HasArtImage(ArtFolders.Battlers, "Wide.png"));
    }

    // 41 square frames of 100 px: a whole strip, refused for its width alone.
    [Fact]
    public void ImportArtImage_EffectWiderThan4096_IsRefused()
    {
        var source = PngHeaderFile.Write(ImportSource("Long.png"), width: 4100, height: 100);

        Assert.Throws<ContentValidationException>(() => Import(ArtFolders.Effects, source));
        Assert.False(_writer.HasArtImage(ArtFolders.Effects, "Long.png"));
    }

    [Fact]
    public void ImportArtImage_PortraitWiderThan4096_IsCopied()
    {
        var source = PngHeaderFile.Write(ImportSource("Panorama.png"), width: 4097, height: 128);

        Import(ArtFolders.Portraits, source);

        Assert.True(_writer.HasArtImage(ArtFolders.Portraits, "Panorama.png"));
    }

    [Fact]
    public void ImportArtImage_BackgroundTallerThan4096_IsRefusedAndNotCopied()
    {
        var source = PngHeaderFile.Write(ImportSource("Tower.png"), width: 2000, height: 4097);

        var exception = Assert.Throws<ContentValidationException>(() => Import(ArtFolders.Backgrounds, source));

        Assert.Equal(
            "Tower.png: a 2000 x 4097 px image is larger than the game loads (4096 px a side).", exception.Message);
        Assert.False(_writer.HasArtImage(ArtFolders.Backgrounds, "Tower.png"));
    }

    [Fact]
    public void ImportArtImage_Background4096Square_IsCopied()
    {
        var source = PngHeaderFile.Write(ImportSource("Square.png"), width: 4096, height: 4096);

        Import(ArtFolders.Backgrounds, source);

        Assert.True(_writer.HasArtImage(ArtFolders.Backgrounds, "Square.png"));
    }

    [Fact]
    public void ImportArtImage_NotAPng_Refuses()
    {
        var source = ImportSource("Notes.png");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllText(source, "not an image");

        Assert.Throws<ContentValidationException>(() => _writer.ImportArtImage(ArtFolders.Portraits, source, _catalog));
        Assert.False(_writer.HasArtImage(ArtFolders.Portraits, "Notes.png"));
    }

    [Fact]
    public void ImportArtImage_TilesheetFolder_Throws()
    {
        var source = PngHeaderFile.Write(ImportSource("Grass.png"), width: 16, height: 16);

        Assert.Throws<ArgumentException>(() => _writer.ImportArtImage(ArtFolders.Tilesheets, source, _catalog));
    }

    // An image without a record yet (the window sets the record up after the import): any PNG is taken, the start
    // checks it against the record later.
    [Fact]
    public void ImportArtImage_FieldSpriteWithoutRecord_CopiesIt()
    {
        var source = PngHeaderFile.Write(ImportSource("Tiny.png"), width: 8, height: 8);

        _writer.ImportArtImage(ArtFolders.FieldSprites, source, _catalog);

        Assert.True(_writer.HasArtImage(ArtFolders.FieldSprites, "Tiny.png"));
    }

    private string Import(string folder, string source) => _writer.ImportArtImage(folder, source, _catalog);

    private string ArtPath(string folder, string fileName) => Path.Combine(ArtFolders.RootIn(_content.Path), folder, fileName);

    // Where a test puts an image before importing it: outside the art folder, as a file the developer picked would be.
    private string ImportSource(string fileName) => Path.Combine(_content.Path, "import-source", fileName);
}
