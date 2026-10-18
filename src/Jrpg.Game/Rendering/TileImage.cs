using Microsoft.Xna.Framework;

namespace Jrpg.Game.Rendering;

/// <summary>Where a tile's picture is: a tilesheet file and the cell within it, in the image's pixels.</summary>
internal readonly record struct TileImage(string SheetFile, Rectangle Source);
