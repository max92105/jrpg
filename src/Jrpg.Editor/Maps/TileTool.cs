namespace Jrpg.Editor.Maps;

/// <summary>What a left click or drag on the map does with the chosen tile (keys B, R and F).</summary>
internal enum TileTool
{
    /// <summary>Drag to paint with the square brush.</summary>
    Brush,

    /// <summary>Drag a rectangle, filled when the button is released.</summary>
    Rectangle,

    /// <summary>Click to fill the connected area of the same tile.</summary>
    Fill,
}
