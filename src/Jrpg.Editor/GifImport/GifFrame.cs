namespace Jrpg.Editor.GifImport;

/// <summary>
/// One frame as a GIF stores it: a patch of the canvas, which <see cref="GifFrameComposer"/> makes whole.
/// </summary>
/// <param name="Left">Where the patch starts on the canvas, in pixels; it may lie past the canvas's edge.</param>
/// <param name="Top">Where the patch starts on the canvas, in pixels.</param>
/// <param name="Width">The patch's width in pixels.</param>
/// <param name="Height">The patch's height in pixels.</param>
/// <param name="Pixels">
/// The patch's pixels, row by row, each BGRA packed little-endian (alpha in the top byte). An array: two frames compare
/// it by reference, and <c>with</c> shares it.
/// </param>
/// <param name="DelayCentiseconds">How long it shows, in hundredths of a second; 0 without a timing block.</param>
/// <param name="Disposal">What happens to the canvas after it.</param>
internal sealed record GifFrame(
    int Left, int Top, int Width, int Height, uint[] Pixels, int DelayCentiseconds, GifDisposal Disposal);
