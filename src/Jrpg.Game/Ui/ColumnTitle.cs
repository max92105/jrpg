namespace Jrpg.Game.Ui;

/// <summary>
/// One caption over a list's column, and where it sits: so many pixels in from the row's left edge, or ending so many
/// pixels in from its right edge.
/// </summary>
internal readonly record struct ColumnTitle
{
    private ColumnTitle(MeasuredText text, int offset, bool isFromRight)
    {
        Text = text;
        Offset = offset;
        IsFromRight = isFromRight;
    }

    public MeasuredText Text { get; }

    /// <summary>Pixels from the row's left edge to the caption's start, or from its end to the row's right edge.</summary>
    public int Offset { get; }

    public bool IsFromRight { get; }

    /// <param name="text">The caption.</param>
    /// <param name="offset">Pixels from the row's left edge to the caption's start.</param>
    public static ColumnTitle FromLeft(MeasuredText text, int offset) => new(text, offset, isFromRight: false);

    /// <param name="text">The caption, measured, since its width places it.</param>
    /// <param name="offset">Pixels from the caption's end to the row's right edge.</param>
    public static ColumnTitle FromRight(MeasuredText text, int offset) => new(text, offset, isFromRight: true);
}
