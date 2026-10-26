namespace Jrpg.Game.Ui;

/// <summary>
/// One caption over a list's column, and where it sits: so many pixels in from the row's left edge, or ending so many
/// pixels in from its right edge.
/// </summary>
internal readonly record struct ColumnTitle
{
    private ColumnTitle(string caption, int offset, bool isFromRight)
    {
        Caption = caption;
        Offset = offset;
        IsFromRight = isFromRight;
    }

    /// <summary>As the message table writes it; <see cref="ColumnTitles"/> shows it in spaced caps.</summary>
    public string Caption { get; }

    /// <summary>Pixels from the row's left edge to the caption's start, or from its end to the row's right edge.</summary>
    public int Offset { get; }

    public bool IsFromRight { get; }

    /// <param name="caption">The caption.</param>
    /// <param name="offset">Pixels from the row's left edge to the caption's start.</param>
    public static ColumnTitle FromLeft(string caption, int offset) => new(caption, offset, isFromRight: false);

    /// <param name="caption">The caption.</param>
    /// <param name="offset">Pixels from the caption's end to the row's right edge.</param>
    public static ColumnTitle FromRight(string caption, int offset) => new(caption, offset, isFromRight: true);
}
