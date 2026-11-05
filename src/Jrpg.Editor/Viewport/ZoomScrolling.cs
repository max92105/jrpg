using System.Windows;
using System.Windows.Controls;

namespace Jrpg.Editor.Viewport;

/// <summary>
/// Scrolls a scroll viewer so the content point under an anchor stays put after its content's scale changed.
/// </summary>
internal static class ZoomScrolling
{
    /// <param name="viewer">The viewer whose content was just scaled.</param>
    /// <param name="anchor">
    /// A position in the viewer: the mouse for the wheel, the viewer's center for the buttons.
    /// </param>
    /// <param name="oldScale">The content's scale before the change.</param>
    /// <param name="newScale">The content's scale now.</param>
    public static void KeepAnchor(ScrollViewer viewer, Point anchor, double oldScale, double newScale)
    {
        // Read before the layout, which clamps the offsets to the new extent.
        var horizontal = CellZoom.OffsetKeeping(viewer.HorizontalOffset, anchor.X, oldScale, newScale);
        var vertical = CellZoom.OffsetKeeping(viewer.VerticalOffset, anchor.Y, oldScale, newScale);
        viewer.UpdateLayout();
        viewer.ScrollToHorizontalOffset(horizontal);
        viewer.ScrollToVerticalOffset(vertical);
    }
}
