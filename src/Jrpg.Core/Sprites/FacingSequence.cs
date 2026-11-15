namespace Jrpg.Core.Sprites;

/// <summary>
/// One facing's frames read off its sheet in order, in place of its row (ADR 0054): cells picked anywhere on the sheet,
/// repeats allowed, the first one the standing frame (decision 764). Compares by its cells in order (decision 769), so
/// two records built apart from the same cells are equal, as a drop table compares its entries.
/// </summary>
public sealed record FacingSequence
{
    /// <summary>
    /// At least one cell once Data has checked it (decision 763); a single cell stands still while walking.
    /// </summary>
    public IReadOnlyList<SpriteCell> Cells { get; init; } = [];

    public bool Equals(FacingSequence? other) => other is not null && Cells.SequenceEqual(other.Cells);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var cell in Cells)
        {
            hash.Add(cell);
        }

        return hash.ToHashCode();
    }
}
