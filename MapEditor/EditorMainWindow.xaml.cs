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
        private enum EditorMode { Tile, Object, Trigger }
        private EditorMode currentMode = EditorMode.Tile;

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

        //Palette
        private Dictionary<int, ObjectDefinition> objectDefinitions;

        private readonly UndoManager undoManager = new UndoManager();

        private TilesheetViewer tilesheetViewer;

        public EditorMainWindow()
        {
            InitializeComponent();

            // Load definitions from JSON
            objectDefinitions = ObjectSetLoader.Load(AssetPathResolver.GetPath(MapConstant.ObjectSetJsonPath));

            map = new Map
            {
                Name = currentMapName,
                Width = mapWidth,
                Height = mapHeight,
                Tiles = new List<Tile>(),
                ObjectDefinitions = objectDefinitions
            };

            this.PreviewKeyDown += EditorMainWindow_PreviewKeyDown;
            this.PreviewKeyUp += EditorMainWindow_PreviewKeyUp;

            InitTilePalette();
            InitObjectPalette();
            InitTilesheetViewer();
            RedrawMap();
        }

        private void InitTilesheetViewer()
        {
            if (tilesheetViewer != null) return;

            tilesheetViewer = new TilesheetViewer();
            tilesheetViewer.Margin = new Thickness(0, 5, 0, 5);


            TilePanelContainer.Children.Add(tilesheetViewer);

            tilesheetViewer.LoadSheet("Tilesheet.png");
        }

        private void InitTilePalette()
        {
            TilePalette.Items.Clear();

            TileDatabase.Load();

            foreach (var kv in TileDatabase.Definitions)
            {
                var def = kv.Value;
                BitmapSource source;

                if (!string.IsNullOrEmpty(def.Tilesheet) && def.SheetX.HasValue && def.SheetY.HasValue)
                {
                    var sheetPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.Tilesheet);
                    var sheet = new BitmapImage(new Uri(sheetPath));
                    source = new CroppedBitmap(sheet,
                        new Int32Rect(def.SheetX.Value * MapConstant.TileSize, def.SheetY.Value * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize));
                }
                else if (!string.IsNullOrEmpty(def.Sprite))
                {
                    source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.Sprite)));
                }
                else
                {
                    continue;
                }

                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = source
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

        private void InitNpcPalette()
        {
            NpcPalette.Items.Clear();

            foreach (var kv in objectDefinitions.Where(o => o.Value.IsNpc))
            {
                var def = kv.Value;
                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.SpritePath)))
                };
                var item = new ListBoxItem { Content = img, Tag = kv.Key };
                NpcPalette.Items.Add(item);
            }

            NpcPalette.SelectionChanged += (s, e) =>
            {
                if (NpcPalette.SelectedItem is ListBoxItem selected)
                    selectedObjectId = (int)selected.Tag;
            };

            NpcPalette.SelectedIndex = 0;
        }

        private bool IsObjectInteractionMode()
        {
            return currentMode == EditorMode.Object;
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
            double canvasWidth = MapScrollViewer.ViewportWidth / zoom;
            double canvasHeight = MapScrollViewer.ViewportHeight / zoom;
            double offsetX = -currentPanOffset.X / zoom;
            double offsetY = -currentPanOffset.Y / zoom;

            int startX = Math.Max(0, (int)Math.Floor(offsetX / MapConstant.TileSize));
            int endX = Math.Min(mapWidth, (int)Math.Ceiling((offsetX + canvasWidth) / MapConstant.TileSize));
            int startY = Math.Max(0, (int)Math.Floor(offsetY / MapConstant.TileSize));
            int endY = Math.Min(mapHeight, (int)Math.Ceiling((offsetY + canvasHeight) / MapConstant.TileSize));

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
                            var sheetPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), tileDef.Tilesheet);

                            var sheet = new BitmapImage();
                            sheet.BeginInit();
                            sheet.CacheOption = BitmapCacheOption.OnLoad;
                            sheet.UriSource = new Uri(sheetPath, UriKind.Absolute);
                            sheet.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            sheet.EndInit();
                            sheet.Freeze();

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
                            Source = source
                        };

                        Canvas.SetLeft(img, x * MapConstant.TileSize);
                        Canvas.SetTop(img, y * MapConstant.TileSize);
                        Panel.SetZIndex(img, tile.Z);
                        MapCanvas.Children.Add(img);
                    }
                }
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
                if (!objectDefinitions.TryGetValue(obj.Id, out var def) || string.IsNullOrEmpty(def.SpritePath))
                    continue;

                double px = obj.X * MapConstant.TileSize - MapConstant.TileSize / 2;
                double py = obj.Y * MapConstant.TileSize - MapConstant.TileSize / 2;

                var img = new Image
                {
                    Width = MapConstant.TileSize,
                    Height = MapConstant.TileSize,
                    Source = new BitmapImage(new Uri(Path.Combine(AssetPathResolver.GetAssetsRootPath(), def.SpritePath)))
                };

                Canvas.SetLeft(img, px);
                Canvas.SetTop(img, py);
                Panel.SetZIndex(img, obj.Z);
                MapCanvas.Children.Add(img);

                // Highlight selection if inside selectionRect
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
                    Canvas.SetLeft(highlight, px);
                    Canvas.SetTop(highlight, py);
                    Panel.SetZIndex(highlight, obj.Z + 1);
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
            MapCanvas.RenderTransform = group;

            RedrawMap();
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
            currentMode = EditorMode.Tile;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Visible;
            NpcPalette.Visibility = Visibility.Collapsed;
            
            if(MapCanvas != null)
                MapCanvas.Focus();
        }

        private void ObjectMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Object;
            ObjectPalette.Visibility = Visibility.Visible;
            TilePalette.Visibility = Visibility.Collapsed;
            NpcPalette.Visibility = Visibility.Collapsed;

            if (MapCanvas != null)
                MapCanvas.Focus();
        }

        private void TriggerMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Trigger;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Collapsed;
            NpcPalette.Visibility = Visibility.Collapsed;

            if (MapCanvas != null)
                MapCanvas.Focus();
        }

        private void NpcMode_Checked(object sender, RoutedEventArgs e)
        {
            currentMode = EditorMode.Object;
            ObjectPalette.Visibility = Visibility.Collapsed;
            TilePalette.Visibility = Visibility.Collapsed;
            NpcPalette.Visibility = Visibility.Visible;

            if (MapCanvas != null)
                MapCanvas.Focus();
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
            else if (e.Key == Key.Delete && currentMode == EditorMode.Tile)
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

        private void ImportTilesheet_Click(object sender, RoutedEventArgs e)
        {
            var importWindow = new TilesheetImportWindow();
            if (importWindow.ShowDialog() == true)
            {
                TileDatabase.AddTiles(importWindow.GeneratedTiles);
                InitTilePalette(); // Refresh the tile palette
            }
        }

        private void LayerSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayerSelector.SelectedItem is ComboBoxItem item && int.TryParse(item.Content.ToString(), out int z))
            {
                selectedLayer = z;
            }
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

            // === Dragging Object ===
            if (IsObjectInteractionMode())
            {
                var target = GetObjectUnderPointer(pos);
                if (target != null)
                {
                    draggedObject = target;
                    dragOffset = new Point(target.X * MapConstant.TileSize - pos.X, target.Y * MapConstant.TileSize - pos.Y);
                    MapCanvas.CaptureMouse();
                    return;
                }
                else
                {
                    selectionRect = Rect.Empty;
                    RedrawMap();
                }
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

            switch (currentMode)
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
            var isNpc = objectDefinitions.TryGetValue(selectedObjectId, out var def) && def.IsNpc;
            var obj = new PlacedObject
            {
                Id = selectedObjectId,
                X = x,
                Y = y,
                Z = selectedLayer,
                Meta = isNpc ? new Dictionary<string, string> { ["npc"] = "true", ["name"] = "New NPC" } : null
            };

            var command = new PlaceObjectCommand(map, obj);
            undoManager.ExecuteCommand(command);
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

            if (draggedObject != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(MapCanvas);
                draggedObject.X = (pos.X + dragOffset.X) / MapConstant.TileSize;
                draggedObject.Y = (pos.Y + dragOffset.Y) / MapConstant.TileSize;
                RedrawMap();
                return;
            }

            if (dragStart.HasValue && e.RightButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(this);
                var delta = currentPos - dragStart.Value;
                var offset = currentPanOffset + delta;
                currentPanOffset = offset;
                dragStart = currentPos;
                ApplyCanvasTransform();
            }

            if (isSelecting && selectionStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var current = e.GetPosition(MapCanvas);
                selectionRect = new Rect(selectionStart.Value, current);
                RedrawMap();
            }
        }

        private void HandleDoubleClick(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            if (IsObjectInteractionMode())
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
            else if (currentMode == EditorMode.Trigger)
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
    }
}
