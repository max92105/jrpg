using Jrpg.Core.Events;
using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>A touch event on a map cell, such as a warp tile.</summary>
public sealed record CellEvent(GridCell Cell, MapEvent Event);
