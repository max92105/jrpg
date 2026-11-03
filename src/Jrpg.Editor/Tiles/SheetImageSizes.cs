using System.IO;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Each tilesheet image's size in pixels, read from its header once and forgotten when an import replaces the file; and
/// which images in the folder are readable, so the sheet box lists only those (decision 442).
/// </summary>
/// <param name="artRoot">The <c>art</c> folder.</param>
internal sealed class SheetImageSizes(string artRoot)
{
    private readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.OrdinalIgnoreCase);

    /// <param name="file">An image's file name in the tilesheet folder; the sheet box lists only readable ones.</param>
    public (int Width, int Height) SizeOf(string file)
    {
        if (!_sizes.TryGetValue(file, out var size))
        {
            size = PngSize.Read(PathOf(file));
            _sizes.Add(file, size);
        }

        return size;
    }

    /// <summary>
    /// Whether the file's PNG header reads; a copy cut short or another format renamed is left out of the sheet box.
    /// </summary>
    public bool IsReadable(string file) => _sizes.ContainsKey(file) || PngSize.IsReadable(PathOf(file));

    public void Forget(string file) => _sizes.Remove(file);

    private string PathOf(string file) => Path.Combine(artRoot, ArtFolders.Tilesheets, file);
}
