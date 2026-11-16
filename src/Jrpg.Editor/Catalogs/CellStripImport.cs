using System.IO;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Writes the frames Strip from sheet cells cut as an effects strip through Data (decision 990): each frame padded to a
/// square, as an effect GIF's are, then copied in by the same check as any imported image. A file copy and not a
/// catalog step, as <see cref="ArtImageImport"/>'s, so it isn't undone. WPF imaging needs an STA thread.
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The records the strip is checked against, unsaved ones included.</param>
internal sealed class CellStripImport(ContentWriter writer, CatalogDraft catalog)
{
    /// <summary>What a strip's temporary folder starts with, under the system's temporary folder.</summary>
    public const string TemporaryFolderPrefix = "jrpg-cell-strip-";

    /// <summary>
    /// Writes the strip into the effects folder under its name, replacing an image already there, which the caller asks
    /// about first (decision 125).
    /// </summary>
    /// <param name="frames">The cells' pixels in play order, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="cell">Each frame's size in pixels.</param>
    /// <param name="fileName">The strip's file name in the effects folder, ending in ".png".</param>
    /// <exception cref="ContentValidationException">Data refused the strip (too wide); nothing was copied.</exception>
    /// <exception cref="IOException">The strip couldn't be written or copied.</exception>
    /// <exception cref="UnauthorizedAccessException">A folder refused the write.</exception>
    public void Write(IReadOnlyList<uint[]> frames, (int Width, int Height) cell, string fileName)
    {
        // The strip waits in a folder of its own until Data has copied it in, or refused it.
        var folder = Directory.CreateTempSubdirectory(TemporaryFolderPrefix).FullName;
        try
        {
            var stripPath = Path.Combine(folder, fileName);
            FrameStrip.WriteSquarePng(frames, cell.Width, cell.Height, stripPath);
            writer.ImportArtImage(ArtFolders.Effects, stripPath, catalog.Current);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
