using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>A canvas cell on one level, 0 the base (ADR 0040): the key of the canvas's painted cells.</summary>
internal readonly record struct CellOnLevel(GridCell Cell, int Level);
