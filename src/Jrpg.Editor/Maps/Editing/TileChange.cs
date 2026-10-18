using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>One cell's tile before and after an edit; 0 is an empty cell.</summary>
internal readonly record struct TileChange(GridCell Cell, int Before, int After);
