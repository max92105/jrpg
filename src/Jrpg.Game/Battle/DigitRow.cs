namespace Jrpg.Game.Battle;

/// <summary>
/// Where each digit of a floating number sits along its row, from the row's left, at the font's own size: drawn digit
/// by digit so each can hop on its own (decision 650). Built once per number when its playback starts.
/// </summary>
internal readonly struct DigitRow
{
    private readonly float[] _lefts;

    private DigitRow(float[] lefts, float width)
    {
        _lefts = lefts;
        Width = width;
    }

    public float Width { get; }

    /// <param name="digits">The number's text: digits only.</param>
    /// <param name="advances">Each digit's width in the font, indexed by its value, 0 to 9.</param>
    /// <param name="spacing">Added between two digits; negative to set them tight.</param>
    public static DigitRow Of(string digits, ReadOnlySpan<float> advances, float spacing)
    {
        var lefts = new float[digits.Length];
        var left = 0f;
        for (var index = 0; index < digits.Length; index++)
        {
            lefts[index] = left;
            left += advances[digits[index] - '0'] + spacing;
        }

        return new DigitRow(lefts, digits.Length == 0 ? 0 : left - spacing);
    }

    public float LeftOf(int index) => _lefts[index];
}
