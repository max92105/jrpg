namespace Jrpg.Editor.Maps;

/// <summary>
/// One row of the layer list: a layer of the open map (tiles or objects on it), or the chosen Z while it is still bare.
/// </summary>
/// <param name="Z">The layer's depth.</param>
/// <param name="Name">The preset's name, or "Layer" for another Z.</param>
/// <param name="IsEmpty">Whether the map has neither tiles nor objects there yet; an empty layer isn't saved.</param>
internal sealed record LayerEntry(int Z, string Name, bool IsEmpty)
{
    public string Text => IsEmpty ? $"{Name} (z {Z}), empty" : $"{Name} (z {Z})";
}
