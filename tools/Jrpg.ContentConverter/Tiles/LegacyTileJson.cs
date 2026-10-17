namespace Jrpg.ContentConverter.Tiles;

/// <summary>One entry of the legacy <c>Tiles/Tileset.json</c>, with every field the file has.</summary>
internal sealed record LegacyTileJson
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required bool IsWalkable { get; init; }

    public string? Sprite { get; init; }

    public IReadOnlyList<string>? Frames { get; init; }

    public required double FrameDuration { get; init; }

    public string? Tilesheet { get; init; }

    public int? SheetX { get; init; }

    public int? SheetY { get; init; }

    /// <summary>WPF points written as <c>"x,y"</c> strings.</summary>
    public IReadOnlyList<string>? CollisionPolygon { get; init; }
}
