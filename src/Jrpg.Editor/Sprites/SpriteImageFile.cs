using System.IO;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A sprite image of an art folder read whole for the editor's own drawing (the field sprites, the Enemies page's
/// battle sprite), or at a width for a preview (the Cinematics page's still); read again each time, since an import may
/// replace the file.
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
        Decode(Path.Combine(artRoot, artFolder, file), decodePixelWidth: 0);

    /// <summary>
    /// The image decoded at a width, its height following: a 1920 px still shown at 480 decodes a sixteenth of it.
    /// </summary>
    /// <returns>
    /// The image, or null when the folder lacks it or it isn't a readable PNG, as <see cref="LoadFrom"/>.
    /// </returns>
    public static BitmapImage? LoadAtWidth(string artRoot, string artFolder, string file, int width) =>
        Decode(Path.Combine(artRoot, artFolder, file), width);

    // A width of 0 decodes the image at its own size.
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
