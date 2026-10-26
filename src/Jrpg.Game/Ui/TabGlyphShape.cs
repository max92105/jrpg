using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The placeholder tab icons' pixels (decision 254): one 12 x 12 mask per <see cref="TabGlyph"/>, white where the mask
/// has '#', transparent elsewhere. Drawn at 2x with point sampling, so they stay pixel-crisp.
/// </summary>
internal static class TabGlyphShape
{
    /// <summary>The masks' width and height, in pixels.</summary>
    public const int Size = 12;

    private static readonly FrozenDictionary<TabGlyph, ImmutableArray<string>> _masks = new Dictionary<TabGlyph, ImmutableArray<string>>
    {
        [TabGlyph.Items] =
        [
            "....####....",
            ".....##.....",
            ".....##.....",
            "....#..#....",
            "...#....#...",
            "..#......#..",
            ".#........#.",
            ".##########.",
            ".##########.",
            ".##########.",
            "..########..",
            "............",
        ],
        [TabGlyph.Party] =
        [
            "............",
            "..##....##..",
            ".####..####.",
            ".####..####.",
            "..##....##..",
            "............",
            ".####..####.",
            "#####..#####",
            "#####..#####",
            "#####..#####",
            "............",
            "............",
        ],
        [TabGlyph.Equipment] =
        [
            "..........##",
            ".........###",
            "........###.",
            ".......###..",
            "......###...",
            ".....###....",
            "..#.###.....",
            "...###......",
            "...##.......",
            "..#..#......",
            ".#..........",
            "#...........",
        ],
        [TabGlyph.Status] =
        [
            "............",
            ".....##.....",
            "....####....",
            "....####....",
            ".....##.....",
            "............",
            "...######...",
            "..########..",
            "..########..",
            "..########..",
            "............",
            "............",
        ],
        [TabGlyph.Options] =
        [
            ".....##.....",
            "..#.####.#..",
            "...######...",
            ".###....###.",
            ".##......##.",
            "###......###",
            "###......###",
            ".##......##.",
            ".###....###.",
            "...######...",
            "..#.####.#..",
            ".....##.....",
        ],
        [TabGlyph.System] =
        [
            "..########..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..#....#.#..",
            "..#....#.#..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "############",
        ],
    }.ToFrozenDictionary();

    /// <returns><see cref="Size"/> x <see cref="Size"/> premultiplied pixels, row by row.</returns>
    /// <exception cref="InvalidOperationException">The mask isn't <see cref="Size"/> pixels square.</exception>
    public static Color[] Pixels(TabGlyph glyph)
    {
        var mask = _masks[glyph];
        if (mask.Length != Size || mask.Any(row => row.Length != Size))
        {
            throw new InvalidOperationException($"The {glyph} glyph's mask isn't {Size} x {Size}.");
        }

        var pixels = new Color[Size * Size];
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                pixels[(row * Size) + column] = mask[row][column] == '#' ? Color.White : Color.Transparent;
            }
        }

        return pixels;
    }
}
