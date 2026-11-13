using System.IO;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A field sprite image read whole for a window's own drawing; read again each time, since an import may replace it.
/// </summary>
internal static class SpriteImageFile
{
    /// <param name="artRoot">The <c>art</c> folder.</param>
    /// <param name="file">The image's file name in the field-sprite folder.</param>
    /// <returns>The image, or null when the folder lacks it.</returns>
    public static BitmapImage? Load(string artRoot, string file)
    {
        var path = Path.Combine(artRoot, ArtFolders.FieldSprites, file);
        if (!File.Exists(path))
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
