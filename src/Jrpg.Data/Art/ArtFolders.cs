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

    /// <summary>Battle sprites: party members' and enemies' pictures on the battle screen.</summary>
    public const string Battlers = "battlers";

    /// <summary>Battle effects: strips of square frames played left to right (decision 57).</summary>
    public const string Effects = "effects";

    /// <summary>Full-screen pictures behind the title, the menus and battles.</summary>
    public const string Backgrounds = "backgrounds";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "art");

    /// <summary>The PNG file names in the tilesheet folder, set up as sheets or not, sorted ordinally.</summary>
    /// <param name="artRoot">The <c>art</c> folder, from <see cref="RootIn"/>.</param>
    public static IReadOnlyList<string> TilesheetImagesIn(string artRoot) =>
        [.. Directory.EnumerateFiles(Path.Combine(artRoot, Tilesheets), "*.png")
            .Select(path => Path.GetFileName(path))
            .Order(StringComparer.Ordinal)];
}
