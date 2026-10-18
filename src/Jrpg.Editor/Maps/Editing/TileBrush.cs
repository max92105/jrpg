using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>What a stroke puts down: a tile, or <see cref="TileLayer.EmptyTileId"/> to erase, on one layer.</summary>
/// <param name="LayerZ">The layer painted on.</param>
/// <param name="TileId">The tile painted, or empty to erase.</param>
/// <param name="Size">The brush's width and height in cells.</param>
internal readonly record struct TileBrush(int LayerZ, int TileId, int Size);
