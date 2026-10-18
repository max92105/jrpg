namespace Jrpg.Game.Exploration;

/// <summary>A block of map cells: columns <c>Left</c> up to but not including <c>Right</c>, rows likewise.</summary>
internal readonly record struct CellRange(int Left, int Top, int Right, int Bottom);
