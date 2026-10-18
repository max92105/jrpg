using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>Where the player is: the map, the position in tiles, and the direction they face.</summary>
public sealed record MapLocation(string MapId, Point2D Position, Facing Facing);
