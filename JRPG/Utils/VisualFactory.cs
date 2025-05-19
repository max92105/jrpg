using JRPG.Core.Utils;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace JRPG.Utils
{
    public static class VisualFactory
    {
        public static Image CreateIcon(string iconPath, double size = 24)
        {
            return new Image
            {
                Width = size,
                Height = size,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = string.IsNullOrEmpty(iconPath) ? 0 : 1,
                Source = string.IsNullOrEmpty(iconPath)
                    ? null
                    : new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(iconPath)))
            };
        }

        public static TextBlock CreateText(string content, int fontSize = 16, Brush color = null, Thickness? margin = null)
        {
            return new TextBlock
            {
                Text = content,
                FontSize = fontSize,
                FontWeight = FontWeights.Light,
                Foreground = color ?? Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = margin ?? new Thickness(0)
            };
        }
    }
}
