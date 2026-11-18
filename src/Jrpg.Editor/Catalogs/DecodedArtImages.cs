using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// An art folder's images as a database page shows them, each decoded once per file while the page is shown, a file
/// that can't be read kept as null so it isn't tried again on every redraw (decision 442); an import forgets the files
/// it replaced, so they are read anew. The Effect strips, Battle backgrounds and Party members pages' previews, and
/// <see cref="ArtPictureConverter"/>'s pictures.
/// </summary>
/// <param name="decode">Decodes an image of the page's art folder by its name there; null when it can't.</param>
internal sealed class DecodedArtImages(Func<string, BitmapSource?> decode)
{
    private readonly Dictionary<string, BitmapSource?> _decoded = new(StringComparer.Ordinal);

    /// <returns>The file's image, decoded on its first call; null when the decoder can't read it.</returns>
    public BitmapSource? Of(string file)
    {
        if (!_decoded.TryGetValue(file, out var image))
        {
            image = decode(file);
            _decoded.Add(file, image);
        }

        return image;
    }

    /// <summary>Drops the files' images, so the next <see cref="Of"/> decodes them again.</summary>
    public void Forget(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            _decoded.Remove(file);
        }
    }
}
