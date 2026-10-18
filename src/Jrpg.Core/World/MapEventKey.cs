namespace Jrpg.Core.World;

/// <summary>Names one map event across the whole game, since event ids are only unique within their map.</summary>
public sealed record MapEventKey(string MapId, string EventId);
