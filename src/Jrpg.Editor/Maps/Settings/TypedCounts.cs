using System.Globalization;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>
/// Whole numbers typed into the map dialogs' boxes. The dialogs keep the text as typed and parse it here, so a box
/// left empty or holding a typo is refused instead of keeping the last number that happened to parse.
/// </summary>
internal static class TypedCounts
{
    /// <summary>The largest width or height a map can have (placeholder limit).</summary>
    public const int MaxMapSide = 1000;

    public static string MapSizeProblem { get; } = $"Width and height must be whole numbers from 1 to {MaxMapSide}.";

    /// <returns>The number, when the text is only digits giving 1 to <paramref name="maximum"/>; otherwise null.</returns>
    public static int? Parse(string text, int maximum) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number >= 1 && number <= maximum
            ? number
            : null;

    /// <returns>The size, when both texts are valid map sides; otherwise null.</returns>
    public static (int Width, int Height)? ParseMapSize(string widthText, string heightText) =>
        Parse(widthText, MaxMapSide) is { } width && Parse(heightText, MaxMapSide) is { } height ? (width, height) : null;
}
