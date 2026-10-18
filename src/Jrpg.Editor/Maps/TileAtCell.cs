namespace Jrpg.Editor.Maps;

/// <summary>A tile id and the Z of the layer it's on.</summary>
internal readonly record struct TileAtCell(int TileId, int LayerZ);
