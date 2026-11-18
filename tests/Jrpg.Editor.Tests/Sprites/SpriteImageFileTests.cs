using Jrpg.Data.Art;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// An art folder's picture decoded for a page's preview: a PNG whose header reads but whose data the decoder refuses
// reads as missing rather than throwing, and stays free to be replaced. WPF imaging runs on an STA thread.
public sealed class SpriteImageFileTests : IDisposable
{
    private const int Side = 32;
    private const int PreviewWidth = 160;

    // The signature and the IHDR chunk are 33 bytes: the cut keeps them and a little of the image data.
    private const int CutLength = 60;

    private readonly string _artRoot = Directory.CreateTempSubdirectory("jrpg-sprite-").FullName;

    public SpriteImageFileTests() => Directory.CreateDirectory(Path.Combine(_artRoot, ArtFolders.Battlers));

    public void Dispose() => Directory.Delete(_artRoot, recursive: true);

    [Fact]
    public void LoadAtWidth_WholePng_DecodesAtTheWidth()
    {
        StaThread.Run(() =>
        {
            WriteBattler("Whole.png");

            var image = SpriteImageFile.LoadAtWidth(_artRoot, ArtFolders.Battlers, "Whole.png", PreviewWidth);

            Assert.Equal(PreviewWidth, image?.PixelWidth);
        });
    }

    [Fact]
    public void LoadAtWidth_PngCutAfterItsHeader_IsNull()
    {
        StaThread.Run(() =>
        {
            WriteCutBattler("Cut.png");

            Assert.Null(SpriteImageFile.LoadAtWidth(_artRoot, ArtFolders.Battlers, "Cut.png", PreviewWidth));
        });
    }

    // An import replaces the file the preview failed on.
    [Fact]
    public void LoadAtWidth_PngCutAfterItsHeader_LeavesTheFileFree()
    {
        StaThread.Run(() =>
        {
            var path = WriteCutBattler("Cut.png");
            SpriteImageFile.LoadAtWidth(_artRoot, ArtFolders.Battlers, "Cut.png", PreviewWidth);

            File.Delete(path);

            Assert.False(File.Exists(path));
        });
    }

    private string WriteBattler(string file)
    {
        var path = Path.Combine(_artRoot, ArtFolders.Battlers, file);
        FrameStrip.WritePng([Noise()], Side, Side, path);
        return path;
    }

    // Opaque pixels that don't repeat, so the compressed data runs far past the cut.
    private static uint[] Noise() => [.. Enumerable.Range(0, Side * Side).Select(i => 0xFF000000 | ((uint)i * 2654435761u))];

    private string WriteCutBattler(string file)
    {
        var path = WriteBattler(file);
        File.WriteAllBytes(path, File.ReadAllBytes(path)[..CutLength]);
        Assert.True(PngSize.IsReadable(path));
        return path;
    }
}
