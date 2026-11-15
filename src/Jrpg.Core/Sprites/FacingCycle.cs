using Jrpg.Core.Maps;

namespace Jrpg.Core.Sprites;

/// <summary>
/// A loop of frames per facing (ADR 0043): a facing reads its sequence when it has one (ADR 0054), else its row, where
/// <see cref="Frames"/> run rightward from column 0 (decision 566, ADR 0039's shape); the first frame is the standing
/// one (decision 564). Every facing loops over its own frame count at <see cref="Frames"/>' frame time (decision 765).
/// A row under a sequence, and the frame count once every facing has one, are kept as written, though then nothing
/// reads them (decision 767).
/// </summary>
public sealed record FacingCycle
{
    public required FacingRows Rows { get; init; }

    public required TileAnimation Frames { get; init; }

    /// <summary>The facings whose sequence overrides their row; null when every facing reads its row.</summary>
    public FacingSequences? Sequences { get; init; }
}
