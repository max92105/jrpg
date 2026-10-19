using System.Windows;
using System.Windows.Media;

namespace MapEditor.Controls
{
    /// <summary>
    /// Hosts a single DrawingVisual so the whole map can be drawn in one pass
    /// instead of one UI element per tile.
    /// </summary>
    public class MapDrawingHost : FrameworkElement
    {
        private readonly DrawingVisual visual = new DrawingVisual();

        public MapDrawingHost()
        {
            AddVisualChild(visual);
            IsHitTestVisible = false;
            Focusable = false;
        }

        public DrawingContext RenderOpen() => visual.RenderOpen();

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => visual;
    }
}
