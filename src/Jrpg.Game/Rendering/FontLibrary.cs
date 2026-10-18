using FontStashSharp;
using Jrpg.Data.Content;

namespace Jrpg.Game.Rendering;

/// <summary>
/// The game's fonts, rendered at any size from the TTF files in <c>{content}/art/fonts/</c> (ADR 0009).
/// </summary>
internal sealed class FontLibrary : IDisposable
{
    private const string FontsFolder = "art/fonts";

    private readonly FontSystem _fonts;

    private FontLibrary(FontSystem fonts)
    {
        _fonts = fonts;
    }

    /// <summary>
    /// Loads every <c>.ttf</c> in the fonts folder in file-name order: the first is the main font, the others supply
    /// characters it lacks.
    /// </summary>
    /// <exception cref="ContentValidationException">The folder holds no font.</exception>
    public static FontLibrary Load(string contentRoot)
    {
        var files = Directory.GetFiles(Path.Combine(contentRoot, FontsFolder), "*.ttf");
        if (files.Length == 0)
        {
            throw new ContentValidationException($"{FontsFolder}: there's no .ttf font.");
        }

        Array.Sort(files, StringComparer.Ordinal);
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

        return new FontLibrary(fonts);
    }

    /// <param name="pixels">Line height in virtual-screen pixels.</param>
    public SpriteFontBase FontAt(int pixels) => _fonts.GetFont(pixels);

    public void Dispose() => _fonts.Dispose();

    private static void AddFont(FontSystem fonts, string file)
    {
        var bytes = File.ReadAllBytes(file);
        try
        {
            fonts.AddFont(bytes);
        }
        catch (Exception exception)
        {
            // The rasterizer throws a bare Exception for bytes that aren't a font; name the file for the startup report.
            throw new ContentValidationException($"{FontsFolder}/{Path.GetFileName(file)}: not a readable font.", exception);
        }
    }
}
