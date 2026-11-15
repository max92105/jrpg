using Jrpg.Core.Geometry;

namespace Jrpg.Core.Sprites;

/// <summary>
/// One facing's frames in place of its row (ADR 0054): cells picked anywhere on the sheet, repeats allowed, the first
/// one the standing frame (decision 764); or the mirror of another facing, its frames flipped left to right (decision
/// 766). Never both: the loader refuses it. Compares by its cells in order and its mirror (decision 769), so two
/// records built apart from the same cells are equal, as a drop table compares its entries.
/// </summary>
public sealed record FacingSequence
{
    /// <summary>
    /// At least one cell once Data has checked it (decision 763); a single cell stands still while walking.
    /// </summary>
    public IReadOnlyList<SpriteCell> Cells { get; init; } = [];

    /// <summary>
    /// The facing whose frames this one shows flipped; null for a sequence of cells. One hop: the loader refuses a
    /// mirror of itself or of another mirror (decision 766).
    /// </summary>
    public Facing? MirrorOf { get; init; }

    public bool Equals(FacingSequence? other) =>
        other is not null && MirrorOf == other.MirrorOf && Cells.SequenceEqual(other.Cells);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MirrorOf);
        foreach (var cell in Cells)
        {
            hash.Add(cell);
        }

        return hash.ToHashCode();
    }
}
