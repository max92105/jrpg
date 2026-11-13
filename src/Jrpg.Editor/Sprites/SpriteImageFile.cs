using System.IO;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A sprite image of an art folder read whole for a window's own drawing (the field sprites, the Enemies window's
/// battle sprite); read again each time, since an import may replace it.
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
    public static BitmapImage? LoadFrom(string artRoot, string artFolder, string file)
    {
        var path = Path.Combine(artRoot, artFolder, file);
        if (!PngSize.IsReadable(path))
        {
            return null;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(path);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
