using JRPG.Core.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using JRPG.Core.Maps;
using JRPG.Core.Utils;
using System.IO;

namespace MapEditor
{
    public partial class TilesheetImportWindow : Window
    {
        public Dictionary<int, TileDefinition> GeneratedTiles { get; private set; } = new Dictionary<int, TileDefinition>();

        private BitmapImage sheet;
        private int columns;
        private int rows;
        private string tilesheetPath;

        public TilesheetImportWindow()
        {
            InitializeComponent();

            var dialog = new OpenFileDialog
            {
                Filter = "Tilesheet PNG|*.png",
                Title = "Select a Tilesheet"
            };

            if (dialog.ShowDialog() != true)
            {
                DialogResult = false;
                Close();
                return;
            }

            var destPath = System.IO.Path.Combine(AssetPathResolver.GetAssetsRootPath(), System.IO.Path.GetFileName(dialog.FileName));
            File.Copy(dialog.FileName, destPath, overwrite: true);
            tilesheetPath = System.IO.Path.GetFileName(destPath);

            sheet = new BitmapImage(new Uri(dialog.FileName));
            columns = sheet.PixelWidth / MapConstant.TileSize;
            rows = sheet.PixelHeight / MapConstant.TileSize;

            TilesheetCanvas.Width = sheet.PixelWidth;
            TilesheetCanvas.Height = sheet.PixelHeight;

            var img = new Image
            {
                Source = sheet,
                Width = sheet.PixelWidth,
                Height = sheet.PixelHeight
            };
            TilesheetCanvas.Children.Add(img);

            DrawGrid();
        }

        private void DrawGrid()
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var rect = new Rectangle
                    {
                        Width = MapConstant.TileSize,
                        Height = MapConstant.TileSize,
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, x * MapConstant.TileSize);
                    Canvas.SetTop(rect, y * MapConstant.TileSize);
                    TilesheetCanvas.Children.Add(rect);
                }
            }
        }

        private void AddAll_Click(object sender, RoutedEventArgs e)
        {
            int id = 1000; // Start high to avoid collisions
            string prefix = NamePrefixBox.Text.Trim();
            bool walkable = WalkableCheck.IsChecked == true;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    GeneratedTiles[id] = new TileDefinition
                    {
                        Name = $"{prefix}_{x}_{y}",
                        IsWalkable = walkable,
                        Tilesheet = tilesheetPath,
                        SheetX = x,
                        SheetY = y
                    };
                    id++;
                }
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}