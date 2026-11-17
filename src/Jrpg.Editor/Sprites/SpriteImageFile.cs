using System.IO;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A sprite image of an art folder read whole for the editor's own drawing (the field sprites, the Enemies page's
/// battle sprite), or at a width for a preview (the Cinematics page's still, the Items page's icon); read again each
/// time, since an import may replace the file. <see cref="Read"/> is the one decode of a whole PNG by its path, the
/// tilesheets' and the strip tool's sheet included.
/// </summary>
internal static class SpriteImageFile
{
    /// <param name="artRoot">The <c>art</c> folder.</param>
    /// <param name="file">The image's file name in the field-sprite folder.</param>
    /// <returns>The image, or null when the folder lacks it or it isn't a readable PNG.</returns>
    public static BitmapImage? Load(string artRoot, string file) => LoadFrom(artRoot, ArtFolders.FieldSprites, file);

    /// <param name="artRoot">The <c>art</c> folder.</param>
    /// <param name="artFolder">The folder, one of <see cref="ArtFolders"/>' names.</param>
    /// <param name="file">The image's file name in that folder.</param>
    /// <returns>
    /// The image, or null when the folder lacks it or it isn't a readable PNG: an enemy may name a broken file
    /// (decision 442 only warns), and decoding one would throw.
    /// </returns>
    public static BitmapImage? LoadFrom(string artRoot, string artFolder, string file) =>
        Read(Path.Combine(artRoot, artFolder, file));

    /// <summary>
    /// A PNG decoded whole at its own size, frozen, from any folder. A readable header can still hide data the decoder
    /// refuses, so a file the developer picked is read under the decoder's refusals too.
    /// </summary>
    /// <param name="path">The file's full path.</param>
    /// <returns>The image, or null when the file is missing or its PNG header doesn't read.</returns>
    public static BitmapImage? Read(string path) => Decode(path, decodePixelWidth: 0);

    /// <summary>
    /// The image decoded at a width, its height following: a 1920 px still shown at 480 decodes a sixteenth of it.
    /// </summary>
    /// <returns>
    /// The image, or null when the folder lacks it or it isn't a readable PNG, as <see cref="LoadFrom"/>.
    /// </returns>
    public static BitmapImage? LoadAtWidth(string artRoot, string artFolder, string file, int width) =>
        Decode(Path.Combine(artRoot, artFolder, file), width);

    // A width of 0 decodes the image at its own size. WPF keeps decoded images by path, so the cache is skipped: a file
    // replaced by an import must be read again.
    private static BitmapImage? Decode(string path, int decodePixelWidth)
    {
        if (!PngSize.IsReadable(path))
        {
            return null;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(path);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = decodePixelWidth;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
