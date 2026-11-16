using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.GifImport;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Catalogs;

/// <summary>
/// Strip from sheet cells' write (decision 990) on a copy of the test world, the frames built here: two tall cells
/// padded to squares, and a strip Data refuses.
/// </summary>
[Collection(GifFixtures.StripFoldersCollection)]
public sealed class CellStripImportTests : IDisposable
{
    private const uint Red = 0xFFFF0000;
    private const uint Green = 0xFF00FF00;

    private readonly EditorTestContent _content = new();
    private readonly CellStripImport _import;

    public CellStripImportTests()
    {
        var root = _content.CopyOfTestWorld();
        _import = new CellStripImport(new ContentWriter(root), new CatalogDraft(ContentCatalogLoader.Load(root)));
    }

    public void Dispose() => _content.Dispose();

    // Cells 2 px wide and 4 px high: each centered in a 4 px square, a column of clear pixels on either side.
    [Fact]
    public void Write_TwoTallCells_WritesASquarePaddedStrip()
    {
        StaThread.Run(() =>
        {
            _import.Write([Filled(Red, 2 * 4), Filled(Green, 2 * 4)], (2, 4), "Tall.png");

            Assert.Equal((2 * 4, 4), PngSize.Read(EffectsImage("Tall.png")));
            var firstRow = PngPixels.Of(EffectsImage("Tall.png"))[..8];
            Assert.Equal([0u, Red, Red, 0u, 0u, Green, Green, 0u], firstRow);
        });
    }

    // One frame more than the game's widest texture holds, each 1 px: Data refuses the strip once it is written.
    [Fact]
    public void Write_ImportRefuses_LeavesNoTemporaryFolder()
    {
        var frames = Enumerable.Range(0, TextureLimit.MaxSide + 1).Select(_ => Filled(Red, 1)).ToArray();
        var stripFoldersBefore = StripFolders();

        StaThread.Run(() => Assert.Throws<ContentValidationException>(() => _import.Write(frames, (1, 1), "Wide.png")));

        Assert.Equal(stripFoldersBefore, StripFolders());
        Assert.False(File.Exists(EffectsImage("Wide.png")));
    }

    private static uint[] Filled(uint color, int count) => [.. Enumerable.Repeat(color, count)];

    private static string[] StripFolders() =>
        [.. Directory.GetDirectories(Path.GetTempPath(), $"{CellStripImport.TemporaryFolderPrefix}*")
            .Order(StringComparer.Ordinal)];

    private string EffectsImage(string fileName) =>
        Path.Combine(ArtFolders.RootIn(_content.ContentCopy), ArtFolders.Effects, fileName);
}
