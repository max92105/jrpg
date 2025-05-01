using System.Windows.Media;
using System.Windows;


public class VisualHost : FrameworkElement
{
    private readonly VisualCollection _children;

    public VisualHost()
    {
        _children = new VisualCollection(this);
        this.Focusable = false;
        this.IsHitTestVisible = false;
    }

    public void SetVisual(Visual visual)
    {
        _children.Clear();
        _children.Add(visual);
        InvalidateVisual(); // ensure it's redrawn
    }

    protected override int VisualChildrenCount => _children.Count;
    protected override Visual GetVisualChild(int index) => _children[index];

    protected override void OnRender(DrawingContext dc)
    {
        // Optional: background fill to confirm it's visible
        dc.DrawRectangle(null, null, new Rect(0, 0, ActualWidth, ActualHeight));
    }
}