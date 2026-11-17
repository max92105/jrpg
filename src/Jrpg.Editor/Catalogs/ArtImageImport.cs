using System.IO;
using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Copies images into an art folder through Data, which refuses what the game couldn't use (a field sprite too small
/// for its record, an effect strip that isn't whole square frames, decision 590); as <see cref="SheetImport"/>, a file
/// copy and not a catalog step, so it isn't undone. Serves the Character sprites, Enemies, Effect strips and Battle
/// backgrounds pages' Import..., a background PNG refused past 4096 px a side (decision 895). A GIF for the battlers or
/// the effects becomes a strip of its name first (ADR 0047), through the same checks, a battle sprite's loop in the
/// result; an effect GIF's loop, there only when its pace was typed, is a pace for the Effect strips session, which
/// writes it as a record on OK (decision 988). A GIF for the backgrounds becomes a grid sheet of its name, one GIF an
/// import, its record in the result for the Battle backgrounds session (ADR 0059, decision 903).
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The records an image is checked against, unsaved ones included.</param>
internal sealed class ArtImageImport(ContentWriter writer, CatalogDraft catalog)
{
    private const string GifExtension = ".gif";

    /// <summary>
    /// Whether a GIF chosen for the folder is converted: the battle sprite's, the attack effect's and the battle
    /// background's (decisions 631, 639, 903); the window offers GIFs and asks their options only there.
    /// </summary>
    public static bool TakesGifs(string artFolder) =>
        artFolder is ArtFolders.Battlers or ArtFolders.Effects or ArtFolders.Backgrounds;

    /// <param name="artFolder">
    /// The field sprites, battlers, portraits, effects or backgrounds folder (<see cref="ArtFolders"/>).
    /// </param>
    /// <param name="paths">The images chosen, in order.</param>
    /// <param name="gifOptions">
    /// How a GIF chosen for the battlers, the effects or the backgrounds is converted, with the pace typed for one GIF
    /// and a background's frame height.
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
        var backgrounds = new Dictionary<string, AnimatedBackgroundDefinition>(StringComparer.Ordinal);
        var notes = new Dictionary<string, string>(StringComparer.Ordinal);
        var firstGif = paths.FirstOrDefault(IsGif);
        foreach (var path in paths)
        {
            var isGif = IsGif(path);
            var copiedName = isGif ? Path.ChangeExtension(Path.GetFileName(path), ".png") : Path.GetFileName(path);
            if (GifRefusalOf(artFolder, path, firstGif) is { } refusal)
            {
                refusals.Add(refusal);
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

                    if (converted.Background is { } background)
                    {
                        backgrounds[copiedName] = background;
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

        return new SheetImportResult(imported, refusals) { Animations = animations, Backgrounds = backgrounds, Notes = notes };
    }

    private static bool IsGif(string path) => Path.GetExtension(path).Equals(GifExtension, StringComparison.OrdinalIgnoreCase);

    // A background GIF's options were chosen for the first one alone, which the dialog played (decision 903).
    private static string? GifRefusalOf(string artFolder, string path, string? firstGif)
    {
        if (!IsGif(path))
        {
            return null;
        }

        if (!TakesGifs(artFolder))
        {
            return $"{Path.GetFileName(path)}: only a battle sprite, an attack effect or a battle background is imported "
                + "from a GIF.";
        }

        return artFolder == ArtFolders.Backgrounds && path != firstGif
            ? $"{Path.GetFileName(path)}: import one background GIF at a time."
            : null;
    }

    // The strip or sheet waits in a folder of its own until Data has copied it in, or refused it. A background's GIF is
    // read again here, as the dialog's reading only served its preview.
    private ConvertedGif ImportGif(string artFolder, string gifPath, GifImportOptions options)
    {
        var converted = artFolder switch
        {
            ArtFolders.Effects => GifConversion.ConvertToEffect(gifPath, options),
            ArtFolders.Backgrounds => GifConversion.ConvertToBackground(gifPath, ComposedGif.Read(gifPath), options),
            _ => GifConversion.Convert(gifPath, options),
        };
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
