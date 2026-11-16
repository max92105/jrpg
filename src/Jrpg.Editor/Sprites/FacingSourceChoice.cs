namespace Jrpg.Editor.Sprites;

/// <summary>
/// One entry of a facing's Shows box in the Character sprites window (decision 771): a source and its words.
/// </summary>
/// <param name="Source">What the facing reads when the entry is chosen.</param>
/// <param name="Label">The entry's words.</param>
internal sealed record FacingSourceChoice(FacingSource Source, string Label)
{
    /// <summary>The box's entries, fixed: the box binds them by <see cref="Source"/>.</summary>
    public static IReadOnlyList<FacingSourceChoice> All { get; } =
    [
        new(FacingSource.Row, "Row"),
        new(FacingSource.Cells, "Cells"),
        new(FacingSource.Mirror, "Mirror of"),
    ];
}
