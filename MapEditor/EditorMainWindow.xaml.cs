using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using System.Collections.Generic;
using JRPG.Core.Maps.Models;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : Window
    {
        private int[,] mapData;
        private List<PlacedObject> placedObjects = new List<PlacedObject>();

        private int mapWidth = 20;
        private int mapHeight = 15;

        private const int tileSize = 64;

        private int selectedTileId = 0;
        private int selectedObjectId = 1;

        private bool showGrid = true;
        private bool placingObjects = false;

        private string currentMapName = "New Map";

        //Zoom
        private double zoom = 1.0;
        private Point? dragStart = null;
        private TranslateTransform canvasOffset = new TranslateTransform();

        //Drag
        private Vector currentPanOffset = new Vector(0, 0);

        private readonly string[] tilePaths =
        {
            //TODO::Change That
            "C:\\git\\jrpg\\JRPG\\Assets\\floor.png",
            "C:\\git\\jrpg\\JRPG\\Assets\\wall.png",
            "C:\\git\\jrpg\\JRPG\\Assets\\Grass1.png",
            "C:\\git\\jrpg\\JRPG\\Assets\\Grass2.png",
            "C:\\git\\jrpg\\JRPG\\Assets\\Grass3.png"
        };

        private readonly ObjectDefinition[] objectDefinitions = new ObjectDefinition[]
        {
            null,
            new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\tree.png",
                HasCollision = true,
                CollisionBox = new Rect(8, 24, 48, 32) // blocks bottom half
            },
            new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\Tree2.png",
                HasCollision = true,
                CollisionBox = new Rect(8, 24, 48, 32) // blocks bottom half
            },
            new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\Tree3.png",
                HasCollision = true,
                CollisionBox = new Rect(8, 24, 48, 32) // blocks bottom half
            },
            new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\Leaf.png",
                HasCollision = false,
            },
            new ObjectDefinition
            {
                SpritePath = "C:\\git\\jrpg\\JRPG\\Assets\\Leafs.png",
                HasCollision = false,
            }
        };

        public EditorMainWindow()
        {
            InitializeComponent();

            mapData = new int[mapWidth, mapHeight];
            InitTilePalette();
            InitObjectPalette();
            RedrawMap();
        }

        private void InitTilePalette()
        {
            for (int i = 0; i < tilePaths.Length; i++)
            {
                var img = new Image
                {
                    Width = tileSize,
                    Height = tileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), tilePaths[i])))
                };
                var item = new ListBoxItem { Content = img, Tag = i };
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
            for (int i = 1; i < objectDefinitions.Length; i++) // skip 0 (empty)
            {
                if (objectDefinitions[i] == null) continue;

                var img = new Image
                {
                    Width = tileSize,
                    Height = tileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), objectDefinitions[i].SpritePath)))
                };
                var item = new ListBoxItem { Content = img, Tag = i };
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
            double x = pos.X / tileSize;
            double y = pos.Y / tileSize;
            int tileX = (int)Math.Floor(x);
            int tileY = (int)Math.Floor(y);

            if (tileX >= 0 && tileY >= 0 && tileX < mapWidth && tileY < mapHeight)
            {
                if (placingObjects)
                    placedObjects.Add(new PlacedObject { Id = selectedObjectId, X = x, Y = y });
                else
                    mapData[tileX, tileY] = selectedTileId;
                RedrawMap();
            }
        }

        private void RedrawMap()
        {
            if (MapCanvas != null)
            {
                MapCanvas.Children.Clear();
                MapCanvas.Width = mapWidth * tileSize;
                MapCanvas.Height = mapHeight * tileSize;

                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        int tileId = mapData[x, y];
                        if (tileId >= 0 && tileId < tilePaths.Length)
                        {
                            var img = new Image
                            {
                                Width = tileSize,
                                Height = tileSize,
                                Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), tilePaths[tileId])))
                            };
                            Canvas.SetLeft(img, x * tileSize);
                            Canvas.SetTop(img, y * tileSize);
                            MapCanvas.Children.Add(img);
                        }
                    }
                }

                foreach (var obj in placedObjects)
                {
                    var def = objectDefinitions[obj.Id];
                    if (def == null || def.SpritePath == null) continue;

                    var img = new Image
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), def.SpritePath)))
                    };
                    Canvas.SetLeft(img, (obj.X * tileSize) - tileSize / 2);
                    Canvas.SetTop(img, (obj.Y * tileSize) - tileSize / 2);
                    Panel.SetZIndex(img, 10);
                    MapCanvas.Children.Add(img);
                }

                DrawGridOverlay();
            }
        }

        private void DrawGridOverlay()
        {
            if (!showGrid) return;

            for (int y = 0; y <= mapHeight; y++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    Y1 = y * tileSize,
                    X2 = mapWidth * tileSize,
                    Y2 = y * tileSize,
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
                    X1 = x * tileSize,
                    Y1 = 0,
                    X2 = x * tileSize,
                    Y2 = mapHeight * tileSize,
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
                Filter = "Map Files (*.map)|*.map",
                DefaultExt = ".map",
                Title = "Save Map"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                {
                    writer.WriteLine($"NAME: {currentMapName}");
                    writer.WriteLine("MAP");
                    for (int y = 0; y < mapHeight; y++)
                    {
                        string line = "";
                        for (int x = 0; x < mapWidth; x++)
                            line += mapData[x, y].ToString();
                        writer.WriteLine(line);
                    }
                    writer.WriteLine("OBJECTS");
                    foreach (var obj in placedObjects)
                    {
                        writer.WriteLine($"{obj.Id}:{obj.X:F2}.{obj.Y:F2}");
                    }
                    writer.WriteLine("TRIGGERS");
                    MessageBox.Show($"Map saved to {Path.GetFileName(saveDialog.FileName)}");
                }
            }
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Map Files (*.map)|*.map",
                Title = "Load Map"
            };

            if (openDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openDialog.FileName);
                placedObjects.Clear();

                bool readingMap = false;
                bool readingObjects = false;
                int y = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith("NAME:"))
                    {
                        currentMapName = line.Substring(5).Trim();
                        continue;
                    }
                    if (line == "MAP") { readingMap = true; readingObjects = false; y = 0; continue; }
                    if (line == "OBJECTS") { readingMap = false; readingObjects = true; y = 0; continue; }
                    if (line.StartsWith("TRIGGERS")) break;

                    if (readingMap && y < mapHeight)
                    {
                        for (int x = 0; x < Math.Min(line.Length, mapWidth); x++)
                            mapData[x, y] = int.Parse(line[x].ToString());
                        y++;
                    }
                    else if (readingObjects)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int id))
                        {
                            var coords = parts[1].Split('.');
                            if (coords.Length == 2 &&
                                double.TryParse(coords[0], out double x) &&
                                double.TryParse(coords[1], out double objY))
                            {
                                placedObjects.Add(new PlacedObject { Id = id, X = x, Y = objY });
                            }
                        }
                    }
                }

                MapNameBox.Text = currentMapName;
                MapWidthBox.Text = mapWidth.ToString();
                MapHeightBox.Text = mapHeight.ToString();

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
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    mapData[x, y] = 0; // default tile ID
                }
            }

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
                mapData = new int[mapWidth, mapHeight];
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
            if (placingObjects)
            {

                var pos = e.GetPosition(MapCanvas);
                var target = GetObjectUnderPointer(pos);

                if (target != null)
                {
                    placedObjects.Remove(target);
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
            placingObjects = false;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Visible;
        }

        private void ObjectMode_Checked(object sender, RoutedEventArgs e)
        {
            placingObjects = true;
            ObjectPalette.Visibility = Visibility.Visible;
            TilePalette.Visibility = Visibility.Collapsed;
        }

        private PlacedObject GetObjectUnderPointer(Point pointer)
        {
            double px = pointer.X / tileSize;
            double py = pointer.Y / tileSize;

            foreach (var obj in placedObjects)
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
