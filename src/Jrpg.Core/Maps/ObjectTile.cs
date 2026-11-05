namespace Jrpg.Core.Maps;

/// <summary>
/// One tile of an object's image, at a cell counted from the object's top-left cell and a level on that cell from
/// <see cref="MinLevel"/> to <see cref="MaxLevel"/>, 0 the base; a lower level is drawn under a higher one (ADR 0040
/// and its amendment).
/// </summary>
public readonly record struct ObjectTile(int X, int Y, int TileId, int Level = 0)
{
    /// <summary>
    /// The lowest level a tile may have: a technical limit, far past a shadow under a wall (decision 488).
    /// </summary>
    public const int MinLevel = -7;

    /// <summary>
    /// The highest level a tile may have: a technical limit, far past a window over a wall (decision 488).
    /// </summary>
    public const int MaxLevel = 7;
}
