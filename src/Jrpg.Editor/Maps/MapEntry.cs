using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>One row of a map list.</summary>
/// <param name="Id">The map's id, which opens it.</param>
/// <param name="Text">What the list shows: the display name, then the id, then " *" while the map is unsaved.</param>
/// <param name="Folder">The folder the tree lists the map under (decision 532); null for the top.</param>
internal sealed record MapEntry(string Id, string Text, string? Folder = null)
{
    /// <param name="map">
    /// The map as it is now, unsaved changes included: its name or folder may not be saved yet.
    /// </param>
    /// <param name="isUnsaved">Whether the map has changes not saved yet (decision 498).</param>
    public static MapEntry Of(MapDefinition map, bool isUnsaved) =>
        new(map.Id, $"{map.Settings.DisplayName} ({map.Id}){(isUnsaved ? " *" : "")}", map.Settings.Folder);

    /// <summary>Whether the filter box's text shows the row: part of its text, any case (decision 504).</summary>
    public bool Matches(string filter) => Text.Contains(filter, StringComparison.OrdinalIgnoreCase);
}
