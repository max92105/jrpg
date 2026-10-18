using Jrpg.Core.Geometry;

namespace Jrpg.Core.Events;

/// <summary>Moves the player to a cell of a map; waits until the Game has faded out.</summary>
public sealed record WarpStep(string MapId, GridCell Cell) : EventStep;
