namespace Jrpg.Data.Art;

/// <summary>Where art lives: <c>{content}/art/</c>, one folder per kind (ADR 0009).</summary>
public static class ArtFolders
{
    public const string Tilesheets = "tilesheets";

    /// <summary>The sprites the party walks the map with.</summary>
    public const string FieldSprites = "field-sprites";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "art");
}
