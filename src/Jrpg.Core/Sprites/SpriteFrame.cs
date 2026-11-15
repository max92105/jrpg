namespace Jrpg.Core.Sprites;

/// <summary>
/// A field sprite frame to draw (decision 770): its cell on the sheet, and whether it is drawn flipped left to right,
/// as a facing that mirrors another is (decision 766).
/// </summary>
public readonly record struct SpriteFrame(SpriteCell Cell, bool IsMirrored);
