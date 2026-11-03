using System.IO;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Each tilesheet image's size in pixels, read from its header once; forgotten when an import replaces the file.
/// </summary>
/// <param name="artRoot">The <c>art</c> folder.</param>
internal sealed class SheetImageSizes(string artRoot)
{
    private readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.OrdinalIgnoreCase);

    /// <param name="file">
    /// An image's file name in the tilesheet folder; the sheet box only lists images that exist.
    /// </param>
    public (int Width, int Height) SizeOf(string file)
    {
        if (!_sizes.TryGetValue(file, out var size))
        {
            size = PngSize.Read(Path.Combine(artRoot, ArtFolders.Tilesheets, file));
            _sizes.Add(file, size);
        }

        return size;
    }

    public void Forget(string file) => _sizes.Remove(file);
}
