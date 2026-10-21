namespace Jrpg.Editor.Catalogs;

/// <summary>One whole cell of a tilesheet's grid, whether or not a tile is cut from it yet.</summary>
/// <param name="SheetId">The tilesheet's id.</param>
/// <param name="Column">From 0 at the left.</param>
/// <param name="Row">From 0 at the top.</param>
internal readonly record struct SheetCell(string SheetId, int Column, int Row);
