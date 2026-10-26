using FontStashSharp;
using Jrpg.Data.Content;

namespace Jrpg.Game.Rendering;

/// <summary>
/// The game's fonts, rendered at any size from the TTF files under <c>{content}/art/fonts/</c> (ADR 0009), one face per
/// role (ADR 0025): the body text from the folder itself, the big titles from its <c>titles</c> subfolder.
/// </summary>
internal sealed class FontLibrary : IDisposable
{
    private const string BodyFolder = "art/fonts";
    private const string TitlesFolder = "art/fonts/titles";

    private readonly FontSystem _body;
    private readonly FontSystem _titles;

    private FontLibrary(FontSystem body, FontSystem titles)
    {
        _body = body;
        _titles = titles;
    }

    /// <summary>
    /// Loads every <c>.ttf</c> of each role's folder in file-name order: the first is the role's font, the others
    /// supply characters it lacks. The title face falls back to the body files after its own.
    /// </summary>
    /// <exception cref="ContentValidationException">
    /// A role's folder holds no font, or a file in it isn't a readable font.
    /// </exception>
    public static FontLibrary Load(string contentRoot)
    {
        var bodyFiles = ReadFonts(contentRoot, BodyFolder);
        var titleFiles = ReadFonts(contentRoot, TitlesFolder);
        var body = CreateFontSystem(bodyFiles);
        try
        {
            return new FontLibrary(body, CreateFontSystem([.. titleFiles, .. bodyFiles]));
        }
        catch
        {
            body.Dispose();
            throw;
        }
    }

    /// <param name="pixels">Line height in virtual-screen pixels.</param>
    public SpriteFontBase FontAt(int pixels) => _body.GetFont(pixels);

    /// <summary>The serif for big titles: the game's name, the outcome titles (ADR 0025).</summary>
    /// <param name="pixels">Line height in virtual-screen pixels.</param>
    public SpriteFontBase TitleFontAt(int pixels) => _titles.GetFont(pixels);

    public void Dispose()
    {
        _body.Dispose();
        _titles.Dispose();
    }

    // Each file read and checked once, so a role's fallbacks reuse the bytes.
    private static List<FontFile> ReadFonts(string contentRoot, string folder)
    {
        var path = Path.Combine(contentRoot, folder);
        var files = Directory.Exists(path) ? Directory.GetFiles(path, "*.ttf") : [];
        if (files.Length == 0)
        {
            throw new ContentValidationException($"{folder}: there's no .ttf font.");
        }

        Array.Sort(files, StringComparer.Ordinal);
        var fonts = new List<FontFile>(files.Length);
        foreach (var file in files)
        {
            var font = new FontFile($"{folder}/{Path.GetFileName(file)}", File.ReadAllBytes(file));
            if (!FontFileHeader.IsComplete(font.Bytes))
            {
                throw new ContentValidationException(NotReadableMessage(font));
            }

            fonts.Add(font);
        }

        return fonts;
    }

    private static FontSystem CreateFontSystem(IEnumerable<FontFile> files)
    {
        var fonts = new FontSystem();
        try
        {
            foreach (var file in files)
            {
                AddFont(fonts, file);
            }
        }
        catch
        {
            fonts.Dispose();
            throw;
        }

        return fonts;
    }

    private static void AddFont(FontSystem fonts, FontFile file)
    {
        try
        {
            fonts.AddFont(file.Bytes);
        }
        catch (Exception exception)
        {
            // The rasterizer throws a bare Exception for bytes that aren't a font; name the file for the startup report.
            throw new ContentValidationException(NotReadableMessage(file), exception);
        }
    }

    private static string NotReadableMessage(FontFile file) => $"{file.Name}: not a readable font.";

    // A font file's bytes and its name in the startup report (its folder under the content root).
    private readonly record struct FontFile(string Name, byte[] Bytes);
}
