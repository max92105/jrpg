namespace Jrpg.Core.Maps;

/// <summary>One part of a map to draw and where it falls in the draw order (decision 520).</summary>
/// <param name="Key">Where the part sorts.</param>
/// <param name="Kind">What it is.</param>
/// <param name="Index">The layer's or the placed object's index in the map, 0 for the player.</param>
/// <param name="Z">The layer the part belongs to, which the editor dims by: a canopy's is the canopy layer.</param>
public readonly record struct MapDrawItem(DrawOrderKey Key, MapDrawItemKind Kind, int Index, int Z);
