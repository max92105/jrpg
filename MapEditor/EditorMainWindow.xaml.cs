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
using MapEditor.Undo;
using MapEditor.Commands;
using MapEditor.Enums;
using MapEditor.Utils;
using MapEditor.Controls;
using static System.Net.WebRequestMethods;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorMainWindow : Window
    {
        private Map map = new Map();

        //Default map Size
        private int mapWidth = 30;
        private int mapHeight = 17;

        //Selection
        private int selectedTileId = 0;
        private int selectedObjectId = 1;

        //Options
        private bool showGrid = true;
        private string currentMapName;

        // Preview panel state
        private double assetZoom = 1.0;
        private Point? assetDragStart = null;
        private Vector assetOffset = new Vector(0, 0);

        // Track current folder selection
        private string currentAssetFolder = "";
        private string selectedAssetPath = "";

        //Zoom
        private double zoom = 1.0;
        private Point? dragStart = null;
        private TranslateTransform canvasOffset = new TranslateTransform();

        //Drag map
        private Vector currentPanOffset = new Vector(0, 0);
        private bool isSpaceHeld = false;

        //Drag Object
        private PlacedObject draggedObject = null;
        private Point dragOffset;

        //Selection
        private Point? selectionStart = null;
        private Rect selectionRect;
        private bool isSelecting = false;

        //Z placement
        private int selectedLayer = 0;

        private int selectedSheetX = -1;
        private int selectedSheetY = -1;
        private string selectedTilesheet = null;

        //SelectionMode
        private PlacedObject hoveredObject = null;
        private Point? hoveredObjectTile = null;


        private readonly Dictionary<string, BitmapImage> tilesheetCache = new Dictionary<string, BitmapImage>();


        private readonly UndoManager undoManager = new UndoManager();

        public EditorMainWindow()
        {
            InitializeComponent();

            TileDatabase.Load();
            ObjectDatabase.Load();

            map = new Map
            {
                Name = currentMapName,
                Width = mapWidth,
                Height = mapHeight,
                Tiles = new List<Tile>()
            };

            this.PreviewKeyDown += EditorMainWindow_PreviewKeyDown;
            this.PreviewKeyUp += EditorMainWindow_PreviewKeyUp;

            PropertyEditorControl.RequestEditTileCollision += OpenCollisionEditorForTile;
            PropertyEditorControl.RequestEditObjectCollision += OpenCollisionEditorForObject;

            RedrawMap();
        }

        private BitmapImage GetOrLoadTilesheet(string path)
        {
            if (tilesheetCache.TryGetValue(path, out var cached))
                return cached;

            var sheet = new BitmapImage();
            sheet.BeginInit();
            sheet.CacheOption = BitmapCacheOption.OnLoad;
            sheet.UriSource = new Uri(path, UriKind.Absolute);
            sheet.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            sheet.EndInit();
            sheet.Freeze();

            tilesheetCache[path] = sheet;
            return sheet;
        }

        private void OpenCollisionEditorForTile(TileDefinition def)
        {
            var bitmap = LoadTileImage(def); // you'll write this helper
            var win = new CollisionEditorWindow(def.CollisionPolygon);
            win.SetImageAndPolygon(bitmap, def.CollisionPolygon);
            if (win.ShowDialog() == true)
            {
                def.CollisionPolygon = win.Result.ToList();
                TileDatabase.AddOrUpdate(def);
                TileDatabase.Save();
            }
        }

        private ImageSource LoadTileImage(TileDefinition def)
        {
            if (!string.IsNullOrEmpty(def.Sprite))
            {
                var path = Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.Sprite);
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }

            if (!string.IsNullOrEmpty(def.Tilesheet) && def.SheetX.HasValue && def.SheetY.HasValue)
            {
                var path = AssetPathResolver.GetTilePath(def.Tilesheet);
                var sheet = new BitmapImage(new Uri(path, UriKind.Absolute));
                var cropped = new CroppedBitmap(sheet, new Int32Rect(def.SheetX.Value * MapConstant.TileSize, def.SheetY.Value * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                return BitmapFrame.Create(cropped);
            }

            return null;
        }

        private void OpenCollisionEditorForObject(ObjectDefinition def)
        {
            var bitmap = RenderObjectPreview(def); // you'll write this too
            var win = new CollisionEditorWindow(def.CollisionPolygon);
            win.SetImageAndPolygon(bitmap, def.CollisionPolygon);
            if (win.ShowDialog() == true)
            {
                def.CollisionPolygon = win.Result.ToList();
                ObjectDatabase.AddOrUpdate(def);
                ObjectDatabase.Save();
            }
        }

        private ImageSource RenderObjectPreview(ObjectDefinition def)
        {
            int tileSize = MapConstant.TileSize;

            int minX = def.Tiles.Min(t => t.X);
            int minY = def.Tiles.Min(t => t.Y);
            int maxX = def.Tiles.Max(t => t.X);
            int maxY = def.Tiles.Max(t => t.Y);

            int width = (maxX - minX + 1) * tileSize;
            int height = (maxY - minY + 1) * tileSize;

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                foreach (var entry in def.Tiles)
                {
                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef))
                        continue;

                    var img = LoadTileImage(tileDef);
                    if (img == null) continue;

                    int dx = (entry.X - minX) * tileSize;
                    int dy = (entry.Y - minY) * tileSize;

                    context.DrawImage(img, new Rect(dx, dy, tileSize, tileSize));
                }
            }

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return BitmapFrame.Create(bmp);
        }

        private void RedrawMap()
        {
            if (MapCanvas == null) return;
            MapCanvas.Children.Clear();
            MapCanvas.Width = mapWidth * MapConstant.TileSize;
            MapCanvas.Height = mapHeight * MapConstant.TileSize;

            DrawTiles();
            DrawTriggers();
            DrawObjects();
            DrawSelectionBox();
            DrawGridOverlay();
        }

        private void DrawTiles()
        {
            bool showCollision = ToggleCollisionOverlay?.IsChecked == true;

            double canvasWidth = MapScrollViewer.ViewportWidth / zoom;
            double canvasHeight = MapScrollViewer.ViewportHeight / zoom;
            double offsetX = -currentPanOffset.X / zoom;
            double offsetY = -currentPanOffset.Y / zoom;

            Point topLeft = MapScrollViewer.TranslatePoint(new Point(0, 0), MapCanvas);
            Point bottomRight = MapScrollViewer.TranslatePoint(new Point(MapScrollViewer.ViewportWidth, MapScrollViewer.ViewportHeight), MapCanvas);

            int startX = Math.Max(0, (int)Math.Floor(topLeft.X / MapConstant.TileSize));
            int endX = Math.Min(mapWidth, (int)Math.Ceiling(bottomRight.X / MapConstant.TileSize));
            int startY = Math.Max(0, (int)Math.Floor(topLeft.Y / MapConstant.TileSize));
            int endY = Math.Min(mapHeight, (int)Math.Ceiling(bottomRight.Y / MapConstant.TileSize));

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    foreach (var tile in map.GetTilesAt(x, y))
                    {
                        if (!TileDatabase.Definitions.TryGetValue(tile.Id, out var tileDef)) continue;

                        BitmapSource source = null;

                        if (!string.IsNullOrEmpty(tileDef.Tilesheet) && tileDef.SheetX.HasValue && tileDef.SheetY.HasValue)
                        {
                            var sheetPath = AssetPathResolver.GetTilePath(tileDef.Tilesheet);

                            var sheet = GetOrLoadTilesheet(sheetPath);

                            source = new CroppedBitmap(sheet,
                                new Int32Rect(tileDef.SheetX.Value * MapConstant.TileSize, tileDef.SheetY.Value * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                        }
                        else if (!string.IsNullOrEmpty(tileDef.Sprite))
                        {
                            var fullPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), tileDef.Sprite);
                            source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                        }

                        if (source == null) continue;

                        var img = new Image
                        {
                            Width = MapConstant.TileSize,
                            Height = MapConstant.TileSize,
                            Source = source,
                            IsHitTestVisible = false
                        };

                        Canvas.SetLeft(img, x * MapConstant.TileSize);
                        Canvas.SetTop(img, y * MapConstant.TileSize);
                        Panel.SetZIndex(img, tile.Z);
                        MapCanvas.Children.Add(img);

                        if (showCollision && tileDef.CollisionPolygon != null && tileDef.CollisionPolygon.Count >= 2)
                        {
                            var polygon = new System.Windows.Shapes.Polygon
                            {
                                Stroke = Brushes.Lime,
                                StrokeThickness = 1,
                                Fill = new SolidColorBrush(Color.FromArgb(40, 0, 255, 0)),
                                Points = new PointCollection(tileDef.CollisionPolygon.Select(p =>
                                  new Point((x + p.X) * MapConstant.TileSize,
                                        (y + p.Y) * MapConstant.TileSize)))
                            };

                            Panel.SetZIndex(polygon, tile.Z + 1);
                            MapCanvas.Children.Add(polygon);
                        }
                    }
                }
            }
        }

        private void RedrawHighlightLayer()
        {
            HighlightCanvas.Children.Clear();

            if (hoveredObject != null && hoveredObjectTile != null)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(hoveredObject.Id, out var def)) return;

                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);

                double tileX = (hoveredObject.X + hoveredObjectTile.Value.X - minX) * MapConstant.TileSize;
                double tileY = (hoveredObject.Y + hoveredObjectTile.Value.Y - minY) * MapConstant.TileSize;

                var rect = new Rectangle
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Stroke = Brushes.Gold,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(40, 255, 215, 0)),
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(rect, tileX);
                Canvas.SetTop(rect, tileY);
                HighlightCanvas.Children.Add(rect);
            }

            if (!selectionRect.IsEmpty && isSelecting)
            {
                var sel = new System.Windows.Shapes.Rectangle
                {
                    Width = selectionRect.Width,
                    Height = selectionRect.Height,
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 255))
                };

                Canvas.SetLeft(sel, selectionRect.X);
                Canvas.SetTop(sel, selectionRect.Y);
                Panel.SetZIndex(sel, 1);
                HighlightCanvas.Children.Add(sel);
            }
        }

        private void DrawTriggers()
        {
            foreach (var tile in map.Tiles.Where(t => t.Trigger != null))
            {
                var overlay = new System.Windows.Shapes.Rectangle
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Yellow,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(overlay, tile.X * MapConstant.TileSize + MapConstant.TileSize - 12);
                Canvas.SetTop(overlay, tile.Y * MapConstant.TileSize + 2);
                Panel.SetZIndex(overlay, 100);
                MapCanvas.Children.Add(overlay);
            }
        }

        private void DrawObjects()
        {
            foreach (var obj in map.Objects)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def)) continue;
                if (def.Tiles == null) continue;

                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);

                foreach (var entry in def.Tiles)
                {
                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef)) continue;

                    BitmapSource source = null;

                    if (!string.IsNullOrEmpty(tileDef.Tilesheet) && tileDef.SheetX.HasValue && tileDef.SheetY.HasValue)
                    {
                        var sheetPath = AssetPathResolver.GetTilePath(tileDef.Tilesheet);
                        var sheet = GetOrLoadTilesheet(sheetPath);

                        source = new CroppedBitmap(sheet,
                            new Int32Rect(tileDef.SheetX.Value * MapConstant.TileSize,
                                          tileDef.SheetY.Value * MapConstant.TileSize,
                                          MapConstant.TileSize,
                                          MapConstant.TileSize));
                    }
                    else if (!string.IsNullOrEmpty(tileDef.Sprite))
                    {
                        var fullPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), tileDef.Sprite);
                        source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                    }

                    if (source == null) continue;

                    double px = (obj.X + (entry.X - minX)) * MapConstant.TileSize;
                    double py = (obj.Y + (entry.Y - minY)) * MapConstant.TileSize;

                    var img = new Image
                    {
                        Width = MapConstant.TileSize,
                        Height = MapConstant.TileSize,
                        Source = source,
                        IsHitTestVisible = false
                    };

                    Canvas.SetLeft(img, px);
                    Canvas.SetTop(img, py);
                    Panel.SetZIndex(img, obj.Z + entry.Z);
                    MapCanvas.Children.Add(img);
                }

                if (ToggleCollisionOverlay?.IsChecked == true && def.CollisionPolygon != null && def.CollisionPolygon.Count >= 2)
                {
                    int minpolygonX = def.Tiles.Min(t => t.X);
                    int mipolygonY = def.Tiles.Min(t => t.Y);
                    int width = def.Tiles.Max(t => t.X) - minpolygonX + 1;
                    int height = def.Tiles.Max(t => t.Y) - mipolygonY + 1;

                    var objectPolygon = new System.Windows.Shapes.Polygon
                    {
                        Stroke = Brushes.Orange,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(40, 255, 165, 0)),
                        Points = new PointCollection(def.CollisionPolygon.Select(p =>
                            new Point((obj.X + p.X * width) * MapConstant.TileSize,
                                      (obj.Y + p.Y * height) * MapConstant.TileSize)))
                    };

                    Panel.SetZIndex(objectPolygon, obj.Z + 1);
                    MapCanvas.Children.Add(objectPolygon);
                }

                var screenPoint = new Point(obj.X * MapConstant.TileSize, obj.Y * MapConstant.TileSize);
                if (!selectionRect.IsEmpty && selectionRect.Contains(screenPoint))
                {
                    var highlight = new System.Windows.Shapes.Rectangle
                    {
                        Width = MapConstant.TileSize,
                        Height = MapConstant.TileSize,
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 2,
                        Fill = null
                    };
                    Canvas.SetLeft(highlight, obj.X * MapConstant.TileSize);
                    Canvas.SetTop(highlight, obj.Y * MapConstant.TileSize);
                    Panel.SetZIndex(highlight, obj.Z + 1000);
                    MapCanvas.Children.Add(highlight);
                }
            }
        }

        private void DrawSelectionBox()
        {
            if (isSelecting && !selectionRect.IsEmpty)
            {
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = selectionRect.Width,
                    Height = selectionRect.Height,
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 255))
                };
                Canvas.SetLeft(rect, selectionRect.X);
                Canvas.SetTop(rect, selectionRect.Y);
                Panel.SetZIndex(rect, 9999);
                MapCanvas.Children.Add(rect);
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
                    Y1 = y * MapConstant.TileSize,
                    X2 = mapWidth * MapConstant.TileSize,
                    Y2 = y * MapConstant.TileSize,
                    Stroke = Brushes.Red,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
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

                map.Width = width;
                map.Height = height;

                RedrawMap();
            }
            else
            {
                MessageBox.Show("Invalid map size.");
            }
        }

        private Rect NormalizeRect(Rect rect)
        {
            double x = Math.Min(rect.Left, rect.Right);
            double y = Math.Min(rect.Top, rect.Bottom);
            double width = Math.Abs(rect.Width);
            double height = Math.Abs(rect.Height);
            return new Rect(x, y, width, height);
        }

        private void ApplyCanvasTransform()
        {
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform(zoom, zoom));
            group.Children.Add(new TranslateTransform(currentPanOffset.X, currentPanOffset.Y));

            MapLayerContainer.RenderTransform = group; // Apply to parent container

            RedrawMap();
            RedrawHighlightLayer();
        }

        private void DeleteSelectedObjects()
        {
            var deleted = new List<IEditorCommand>();

            foreach (var obj in map.Objects.ToList())
            {
                double px = obj.X * MapConstant.TileSize;
                double py = obj.Y * MapConstant.TileSize;

                if (selectionRect.Contains(new Point(px, py)))
                {
                    deleted.Add(new RemoveObjectCommand(map, obj));
                }
            }

            if (deleted.Count > 0)
            {
                var batch = new BatchCommand(deleted);
                undoManager.ExecuteCommand(batch);
                RedrawMap();
            }

            selectionRect = Rect.Empty;
        }

        private void TileMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.Tile;
            FolderBrowser.Visibility = Visibility.Visible;
            ObjectBrowser.Visibility = Visibility.Collapsed;
            AssetPreviewHost.Content = null;
            MapCanvas.Focus();
            RefreshFolderBrowser();
        }

        private void ObjectMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.Object;
            FolderBrowser.Visibility = Visibility.Collapsed;
            ObjectBrowser.Visibility = Visibility.Visible;
            AssetPreviewHost.Content = null;
            MapCanvas.Focus();
            RefreshObjectBrowser();
        }

        private void TriggerMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.Trigger;
            RefreshFolderBrowser();
        }

        private void NpcMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.NPC;
            RefreshFolderBrowser();
        }

        private void SelectionMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.Selection;
            AssetPreviewHost.Content = null;
            MapCanvas.Focus();
        }

        private void SelectionObjectMode_Checked(object sender, RoutedEventArgs e)
        {
            EditModeHelper.CurrentMode = EditorMode.ObjectSelection;
            AssetPreviewHost.Content = null;
            MapCanvas.Focus();
        }

        private void RefreshObjectBrowser()
        {
            ObjectBrowser.Items.Clear();

            var grouped = ObjectDatabase.Definitions.Values
                .GroupBy(def => def.Category)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                var categoryItem = new TreeViewItem
                {
                    Header = group.Key,
                    IsExpanded = true
                };

                foreach (var def in group.OrderBy(d => d.Name))
                {
                    var objItem = new TreeViewItem
                    {
                        Header = def.Name,
                        Tag = def.Id
                    };
                    categoryItem.Items.Add(objItem);
                }

                ObjectBrowser.Items.Add(categoryItem);
            }
        }

        private void ObjectBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is int id && ObjectDatabase.Definitions.TryGetValue(id, out var def))
            {
                selectedObjectId = id;

                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);

                var canvas = new Canvas
                {
                    Width = 128,
                    Height = 128
                };

                foreach (var entry in def.Tiles)
                {
                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef))
                        continue;

                    BitmapSource source = null;

                    if (!string.IsNullOrEmpty(tileDef.Tilesheet) && tileDef.SheetX.HasValue && tileDef.SheetY.HasValue)
                    {
                        var sheetPath = AssetPathResolver.GetTilePath(tileDef.Tilesheet);
                        var sheet = GetOrLoadTilesheet(sheetPath);

                        source = new CroppedBitmap(sheet,
                            new Int32Rect(tileDef.SheetX.Value * MapConstant.TileSize,
                                          tileDef.SheetY.Value * MapConstant.TileSize,
                                          MapConstant.TileSize,
                                          MapConstant.TileSize));
                    }
                    else if (!string.IsNullOrEmpty(tileDef.Sprite))
                    {
                        var fullPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), tileDef.Sprite);
                        source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                    }

                    if (source == null) continue;

                    var img = new Image
                    {
                        Source = source,
                        Width = MapConstant.TileSize,
                        Height = MapConstant.TileSize
                    };

                    Canvas.SetLeft(img, (entry.X - minX) * MapConstant.TileSize);
                    Canvas.SetTop(img, (entry.Y - minY) * MapConstant.TileSize);
                    canvas.Children.Add(img);
                }

                AssetPreviewHost.Content = canvas;
                ShowObjectProperties(def);
            }
        }

        private void RefreshFolderBrowser()
        {
            FolderBrowser.Items.Clear();

            string targetFolder = "Tiles";

            if (string.IsNullOrEmpty(targetFolder))
            {
                AssetPreviewHost.Content = null;
                return;
            }

            string fullPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), targetFolder);
            if (!Directory.Exists(fullPath)) return;

            currentAssetFolder = fullPath;

            foreach (var file in Directory.GetFiles(fullPath).Where(file => !file.EndsWith(".json")))
            {
                var item = new ListBoxItem
                {
                    Content = System.IO.Path.GetFileName(file),
                    Tag = file
                };
                FolderBrowser.Items.Add(item);
            }

            FolderBrowser.SelectionChanged += FolderBrowser_SelectionChanged;
        }

        private void FolderBrowser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderBrowser.SelectedItem is ListBoxItem selected)
            {
                selectedAssetPath = selected.Tag as string;
                DisplayAsset(selectedAssetPath);
            }
        }

        private void DisplayAsset(string path)
        {
            AssetPreviewHost.Content = null;

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return;

            var image = new BitmapImage(new Uri(path, UriKind.Absolute));

            var viewer = new ZoomableImageViewer();
            viewer.SetImage(image);

            if (EditModeHelper.IsInTileMode)
            {
                selectedTilesheet = Path.GetFileName(path);

                viewer.TileClicked += (tileX, tileY) =>
                {
                    var match = TileDatabase.Definitions.FirstOrDefault(kvp =>
                        kvp.Value.Tilesheet != null &&
                        Path.GetFileName(kvp.Value.Tilesheet).Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase) &&
                        kvp.Value.SheetX == tileX &&
                        kvp.Value.SheetY == tileY
                    );

                    TileDefinition def;
                    if (!match.Equals(default(KeyValuePair<int, TileDefinition>)))
                    {
                        def = match.Value;
                        selectedTileId = match.Key;
                    }
                    else
                    {
                        def = new TileDefinition
                        {
                            Id = TileDatabase.GetNextId(),
                            Name = "New Tile",
                            IsWalkable = true,
                            Tilesheet = Path.GetFileName(path),
                            SheetX = tileX,
                            SheetY = tileY
                        };
                        TileDatabase.AddOrUpdate(def);
                        selectedTileId = def.Id;
                    }

                    ShowTileProperties(def);
                };
            }

            AssetPreviewHost.Content = viewer;
        }

        private void ShowTileProperties(TileDefinition def)
        {
            PropertyEditorControl.LoadTileDefinition(def, () =>
            {
                TileDatabase.AddOrUpdate(def);
                TileDatabase.Save();
            });
        }

        private void ShowObjectProperties(ObjectDefinition def)
        {
            PropertyEditorControl.LoadObjectDefinition(def, () =>
            {
                ObjectDatabase.AddOrUpdate(def);
                ObjectDatabase.Save();
            });
        }

        private void ToggleGroupEditMode(bool enable)
        {
            var isGroupSelectionMode = enable;

            if (AssetPreviewHost.Content is ZoomableImageViewer viewer)
            {
                if (enable)
                    viewer.EnableGroupTileEdit();
                else
                    viewer.DisableGroupTileEdit();
            }

            // Reset state
            if (!enable)
            {
                selectedTilesheet = null;
            }
        }

        private void CreateObjectFromGroupSelection()
        {
            if (AssetPreviewHost.Content is ZoomableImageViewer viewer)
            {
                var selectedPoints = viewer.GetSelectedGroupTiles();
                if (TileGroupObjectCreator.TryCreateObjectFromSelection(selectedTilesheet, selectedPoints, out var newObj))
                {
                    RefreshObjectBrowser();
                    MessageBox.Show("New object created and saved.");
                }
                else
                {
                    MessageBox.Show("Failed to create object from selection.");
                }
            }
        }

        private PlacedObject GetObjectUnderPointer(Point pointer)
        {
            double px = pointer.X / MapConstant.TileSize;
            double py = pointer.Y / MapConstant.TileSize;

            return map.Objects.LastOrDefault(obj =>
            {
                double dx = obj.X - px;
                double dy = obj.Y - py;
                return dx * dx + dy * dy < 0.25;
            });
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HandleKeyboardInput(e);
            base.OnKeyDown(e);
        }

        private void HandleKeyboardInput(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    undoManager.Undo();
                    RedrawMap();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    undoManager.Redo();
                    RedrawMap();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete && !selectionRect.IsEmpty)
            {
                DeleteSelectedObjects();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && EditModeHelper.IsInTileMode)
            {
                // Determine tile coordinates from mouse position
                Point pos = Mouse.GetPosition(MapCanvas);
                int tileX = (int)(pos.X / MapConstant.TileSize);
                int tileY = (int)(pos.Y / MapConstant.TileSize);

                var command = new RemoveTileCommand(map, tileX, tileY, selectedLayer);
                undoManager.ExecuteCommand(command);
                RedrawMap();
                e.Handled = true;
            }
        }

        private void EditorMainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                isSpaceHeld = true;
        }

        private void EditorMainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                isSpaceHeld = false;
        }

        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 0.1 : -0.1;
            zoom = Math.Max(0.2, Math.Min(5, zoom + delta));
            ApplyCanvasTransform();
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            // === Prioritize Panning (Space + Left Click) ===
            if (isSpaceHeld)
            {
                dragStart = e.GetPosition(this); // screen coords
                MapCanvas.CaptureMouse();

                e.Handled = true;
                return;
            }

            // === Begin Selection with Ctrl ===
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                selectionStart = pos;
                isSelecting = true;
                MapCanvas.CaptureMouse();
                return;
            }

            if (EditModeHelper.CurrentMode == EditorMode.Selection)
            {
                int stileX = (int)(pos.X / MapConstant.TileSize);
                int stileY = (int)(pos.Y / MapConstant.TileSize);

                // Check tile map first
                var tile = map.GetTileAt(stileX, stileY);
                if (tile != null)
                {
                    var zEditor = new ZEditPopup(tile.Z);
                    if (zEditor.ShowDialog() == true)
                    {
                        tile.Z = zEditor.Result;
                        RedrawMap();
                    }
                    return;
                }
            }

            // If no tile, check for object tile
            if (EditModeHelper.CurrentMode == EditorMode.ObjectSelection)
            {
                var opos = e.GetPosition(MapCanvas);
                double px = opos.X / MapConstant.TileSize;
                double py = opos.Y / MapConstant.TileSize;

                foreach (var obj in map.Objects)
                {
                    if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def)) continue;
                    if (def.Tiles == null) continue;

                    int minX = def.Tiles.Min(t => t.X);
                    int minY = def.Tiles.Min(t => t.Y);

                    foreach (var entry in def.Tiles)
                    {
                        // Match exact draw logic
                        double otileX = obj.X + (entry.X - minX);
                        double otileY = obj.Y + (entry.Y - minY);

                        int gridX = (int)Math.Floor(otileX);
                        int gridY = (int)Math.Floor(otileY);

                        if (gridX == (int)Math.Floor(px) && gridY == (int)Math.Floor(py))
                        {
                            string key = $"{entry.X},{entry.Y}";
                            var currentZ = obj.ZOverrides.TryGetValue(key, out var overrideZ) ? overrideZ : obj.Z;

                            var popup = new ZEditPopup(currentZ);
                            if (popup.ShowDialog() == true)
                            {
                                obj.ZOverrides[key] = popup.Result;
                                RedrawMap();
                            }
                            return;
                        }
                    }
                }

                return;
            }

            // === Dragging Object ===
            if (EditModeHelper.IsInObjectMode)
            {
                var target = GetObjectUnderPointer(pos);

                if (Keyboard.Modifiers == ModifierKeys.Shift && target != null)
                {
                    draggedObject = target;
                    dragOffset = new Point(target.X * MapConstant.TileSize - pos.X, target.Y * MapConstant.TileSize - pos.Y);
                    MapCanvas.CaptureMouse();
                    return;
                }

                // Always place object otherwise
                double objx = pos.X / MapConstant.TileSize;
                double objy = pos.Y / MapConstant.TileSize;
                HandleObjectClick(objx, objy);
                return;
            }

            // === Default Click Actions ===
            double x = pos.X / MapConstant.TileSize;
            double y = pos.Y / MapConstant.TileSize;
            int tileX = (int)Math.Floor(x);
            int tileY = (int)Math.Floor(y);

            if (tileX < 0 || tileY < 0 || tileX >= map.Width || tileY >= map.Height)
                return;

            if (isSelecting)
                return;

            switch (EditModeHelper.CurrentMode)
            {
                case EditorMode.Tile:
                    HandleTileClick(tileX, tileY);
                    break;
                case EditorMode.Object:
                    HandleObjectClick(x, y);
                    break;
                case EditorMode.Trigger:
                    HandleTriggerClick(tileX, tileY);
                    break;
            }
        }

        private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // === End panning ===
            if (dragStart.HasValue && isSpaceHeld)
            {
                dragStart = null;
                MapCanvas.ReleaseMouseCapture();
                e.Handled = true;
                return;
            }

            // === End drag object ===
            if (draggedObject != null)
            {
                var pos = e.GetPosition(MapCanvas);
                draggedObject.X = (pos.X + dragOffset.X) / MapConstant.TileSize;
                draggedObject.Y = (pos.Y + dragOffset.Y) / MapConstant.TileSize;
                draggedObject = null;
                MapCanvas.ReleaseMouseCapture();
                RedrawMap();
                return;
            }

            // === Selection finalization ===
            if (isSelecting)
            {
                isSelecting = false;
                MapCanvas.ReleaseMouseCapture();

                if (selectionStart.HasValue)
                {
                    var end = e.GetPosition(MapCanvas);
                    var norm = NormalizeRect(new Rect(selectionStart.Value, end));
                    selectionRect = (norm.Width > 1 && norm.Height > 1) ? norm : Rect.Empty;
                }
                else
                {
                    selectionRect = Rect.Empty;
                }

                selectionStart = null;
                RedrawMap();
            }

            if (e.ClickCount == 2)
            {
                HandleDoubleClick(e);
            }
        }

        private void HandleTileClick(int tileX, int tileY)
        {
            IEditorCommand command;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                command = new FillTileCommand(map, tileX, tileY, selectedLayer, selectedTileId);
            }
            else
            {
                command = new SetTileCommand(map, tileX, tileY, selectedLayer, selectedTileId);
            }

            undoManager.ExecuteCommand(command);
            RedrawMap();
        }

        private void HandleObjectClick(double x, double y)
        {
            if (!ObjectDatabase.Definitions.TryGetValue(selectedObjectId, out var def) || def.Tiles == null)
                return;

            var isNpc = def.IsNpc;

            // Use raw x, y (NOT floored), still center based on object size
            int minX = def.Tiles.Min(t => t.X);
            int maxX = def.Tiles.Max(t => t.X);
            int minY = def.Tiles.Min(t => t.Y);
            int maxY = def.Tiles.Max(t => t.Y);

            double objWidth = maxX - minX + 1;
            double objHeight = maxY - minY + 1;

            double originX = x - objWidth / 2.0;
            double originY = y - objHeight / 2.0;

            var obj = new PlacedObject
            {
                Id = selectedObjectId,
                X = originX,
                Y = originY,
                Z = selectedLayer,
                Meta = isNpc ? new Dictionary<string, string> { ["npc"] = "true", ["name"] = "New NPC" } : null
            };

            undoManager.ExecuteCommand(new PlaceObjectCommand(map, obj));
            RedrawMap();
        }

        private void HandleTriggerClick(int tileX, int tileY)
        {
            var existingTile = map.GetTileAt(tileX, tileY);
            var triggerEditWindow = new TriggerEditWindow(existingTile?.Trigger);
            if (triggerEditWindow.ShowDialog() == true)
            {
                var command = new SetTriggerCommand(map, tileX, tileY, triggerEditWindow.Result);
                undoManager.ExecuteCommand(command);
                RedrawMap();
            }
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            if (e.LeftButton == MouseButtonState.Pressed && isSpaceHeld)
            {
                if (!dragStart.HasValue)
                {
                    dragStart = e.GetPosition(this); // Start panning on first move
                    MapCanvas.CaptureMouse();
                }

                var currentPos = e.GetPosition(this);
                var delta = currentPos - dragStart.Value;
                currentPanOffset += (Vector)delta;
                dragStart = currentPos;
                ApplyCanvasTransform();
                e.Handled = true;
                return;
            }

            if (EditModeHelper.CurrentMode == EditorMode.ObjectSelection)
            {
                var mapPos = GetMouseMapPosition(e);
                int tileMouseX = (int)(mapPos.X / MapConstant.TileSize);
                int tileMouseY = (int)(mapPos.Y / MapConstant.TileSize);

                hoveredObject = null;
                hoveredObjectTile = null;

                foreach (var obj in map.Objects)
                {
                    if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def)) continue;
                    if (def.Tiles == null) continue;

                    int minX = def.Tiles.Min(t => t.X);
                    int minY = def.Tiles.Min(t => t.Y);

                    foreach (var entry in def.Tiles)
                    {
                        int tileX = (int)Math.Floor(obj.X + (entry.X - minX));
                        int tileY = (int)Math.Floor(obj.Y + (entry.Y - minY));

                        if (tileX == tileMouseX && tileY == tileMouseY)
                        {
                            hoveredObject = obj;
                            hoveredObjectTile = new Point(entry.X, entry.Y);

                            RedrawHighlightLayer();
                            return;
                        }
                    }
                }

                RedrawHighlightLayer();
            }

            if (draggedObject != null && e.LeftButton == MouseButtonState.Pressed)
            {
                draggedObject.X = (pos.X + dragOffset.X) / MapConstant.TileSize;
                draggedObject.Y = (pos.Y + dragOffset.Y) / MapConstant.TileSize;
                return;
            }

            if (isSelecting && selectionStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var current = e.GetPosition(MapCanvas);
                selectionRect = new Rect(selectionStart.Value, current);
                RedrawMap();
            }
            else
            {
                RedrawMap();
            }
        }

        private Point GetMouseMapPosition(MouseEventArgs e)
        {
            Point screenPos = e.GetPosition(MapCanvas);
            double mapX = (screenPos.X - currentPanOffset.X) / zoom;
            double mapY = (screenPos.Y - currentPanOffset.Y) / zoom;
            return new Point(mapX, mapY);
        }

        private void HandleDoubleClick(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            if (EditModeHelper.IsInObjectMode)
            {
                var target = GetObjectUnderPointer(pos);
                if (target != null && target.Meta != null && target.Meta.TryGetValue("npc", out var tag) && tag == "true")
                {
                    var npcWindow = new NpcEditWindow(target.Meta);
                    if (npcWindow.ShowDialog() == true)
                    {
                        target.Meta = npcWindow.ResultMeta;
                        RedrawMap();
                    }
                }
            }
            else if (EditModeHelper.IsInTileMode)
            {
                int tileX = (int)(pos.X / MapConstant.TileSize);
                int tileY = (int)(pos.Y / MapConstant.TileSize);
                var tile = map.GetTileAt(tileX, tileY);
                if (tile == null || tile.Trigger == null) return;

                var triggerEditWindow = new TriggerEditWindow(tile.Trigger);
                if (triggerEditWindow.ShowDialog() == true)
                {
                    var command = new SetTriggerCommand(map, tileX, tileY, triggerEditWindow.Result);
                    undoManager.ExecuteCommand(command);
                    RedrawMap();
                }
            }
        }

        private void LayerInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(LayerInput.Text, out int z))
            {
                selectedLayer = z;
            }
        }

        private void ToggleCollisionOverlay_Checked(object sender, RoutedEventArgs e)
        {
            RedrawMap();
        }

        private void ToggleGroupSelection_Checked(object sender, RoutedEventArgs e)
        {
            ToggleGroupEditMode(ToggleGroupSelection.IsChecked.Value);
        }

        private void CreateObject_Click(object sender, RoutedEventArgs e)
        {
            CreateObjectFromGroupSelection();
        }

    }
}
