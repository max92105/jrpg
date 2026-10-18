namespace Jrpg.Core.Maps;

/// <summary>One tile of an object's image, at a cell counted from the object's top-left cell.</summary>
public readonly record struct ObjectTile(int X, int Y, int TileId);
