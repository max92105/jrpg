using Jrpg.Data.Content;

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

    /// <summary>The stills cutscenes show over the whole screen (ADR 0016).</summary>
    public const string Cinematics = "cinematics";

    /// <summary>The game's fonts, and the damage digit strips drawn as one (decision 653).</summary>
    public const string Fonts = "fonts";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "art");

    /// <summary>The PNG file names in the tilesheet folder, set up as sheets or not, sorted ordinally.</summary>
    /// <param name="artRoot">The <c>art</c> folder, from <see cref="RootIn"/>.</param>
    public static IReadOnlyList<string> TilesheetImagesIn(string artRoot) => ImagesIn(artRoot, Tilesheets);

    /// <summary>The PNG file names in one art folder, sorted ordinally; the editor's pickers list them.</summary>
    /// <param name="artRoot">The <c>art</c> folder, from <see cref="RootIn"/>.</param>
    /// <param name="folder">One of the folder names above, such as <see cref="Portraits"/>.</param>
    public static IReadOnlyList<string> ImagesIn(string artRoot, string folder) =>
        [.. Directory.EnumerateFiles(Path.Combine(artRoot, folder), "*.png")
            .Select(path => Path.GetFileName(path))
            .Order(StringComparer.Ordinal)];

    /// <summary>
    /// For a picker over a folder of folders, such as the cinematics' pictures, one folder per cinematic (ADR 0046).
    /// </summary>
    /// <param name="artRoot">The <c>art</c> folder, from <see cref="RootIn"/>.</param>
    /// <param name="folder">One of the folder names above, such as <see cref="Cinematics"/>.</param>
    /// <returns>
    /// The PNG names in the folder and its subfolders, by exact name under the folder with <c>/</c>, sorted ordinally;
    /// empty when the folder is missing.
    /// </returns>
    public static IReadOnlyList<string> ImagesUnder(string artRoot, string folder)
    {
        var path = Path.Combine(artRoot, folder);
        return Directory.Exists(path)
            ? [.. Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories)
                .Select(file => ExactFileName.NameUnder(path, file))
                .Order(StringComparer.Ordinal)]
            : [];
    }
}
