namespace Jrpg.Data.Art;

/// <summary>Where art lives: <c>{content}/art/</c>, one folder per kind (ADR 0009).</summary>
public static class ArtFolders
{
    public const string Tilesheets = "tilesheets";

    /// <summary>The sprites the party walks the map with.</summary>
    public const string FieldSprites = "field-sprites";

    /// <summary>The faces dialogue shows next to a speaker's lines.</summary>
    public const string Portraits = "portraits";

    /// <summary>Item and menu icons.</summary>
    public const string Icons = "icons";

    /// <summary>Full-screen pictures behind the title and the menus.</summary>
    public const string Backgrounds = "backgrounds";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "art");
}
