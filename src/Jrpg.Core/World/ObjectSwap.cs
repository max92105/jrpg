namespace Jrpg.Core.World;

/// <summary>A placed object that now shows another object, such as an opened chest.</summary>
public sealed record ObjectSwap(string MapId, string ObjectUid, string ObjectId);
