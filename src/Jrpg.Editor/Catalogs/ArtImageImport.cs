using System.IO;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Copies images into an art folder through Data, which refuses what the game couldn't use (a field sprite too small
/// for its record, an effect strip that isn't whole square frames, decision 590); as <see cref="SheetImport"/>, a file
/// copy and not a catalog step, so it isn't undone. Serves the Character sprites, Enemies and Effect strips pages'
/// Import.... A GIF for the battlers or the effects becomes a strip of its name first (ADR 0047), through the same
/// checks, a battle sprite's loop in the result; an effect GIF's loop, there only when its pace was typed, is a pace
/// for the Effect strips session, which writes it as a record on OK (decision 988).
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The records an image is checked against, unsaved ones included.</param>
internal sealed class ArtImageImport(ContentWriter writer, CatalogDraft catalog)
{
    private const string GifExtension = ".gif";

    /// <summary>
    /// Whether a GIF chosen for the folder is converted: the battle sprite's and the attack effect's (decisions 631,
    /// 639); the window offers GIFs and asks their options only there.
    /// </summary>
    public static bool TakesGifs(string artFolder) => artFolder is ArtFolders.Battlers or ArtFolders.Effects;

    /// <param name="artFolder">
    /// The field sprites, battlers, portraits or effects folder (<see cref="ArtFolders"/>).
    /// </param>
    /// <param name="paths">The images chosen, in order.</param>
    /// <param name="gifOptions">
    /// How a GIF chosen for the battlers or the effects is converted, the pace typed for one GIF included.
    /// </param>
    /// <param name="confirmReplace">
    /// Asked, with the file name copied into the folder, before an image already there is replaced (decision 125).
    /// </param>
    public SheetImportResult Import(
        string artFolder, IReadOnlyList<string> paths, GifImportOptions gifOptions, Func<string, bool> confirmReplace)
    {
        var imported = new List<string>();
        var refusals = new List<string>();
        var animations = new Dictionary<string, TileAnimation>(StringComparer.Ordinal);
        var notes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            var isGif = Path.GetExtension(path).Equals(GifExtension, StringComparison.OrdinalIgnoreCase);
            var fileName = Path.GetFileName(path);
            var copiedName = isGif ? Path.ChangeExtension(fileName, ".png") : fileName;
            if (isGif && !TakesGifs(artFolder))
            {
                refusals.Add($"{fileName}: only a battle sprite or an attack effect is imported from a GIF.");
                continue;
            }

            if (writer.HasArtImage(artFolder, copiedName) && !confirmReplace(copiedName))
            {
                continue;
            }

            try
            {
                if (isGif)
                {
                    var converted = ImportGif(artFolder, path, gifOptions);
                    notes[copiedName] = converted.Note;
                    if (converted.Loop is { } loop)
                    {
                        animations[copiedName] = loop;
                    }
                }
                else
                {
                    writer.ImportArtImage(artFolder, path, catalog.Current);
                }

                imported.Add(copiedName);
            }
            catch (Exception exception)
                when (exception is ContentValidationException or InvalidDataException or IOException
                    or UnauthorizedAccessException)
            {
                refusals.Add(exception.Message);
            }
        }

        return new SheetImportResult(imported, refusals) { Animations = animations, Notes = notes };
    }

    // The strip waits in a folder of its own until Data has copied it in, or refused it.
    private ConvertedGif ImportGif(string artFolder, string gifPath, GifImportOptions options)
    {
        var converted = artFolder == ArtFolders.Effects
            ? GifConversion.ConvertToEffect(gifPath, options)
            : GifConversion.Convert(gifPath, options);
        try
        {
            writer.ImportArtImage(artFolder, converted.StripPath, catalog.Current);
            return converted;
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(converted.StripPath)!, recursive: true);
        }
    }
}
