using System.Collections.Frozen;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menu tabs' placeholder icons as pixels (decision 254): a 12 x 12 mask per <see cref="TabGlyph"/>, white where
/// the mask has <c>#</c>, drawn at 2x and point-sampled so they stay crisp. Art replaces them when it exists.
/// </summary>
internal static class TabGlyphShape
{
    /// <summary>A mask's side in pixels.</summary>
    public const int Size = 12;

    private static readonly FrozenDictionary<TabGlyph, string[]> _masks = new Dictionary<TabGlyph, string[]>
    {
        [TabGlyph.Status] =
        [
            "............",
            "....####....",
            "....####....",
            "....####....",
            "....####....",
            "............",
            "..########..",
            ".##########.",
            ".##########.",
            ".##########.",
            "............",
            "............",
        ],
        [TabGlyph.Items] =
        [
            "....####....",
            ".....##.....",
            ".....##.....",
            "....#..#....",
            "...#....#...",
            "..#......#..",
            ".##########.",
            ".##########.",
            ".##########.",
            ".##########.",
            "..########..",
            "............",
        ],
        [TabGlyph.Equipment] =
        [
            ".....##.....",
            ".....##.....",
            ".....##.....",
            ".....##.....",
            ".....##.....",
            ".....##.....",
            ".....##.....",
            "..########..",
            ".....##.....",
            ".....##.....",
            "....####....",
            "............",
        ],
        [TabGlyph.Party] =
        [
            "............",
            "............",
            ".###....###.",
            ".###....###.",
            ".###....###.",
            "............",
            "#####..#####",
            "#####..#####",
            "#####..#####",
            "#####..#####",
            "............",
            "............",
        ],
        [TabGlyph.Options] =
        [
            "............",
            ".....##.....",
            "..#.####.#..",
            "...######...",
            "..###..###..",
            ".###....###.",
            ".###....###.",
            "..###..###..",
            "...######...",
            "..#.####.#..",
            ".....##.....",
            "............",
        ],
        [TabGlyph.System] =
        [
            "............",
            "..########..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..#....#.#..",
            "..#......#..",
            "..#......#..",
            "..#......#..",
            "..########..",
            "............",
        ],
    }.ToFrozenDictionary();

    /// <returns><see cref="Size"/> x <see cref="Size"/> premultiplied pixels, row by row, white or clear.</returns>
    public static Color[] Pixels(TabGlyph glyph) =>
        [.. _masks[glyph].SelectMany(row => row).Select(cell => cell == '#' ? Color.White : Color.Transparent)];
}
