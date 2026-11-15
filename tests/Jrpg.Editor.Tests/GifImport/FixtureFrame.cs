namespace Jrpg.Editor.Tests.GifImport;

/// <summary>One frame of a fixture GIF: a patch of palette indices, row by row.</summary>
/// <param name="Left">Where the patch lies on the canvas.</param>
/// <param name="Top">Where the patch lies on the canvas.</param>
/// <param name="Width">The patch's width.</param>
/// <param name="Height">The patch's height.</param>
/// <param name="Colors">One palette index per pixel (see <see cref="GifFixtures"/>).</param>
internal sealed record FixtureFrame(int Left, int Top, int Width, int Height, byte[] Colors)
{
    /// <summary>Hundredths of a second; null writes no timing block, so no disposal or transparency either.</summary>
    public int? DelayCentiseconds { get; init; } = 10;

    /// <summary>As stored, 0 to 7.</summary>
    public int Disposal { get; init; }

    public byte? TransparentColor { get; init; }

    public static FixtureFrame Filled(int left, int top, int width, int height, byte color) =>
        new(left, top, width, height, Enumerable.Repeat(color, width * height).ToArray());
}
