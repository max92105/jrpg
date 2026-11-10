using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>
/// The map's parts in the order every drawer draws them (decision 520): its tile layers, its placed objects and their
/// canopies and, in the game, the player, sorted by <see cref="DrawOrderKey"/>, so the editor shows what the game
/// draws. Fills a caller's list, allocating nothing once it has grown: the game fills it every update.
/// </summary>
public static class MapDrawOrder
{
    /// <summary>
    /// The player's sprite is one cell tall and centered on its position, so the feet are half a cell below it.
    /// </summary>
    public const double PlayerFeetBelowPosition = 0.5;

    // A Comparison, not an IComparer: List.Sort wraps a comparer in a new delegate on every call.
    private static readonly Comparison<MapDrawItem> _byKey = (left, right) => left.Key.CompareTo(right.Key);

    /// <param name="into">Cleared, then filled.</param>
    /// <param name="layout">The map whose layers and placed objects are drawn.</param>
    /// <param name="definitionOf">What each placed object is now, after any swap, for its height and canopy.</param>
    public static void Fill(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf)
    {
        AddParts(into, layout, definitionOf);
        into.Sort(_byKey);
    }

    /// <summary>
    /// <see cref="Fill"/> with the player, placed after every object in insertion order, so a tie with an object's base
    /// never depends on the sort.
    /// </summary>
    /// <param name="into">Cleared, then filled.</param>
    /// <param name="layout">The map whose layers and placed objects are drawn.</param>
    /// <param name="definitionOf">What each placed object is now, after any swap, for its height and canopy.</param>
    /// <param name="playerPosition">The player's position, in cells.</param>
    public static void FillWithPlayer(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf, Point2D playerPosition)
    {
        AddParts(into, layout, definitionOf);
        var feet = DrawOrderKey.ForSprite(MapLayers.PlayerZ, playerPosition.Y + PlayerFeetBelowPosition, layout.Objects.Count);
        into.Add(new MapDrawItem(feet, MapDrawItemKind.Player, 0, MapLayers.PlayerZ));
        into.Sort(_byKey);
    }

    /// <summary>
    /// A placed object's body key: it sorts as a whole by its bottom edge, so overlapping objects never interleave
    /// their tiles; <paramref name="order"/> is the object's index in the map, for objects with the same base.
    /// </summary>
    public static DrawOrderKey SpriteKeyOf(PlacedObject placed, ObjectDefinition definition, int order) =>
        DrawOrderKey.ForSprite(placed.Z, BaseOf(placed, definition), order);

    /// <summary>A placed object's canopy key: its above-player zones, drawn over the player (decision 521).</summary>
    public static DrawOrderKey CanopyKeyOf(PlacedObject placed, ObjectDefinition definition, int order) =>
        DrawOrderKey.ForCanopy(placed.Z, BaseOf(placed, definition), order);

    private static void AddParts(List<MapDrawItem> into, MapLayout layout, Func<PlacedObject, ObjectDefinition> definitionOf)
    {
        into.Clear();
        for (var index = 0; index < layout.Layers.Count; index++)
        {
            var z = layout.Layers[index].Z;
            into.Add(new MapDrawItem(DrawOrderKey.ForTile(z, index), MapDrawItemKind.Layer, index, z));
        }

        for (var index = 0; index < layout.Objects.Count; index++)
        {
            var placed = layout.Objects[index];
            var definition = definitionOf(placed);
            into.Add(new MapDrawItem(SpriteKeyOf(placed, definition, index), MapDrawItemKind.ObjectBody, index, placed.Z));
            // An object already on or above the canopy layer covers the player whole: a canopy there would only tie
            // with the body it comes from. The item belongs to the object's layer (decision 522).
            if (definition.AbovePlayerZones.Count > 0 && MapLayers.CanopyZOf(placed.Z) > placed.Z)
            {
                into.Add(new MapDrawItem(CanopyKeyOf(placed, definition, index), MapDrawItemKind.Canopy, index, placed.Z));
            }
        }
    }

    private static double BaseOf(PlacedObject placed, ObjectDefinition definition) => placed.Position.Y + definition.Height;
}
