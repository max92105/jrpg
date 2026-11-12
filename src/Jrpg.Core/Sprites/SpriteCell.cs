namespace Jrpg.Core.Sprites;

/// <summary>A frame's place on a field sprite sheet, in whole frames from the top-left (ADR 0043).</summary>
public readonly record struct SpriteCell(int Column, int Row);
