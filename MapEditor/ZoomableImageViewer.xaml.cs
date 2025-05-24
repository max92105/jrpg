using JRPG.Core.Maps;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
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

        public ZoomableImageViewer()
        {
            InitializeComponent();
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

        public void SetImage(BitmapImage image)
        {
            ImageCanvas.Children.Clear();

            displayedImage = new Image
            {
                Source = image,
                Width = image.PixelWidth,
                Height = image.PixelHeight,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(displayedImage, 0);
            Canvas.SetTop(displayedImage, 0);
            ImageCanvas.Width = image.PixelWidth;
            ImageCanvas.Height = image.PixelHeight;
            ImageCanvas.Children.Add(displayedImage);

            DrawGrid(image.PixelWidth, image.PixelHeight);

            // Reset transforms
            zoom = 1.0;
            offset = new Vector(0, 0);
            ZoomTransform.ScaleX = zoom;
            ZoomTransform.ScaleY = zoom;
            PanTransform.X = 0;
            PanTransform.Y = 0;
        }

        private void DrawGrid(double width, double height)
        {
            int tilesX = (int)(width / MapConstant.TileSize);
            int tilesY = (int)(height / MapConstant.TileSize);

            for (int x = 0; x <= tilesX; x++)
            {
                var line = new Line
                {
                    X1 = x * MapConstant.TileSize,
                    Y1 = 0,
                    X2 = x * MapConstant.TileSize,
                    Y2 = height,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                ImageCanvas.Children.Add(line);
            }

            for (int y = 0; y <= tilesY; y++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y * MapConstant.TileSize,
                    X2 = width,
                    Y2 = y * MapConstant.TileSize,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                ImageCanvas.Children.Add(line);
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

            dragStart = e.GetPosition(this);
            ImageCanvas.CaptureMouse();

            if (TileClicked != null && displayedImage?.Source != null)
            {
                var pos = e.GetPosition(ImageCanvas);
                int tileX = (int)(pos.X / MapConstant.TileSize);
                int tileY = (int)(pos.Y / MapConstant.TileSize);

                HighlightSelectedTile(tileX, tileY);
                TileClicked?.Invoke(tileX, tileY);
            }
        }

        private void HighlightSelectedTile(int tileX, int tileY)
        {
            if (selectionHighlight != null)
            {
                ImageCanvas.Children.Remove(selectionHighlight);
            }

            selectionHighlight = new Rectangle
            {
                Width = MapConstant.TileSize,
                Height = MapConstant.TileSize,
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0))
            };

            Canvas.SetLeft(selectionHighlight, tileX * MapConstant.TileSize);
            Canvas.SetTop(selectionHighlight, tileY * MapConstant.TileSize);
            Panel.SetZIndex(selectionHighlight, 1000);
            ImageCanvas.Children.Add(selectionHighlight);
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
