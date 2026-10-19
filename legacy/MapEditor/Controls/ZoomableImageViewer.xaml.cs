using JRPG.Core.Maps;
using JRPG.Core.Maps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor.Controls
{
    public partial class ZoomableImageViewer : UserControl
    {
        private double zoom = 1.0;
        private Point? dragStart = null;
        private Vector offset = new Vector(0, 0);

        private Image displayedImage;
        private Rectangle selectionHighlight;

        private bool isEditingCollision = false;
        private List<Point> polygonPoints = new List<Point>();
        private Polygon polygonOverlay;

        private List<Ellipse> pointHandles = new List<Ellipse>();
        public List<Point> GetCollisionPolygon() => new List<Point>(polygonPoints);

        private Ellipse draggingHandle = null;
        private Point dragOffset;

        private bool isGroupEditMode = false;
        private List<Point> selectedTiles = new List<Point>();

        // How the shown sheet is cut into cells (default: the map's tile size)
        private TilesheetDefinition grid = new TilesheetDefinition();
        private List<Point> markedCells = new List<Point>();

        private static readonly Brush MarkedFill = Frozen(new SolidColorBrush(Color.FromArgb(45, 0, 200, 255)));

        public ZoomableImageViewer()
        {
            InitializeComponent();
        }

        /// <summary>Sets how the image is cut into cells and redraws the grid.</summary>
        public void SetGrid(TilesheetDefinition sheet)
        {
            grid = sheet ?? new TilesheetDefinition();

            if (displayedImage != null)
                DrawGrid(displayedImage.Width, displayedImage.Height);

            ClearSelectedTileHighlight();
            RedrawGroupOverlay();
            RedrawMarkedCells();
        }

        /// <summary>Tints cells (e.g. cells that are already defined as tiles).</summary>
        public void SetMarkedCells(IEnumerable<Point> cells)
        {
            markedCells = cells?.ToList() ?? new List<Point>();
            RedrawMarkedCells();
        }

        public void SelectCell(int column, int row)
        {
            HighlightSelectedTile(column, row);
        }

        private Rect CellRect(int column, int row)
        {
            return new Rect(grid.CellLeft(column), grid.CellTop(row), grid.TileWidth, grid.TileHeight);
        }

        /// <summary>Cell under a point of the image, or null when outside the grid (margin, spacing, edges).</summary>
        private Point? PointToCell(Point pos)
        {
            if (grid.TileWidth <= 0 || grid.TileHeight <= 0 || displayedImage == null)
                return null;

            int column = (int)Math.Floor((pos.X - grid.Margin) / (grid.TileWidth + grid.Spacing));
            int row = (int)Math.Floor((pos.Y - grid.Margin) / (grid.TileHeight + grid.Spacing));

            if (column < 0 || row < 0 ||
                column >= grid.ColumnsFor((int)displayedImage.Width) || row >= grid.RowsFor((int)displayedImage.Height))
                return null;

            if (!CellRect(column, row).Contains(pos))
                return null; // in the spacing between cells

            return new Point(column, row);
        }

        private void RedrawMarkedCells()
        {
            RemoveTagged("Marked");

            foreach (var cell in markedCells)
            {
                var rect = CellRect((int)cell.X, (int)cell.Y);
                var shape = new Rectangle { Width = rect.Width, Height = rect.Height, Fill = MarkedFill, Tag = "Marked", IsHitTestVisible = false };
                Canvas.SetLeft(shape, rect.X);
                Canvas.SetTop(shape, rect.Y);
                Panel.SetZIndex(shape, 500);
                ImageCanvas.Children.Add(shape);
            }
        }

        private void RemoveTagged(string tag)
        {
            ImageCanvas.Children.OfType<FrameworkElement>()
                .Where(e => (e.Tag as string) == tag)
                .ToList()
                .ForEach(e => ImageCanvas.Children.Remove(e));
        }

        private static T Frozen<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

        public event Action<int, int> TileClicked;

        public void EnableCollisionPolygonEdit(List<Point> existingPoints)
        {
            isEditingCollision = true;
            polygonPoints = new List<Point>(existingPoints);
            RedrawOverlay();
        }

        public void DisableCollisionPolygonEdit()
        {
            isEditingCollision = false;
            polygonPoints.Clear();
            RemoveOverlay();
        }

        public void EnableGroupTileEdit()
        {
            isGroupEditMode = true;
            selectedTiles.Clear();
            ClearSelectedTileHighlight();
            RedrawGroupOverlay();
        }

        public void DisableGroupTileEdit()
        {
            isGroupEditMode = false;
            selectedTiles.Clear();
            RedrawGroupOverlay();
        }

        public List<Point> GetSelectedGroupTiles()
        {
            return new List<Point>(selectedTiles);
        }

        public void SetImage(ImageSource image)
        {
            ImageCanvas.Children.Clear();
            selectionHighlight = null;

            // Show 1 image pixel = 1 unit, so the grid (in pixels) lines up even when the
            // PNG was saved with a DPI other than 96 (image.Width would then be scaled)
            double width = image is BitmapSource bitmap ? bitmap.PixelWidth : image.Width;
            double height = image is BitmapSource bitmap2 ? bitmap2.PixelHeight : image.Height;

            displayedImage = new Image
            {
                Source = image,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(displayedImage, BitmapScalingMode.NearestNeighbor);

            Canvas.SetLeft(displayedImage, 0);
            Canvas.SetTop(displayedImage, 0);
            ImageCanvas.Width = width;
            ImageCanvas.Height = height;
            ImageCanvas.Children.Add(displayedImage);

            DrawGrid(width, height);

            zoom = 1.0;
            offset = new Vector(0, 0);
            ZoomTransform.ScaleX = zoom;
            ZoomTransform.ScaleY = zoom;
            PanTransform.X = 0;
            PanTransform.Y = 0;
        }

        private void DrawGrid(double width, double height)
        {
            RemoveTagged("Grid");

            int columns = grid.ColumnsFor((int)width);
            int rows = grid.RowsFor((int)height);
            if (columns <= 0 || rows <= 0)
                return;

            double top = grid.CellTop(0);
            double bottom = grid.CellTop(rows - 1) + grid.TileHeight;
            double left = grid.CellLeft(0);
            double right = grid.CellLeft(columns - 1) + grid.TileWidth;

            void AddLine(double x1, double y1, double x2, double y2)
            {
                ImageCanvas.Children.Add(new Line
                {
                    X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    Tag = "Grid",
                    IsHitTestVisible = false
                });
            }

            // Both edges of every cell, so margin/spacing gaps are visible
            for (int c = 0; c < columns; c++)
            {
                AddLine(grid.CellLeft(c), top, grid.CellLeft(c), bottom);
                AddLine(grid.CellLeft(c) + grid.TileWidth, top, grid.CellLeft(c) + grid.TileWidth, bottom);
            }

            for (int r = 0; r < rows; r++)
            {
                AddLine(left, grid.CellTop(r), right, grid.CellTop(r));
                AddLine(left, grid.CellTop(r) + grid.TileHeight, right, grid.CellTop(r) + grid.TileHeight);
            }
        }

        private void RedrawOverlay()
        {
            RemoveOverlay();

            polygonOverlay = new Polygon
            {
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(60, 0, 255, 0))
            };

            foreach (var pt in polygonPoints)
            {
                polygonOverlay.Points.Add(new Point(pt.X * displayedImage.Width, pt.Y * displayedImage.Height));
            }

            ImageCanvas.Children.Add(polygonOverlay);

            // Draw drag handles
            pointHandles.Clear();

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                var norm = polygonPoints[i];
                var screen = new Point(norm.X * displayedImage.Width, norm.Y * displayedImage.Height);

                var handle = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Tag = i,
                    Cursor = Cursors.Hand
                };

                Canvas.SetLeft(handle, screen.X - 4);
                Canvas.SetTop(handle, screen.Y - 4);
                Panel.SetZIndex(handle, 1001);

                handle.MouseLeftButtonDown += Handle_MouseLeftButtonDown;
                handle.MouseMove += Handle_MouseMove;
                handle.MouseRightButtonDown += Handle_MouseRightButtonDown;
                handle.MouseLeftButtonUp += Handle_MouseLeftButtonUp;

                pointHandles.Add(handle);
                ImageCanvas.Children.Add(handle);
            }
        }

        private void RemoveOverlay()
        {
            if (polygonOverlay != null)
                ImageCanvas.Children.Remove(polygonOverlay);

            foreach (var h in pointHandles)
                ImageCanvas.Children.Remove(h);

            pointHandles.Clear();
        }

        private void RedrawGroupOverlay()
        {
            // Remove old highlights
            ImageCanvas.Children.OfType<Rectangle>()
                .Where(r => (string)r.Tag == "GroupOverlay")
                .ToList()
                .ForEach(r => ImageCanvas.Children.Remove(r));

            foreach (var tile in selectedTiles)
            {
                var cell = CellRect((int)tile.X, (int)tile.Y);
                var rect = new Rectangle
                {
                    Width = cell.Width,
                    Height = cell.Height,
                    Stroke = Brushes.Orange,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 165, 0)),
                    Tag = "GroupOverlay",
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(rect, cell.X);
                Canvas.SetTop(rect, cell.Y);
                Panel.SetZIndex(rect, 1000);
                ImageCanvas.Children.Add(rect);
            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 0.1 : -0.1;
            zoom = Math.Max(0.2, Math.Min(5.0, zoom + delta));
            ZoomTransform.ScaleX = zoom;
            ZoomTransform.ScaleY = zoom;
            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingCollision)
            {
                var pos = e.GetPosition(displayedImage);
                double normX = pos.X / displayedImage.Width;
                double normY = pos.Y / displayedImage.Height;

                polygonPoints.Add(new Point(normX, normY));
                RedrawOverlay();
                return;
            }

            if (isGroupEditMode)
            {
                ToggleGroupCell(e.GetPosition(ImageCanvas));
                return;
            }

            dragStart = e.GetPosition(this);
            ImageCanvas.CaptureMouse();

            if (TileClicked != null && displayedImage?.Source != null)
            {
                var cell = PointToCell(e.GetPosition(ImageCanvas));
                if (cell == null)
                    return;

                int tileX = (int)cell.Value.X;
                int tileY = (int)cell.Value.Y;

                HighlightSelectedTile(tileX, tileY);
                TileClicked?.Invoke(tileX, tileY);
            }
        }

        private void ToggleGroupCell(Point pos)
        {
            var cell = PointToCell(pos);
            if (cell == null)
                return;

            if (!selectedTiles.Contains(cell.Value))
                selectedTiles.Add(cell.Value);
            else
                selectedTiles.Remove(cell.Value);

            RedrawGroupOverlay();
        }

        private void HighlightSelectedTile(int tileX, int tileY)
        {
            if (selectionHighlight != null)
            {
                ImageCanvas.Children.Remove(selectionHighlight);
            }

            var cell = CellRect(tileX, tileY);
            selectionHighlight = new Rectangle
            {
                Width = cell.Width,
                Height = cell.Height,
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0)),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(selectionHighlight, cell.X);
            Canvas.SetTop(selectionHighlight, cell.Y);
            Panel.SetZIndex(selectionHighlight, 1000);
            ImageCanvas.Children.Add(selectionHighlight);
        }

        public void ClearSelectedTileHighlight()
        {
            if (selectionHighlight != null)
            {
                ImageCanvas.Children.Remove(selectionHighlight);
                selectionHighlight = null;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragStart = null;
            ImageCanvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var current = e.GetPosition(this);
                var delta = current - dragStart.Value;
                offset += (Vector)delta;
                dragStart = current;

                PanTransform.X = offset.X;
                PanTransform.Y = offset.Y;
            }
        }

        private void Handle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isGroupEditMode)
            {
                ToggleGroupCell(e.GetPosition(ImageCanvas));
                return;
            }

            if (sender is Ellipse ellipse)
            {
                draggingHandle = ellipse;
                dragOffset = e.GetPosition(ellipse);
                ellipse.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Handle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggingHandle != null)
            {
                draggingHandle.ReleaseMouseCapture();
                draggingHandle = null;
                e.Handled = true;
            }
        }

        private void Handle_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingHandle != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var index = (int)draggingHandle.Tag;
                var mousePos = e.GetPosition(displayedImage);
                double normX = mousePos.X / displayedImage.Width;
                double normY = mousePos.Y / displayedImage.Height;

                polygonPoints[index] = new Point(normX, normY);
                RedrawOverlay(); // live update
                e.Handled = true;
            }
        }

        private void Handle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                int index = (int)ellipse.Tag;
                polygonPoints.RemoveAt(index);
                RedrawOverlay();
                e.Handled = true;
            }
        }
    }
}
