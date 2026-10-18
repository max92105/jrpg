namespace Jrpg.Core.Maps;

/// <summary>
/// What the game and the editor draw over what, so the editor shows exactly what the game draws. Lower keys draw first:
/// by Z; on one Z, map tiles before sprites (objects and the player); sprites by their base, lower on screen in front,
/// each as a whole so overlapping objects don't interleave their tiles; then in insertion order.
/// </summary>
public readonly record struct DrawOrderKey : IComparable<DrawOrderKey>
{
    private const int TileGroup = 0;
    private const int SpriteGroup = 1;

    private readonly int _z;
    private readonly int _group;
    private readonly double _baseY;
    private readonly int _order;

    private DrawOrderKey(int z, int group, double baseY, int order)
    {
        _z = z;
        _group = group;
        _baseY = baseY;
        _order = order;
    }

    /// <param name="z">The tile layer's Z.</param>
    /// <param name="order">The layer's index in the map.</param>
    public static DrawOrderKey ForTile(int z, int order) => new(z, TileGroup, 0, order);

    /// <param name="z">The object's or the player's Z.</param>
    /// <param name="baseY">The bottom edge in map cells: an object's top plus its height, the player's feet.</param>
    /// <param name="order">The placement order, for sprites with the same base.</param>
    public static DrawOrderKey ForSprite(int z, double baseY, int order) => new(z, SpriteGroup, baseY, order);

    public int CompareTo(DrawOrderKey other)
    {
        var byLayer = _z != other._z ? _z.CompareTo(other._z) : _group.CompareTo(other._group);
        if (byLayer != 0)
        {
            return byLayer;
        }

        var byBase = _baseY.CompareTo(other._baseY);
        return byBase != 0 ? byBase : _order.CompareTo(other._order);
    }

    public static bool operator <(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) < 0;

    public static bool operator <=(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) <= 0;

    public static bool operator >(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) > 0;

    public static bool operator >=(DrawOrderKey left, DrawOrderKey right) => left.CompareTo(right) >= 0;
}
