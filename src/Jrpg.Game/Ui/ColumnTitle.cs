namespace Jrpg.Game.Ui;

/// <summary>
/// One caption over a list's column, and where it sits: so many pixels in from the row's left edge, or ending so many
/// pixels in from its right edge.
/// </summary>
internal readonly record struct ColumnTitle
{
    private ColumnTitle(string text, int offset, bool isFromRight)
    {
        Text = text;
        Offset = offset;
        IsFromRight = isFromRight;
    }

    /// <summary>The caption as written in the messages; <see cref="ColumnTitles"/> shows it in spaced caps.</summary>
    public string Text { get; }

    /// <summary>Pixels from the row's left edge to the caption's start, or from its end to the row's right edge.</summary>
    public int Offset { get; }

    public bool IsFromRight { get; }

    /// <param name="text">The caption.</param>
    /// <param name="offset">Pixels from the row's left edge to the caption's start.</param>
    public static ColumnTitle FromLeft(string text, int offset) => new(text, offset, isFromRight: false);

    /// <param name="text">The caption.</param>
    /// <param name="offset">Pixels from the caption's end to the row's right edge.</param>
    public static ColumnTitle FromRight(string text, int offset) => new(text, offset, isFromRight: true);
}
