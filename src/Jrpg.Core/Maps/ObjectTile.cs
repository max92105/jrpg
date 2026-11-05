namespace Jrpg.Core.Maps;

/// <summary>
/// One tile of an object's image, at a cell counted from the object's top-left cell and a level on that cell, 0 the
/// base; a higher level is drawn over a lower one (ADR 0040).
/// </summary>
public readonly record struct ObjectTile(int X, int Y, int TileId, int Level = 0)
{
    /// <summary>
    /// The highest level a tile may have: a technical limit, far past a wall, a window and a sign (decision 473).
    /// </summary>
    public const int MaxLevel = 7;
}
