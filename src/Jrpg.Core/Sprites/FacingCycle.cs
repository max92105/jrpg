using Jrpg.Core.Maps;

namespace Jrpg.Core.Sprites;

/// <summary>
/// A loop of frames per facing (ADR 0043): on each facing's row, <see cref="Frames"/> run rightward from column 0
/// (decision 566, ADR 0039's shape), the first one the standing frame (decision 564).
/// </summary>
public sealed record FacingCycle
{
    public required FacingRows Rows { get; init; }

    public required TileAnimation Frames { get; init; }
}
