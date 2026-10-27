using Jrpg.Core.Saving;

namespace Jrpg.Data.Saving;

/// <summary>
/// A save file: the snapshot inside a format version envelope (ADR 0007). A Data type because the file holds more than
/// the model (ADR 0002 option C).
/// </summary>
internal sealed record SaveFile(int FormatVersion, SaveGame Game)
{
    /// <summary>The format this build writes; until release, older ones are refused (ADR 0022).</summary>
    public const int CurrentFormatVersion = 4;
}
