using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, BitmapSource> cache = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Loads a full-screen background, decoded at most at the screen's pixel width
        /// (e.g. GameMenu.png is 4096px wide; decoding it at 1920px is ~4x less work and memory).
        /// </summary>
        public static BitmapSource LoadBackground(string file)
        {
            return Load(AssetPathResolver.GetBackgroundsPath(file), ScreenPixelWidth);
        }

        public static BitmapSource Load(string path, int maxPixelWidth = 0)
        {
            string key = path + "|" + maxPixelWidth;
            if (cache.TryGetValue(key, out var cached))
                return cached;

            var uri = new Uri(path, UriKind.Absolute);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;

            // Only shrink, never upscale
            if (maxPixelWidth > 0 && GetPixelWidth(uri) > maxPixelWidth)
                bitmap.DecodePixelWidth = maxPixelWidth;

            bitmap.EndInit();
            bitmap.Freeze();

            cache[key] = bitmap;
            return bitmap;
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
