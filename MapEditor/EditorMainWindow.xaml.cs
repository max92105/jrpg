using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using JRPG.Core.Maps.Models;
using System.Linq;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps;
using JRPG.Core.Utils;
using JRPG.Core.Models;
using System.Collections.Generic;
using JRPG.Core.Triggers.Models;
using System.Threading;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : Window
    {
        private Map map = new Map();

        //Default map Size
        private int mapWidth = 20;
        private int mapHeight = 15;

        //Selection
        private int selectedTileId = 0;
        private int selectedObjectId = 1;

        //Options
        private bool showGrid = true;
        private string currentMapName;
        private enum EditorMode { Tile, Object, Trigger }
        private EditorMode currentMode = EditorMode.Tile;

        //Zoom
        private double zoom = 1.0;
        private Point? dragStart = null;
        private TranslateTransform canvasOffset = new TranslateTransform();

        //Drag
        private Vector currentPanOffset = new Vector(0, 0);

        //Palette
        private Dictionary<int, TileDefinition> tileDefinitions;
        private Dictionary<int, ObjectDefinition> objectDefinitions;

        public EditorMainWindow()
        {
            InitializeComponent();

            // Load definitions from JSON
            tileDefinitions = TilesetLoader.Load(AssetPathResolver.GetPath(MapConstant.TilesetJsonPath));
            objectDefinitions = ObjectSetLoader.Load(AssetPathResolver.GetPath(MapConstant.ObjectSetJsonPath));

            map = new Map
            {
                Name = currentMapName,
                Width = mapWidth,
                Height = mapHeight,
                Tiles = new List<Tile>(),
                TileDefinitions = tileDefinitions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ObjectDefinitions = objectDefinitions
            };

            InitTilePalette();
            InitObjectPalette();
            RedrawMap();
        }

        private void InitTilePalette()
        {
            TilePalette.Items.Clear();

            foreach (var kv in tileDefinitions)
            {
                var def = kv.Value;
                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.Sprite ?? def.Frames?.FirstOrDefault())))
                };
                var item = new ListBoxItem { Content = img, Tag = kv.Key };
                TilePalette.Items.Add(item);
            }

            TilePalette.SelectionChanged += (s, e) =>
            {
                if (TilePalette.SelectedItem is ListBoxItem selected)
                    selectedTileId = (int)selected.Tag;
            };

            TilePalette.SelectedIndex = 0;
        }

        private void InitObjectPalette()
        {
            ObjectPalette.Items.Clear();

            foreach (var kv in objectDefinitions)
            {
                var def = kv.Value;
                if (string.IsNullOrEmpty(def.SpritePath)) continue;

                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.SpritePath)))
                };
                var item = new ListBoxItem { Content = img, Tag = kv.Key };
                ObjectPalette.Items.Add(item);
            }

            ObjectPalette.SelectionChanged += (s, e) =>
            {
                if (ObjectPalette.SelectedItem is ListBoxItem selected)
                    selectedObjectId = (int)selected.Tag;
            };

            ObjectPalette.SelectedIndex = 0;
        }

        private void MapCanvas_Click(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            double x = pos.X / MapConstant.TileSize;
            double y = pos.Y / MapConstant.TileSize;
            int tileX = (int)Math.Floor(x);
            int tileY = (int)Math.Floor(y);

            if (tileX >= 0 && tileY >= 0 && tileX < map.Width && tileY < map.Height)
            {
                if (currentMode == EditorMode.Trigger)
                {
                    var tile = map.GetTileAt(tileX, tileY);
                    if (tile == null)
                    {
                        tile = new Tile { X = tileX, Y = tileY, Id = 0 };
                        map.Tiles.Add(tile);
                    }

                    var triggerEditWindow = new TriggerEditWindow(tile.Trigger);
                    if (triggerEditWindow.ShowDialog() == true)
                    {
                        tile.Trigger = triggerEditWindow.Result;
                    }
                }

                if (currentMode == EditorMode.Object)
                    map.Objects.Add(new PlacedObject { Id = selectedObjectId, X = x, Y = y });

                if (currentMode == EditorMode.Tile)
                    map.SetTile(tileX, tileY, selectedTileId);

                RedrawMap();
            }
        }

        private void RedrawMap()
        {
            if (MapCanvas == null) return;

            MapCanvas.Children.Clear();
            MapCanvas.Width = mapWidth * MapConstant.TileSize;
            MapCanvas.Height = mapHeight * MapConstant.TileSize;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int tileId = map.GetTileIdAt(x, y);

                    if (tileDefinitions.TryGetValue(tileId, out var tileDef))
                    {
                        string spritePath = tileDef.Sprite ?? tileDef.Frames?.FirstOrDefault();
                        if (!string.IsNullOrEmpty(spritePath))
                        {
                            var img = new Image
                            {
                                Width = MapConstant.TileSize,
                                Height = MapConstant.TileSize,
                                Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetPath(), spritePath)))
                            };
                            Canvas.SetLeft(img, x * MapConstant.TileSize);
                            Canvas.SetTop(img, y * MapConstant.TileSize);
                            MapCanvas.Children.Add(img);
                        }
                    }

                    var tile = map.GetTileAt(x, y);
                    if (tile?.Trigger != null)
                    {
                        var overlay = new System.Windows.Shapes.Rectangle
                        {
                            Width = 10,
                            Height = 10,
                            Fill = Brushes.Yellow,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(overlay, x * MapConstant.TileSize + MapConstant.TileSize - 12);
                        Canvas.SetTop(overlay, y * MapConstant.TileSize + 2);
                        Panel.SetZIndex(overlay, 100); // on top
                        MapCanvas.Children.Add(overlay);
                    }
                }
            }

            foreach (var obj in map.Objects)
            {
                if (!objectDefinitions.TryGetValue(obj.Id, out var def) || string.IsNullOrEmpty(def.SpritePath)) continue;

                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.SpritePath)))
                };
                Canvas.SetLeft(img, (obj.X * MapConstant.TileSize) - MapConstant.TileSize / 2);
                Canvas.SetTop(img, (obj.Y * MapConstant.TileSize) - MapConstant.TileSize / 2);
                Panel.SetZIndex(img, 10);
                MapCanvas.Children.Add(img);
            }

            DrawGridOverlay();
        }

        private void DrawGridOverlay()
        {
            if (!showGrid) return;

            for (int y = 0; y <= mapHeight; y++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    Y1 = y * MapConstant.TileSize,
                    X2 = mapWidth * MapConstant.TileSize,
                    Y2 = y * MapConstant.TileSize,
                    Stroke = Brushes.Red,
                    StrokeThickness = 0.5
                };
                Panel.SetZIndex(line, 1000);
                MapCanvas.Children.Add(line);
            }

            for (int x = 0; x <= mapWidth; x++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x * MapConstant.TileSize,
                    Y1 = 0,
                    X2 = x * MapConstant.TileSize,
                    Y2 = mapHeight * MapConstant.TileSize,
                    Stroke = Brushes.Red,
                    StrokeThickness = 0.5
                };
                Panel.SetZIndex(line, 1000);
                MapCanvas.Children.Add(line);
            }
        }

        private void SaveMap_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON Map (*.json)|*.json",
                DefaultExt = ".json",
                Title = "Save Map",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
                FileName = currentMapName
            };

            if (saveDialog.ShowDialog() == true)
            {
                map.Name = currentMapName;
                map.Width = mapWidth;
                map.Height = mapHeight;

                // Keep only valid tile definitions
                map.TileDefinitions = tileDefinitions
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Sprite ?? kvp.Value.Frames?.FirstOrDefault()))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                    );

                // Keep only valid object definitions
                map.ObjectDefinitions = objectDefinitions
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value.SpritePath))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                    );

                MapSerializer.Save(saveDialog.FileName, map);
                MessageBox.Show($"Map saved to {Path.GetFileName(saveDialog.FileName)}");
            }
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON Map (*.json)|*.json",
                Title = "Load Map",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
            };

            if (openDialog.ShowDialog() == true)
            {
                map = MapSerializer.Load(openDialog.FileName);
                if (map == null)
                {
                    MessageBox.Show("Failed to load map.");
                    return;
                }

                currentMapName = map.Name;
                mapWidth = map.Width;
                mapHeight = map.Height;

                MapNameBox.Text = map.Name;
                MapWidthBox.Text = map.Width.ToString();
                MapHeightBox.Text = map.Height.ToString();

                RedrawMap();
                ApplyCanvasTransform();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MapNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentMapName = MapNameBox.Text.Trim();
        }

        private void GridToggle_Checked(object sender, RoutedEventArgs e)
        {
            showGrid = (sender as CheckBox)?.IsChecked == true;
            RedrawMap();
        }

        private void NewMap_Click(object sender, RoutedEventArgs e)
        {
            map.Tiles.Clear();
            map.Objects.Clear();

            currentMapName = "New Map";
            MapNameBox.Text = currentMapName;

            RedrawMap();
        }

        private void ApplySize_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MapWidthBox.Text, out int width) && int.TryParse(MapHeightBox.Text, out int height))
            {
                if (width <= 0 || height <= 0)
                {
                    MessageBox.Show("Size must be positive.");
                    return;
                }

                mapWidth = width;
                mapHeight = height;
                map.Tiles = new List<Tile>();

                map.Width = width;
                map.Height = height;

                RedrawMap();
            }
            else
            {
                MessageBox.Show("Invalid map size.");
            }
        }

        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 0.1 : -0.1;
            zoom = Math.Max(0.2, Math.Min(5, zoom + delta));
            ApplyCanvasTransform();
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragStart.HasValue && e.RightButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(this);
                var delta = currentPos - dragStart.Value;
                var offset = currentPanOffset + delta;

                currentPanOffset = offset;
                dragStart = currentPos;

                ApplyCanvasTransform();
            }
        }

        private void ApplyCanvasTransform()
        {
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform(zoom, zoom));
            group.Children.Add(new TranslateTransform(currentPanOffset.X, currentPanOffset.Y));
            MapCanvas.RenderTransform = group;
        }

        private void MapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentMode == EditorMode.Object)
            {

                var pos = e.GetPosition(MapCanvas);
                var target = GetObjectUnderPointer(pos);

                if (target != null)
                {
                    map.Objects.Remove(target);
                    RedrawMap();
                }

                return;
            }

            if (currentMode == EditorMode.Trigger)
            {
                var pos = e.GetPosition(MapCanvas);
                int tileX = (int)Math.Floor(pos.X / MapConstant.TileSize);
                int tileY = (int)Math.Floor(pos.Y / MapConstant.TileSize);

                var tile = map.GetTileAt(tileX, tileY);

                if (tile != null && tile.Trigger != null)
                {
                    tile.Trigger = null;
                    RedrawMap();
                }

                return;
            }

            dragStart = e.GetPosition(this); // screen-relative
            MapCanvas.CaptureMouse();
        }

        private void MapCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragStart = null;
            MapCanvas.ReleaseMouseCapture();
        }

        private void TileMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Tile;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Visible;
        }

        private void ObjectMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Object;
            ObjectPalette.Visibility = Visibility.Visible;
            TilePalette.Visibility = Visibility.Collapsed;
        }

        private void TriggerMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Trigger;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Collapsed;
        }

        private PlacedObject GetObjectUnderPointer(Point pointer)
        {
            double px = pointer.X / MapConstant.TileSize;
            double py = pointer.Y / MapConstant.TileSize;

            foreach (var obj in map.Objects)
            {
                double dx = obj.X - px;
                double dy = obj.Y - py;
                double distSq = dx * dx + dy * dy;

                if (distSq < 0.25) // ~half-tile radius
                    return obj;
            }

            return null;
        }
    }
}
