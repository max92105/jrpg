using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Tests.GifImport;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Strip from sheet cells' OK through the editor (decisions 988, 990), on a copy of the test world: the strip copied
/// into the effects folder at once, its pace handed on in the result, no undo step.
/// </summary>
[Collection(GifFixtures.StripFoldersCollection)]
public sealed class MainViewModelCellStripTests : IDisposable
{
    private const uint Red = 0xFFFF0000;

    private static readonly CellStrip _fireStart = new("FireStart.png", (2, 2), [new(0, 0), new(1, 0), new(0, 0)], 120);

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    // Three picks of 2 x 2 cells at 120 ms, told from the effects' unlisted 80 ms.
    [Fact]
    public void ImportCellStrip_At120Ms_WritesTheStripAndReportsItsPace()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), answer: true);

        StaThread.Run(() =>
        {
            var result = viewModel.ImportCellStrip(_fireStart, Frames(3, 2 * 2));

            Assert.Equal(["FireStart.png"], result.Imported);
            var loop = result.Animations["FireStart.png"];
            Assert.Equal((3, 120), (loop.FrameCount, loop.FrameMilliseconds));
        });

        Assert.Equal((3 * 2, 2), PngSize.Read(EffectsImage("FireStart.png")));
        Assert.Equal("Imported FireStart.png.", viewModel.ImportStatus);
    }

    // The record waits for the Effect strips page's OK (decision 988): the file copy alone is no step to undo.
    [Fact]
    public void ImportCellStrip_Written_RecordsNoUndoStep()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), answer: true);

        StaThread.Run(() => viewModel.ImportCellStrip(_fireStart, Frames(3, 2 * 2)));

        Assert.False(viewModel.History!.UndoCommand.CanExecute(null));
    }

    // FireExplosion.png is in the test world's effects folder; the answer is no.
    [Fact]
    public void ImportCellStrip_ReplaceDeclined_WritesNothing()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var bytesBefore = File.ReadAllBytes(EffectsImage("FireExplosion.png"));
        var strip = new CellStrip("FireExplosion.png", (2, 2), [new(0, 0), new(1, 0)], 120);

        var result = viewModel.ImportCellStrip(strip, Frames(2, 2 * 2));

        Assert.Equal(["FireExplosion.png is already in the effects art. Replace it?"], _content.Questions);
        Assert.Equal(([], []), (result.Imported, result.Refusals));
        Assert.Equal(bytesBefore, File.ReadAllBytes(EffectsImage("FireExplosion.png")));
    }

    // One 1 px frame more than the game's widest texture holds: Data's refusal comes back in the result, never thrown.
    [Fact]
    public void ImportCellStrip_StripTooWide_RefusalInTheResult()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), answer: true);
        var frameCount = TextureLimit.MaxSide + 1;
        var strip = new CellStrip("Wide.png", (1, 1), [.. Enumerable.Repeat(new SpriteCell(0, 0), frameCount)], 120);

        StaThread.Run(() =>
        {
            var result = viewModel.ImportCellStrip(strip, Frames(frameCount, 1));

            Assert.Empty(result.Imported);
            Assert.StartsWith("Wide.png: a 4097 px wide image", Assert.Single(result.Refusals), StringComparison.Ordinal);
        });

        Assert.False(File.Exists(EffectsImage("Wide.png")));
    }

    private static uint[][] Frames(int count, int pixelsEach) =>
        [.. Enumerable.Range(0, count).Select(_ => Enumerable.Repeat(Red, pixelsEach).ToArray())];

    private string EffectsImage(string fileName) =>
        Path.Combine(ArtFolders.RootIn(_content.ContentCopy), ArtFolders.Effects, fileName);
}
