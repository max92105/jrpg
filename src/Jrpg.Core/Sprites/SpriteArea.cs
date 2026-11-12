namespace Jrpg.Core.Sprites;

/// <summary>
/// Where a field sprite's frame is drawn, in screen pixels from the top-left of the cell it stands on (decision 562): a
/// frame wider or taller than the cell spills over its sides and above it.
/// </summary>
public readonly record struct SpriteArea(int Left, int Top, int Width, int Height);
