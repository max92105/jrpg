using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// Whole frames from a GIF's patches by its disposal rules, on 4 x 4 canvases built here.
public sealed class GifFrameComposerTests
{
    private const int Size = 4;
    private const uint Clear = 0;
    private const uint Red = 0xFFFF0000;
    private const uint Green = 0xFF00FF00;
    private const uint Blue = 0xFF0000FF;

    [Fact]
    public void Compose_FullFrames_KeepsEachAsIs()
    {
        var frames = Composed(Patch(0, 0, Size, Red), Patch(0, 0, Size, Green));

        Assert.Equal((Red, Green), (frames[0].Distinct().Single(), frames[1].Distinct().Single()));
    }

    [Fact]
    public void Compose_PatchAtOffset_DrawsOverThePreviousFrame()
    {
        var second = Composed(Patch(0, 0, Size, Red), Patch(1, 1, 2, Green))[1];

        Assert.Equal((Red, Green, Green, Red), (At(second, 0, 0), At(second, 1, 1), At(second, 2, 2), At(second, 3, 3)));
    }

    [Fact]
    public void Compose_TransparentPatchPixel_LeavesWhatIsUnder()
    {
        var greenWithAHole = Patch(0, 0, Size, Green);
        greenWithAHole.Pixels[0] = Clear;

        var second = Composed(Patch(0, 0, Size, Red), greenWithAHole)[1];

        Assert.Equal((Red, Green), (At(second, 0, 0), At(second, 1, 0)));
    }

    [Fact]
    public void Compose_ClearToBackground_ClearsOnlyThatFramesRectangle()
    {
        var third = Composed(
            Patch(0, 0, Size, Red),
            Patch(1, 1, 2, Green) with { Disposal = GifDisposal.ClearToBackground },
            Patch(0, 0, 1, Blue))[2];

        Assert.Equal((Blue, Clear, Clear, Red), (At(third, 0, 0), At(third, 1, 1), At(third, 2, 2), At(third, 3, 3)));
    }

    // Frame 3 sees frame 1's canvas: frame 2 drew green everywhere, then asked for the canvas back.
    [Fact]
    public void Compose_RestorePrevious_PutsTheCanvasBack()
    {
        var third = Composed(
            Patch(0, 0, Size, Red),
            Patch(0, 0, Size, Green) with { Disposal = GifDisposal.RestorePrevious },
            Patch(0, 0, 1, Blue))[2];

        Assert.Equal((Blue, Red), (At(third, 0, 0), At(third, 1, 1)));
    }

    [Fact]
    public void Compose_PatchPastTheEdge_IsClipped()
    {
        var first = Composed(Patch(2, 2, 3, Green))[0];

        Assert.Equal((4, Clear), (first.Count(pixel => pixel == Green), At(first, 1, 1)));
    }

    [Fact]
    public void Compose_ClearPastTheEdge_ClearsTheClippedRectangle()
    {
        var third = Composed(
            Patch(0, 0, Size, Red),
            Patch(2, 2, 3, Green) with { Disposal = GifDisposal.ClearToBackground },
            Patch(0, 0, 1, Blue))[2];

        Assert.Equal((Red, Clear, Clear), (At(third, 1, 1), At(third, 2, 2), At(third, 3, 3)));
    }

    // A patch wholly right of the canvas draws and clears nothing.
    [Fact]
    public void Compose_ClearWhollyOffTheCanvas_LeavesTheCanvas()
    {
        var third = Composed(
            Patch(0, 0, Size, Red),
            Patch(5, 0, 1, Green) with { Disposal = GifDisposal.ClearToBackground },
            Patch(0, 0, 1, Blue))[2];

        Assert.Equal((Blue, Red), (At(third, 0, 0), At(third, 3, 0)));
    }

    private static IReadOnlyList<uint[]> Composed(params GifFrame[] frames) =>
        GifFrameComposer.Compose(new GifImage(Size, Size, frames));

    // A square patch of one color, kept after it shows.
    private static GifFrame Patch(int left, int top, int side, uint color) =>
        new(left, top, side, side, Enumerable.Repeat(color, side * side).ToArray(), 10, GifDisposal.Keep);

    private static uint At(uint[] canvas, int x, int y) => canvas[(y * Size) + x];
}
