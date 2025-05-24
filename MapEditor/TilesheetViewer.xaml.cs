using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Maps;
using JRPG.Core.Utils;

namespace MapEditor
{
    public partial class TilesheetViewer : UserControl
    {
        private BitmapImage tilesheet;
        private int tileSize = MapConstant.TileSize;
        private int columns;
        private int rows;

        public string TilesheetPath { get; private set; }
        public int SelectedX { get; private set; }
        public int SelectedY { get; private set; }

        public event Action<int, int> TileSelected;

        public TilesheetViewer()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        public void LoadSheet(string relativePath)
        {
            TilesheetPath = relativePath;

            tilesheet = new BitmapImage(new Uri(AssetPathResolver.GetTilePath(relativePath)));

            columns = tilesheet.PixelWidth / tileSize;
            rows = tilesheet.PixelHeight / tileSize;

            SheetImage.Source = tilesheet;
            SheetImage.Width = tilesheet.PixelWidth;
            SheetImage.Height = tilesheet.PixelHeight;

            Width = SheetImage.Width;
            Height = SheetImage.Height;

            DrawGrid();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SheetImage.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(SheetImage);
            int x = (int)(pos.X / tileSize);
            int y = (int)(pos.Y / tileSize);

            SelectedX = x;
            SelectedY = y;

            HighlightSelectedTile(x, y);
            TileSelected?.Invoke(x, y);
        }

        private void HighlightSelectedTile(int x, int y)
        {
            HighlightBox.Width = tileSize;
            HighlightBox.Height = tileSize;
            Canvas.SetLeft(HighlightBox, x * tileSize);
            Canvas.SetTop(HighlightBox, y * tileSize);
            HighlightBox.Visibility = Visibility.Visible;
        }

        private void DrawGrid()
        {
            GridCanvas.Children.Clear();

            for (int x = 0; x < columns; x++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x * tileSize,
                    X2 = x * tileSize,
                    Y1 = 0,
                    Y2 = rows * tileSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                GridCanvas.Children.Add(line);
            }

            for (int y = 0; y < rows; y++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    X2 = columns * tileSize,
                    Y1 = y * tileSize,
                    Y2 = y * tileSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                GridCanvas.Children.Add(line);
            }
        }
    }
}
