namespace Jrpg.Editor.Maps;

/// <summary>One row of the layer list: a layer of the open map, or the chosen Z before anything is painted on it.</summary>
/// <param name="Z">The layer's depth.</param>
/// <param name="Name">The preset's name, or "Layer" for another Z.</param>
/// <param name="IsEmpty">Whether the map has no tiles there yet; an empty layer isn't saved.</param>
internal sealed record LayerEntry(int Z, string Name, bool IsEmpty)
{
    public string Text => IsEmpty ? $"{Name} (z {Z}), empty" : $"{Name} (z {Z})";
}
