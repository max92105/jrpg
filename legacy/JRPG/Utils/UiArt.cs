using JRPG.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace JRPG.Utils
{
    /// <summary>
    /// Finds UI art by key, e.g. "Menu/Inventory":
    ///   1. if Assets/UI/Menu/Inventory.png exists, that image is used;
    ///   2. otherwise the placeholder shape with the same key in UI/Styles/Art.xaml is drawn.
    /// Swapping a placeholder for real art is just dropping a PNG at the matching path.
    /// </summary>
    public static class UiArt
    {
        // Placeholder shapes are drawn on a 24x24 grid
        private const double PlaceholderSize = 24;
        private const int MaxPixelWidth = 256;

        private static readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>();

        public static ImageSource Get(string key)
        {
            if (cache.TryGetValue(key, out var cached))
                return cached;

            ImageSource image = LoadFile(key) ?? CreatePlaceholder(key);

            // Not cached when missing: the art dictionary may simply not be loaded yet
            if (image != null)
                cache[key] = image;

            return image;
        }

        private static ImageSource LoadFile(string key)
        {
            string path = AssetPathResolver.GetPath("UI", key + ".png");
            return File.Exists(path) ? ImageLoader.Load(path, MaxPixelWidth) : null;
        }

        private static ImageSource CreatePlaceholder(string key)
        {
            if (!(Application.Current?.TryFindResource(key) is Geometry shape))
                return null;

            var fill = Application.Current.TryFindResource("Brush.Text.Primary") as Brush ?? Brushes.White;

            var drawing = new DrawingGroup();
            // An invisible 24x24 frame, so every icon keeps the same size and position whatever its shape
            drawing.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, PlaceholderSize, PlaceholderSize))));
            drawing.Children.Add(new GeometryDrawing(fill, null, shape));

            var image = new DrawingImage(drawing);
            image.Freeze();
            return image;
        }
    }
}
