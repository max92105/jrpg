using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.Utils
{
    /// <summary>
    /// Loads images fully up front (decoded + frozen) and caches them, so nothing
    /// gets decoded on the UI thread the first time a screen is shown.
    /// </summary>
    public static class ImageLoader
    {
        // UI images (icons, portraits, battle sprites) are shown at 96px or less; 256px keeps them
        // sharp up to 200% display scaling while decoding ~16x fewer pixels than the 1024px files
        private const int UiImageSize = 256;

        private static readonly Dictionary<string, BitmapSource> cache = new Dictionary<string, BitmapSource>();
        private static readonly object cacheLock = new object();

        /// <summary>
        /// Loads a full-screen background, decoded at most at the screen's pixel width
        /// (e.g. GameMenu.png is 4096px wide; decoding it at 1920px is ~4x less work and memory).
        /// </summary>
        public static BitmapSource LoadBackground(string file)
        {
            return Load(AssetPathResolver.GetBackgroundsPath(file), ScreenPixelWidth);
        }

        public static BitmapSource LoadIcon(string file) =>
            string.IsNullOrEmpty(file) ? null : Load(AssetPathResolver.GetIconsPath(file), UiImageSize);

        public static BitmapSource LoadPortrait(string file) =>
            string.IsNullOrEmpty(file) ? null : Load(AssetPathResolver.GetPortraitPath(file), UiImageSize);

        public static BitmapSource LoadSprite(string file) =>
            string.IsNullOrEmpty(file) ? null : Load(AssetPathResolver.GetSpritesPath(file), UiImageSize);

        /// <summary>Loads (or reuses) an image. Returns null if the file is missing or unreadable.</summary>
        public static BitmapSource Load(string path, int maxPixelWidth = 0)
        {
            string key = path + "|" + maxPixelWidth;
            lock (cacheLock)
            {
                if (cache.TryGetValue(key, out var cached))
                    return cached;
            }

            BitmapSource bitmap = Decode(path, maxPixelWidth);

            lock (cacheLock)
            {
                cache[key] = bitmap; // a failed load is cached too, so it isn't retried every frame
            }
            return bitmap;
        }

        /// <summary>
        /// Decodes every icon, portrait and battle sprite on a background thread, so opening the
        /// menus or starting a battle never waits for an image.
        /// </summary>
        public static Task PreloadUiImagesAsync()
        {
            return Task.Run(() =>
            {
                foreach (var folder in new[] { "Icons", "Portraits", "Sprites" })
                {
                    string directory = AssetPathResolver.GetPath(folder);
                    if (!Directory.Exists(directory))
                        continue;

                    foreach (var path in Directory.GetFiles(directory, "*.png"))
                        Load(path, UiImageSize);
                }
            });
        }

        private static BitmapSource Decode(string path, int maxPixelWidth)
        {
            try
            {
                var uri = new Uri(path, UriKind.Absolute);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                // Only shrink, never upscale
                if (maxPixelWidth > 0 && GetPixelWidth(uri) > maxPixelWidth)
                    bitmap.DecodePixelWidth = maxPixelWidth;

                bitmap.EndInit();
                bitmap.Freeze(); // frozen images can be created on a background thread and used on the UI thread
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoader: could not load {path}: {ex.Message}");
                return null;
            }
        }

        private static int GetPixelWidth(Uri uri)
        {
            // Reads only the header, not the pixels
            var decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            return decoder.Frames[0].PixelWidth;
        }

        private static int ScreenPixelWidth
        {
            get
            {
                double dpiScale = VisualTreeHelper.GetDpi(new DrawingVisual()).DpiScaleX;
                return (int)Math.Ceiling(SystemParameters.PrimaryScreenWidth * dpiScale);
            }
        }
    }
}
