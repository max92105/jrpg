using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// Where each digit lies in a digit strip image (decision 653): ten cells side by side, "0" to "9", each digit cut to
/// its cell's opaque columns so a narrow "1" sits as tight as the others. Measured once from the image's alpha.
/// </summary>
internal sealed class DigitStrip
{
    private const int DigitCount = 10;

    private readonly Rectangle[] _sources;
    private readonly float[] _advances;

    private DigitStrip(Rectangle[] sources, int height)
    {
        _sources = sources;
        _advances = [.. sources.Select(source => (float)source.Width)];
        Height = height;
    }

    /// <summary>The strip's height, every digit's: the row's bottom is each digit's.</summary>
    public int Height { get; }

    /// <param name="alpha">The image's alpha, row by row.</param>
    /// <param name="imageSize">The image's width and height in pixels.</param>
    /// <param name="file">The image's name, for the message.</param>
    /// <exception cref="InvalidOperationException">
    /// The width isn't ten whole cells, or a cell holds no ink; the message names the file.
    /// </exception>
    public static DigitStrip Measure(ReadOnlySpan<byte> alpha, Point imageSize, string file)
    {
        if (imageSize.X % DigitCount != 0)
        {
            throw new InvalidOperationException($"{file}: a {imageSize.X}x{imageSize.Y} strip isn't ten digit cells.");
        }

        var cellWidth = imageSize.X / DigitCount;
        var sources = new Rectangle[DigitCount];
        for (var digit = 0; digit < DigitCount; digit++)
        {
            var cellLeft = digit * cellWidth;
            var first = FirstInkedColumn(alpha, imageSize, cellLeft, cellLeft + cellWidth)
                ?? throw new InvalidOperationException($"{file}: the cell of the digit {digit} is empty.");
            var last = LastInkedColumn(alpha, imageSize, cellLeft, cellLeft + cellWidth)!.Value;
            sources[digit] = new Rectangle(first, 0, last - first + 1, imageSize.Y);
        }

        return new DigitStrip(sources, imageSize.Y);
    }

    /// <summary>The digit's part of the image: its cell's opaque columns, the strip's full height.</summary>
    public Rectangle SourceOf(int digit) => _sources[digit];

    /// <summary>
    /// Each digit's measured width, by its value from 0 to 9: where the next digit starts after it, before the spacing.
    /// </summary>
    public ReadOnlySpan<float> Advances => _advances;

    private static int? FirstInkedColumn(ReadOnlySpan<byte> alpha, Point imageSize, int from, int to)
    {
        for (var column = from; column < to; column++)
        {
            if (HasInk(alpha, imageSize, column))
            {
                return column;
            }
        }

        return null;
    }

    private static int? LastInkedColumn(ReadOnlySpan<byte> alpha, Point imageSize, int from, int to)
    {
        for (var column = to - 1; column >= from; column--)
        {
            if (HasInk(alpha, imageSize, column))
            {
                return column;
            }
        }

        return null;
    }

    private static bool HasInk(ReadOnlySpan<byte> alpha, Point imageSize, int column)
    {
        for (var row = 0; row < imageSize.Y; row++)
        {
            if (alpha[(row * imageSize.X) + column] > 0)
            {
                return true;
            }
        }

        return false;
    }
}
