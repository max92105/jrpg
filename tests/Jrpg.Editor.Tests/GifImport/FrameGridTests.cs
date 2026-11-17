using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// Frames laid row by row in one sheet within the 4096 px texture limit (ADR 0059), read back from the file. Frames 1000
// px wide fit four to a row, a step that doesn't divide the limit.
public sealed class FrameGridTests : IDisposable
{
    private const int FrameWidth = 1000;
    private const int FrameHeight = 2;
    private const int SheetWidth = 4 * FrameWidth;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-grid-").FullName;

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void ColumnsFor_FewerFramesThanFit_IsTheFrameCount()
    {
        Assert.Equal(3, FrameGrid.ColumnsFor(480, 3));
    }

    [Fact]
    public void ColumnsFor_MoreFramesThanFit_IsWhatFits()
    {
        Assert.Equal(8, FrameGrid.ColumnsFor(480, 24));
    }

    [Fact]
    public void MostFrames_960By540_Is28()
    {
        Assert.Equal(28, FrameGrid.MostFrames(960, 540));
    }

    [Fact]
    public void SheetSize_TenFramesInFourColumns_IsThreeRowsHigh()
    {
        Assert.Equal((SheetWidth, 3 * FrameHeight), FrameGrid.SheetSize(FrameWidth, FrameHeight, 10));
    }

    // 1024 x 2048 frames: four columns, two rows, eight frames at most.
    [Fact]
    public void SheetSize_AsManyFramesAsFit_IsTheWholeLimit()
    {
        Assert.Equal((4096, 4096), FrameGrid.SheetSize(1024, 2048, 8));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(int.MaxValue)]
    public void SheetSize_MoreFramesThanFit_Throws(int frameCount)
    {
        Assert.Throws<ArgumentException>(() => FrameGrid.SheetSize(1024, 2048, frameCount));
    }

    [Fact]
    public void WritePng_TenFramesInFourColumns_PutsFrameFiveAtColumnOneRowOne()
    {
        var path = Path.Combine(_folder, "Storm.png");

        StaThread.Run(() =>
        {
            FrameGrid.WritePng(FlatFrames(10), FrameWidth, FrameHeight, path);

            var pixels = PngPixels.Of(path);
            Assert.Equal((ColorOf(5), ColorOf(5)), (PixelAt(pixels, FrameWidth, 2), PixelAt(pixels, (2 * FrameWidth) - 1, 3)));
            Assert.Equal(ColorOf(4), PixelAt(pixels, FrameWidth - 1, 2));
        });
    }

    // Frames 8 and 9 fill the third row's first two cells; the other two stay empty.
    [Fact]
    public void WritePng_LastRowPartial_LeavesTheEmptyCellsTransparent()
    {
        var path = Path.Combine(_folder, "Storm.png");

        StaThread.Run(() =>
        {
            FrameGrid.WritePng(FlatFrames(10), FrameWidth, FrameHeight, path);

            var pixels = PngPixels.Of(path);
            Assert.Equal(ColorOf(9), PixelAt(pixels, (2 * FrameWidth) - 1, 5));
            Assert.Equal((0u, 0u), (PixelAt(pixels, 2 * FrameWidth, 4) >> 24, PixelAt(pixels, SheetWidth - 1, 5) >> 24));
        });
    }

    // Core's rule for where a frame sits, which the game and the art check read, finds each frame where it was written.
    [Fact]
    public void WritePng_EveryFrame_IsAtItsRecordsFrameOrigin()
    {
        var path = Path.Combine(_folder, "Storm.png");
        var record = new AnimatedBackgroundDefinition
        {
            File = "Storm.png",
            FrameWidth = FrameWidth,
            FrameHeight = FrameHeight,
            Animation = new TileAnimation { FrameCount = 10, FrameMilliseconds = 100 },
        };

        StaThread.Run(() =>
        {
            FrameGrid.WritePng(FlatFrames(10), FrameWidth, FrameHeight, path);

            var pixels = PngPixels.Of(path);
            var colorsAtOrigins = Enumerable.Range(0, 10)
                .Select(frame => record.FrameOrigin(frame, SheetWidth))
                .Select(origin => PixelAt(pixels, origin.X, origin.Y));
            Assert.Equal(Enumerable.Range(0, 10).Select(ColorOf), colorsAtOrigins);
        });
    }

    // Nine frames where eight fit: refused on the count, before a frame is read.
    [Fact]
    public void WritePng_FramesThatDontFit_Throws()
    {
        var path = Path.Combine(_folder, "Storm.png");
        uint[][] frames = [.. Enumerable.Repeat(Array.Empty<uint>(), 9)];

        Assert.Throws<ArgumentException>(() => FrameGrid.WritePng(frames, 1024, 2048, path));
        Assert.False(File.Exists(path));
    }

    private static uint ColorOf(int frame) => 0xFF000000u | (uint)((frame * 20) + 10);

    private static uint[][] FlatFrames(int count) =>
        [.. Enumerable.Range(0, count).Select(frame => Enumerable.Repeat(ColorOf(frame), FrameWidth * FrameHeight).ToArray())];

    private static uint PixelAt(uint[] pixels, int x, int y) => pixels[(y * SheetWidth) + x];
}
