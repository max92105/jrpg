using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>One row of a map list.</summary>
/// <param name="Id">The map's id, which opens it.</param>
/// <param name="Text">What the list shows: the display name, then the id, then " *" while the map is unsaved.</param>
internal sealed record MapEntry(string Id, string Text)
{
    /// <param name="map">The map as it is now, unsaved changes included: its name may not be saved yet.</param>
    /// <param name="isUnsaved">Whether the map has changes not saved yet (decision 498).</param>
    public static MapEntry Of(MapDefinition map, bool isUnsaved) =>
        new(map.Id, $"{map.Settings.DisplayName} ({map.Id}){(isUnsaved ? " *" : "")}");
}
