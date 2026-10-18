namespace Jrpg.ContentConverter.Objects;

/// <summary>One tile of a legacy object's tile group, at a cell relative to the group.</summary>
internal sealed record LegacyTileGroupEntryJson(int X, int Y, int TileId, int Z);
