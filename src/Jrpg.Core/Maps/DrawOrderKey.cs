namespace Jrpg.Core.Maps;

/// <summary>
/// What the game and the editor draw over what, so the editor shows exactly what the game draws (ADR 0041). Lower keys
/// draw first: by Z; on one Z, map tiles before sprites (objects, canopies and the player); sprites by the layer they
/// come from (a canopy: its object's, decision 521), then by their base, lower on screen in front, each as a whole so
/// overlapping objects don't interleave their tiles; then in insertion order.
/// </summary>
public readonly record struct DrawOrderKey : IComparable<DrawOrderKey>
{
    private const int TileGroup = 0;
    private const int SpriteGroup = 1;

    private readonly Layers _layers;
    private readonly int _group;
    private readonly double _baseY;
    private readonly int _order;

    private DrawOrderKey(Layers layers, int group, double baseY, int order)
    {
        _layers = layers;
        _group = group;
        _baseY = baseY;
        _order = order;
    }

    /// <param name="z">The tile layer's Z.</param>
    /// <param name="order">The layer's index in the map.</param>
    public static DrawOrderKey ForTile(int z, int order) => new(new Layers(z, z), TileGroup, 0, order);

    /// <param name="z">The object's or the player's Z.</param>
    /// <param name="baseY">The bottom edge in map cells: an object's top plus its height, the player's feet.</param>
    /// <param name="order">The placement order, for sprites with the same base.</param>
    public static DrawOrderKey ForSprite(int z, double baseY, int order) =>
        new(new Layers(z, z), SpriteGroup, baseY, order);

    /// <summary>An object's above-player zones: drawn on the canopy layer, ordered there by the object's Z.</summary>
    /// <param name="objectZ">The object's Z.</param>
    /// <param name="baseY">The object's bottom edge in map cells.</param>
    /// <param name="order">The object's placement order.</param>
    public static DrawOrderKey ForCanopy(int objectZ, double baseY, int order) =>
        new(new Layers(MapLayers.CanopyZOf(objectZ), objectZ), SpriteGroup, baseY, order);

    public int CompareTo(DrawOrderKey other)
    {
        var byLayer = _layers.DrawnOn != other._layers.DrawnOn
            ? _layers.DrawnOn.CompareTo(other._layers.DrawnOn)
            : _group.CompareTo(other._group);
        if (byLayer != 0)
        {
            return byLayer;
        }

        var byOrigin = _layers.Origin.CompareTo(other._layers.Origin);
        if (byOrigin != 0)
        {
            return byOrigin;
        }

        var byBase = _baseY.CompareTo(other._baseY);
        return byBase != 0 ? byBase : _order.CompareTo(other._order);
    }

    public static bool operator <(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) < 0;

    public static bool operator <=(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) <= 0;

    public static bool operator >(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) > 0;

    public static bool operator >=(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) >= 0;

    /// <summary>The Z a part draws on and the Z it comes from: equal, but a canopy draws over the player.</summary>
    private readonly record struct Layers(int DrawnOn, int Origin);
}
