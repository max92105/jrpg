using Jrpg.Core.Items;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Content;

/// <summary>The state a new playthrough starts from.</summary>
public sealed record NewGameDefinition
{
    /// <summary>
    /// The characters the player may choose to lead, in the order the choice lists them. The chosen one starts alone; the
    /// others join through the story (decision 163).
    /// </summary>
    public required IReadOnlyList<LeaderChoice> LeaderChoices { get; init; }

    public required IReadOnlyList<InventoryEntry> StartingItems { get; init; }

    public required MapLocation StartLocation { get; init; }

    /// <summary>The sprite the party walks the map with, a file in the field-sprite art folder (one for everyone; question 58).</summary>
    public required string FieldSpriteFile { get; init; }
}
