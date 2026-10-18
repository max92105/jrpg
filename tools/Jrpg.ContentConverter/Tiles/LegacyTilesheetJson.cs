namespace Jrpg.ContentConverter.Tiles;

/// <summary>One entry of the legacy <c>Tiles/Tilesheets.json</c>, with every field the file has.</summary>
internal sealed record LegacyTilesheetJson
{
    public required string File { get; init; }

    /// <summary>A display name; dropped, since the sheet id comes from the file name.</summary>
    public string? Name { get; init; }

    public required int TileWidth { get; init; }

    public required int TileHeight { get; init; }

    public required int Margin { get; init; }

    public required int Spacing { get; init; }
}
