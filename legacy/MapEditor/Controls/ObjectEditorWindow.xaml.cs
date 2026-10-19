using JRPG.Core.Maps;
using JRPG.Core.Maps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapEditor.Controls
{
    /// <summary>
    /// Edits an object definition: name, category, NPC flag, the collision polygon and the
    /// "over the player" zones. Shapes are stored normalized (0..1) over the object's tile
    /// bounds, the same convention the game uses for collision.
    /// </summary>
    public partial class ObjectEditorWindow : Window
    {
        private static readonly Brush CollisionStroke = Frozen(new SolidColorBrush(Color.FromRgb(124, 252, 124)));
        private static readonly Brush CollisionFill = Frozen(new SolidColorBrush(Color.FromArgb(70, 0, 255, 0)));
        private static readonly Brush ZoneStroke = Frozen(new SolidColorBrush(Color.FromRgb(110, 200, 255)));
        private static readonly Brush ZoneFill = Frozen(new SolidColorBrush(Color.FromArgb(80, 0, 150, 255)));
        private static readonly Brush GridBrush = Frozen(new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)));

        private const double EdgeInsertDistance = 8; // screen pixels

        private readonly ObjectDefinition definition;
        private readonly ImageSource objectImage;
        private readonly int widthTiles;
        private readonly int heightTiles;
        private readonly double canvasWidth;
        private readonly double canvasHeight;

        // Working copies, written back only on Save
        private readonly List<Point> collision;
        private readonly List<List<Point>> zones;
        private int activeZone = -1;

        private double zoom = 1.0;

        // Mouse interaction state
        private int draggedPoint = -1;
        private Point? rectStart;

        public ObjectEditorWindow(ObjectDefinition definition, ImageSource objectImage)
        {
            InitializeComponent();

            this.definition = definition;
            this.objectImage = objectImage;

            var (_, _, width, height) = definition.GetTileBounds();
            widthTiles = width;
            heightTiles = height;
            canvasWidth = width * MapConstant.TileSize;
            canvasHeight = height * MapConstant.TileSize;

            collision = new List<Point>(definition.CollisionPolygon ?? new List<Point>());
            zones = (definition.AbovePlayerZones ?? new List<List<Point>>())
                .Select(z => new List<Point>(z))
                .ToList();
            activeZone = zones.Count > 0 ? 0 : -1;

            NameBox.Text = definition.Name ?? "";
            CategoryBox.Text = definition.Category ?? "";
            IsNpcBox.IsChecked = definition.IsNpc;

            DrawCanvas.Width = canvasWidth;
            DrawCanvas.Height = canvasHeight;

            // Start zoomed so the object is comfortably big
            zoom = Math.Max(1.0, Math.Min(6.0, 560.0 / Math.Max(canvasWidth, canvasHeight)));
            ApplyZoom();

            PreviewKeyDown += ObjectEditorWindow_PreviewKeyDown;

            RefreshZoneList();
            Redraw();
        }

        // === Shapes ===

        private bool IsZoneTool => ZoneToolRadio.IsChecked == true;

        private List<Point> ActiveShape
        {
            get
            {
                if (!IsZoneTool)
                    return collision;

                return activeZone >= 0 && activeZone < zones.Count ? zones[activeZone] : null;
            }
        }

        private List<Point> GetOrCreateActiveShape()
        {
            if (ActiveShape == null && IsZoneTool)
            {
                zones.Add(new List<Point>());
                activeZone = zones.Count - 1;
                RefreshZoneList();
            }
            return ActiveShape;
        }

        private Point ToNormalized(Point canvasPos)
        {
            double x = Clamp01(canvasPos.X / canvasWidth);
            double y = Clamp01(canvasPos.Y / canvasHeight);

            if (SnapBox.IsChecked == true)
            {
                // Quarter-tile steps, expressed in normalized units
                double stepX = 0.25 / widthTiles;
                double stepY = 0.25 / heightTiles;
                x = Math.Round(x / stepX) * stepX;
                y = Math.Round(y / stepY) * stepY;
            }

            return new Point(x, y);
        }

        private Point ToCanvas(Point normalized)
        {
            return new Point(normalized.X * canvasWidth, normalized.Y * canvasHeight);
        }

        private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

        /// <summary>Index after which a click should be inserted (near an edge), or -1 to append.</summary>
        private int FindEdgeToInsert(List<Point> shape, Point canvasPos)
        {
            if (shape.Count < 3)
                return -1;

            double best = EdgeInsertDistance / zoom;
            int bestIndex = -1;

            for (int i = 0; i < shape.Count; i++)
            {
                var a = ToCanvas(shape[i]);
                var b = ToCanvas(shape[(i + 1) % shape.Count]);
                double d = DistanceToSegment(canvasPos, a, b);
                if (d < best)
                {
                    best = d;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static double DistanceToSegment(Point p, Point a, Point b)
        {
            var ab = b - a;
            double lengthSq = ab.X * ab.X + ab.Y * ab.Y;
            if (lengthSq < 1e-9)
                return (p - a).Length;

            double t = Math.Max(0, Math.Min(1, ((p - a).X * ab.X + (p - a).Y * ab.Y) / lengthSq));
            var projection = a + t * ab;
            return (p - projection).Length;
        }

        // === Mouse ===

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);

            // Grab an existing point
            if (e.OriginalSource is Ellipse handle && handle.Tag is int index)
            {
                draggedPoint = index;
                DrawCanvas.CaptureMouse();
                e.Handled = true;
                return;
            }

            var shape = GetOrCreateActiveShape();
            if (shape == null)
                return;

            // Shift+drag: rectangle
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                rectStart = ToNormalized(pos);
                DrawCanvas.CaptureMouse();
                e.Handled = true;
                return;
            }

            // Add a point, or insert it into the edge that was clicked
            var point = ToNormalized(pos);
            int edge = FindEdgeToInsert(shape, pos);
            if (edge >= 0)
                shape.Insert(edge + 1, point);
            else
                shape.Add(point);

            RefreshZoneList();
            Redraw();
            e.Handled = true;
        }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var shape = ActiveShape;
            if (shape == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            var point = ToNormalized(e.GetPosition(DrawCanvas));

            if (draggedPoint >= 0 && draggedPoint < shape.Count)
            {
                shape[draggedPoint] = point;
                Redraw();
            }
            else if (rectStart.HasValue)
            {
                var a = rectStart.Value;
                shape.Clear();
                shape.Add(new Point(a.X, a.Y));
                shape.Add(new Point(point.X, a.Y));
                shape.Add(new Point(point.X, point.Y));
                shape.Add(new Point(a.X, point.Y));
                Redraw();
            }
        }

        private void DrawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedPoint = -1;
            rectStart = null;
            DrawCanvas.ReleaseMouseCapture();
            RefreshZoneList();
        }

        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var shape = ActiveShape;
            if (shape != null && e.OriginalSource is Ellipse handle && handle.Tag is int index && index < shape.Count)
            {
                shape.RemoveAt(index);
                RefreshZoneList();
                Redraw();
            }
            e.Handled = true;
        }

        private void DrawCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoom = Math.Max(0.5, Math.Min(12.0, zoom * (e.Delta > 0 ? 1.2 : 1 / 1.2)));
            ApplyZoom();
            Redraw(); // handle sizes follow the zoom
            e.Handled = true;
        }

        private void ApplyZoom()
        {
            ZoomTransform.ScaleX = zoom;
            ZoomTransform.ScaleY = zoom;
        }

        private void ObjectEditorWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                return;

            if (e.Key == Key.Back || (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control))
            {
                var shape = ActiveShape;
                if (shape != null && shape.Count > 0)
                {
                    shape.RemoveAt(shape.Count - 1);
                    RefreshZoneList();
                    Redraw();
                }
                e.Handled = true;
            }
        }

        // === Tools and zones ===

        private void Tool_Checked(object sender, RoutedEventArgs e)
        {
            if (ZonePanel == null) return; // during InitializeComponent

            ZonePanel.Visibility = IsZoneTool ? Visibility.Visible : Visibility.Collapsed;
            Redraw();
        }

        private void RefreshZoneList()
        {
            ZoneList.SelectionChanged -= ZoneList_SelectionChanged;
            ZoneList.Items.Clear();
            for (int i = 0; i < zones.Count; i++)
                ZoneList.Items.Add($"Zone {i + 1}  ({zones[i].Count} points)");
            ZoneList.SelectedIndex = activeZone;
            ZoneList.SelectionChanged += ZoneList_SelectionChanged;
        }

        private void ZoneList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activeZone = ZoneList.SelectedIndex;
            Redraw();
        }

        private void NewZone_Click(object sender, RoutedEventArgs e)
        {
            zones.Add(new List<Point>());
            activeZone = zones.Count - 1;
            ZoneToolRadio.IsChecked = true;
            RefreshZoneList();
            Redraw();
        }

        private void DeleteZone_Click(object sender, RoutedEventArgs e)
        {
            if (activeZone < 0 || activeZone >= zones.Count)
                return;

            zones.RemoveAt(activeZone);
            activeZone = Math.Min(activeZone, zones.Count - 1);
            RefreshZoneList();
            Redraw();
        }

        private void ClearShape_Click(object sender, RoutedEventArgs e)
        {
            ActiveShape?.Clear();
            RefreshZoneList();
            Redraw();
        }

        private void WholeObject_Click(object sender, RoutedEventArgs e)
        {
            var shape = GetOrCreateActiveShape();
            if (shape == null) return;

            shape.Clear();
            shape.AddRange(new[] { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1) });
            RefreshZoneList();
            Redraw();
        }

        // === Drawing ===

        private void Redraw()
        {
            DrawCanvas.Children.Clear();

            var image = new Image { Source = objectImage, Width = canvasWidth, Height = canvasHeight, IsHitTestVisible = false };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            DrawCanvas.Children.Add(image);

            DrawGrid();

            // Inactive shapes first (faint), then the active one with its handles
            var active = ActiveShape;

            if (!ReferenceEquals(collision, active))
                DrawShape(collision, CollisionStroke, CollisionFill, 0.35);

            foreach (var zone in zones)
            {
                if (!ReferenceEquals(zone, active))
                    DrawShape(zone, ZoneStroke, ZoneFill, 0.35);
            }

            if (active != null)
            {
                bool isZone = !ReferenceEquals(active, collision);
                DrawShape(active, isZone ? ZoneStroke : CollisionStroke, isZone ? ZoneFill : CollisionFill, 1.0);
                DrawHandles(active);
            }
        }

        private void DrawGrid()
        {
            double thickness = 1 / zoom;

            for (int x = 0; x <= widthTiles; x++)
            {
                DrawCanvas.Children.Add(new Line
                {
                    X1 = x * MapConstant.TileSize, Y1 = 0, X2 = x * MapConstant.TileSize, Y2 = canvasHeight,
                    Stroke = GridBrush, StrokeThickness = thickness, IsHitTestVisible = false
                });
            }

            for (int y = 0; y <= heightTiles; y++)
            {
                DrawCanvas.Children.Add(new Line
                {
                    X1 = 0, Y1 = y * MapConstant.TileSize, X2 = canvasWidth, Y2 = y * MapConstant.TileSize,
                    Stroke = GridBrush, StrokeThickness = thickness, IsHitTestVisible = false
                });
            }
        }

        private void DrawShape(List<Point> shape, Brush stroke, Brush fill, double opacity)
        {
            if (shape == null || shape.Count < 2)
                return;

            var polygon = new Polygon
            {
                Stroke = stroke,
                StrokeThickness = 2 / zoom,
                Fill = shape.Count >= 3 ? fill : null,
                Opacity = opacity,
                IsHitTestVisible = false
            };

            foreach (var p in shape)
                polygon.Points.Add(ToCanvas(p));

            DrawCanvas.Children.Add(polygon);
        }

        private void DrawHandles(List<Point> shape)
        {
            double size = 10 / zoom; // constant size on screen

            for (int i = 0; i < shape.Count; i++)
            {
                var p = ToCanvas(shape[i]);
                var handle = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = i == shape.Count - 1 ? Brushes.Yellow : Brushes.White, // last point = where the next one connects
                    Stroke = Brushes.Black,
                    StrokeThickness = 1 / zoom,
                    Cursor = Cursors.SizeAll,
                    Tag = i,
                    ToolTip = "Drag to move, right-click to delete"
                };

                Canvas.SetLeft(handle, p.X - size / 2);
                Canvas.SetTop(handle, p.Y - size / 2);
                DrawCanvas.Children.Add(handle);
            }
        }

        // === Save ===

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            definition.Name = NameBox.Text.Trim();
            definition.Category = string.IsNullOrWhiteSpace(CategoryBox.Text) ? "General" : CategoryBox.Text.Trim();
            definition.IsNpc = IsNpcBox.IsChecked == true;

            // Shapes need at least 3 points to cover an area
            definition.CollisionPolygon = collision.Count >= 3 ? new List<Point>(collision) : new List<Point>();
            definition.AbovePlayerZones = zones.Where(z => z.Count >= 3).Select(z => new List<Point>(z)).ToList();

            DialogResult = true;
        }

        private static T Frozen<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }
    }
}
