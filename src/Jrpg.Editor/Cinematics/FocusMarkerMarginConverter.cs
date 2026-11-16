using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Jrpg.Core.Cinematics;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// Places the focus marker over the Cinematics page's preview frame (decision 755): a still's focus to the margin that
/// centers the marker on its point; no margin without one, where the marker is hidden.
/// </summary>
internal sealed class FocusMarkerMarginConverter : IValueConverter
{
    /// <summary>The ring's width and height; a double, since WPF refuses an int bound to a size.</summary>
    public const double MarkerSize = 16;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not StillFocus focus)
        {
            return new Thickness();
        }

        var share = focus.Share;
        return new Thickness(
            (share.X * CinematicCatalogPage.PreviewWidth) - (MarkerSize / 2),
            (share.Y * CinematicCatalogPage.PreviewHeight) - (MarkerSize / 2),
            0,
            0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The marker only shows the focus.");
}
