using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Events;

/// <summary>Where a warp step leads, once its fields are valid: what Go to follows (decision 508).</summary>
internal sealed record WarpDestination(string MapId, GridCell Cell);
