using Microsoft.Xna.Framework;

namespace Jrpg.Game.Rendering;

/// <summary>A tilesheet's pixels, row by row, as the graphics card holds them (premultiplied alpha).</summary>
internal readonly record struct SheetPixels(Color[] Pixels, int Width);
