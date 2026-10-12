using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using JRPG.Core.Maps.Models;
using JRPG.Core.Events.Models;
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
        private int selectedLayer = -5; // "Ground" preset

        private int selectedSheetX = -1;
        private int selectedSheetY = -1;
        private string selectedTilesheet = null;

        //SelectionMode
        private PlacedObject hoveredObject = null;
        private Point? hoveredObjectTile = null;


        private readonly Dictionary<string, BitmapImage> tilesheetCache = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<string, ImageSource> tileImageCache = new Dictionary<string, ImageSource>();

        //Map drawing (one visual for the whole map instead of one element per tile)
        private struct MapDrawable
        {
            public DrawSortKey Key;
            public Action<DrawingContext> Draw;
        }

        private MapDrawingHost mapDrawing;
        private readonly List<MapDrawable> mapDrawables = new List<MapDrawable>();

        private static readonly Brush TileCollisionFill = Frozen(new SolidColorBrush(Color.FromArgb(40, 0, 255, 0)));
        private static readonly Pen TileCollisionPen = Frozen(new Pen(Brushes.Lime, 1));
        private static readonly Brush ObjectCollisionFill = Frozen(new SolidColorBrush(Color.FromArgb(40, 255, 165, 0)));
        private static readonly Pen ObjectCollisionPen = Frozen(new Pen(Brushes.Orange, 2));
        private static readonly Brush ZoneFill = Frozen(new SolidColorBrush(Color.FromArgb(50, 0, 150, 255)));
        private static readonly Pen ZonePen = Frozen(new Pen(Brushes.DeepSkyBlue, 2));
        private static readonly Pen TriggerPen = Frozen(new Pen(Brushes.Black, 1));
        private static readonly Pen EventMarkerPen = Frozen(new Pen(Brushes.Black, 1.5));
        private static readonly Pen SelectedObjectPen = Frozen(new Pen(Brushes.Cyan, 2));
        private static readonly Pen GridPen = Frozen(new Pen(Brushes.Red, 0.5));


        private static readonly Brush HoverFill = Frozen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));
        private static readonly Brush DeleteHoverFill = Frozen(new SolidColorBrush(Color.FromArgb(50, 255, 69, 0)));

        private readonly UndoManager undoManager = new UndoManager();

        // File state
        private string currentFilePath;
        private bool isDirty;
        private bool suppressDirty; // set while filling the UI from a loaded map

        // Drag painting / erasing: one stroke = one undo step
        private enum StrokeKind { None, Paint, Erase, RectPaint, RectErase }

        // Paint Tiles tools
        private enum TileTool { Brush, Rectangle, Fill }
        private TileTool tileTool = TileTool.Brush;
        private int brushSize = 1;
        private static readonly int[] BrushSizes = { 1, 2, 3, 4, 5, 7, 9 };
        private (int x, int y) rectStartCell;
        private StrokeKind strokeKind = StrokeKind.None;
        private readonly List<IEditorCommand> strokeCommands = new List<IEditorCommand>();
        private (int x, int y) lastStrokeCell = (-1, -1);

        // Object drag start position, for the undoable move
        private double dragStartX, dragStartY;

        // Mouse position on the map (for status bar and brush preview)
        private Point? hoverPoint;

        // Layer presets. The player is drawn at Z = 1: lower is below the player,
        // Z = 1 is depth-sorted with the player by Y, higher is drawn over the player.
        private static readonly (string Name, int Z)[] LayerPresets =
        {
            ("Ground", -5),
            ("Ground detail", -4),
            ("Decoration (below player)", -3),
            ("Same as player (Y-sorted)", MapConstant.PlayerZ),
            ("Above player (roofs, treetops)", MapConstant.AbovePlayerZ),
        };

        public EditorMainWindow()
        {
            InitializeComponent();

            TileDatabase.Load();
            ObjectDatabase.Load();

            foreach (var preset in LayerPresets)
                LayerPresetBox.Items.Add(new ComboBoxItem { Content = $"{preset.Name}  ({preset.Z})", Tag = preset.Z });
            SyncLayerPreset();

            currentMapName = "New Map";
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
            PropertyEditorControl.RequestEditObjectCollision += OpenObjectEditor;

            // Subscribed once here; RefreshFolderBrowser used to add it again on every mode switch
            FolderBrowser.SelectionChanged += FolderBrowser_SelectionChanged;

            // Start in Tile mode with the tile browser filled (nothing was selected before)
            TileModeRadio.IsChecked = true;

            RedrawMap();

            // Filling the text boxes from XAML fires their change handlers; nothing is edited yet
            isDirty = false;
            UpdateTitle();
            UpdateStatusBar();
        }

        // === Edit tracking ===

        private void ExecuteEdit(IEditorCommand command)
        {
            undoManager.ExecuteCommand(command);
            MarkDirty();
        }

        private void MarkDirty()
        {
            if (suppressDirty || isDirty) return;
            isDirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string file = currentFilePath != null ? Path.GetFileName(currentFilePath) : $"{currentMapName} (not saved yet)";
            Title = $"JRPG Map Editor - {file}{(isDirty ? " *" : "")}";
        }

        private void ShowStatusMessage(string message)
        {
            StatusMessage.Text = message;
        }

        private void UpdateStatusBar()
        {
            if (StatusText == null) return;

            string cell = "-";
            if (hoverPoint.HasValue)
            {
                var (x, y) = GetCell(hoverPoint.Value);
                if (IsInsideMap(x, y))
                    cell = $"{x}, {y}";
            }

            string brush = "-";
            if (EditModeHelper.IsInTileMode && TileDatabase.Definitions.TryGetValue(selectedTileId, out var tileDef))
                brush = $"{tileDef.Name} (#{selectedTileId})";
            else if (EditModeHelper.IsInObjectMode && ObjectDatabase.Definitions.TryGetValue(selectedObjectId, out var objDef))
                brush = objDef.Name;

            string tool = "";
            if (EditModeHelper.IsInTileMode)
                tool = tileTool == TileTool.Brush ? $"    Tool: Brush {brushSize}x{brushSize}" : $"    Tool: {tileTool}";

            StatusText.Text = $"Tile: {cell}    Mode: {EditModeHelper.CurrentMode}{tool}    Layer Z: {selectedLayer}    Brush: {brush}    Map: {mapWidth} x {mapHeight}";
        }

        private static (int x, int y) GetCell(Point pos)
        {
            return ((int)Math.Floor(pos.X / MapConstant.TileSize), (int)Math.Floor(pos.Y / MapConstant.TileSize));
        }

        private bool IsInsideMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < mapWidth && y < mapHeight;
        }

        // === Layers ===

        private void SyncLayerPreset()
        {
            // Select the preset matching the current Z, or none for a custom value
            LayerPresetBox.SelectedItem = LayerPresetBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (int)i.Tag == selectedLayer);
        }

        private void LayerPresetBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayerPresetBox.SelectedItem is ComboBoxItem item)
                LayerInput.Text = item.Tag.ToString();
        }

        // View menu: grid / collision overlay
        private void ViewOption_Click(object sender, RoutedEventArgs e)
        {
            showGrid = ShowGridItem.IsChecked;
            RedrawMap();
        }

        private void DimOtherLayers_Changed(object sender, RoutedEventArgs e)
        {
            RedrawMap();
        }

        private bool ShouldDim(int z)
        {
            return DimOtherLayersBox?.IsChecked == true && z != selectedLayer;
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

        private ImageSource LoadTileImage(TileDefinition def) => GetTileSource(def);

        /// <summary>Opens the Object Editor (name, NPC flag, collision, over-the-player zones).</summary>
        private void OpenObjectEditor(ObjectDefinition def)
        {
            if (def.Tiles == null || def.Tiles.Count == 0)
            {
                MessageBox.Show(this, "This object has no tiles.", "Object Editor");
                return;
            }

            var win = new ObjectEditorWindow(def, RenderObjectPreview(def)) { Owner = this };
            if (win.ShowDialog() != true)
                return;

            ObjectDatabase.AddOrUpdate(def);
            ObjectDatabase.Save();

            RefreshObjectBrowser();
            AssetPreviewHost.Content = BuildObjectPreview(def);
            ShowObjectProperties(def);
            RedrawMap();
            ShowStatusMessage($"Saved object \"{def.Name}\"");
        }

        private void TilesetManager_Click(object sender, RoutedEventArgs e) => OpenTilesetManager();

        private void OpenTilesetManager()
        {
            // Open on the sheet shown in the tile picker, if any
            var window = new TilesetManagerWindow(EditModeHelper.IsInTileMode ? selectedAssetPath : null) { Owner = this };
            window.ShowDialog();

            // Sheets may have been re-cut, tiles renamed / deleted / added: drop cached tile images
            tileImageCache.Clear();
            tilesheetCache.Clear();
            RedrawMap();

            if (EditModeHelper.IsInTileMode)
            {
                RefreshFolderBrowser();
                if (!string.IsNullOrEmpty(selectedAssetPath))
                    DisplayAsset(selectedAssetPath);
            }

            UpdateStatusBar();
        }

        private void EditSelectedObject_Click(object sender, RoutedEventArgs e)
        {
            if (ObjectDatabase.Definitions.TryGetValue(selectedObjectId, out var def))
                OpenObjectEditor(def);
            else
                MessageBox.Show(this, "Select an object in Place Objects mode first.", "Object Editor");
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
            MapCanvas.Width = mapWidth * MapConstant.TileSize;
            MapCanvas.Height = mapHeight * MapConstant.TileSize;

            // Same size as the map so both are placed identically in the grid; otherwise the map is
            // centered while the overlay stays top-left, and previews appear far from the mouse
            HighlightCanvas.Width = MapCanvas.Width;
            HighlightCanvas.Height = MapCanvas.Height;

            if (mapDrawing == null)
            {
                mapDrawing = new MapDrawingHost();
                MapCanvas.Children.Add(mapDrawing);
            }

            mapDrawables.Clear();

            DrawTiles();
            DrawTriggers();
            DrawObjects();
            DrawGridOverlay();

            // Same draw order rule as the game (DrawSortKey), so the editor shows what the game draws
            mapDrawables.Sort((a, b) => a.Key.CompareTo(b.Key));
            using (var dc = mapDrawing.RenderOpen())
            {
                foreach (var drawable in mapDrawables)
                    drawable.Draw(dc);
            }
        }

        // Overlays (grid, collision, trigger markers) and anything sorted like a map tile
        private void AddMapDrawable(int z, Action<DrawingContext> draw)
        {
            AddMapDrawable(DrawSortKey.ForMapTile(z, 0), draw);
        }

        private void AddMapDrawable(DrawSortKey key, Action<DrawingContext> draw)
        {
            key.Order = mapDrawables.Count; // insertion order breaks ties
            mapDrawables.Add(new MapDrawable { Key = key, Draw = draw });
        }

        private void AddImageDrawable(DrawSortKey key, ImageSource source, Rect rect)
        {
            if (ShouldDim(key.Z))
            {
                AddMapDrawable(key, dc =>
                {
                    dc.PushOpacity(0.3);
                    dc.DrawImage(source, rect);
                    dc.Pop();
                });
            }
            else
            {
                AddMapDrawable(key, dc => dc.DrawImage(source, rect));
            }
        }

        private ImageSource GetTileSource(TileDefinition tileDef)
        {
            string key;
            if (!string.IsNullOrEmpty(tileDef.Tilesheet) && tileDef.SheetX.HasValue && tileDef.SheetY.HasValue)
                key = $"{tileDef.Tilesheet}|{tileDef.SheetX.Value}|{tileDef.SheetY.Value}";
            else if (!string.IsNullOrEmpty(tileDef.Sprite))
                key = tileDef.Sprite;
            else
                return null;

            if (tileImageCache.TryGetValue(key, out var cached))
                return cached;

            BitmapSource source;
            if (!string.IsNullOrEmpty(tileDef.Tilesheet) && tileDef.SheetX.HasValue && tileDef.SheetY.HasValue)
            {
                // Cut with the sheet's own grid, exactly like the game does
                var sheet = GetOrLoadTilesheet(AssetPathResolver.GetTilePath(tileDef.Tilesheet));
                var rect = TilesheetDatabase.GetSourceRect(tileDef, sheet.PixelWidth, sheet.PixelHeight);
                if (rect == null)
                    return null;

                source = new CroppedBitmap(sheet, rect.Value);
            }
            else
            {
                var fullPath = Path.Combine(AssetPathResolver.GetAssetsRootPath(), tileDef.Sprite);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.EndInit();
                source = bitmap;
            }

            source.Freeze();
            tileImageCache[key] = source;
            return source;
        }

        private static Geometry CreatePolygonGeometry(IEnumerable<Point> points)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                bool first = true;
                foreach (var point in points)
                {
                    if (first)
                    {
                        ctx.BeginFigure(point, true, true);
                        first = false;
                    }
                    else
                    {
                        ctx.LineTo(point, true, false);
                    }
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static T Frozen<T>(T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

        private void DrawTiles()
        {
            bool showCollision = ToggleCollisionOverlay?.IsChecked == true;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    foreach (var tile in map.GetTilesAt(x, y))
                    {
                        if (!TileDatabase.Definitions.TryGetValue(tile.Id, out var tileDef)) continue;

                        var source = GetTileSource(tileDef);
                        if (source == null) continue;

                        var rect = new Rect(x * MapConstant.TileSize, y * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize);
                        AddImageDrawable(DrawSortKey.ForMapTile(tile.Z, 0), source, rect);

                        if (showCollision && tileDef.CollisionPolygon != null && tileDef.CollisionPolygon.Count >= 2)
                        {
                            int tx = x, ty = y;
                            var polygon = CreatePolygonGeometry(tileDef.CollisionPolygon.Select(p =>
                                new Point((tx + p.X) * MapConstant.TileSize,
                                          (ty + p.Y) * MapConstant.TileSize)));

                            AddMapDrawable(tile.Z + 1, dc => dc.DrawGeometry(TileCollisionFill, TileCollisionPen, polygon));
                        }
                    }
                }
            }
        }

        private void RedrawHighlightLayer()
        {
            HighlightCanvas.Children.Clear();

            DrawBrushPreview();

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

        private void DrawBrushPreview()
        {
            if (!hoverPoint.HasValue || isSelecting || draggedObject != null)
                return;

            var pos = hoverPoint.Value;

            // Rectangle being dragged: show the area that will be filled / erased (even if the
            // mouse goes past the map edge)
            if (strokeKind == StrokeKind.RectPaint || strokeKind == StrokeKind.RectErase)
            {
                var (left, top, right, bottom) = CurrentRect();
                left = Math.Max(0, left); top = Math.Max(0, top);
                right = Math.Min(mapWidth - 1, right); bottom = Math.Min(mapHeight - 1, bottom);
                if (right < left || bottom < top) return;

                var area = new Rect(left * MapConstant.TileSize, top * MapConstant.TileSize,
                    (right - left + 1) * MapConstant.TileSize, (bottom - top + 1) * MapConstant.TileSize);
                bool erase = strokeKind == StrokeKind.RectErase;
                AddHighlightRect(area, erase ? Brushes.OrangeRed : Brushes.White, erase ? DeleteHoverFill : HoverFill, 2);
                ShowStatusMessage($"{(erase ? "Erase" : "Fill")} {right - left + 1} x {bottom - top + 1}");
                return;
            }

            var (cellX, cellY) = GetCell(pos);
            if (!IsInsideMap(cellX, cellY))
                return;

            var cellRect = new Rect(cellX * MapConstant.TileSize, cellY * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize);

            switch (EditModeHelper.CurrentMode)
            {
                case EditorMode.Tile:
                    if (tileTool == TileTool.Fill)
                    {
                        AddHighlightRect(cellRect, Brushes.Cyan, HoverFill, 2);
                        break;
                    }

                    // Brush (and the rectangle tool before dragging): the cells under the brush
                    int size = tileTool == TileTool.Brush ? brushSize : 1;
                    int start = -(size - 1) / 2;
                    var brushArea = new Rect((cellX + start) * MapConstant.TileSize, (cellY + start) * MapConstant.TileSize,
                        size * MapConstant.TileSize, size * MapConstant.TileSize);

                    if (strokeKind == StrokeKind.Erase)
                    {
                        AddHighlightRect(brushArea, Brushes.OrangeRed, DeleteHoverFill, 2);
                    }
                    else
                    {
                        if (TileDatabase.Definitions.TryGetValue(selectedTileId, out var tileDef))
                        {
                            var source = GetTileSource(tileDef);
                            if (source != null)
                            {
                                for (int dy = 0; dy < size; dy++)
                                    for (int dx = 0; dx < size; dx++)
                                    {
                                        int x = cellX + start + dx, y = cellY + start + dy;
                                        if (IsInsideMap(x, y))
                                            AddHighlightImage(source, new Rect(x * MapConstant.TileSize, y * MapConstant.TileSize, MapConstant.TileSize, MapConstant.TileSize), 0.6);
                                    }
                            }
                        }
                        AddHighlightRect(brushArea, Brushes.White, null, 1);
                    }
                    break;

                case EditorMode.Object:
                    var hovered = GetObjectUnderPointer(pos);
                    if (hovered != null)
                    {
                        // Outline the whole object: right-click / Delete removes it
                        foreach (var rect in GetObjectTileRects(hovered))
                            AddHighlightRect(rect, Brushes.OrangeRed, DeleteHoverFill, 2);
                    }
                    else if (ObjectDatabase.Definitions.TryGetValue(selectedObjectId, out var objDef) && objDef.Tiles != null && objDef.Tiles.Count > 0)
                    {
                        // Ghost of the object where it would be placed (centered on the mouse, like HandleObjectClick)
                        int minX = objDef.Tiles.Min(t => t.X);
                        int minY = objDef.Tiles.Min(t => t.Y);
                        double width = objDef.Tiles.Max(t => t.X) - minX + 1;
                        double height = objDef.Tiles.Max(t => t.Y) - minY + 1;
                        double originX = pos.X / MapConstant.TileSize - width / 2.0;
                        double originY = pos.Y / MapConstant.TileSize - height / 2.0;

                        foreach (var entry in objDef.Tiles)
                        {
                            if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var entryDef)) continue;
                            var source = GetTileSource(entryDef);
                            if (source == null) continue;

                            var rect = new Rect(
                                (originX + entry.X - minX) * MapConstant.TileSize,
                                (originY + entry.Y - minY) * MapConstant.TileSize,
                                MapConstant.TileSize, MapConstant.TileSize);
                            AddHighlightImage(source, rect, 0.6);
                        }
                    }
                    break;

                case EditorMode.Trigger:
                    // Clicking an object edits its event: outline the whole object; otherwise the tile
                    var hoveredForEvent = GetObjectUnderPointer(pos);
                    if (hoveredForEvent != null)
                    {
                        foreach (var rect in GetObjectTileRects(hoveredForEvent))
                            AddHighlightRect(rect, Brushes.Yellow, HoverFill, 2);
                    }
                    else
                    {
                        AddHighlightRect(cellRect, Brushes.Yellow, HoverFill, 2);
                    }
                    break;
            }
        }

        private IEnumerable<Rect> GetObjectTileRects(PlacedObject obj)
        {
            if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def) || def.Tiles == null || def.Tiles.Count == 0)
                yield break;

            int minX = def.Tiles.Min(t => t.X);
            int minY = def.Tiles.Min(t => t.Y);

            foreach (var entry in def.Tiles)
            {
                yield return new Rect(
                    (obj.X + entry.X - minX) * MapConstant.TileSize,
                    (obj.Y + entry.Y - minY) * MapConstant.TileSize,
                    MapConstant.TileSize, MapConstant.TileSize);
            }
        }

        private void AddHighlightRect(Rect rect, Brush stroke, Brush fill, double thickness)
        {
            var shape = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Stroke = stroke,
                StrokeThickness = thickness,
                Fill = fill,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shape, rect.X);
            Canvas.SetTop(shape, rect.Y);
            HighlightCanvas.Children.Add(shape);
        }

        private void AddHighlightImage(ImageSource source, Rect rect, double opacity)
        {
            var image = new Image
            {
                Source = source,
                Width = rect.Width,
                Height = rect.Height,
                Opacity = opacity,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(image, rect.X);
            Canvas.SetTop(image, rect.Y);
            HighlightCanvas.Children.Add(image);
        }

        private void DrawTriggers()
        {
            foreach (var tile in map.Tiles.Where(t => t.Trigger != null))
            {
                var rect = new Rect(tile.X * MapConstant.TileSize + MapConstant.TileSize - 12, tile.Y * MapConstant.TileSize + 2, 10, 10);
                AddMapDrawable(100, dc => dc.DrawRectangle(Brushes.Yellow, TriggerPen, rect));
            }
        }

        private void DrawObjects()
        {
            bool showCollision = ToggleCollisionOverlay?.IsChecked == true;

            foreach (var obj in map.Objects)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def)) continue;
                if (def.Tiles == null || def.Tiles.Count == 0) continue;

                var (minX, minY, width, height) = def.GetTileBounds();
                var tileImages = new List<(ImageSource source, Rect rect)>();

                foreach (var entry in def.Tiles)
                {
                    if (!TileDatabase.Definitions.TryGetValue(entry.TileId, out var tileDef)) continue;

                    var source = GetTileSource(tileDef);
                    if (source == null) continue;

                    double px = (obj.X + (entry.X - minX)) * MapConstant.TileSize;
                    double py = (obj.Y + (entry.Y - minY)) * MapConstant.TileSize;

                    var rect = new Rect(px, py, MapConstant.TileSize, MapConstant.TileSize);
                    // Every tile uses the object's base, so the object sorts as one unit (as in the game)
                    AddImageDrawable(DrawSortKey.ForObject(obj.GetTileZ(entry), obj, def, 0), source, rect);
                    tileImages.Add((source, rect));
                }

                // Shape stored normalized over the object's bounds -> map pixels
                Geometry ToMapGeometry(List<Point> shape) => CreatePolygonGeometry(shape.Select(p =>
                    new Point((obj.X + p.X * width) * MapConstant.TileSize,
                              (obj.Y + p.Y * height) * MapConstant.TileSize)));

                // Over-the-player zones: the object's pixels inside the zone, drawn again on the
                // above-player layer (same as the game)
                foreach (var zone in def.AbovePlayerZones ?? new List<List<Point>>())
                {
                    if (zone == null || zone.Count < 3) continue;

                    var clip = ToMapGeometry(zone);
                    int zoneZ = obj.AbovePlayerZoneZ;
                    bool dim = ShouldDim(zoneZ);

                    AddMapDrawable(DrawSortKey.ForObject(zoneZ, obj, def, 0), dc =>
                    {
                        if (dim) dc.PushOpacity(0.3);
                        dc.PushClip(clip);
                        foreach (var (source, rect) in tileImages)
                            dc.DrawImage(source, rect);
                        dc.Pop();
                        if (dim) dc.Pop();
                    });

                    if (showCollision)
                        AddMapDrawable(zoneZ + 1, dc => dc.DrawGeometry(ZoneFill, ZonePen, clip));
                }

                // Event marker at the object's top-right corner: yellow = press to use, cyan = walk into
                if (obj.Event != null)
                {
                    var center = new Point((obj.X + width) * MapConstant.TileSize - 8, obj.Y * MapConstant.TileSize + 8);
                    var fill = obj.Event.Activation == EventActivation.Touch ? Brushes.Cyan : Brushes.Gold;
                    AddMapDrawable(100, dc => dc.DrawEllipse(fill, EventMarkerPen, center, 6, 6));
                }

                if (showCollision && def.CollisionPolygon != null && def.CollisionPolygon.Count >= 2)
                {
                    var polygon = ToMapGeometry(def.CollisionPolygon);
                    AddMapDrawable(obj.Z + 1, dc => dc.DrawGeometry(ObjectCollisionFill, ObjectCollisionPen, polygon));
                }

                var screenPoint = new Point(obj.X * MapConstant.TileSize, obj.Y * MapConstant.TileSize);
                if (!selectionRect.IsEmpty && selectionRect.Contains(screenPoint))
                {
                    var rect = new Rect(screenPoint.X, screenPoint.Y, MapConstant.TileSize, MapConstant.TileSize);
                    AddMapDrawable(obj.Z + 1000, dc => dc.DrawRectangle(null, SelectedObjectPen, rect));
                }
            }
        }

        private void DrawGridOverlay()
        {
            if (!showGrid) return;

            double width = mapWidth * MapConstant.TileSize;
            double height = mapHeight * MapConstant.TileSize;

            AddMapDrawable(1000, dc =>
            {
                for (int y = 0; y <= mapHeight; y++)
                    dc.DrawLine(GridPen, new Point(0, y * MapConstant.TileSize), new Point(width, y * MapConstant.TileSize));

                for (int x = 0; x <= mapWidth; x++)
                    dc.DrawLine(GridPen, new Point(x * MapConstant.TileSize, 0), new Point(x * MapConstant.TileSize, height));
            });
        }

        // === Files ===

        private void SaveMap_Click(object sender, RoutedEventArgs e) => Save();

        private void SaveMapAs_Click(object sender, RoutedEventArgs e) => SaveAs();

        private void LoadMap_Click(object sender, RoutedEventArgs e) => OpenMap();

        private void NewMap_Click(object sender, RoutedEventArgs e) => NewMap();

        /// <summary>Saves to the current file, or asks for one. Returns false if cancelled or failed.</summary>
        private bool Save()
        {
            if (currentFilePath == null)
                return SaveAs();

            return WriteMap(currentFilePath);
        }

        private bool SaveAs()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON Map (*.json)|*.json",
                DefaultExt = ".json",
                Title = "Save Map As",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
                FileName = currentFilePath != null ? Path.GetFileName(currentFilePath) : currentMapName
            };

            if (saveDialog.ShowDialog() != true)
                return false;

            return WriteMap(saveDialog.FileName);
        }

        private bool WriteMap(string path)
        {
            // The map's name is its file name (the game refers to maps by file, e.g. in warps)
            string name = Path.GetFileNameWithoutExtension(path);

            try
            {
                map.Name = name;
                map.Width = mapWidth;
                map.Height = mapHeight;

                MapSerializer.Save(path, map);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save the map:\n{ex.Message}", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // From now on Save writes to this file (after Save As, the new file becomes the current map)
            currentFilePath = path;
            currentMapName = name;
            isDirty = false;
            UpdateTitle();
            ShowStatusMessage($"Saved {Path.GetFileName(path)} at {DateTime.Now:HH:mm:ss}");
            return true;
        }

        private void OpenMap()
        {
            if (!ConfirmDiscardChanges())
                return;

            var openDialog = new OpenFileDialog
            {
                Filter = "JSON Map (*.json)|*.json",
                Title = "Open Map",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
            };

            if (openDialog.ShowDialog() != true)
                return;

            Map loaded;
            try
            {
                loaded = MapSerializer.Load(openDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not open the map:\n{ex.Message}", "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (loaded == null)
            {
                MessageBox.Show(this, "Failed to load map.");
                return;
            }

            SetCurrentMap(loaded, openDialog.FileName);
            ShowStatusMessage($"Opened {Path.GetFileName(openDialog.FileName)}");
        }

        private void NewMap()
        {
            if (!ConfirmDiscardChanges())
                return;

            var dialog = new MapPropertiesWindow("New Map", "New Map", 30, 17) { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            SetCurrentMap(new Map
            {
                Name = dialog.MapName,
                Width = dialog.MapWidth,
                Height = dialog.MapHeight,
                RandomEncounters = dialog.RandomEncounters
            }, null);
            ShowStatusMessage("New map");
        }

        private void SetCurrentMap(Map newMap, string filePath)
        {
            map = newMap;
            currentFilePath = filePath;

            // An opened map is named after its file (the "name" stored inside old files was often
            // a leftover "New Map"); a new map keeps the name typed in the New Map dialog
            if (filePath != null)
                currentMapName = Path.GetFileNameWithoutExtension(filePath);
            else
                currentMapName = string.IsNullOrWhiteSpace(newMap.Name) ? "New Map" : newMap.Name;
            mapWidth = newMap.Width;
            mapHeight = newMap.Height;

            undoManager.Clear();
            selectionRect = Rect.Empty;
            isDirty = false;

            RedrawMap();
            FitMapToView();
            RedrawHighlightLayer();
            UpdateTitle();
            UpdateStatusBar();
        }

        /// <summary>Asks to save unsaved changes. Returns false if the user cancelled.</summary>
        private bool ConfirmDiscardChanges()
        {
            if (!isDirty)
                return true;

            var answer = MessageBox.Show(this,
                $"Save changes to \"{currentMapName}\"?",
                "Unsaved changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (answer == MessageBoxResult.Cancel)
                return false;

            if (answer == MessageBoxResult.Yes)
                return Save();

            return true; // No: discard
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                e.Cancel = true;

            base.OnClosing(e);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MapProperties_Click(object sender, RoutedEventArgs e)
        {
            // A saved map is named after its file, so it is renamed with Save As, not here
            bool canRename = currentFilePath == null;

            var dialog = new MapPropertiesWindow("Map Properties", currentMapName, mapWidth, mapHeight, canRename, map.RandomEncounters) { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            string newName = canRename ? dialog.MapName : currentMapName;
            if (newName == currentMapName && dialog.MapWidth == mapWidth && dialog.MapHeight == mapHeight &&
                dialog.RandomEncounters == map.RandomEncounters)
                return;

            map.RandomEncounters = dialog.RandomEncounters;

            bool resized = dialog.MapWidth != mapWidth || dialog.MapHeight != mapHeight;
            currentMapName = newName;
            mapWidth = dialog.MapWidth;
            mapHeight = dialog.MapHeight;
            map.Name = currentMapName;
            map.Width = mapWidth;
            map.Height = mapHeight;

            MarkDirty();
            RedrawMap();
            if (resized)
                FitMapToView();
            UpdateTitle();
            UpdateStatusBar();
        }

        // === Edit menu ===

        private void Undo()
        {
            if (undoManager.Undo()) { MarkDirty(); RedrawMap(); }
            else ShowStatusMessage("Nothing to undo");
        }

        private void Redo()
        {
            if (undoManager.Redo()) { MarkDirty(); RedrawMap(); }
            else ShowStatusMessage("Nothing to redo");
        }

        private void ClearSelection()
        {
            selectionRect = Rect.Empty;
            RedrawMap();
            RedrawHighlightLayer();
        }

        private void Undo_Click(object sender, RoutedEventArgs e) => Undo();
        private void Redo_Click(object sender, RoutedEventArgs e) => Redo();
        private void ClearSelection_Click(object sender, RoutedEventArgs e) => ClearSelection();

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectionRect.IsEmpty)
                ShowStatusMessage("Nothing selected: Ctrl+drag on the map to select objects");
            else
                DeleteSelectedObjects();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e) => FitMapToView();

        private void ShowControls_Click(object sender, RoutedEventArgs e) => ShowControls();

        private void ShowControls()
        {
            MessageBox.Show(this,
                "PAINT TILES\n" +
                "  Brush (B): left drag paints, right drag erases. Size: [ and ] (1 to 9 tiles)\n" +
                "  Rectangle (R): left drag fills a rectangle, right drag erases it\n" +
                "  Fill (F): click fills the connected area of the same tile (Shift+click with any tool)\n" +
                "  Fill layer / Clear layer buttons: the whole current layer at once\n" +
                "  Alt + click: pick the tile under the mouse (and its layer)\n\n" +
                "PLACE OBJECTS\n" +
                "  Left click: place the selected object (click an NPC: edit it)\n" +
                "  Right click or Delete: delete the object under the mouse\n" +
                "  Shift + drag: move an object\n" +
                "  Double-click an object in the list: open the Object Editor\n\n" +
                "EVENTS\n" +
                "  Left click an object: edit its event (presets: Chest, Door, Sign)\n" +
                "  Left click a tile: add or edit the tile's trigger\n" +
                "  Right click: remove the event / trigger\n" +
                "  Markers on the map: yellow = press to use, cyan = walk into\n\n" +
                "EVERYWHERE\n" +
                "  Ctrl + drag: select objects, then Delete to remove them (Esc to clear)\n" +
                "\nCAMERA\n" +
                "  Middle mouse drag (or Space + drag): move the view\n" +
                "  Arrow keys: move the view,  Mouse wheel: zoom toward the mouse\n" +
                "  Ctrl+0: fit and center the map,  Ctrl+1: 100%\n\n" +
                "  Ctrl+Z / Ctrl+Y: undo / redo,  Ctrl+S: save",
                "Controls", MessageBoxButton.OK, MessageBoxImage.Information);
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

            // Apply to parent container. The map and highlight layers are retained, so no redraw is needed
            MapLayerContainer.RenderTransform = group;
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
                ExecuteEdit(new BatchCommand(deleted));
                RedrawMap();
                ShowStatusMessage($"Deleted {deleted.Count} object(s)");
            }

            selectionRect = Rect.Empty;
            RedrawHighlightLayer();
        }

        private void DeleteObject(PlacedObject obj)
        {
            ExecuteEdit(new RemoveObjectCommand(map, obj));
            RedrawMap();
            RedrawHighlightLayer();

            string name = ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def) ? def.Name : "object";
            ShowStatusMessage($"Deleted {name}");
        }

        // === Modes ===

        private void TileMode_Checked(object sender, RoutedEventArgs e) =>
            SetMode(EditorMode.Tile, "Pick a tile in a sheet, then paint. Tools: B brush, R rectangle, F fill. [ ] brush size. Right: erase, Alt+click: pick");

        private void ObjectMode_Checked(object sender, RoutedEventArgs e) =>
            SetMode(EditorMode.Object, "Pick an object in the list, click to place. Right/Del: delete, Shift+drag: move. Double-click the list to edit");

        private void TriggerMode_Checked(object sender, RoutedEventArgs e) =>
            SetMode(EditorMode.Trigger, "Click an object to give it an event (chest, door, sign), or a tile for a tile trigger. Right-click removes it");

        private void SelectionMode_Checked(object sender, RoutedEventArgs e) =>
            SetMode(EditorMode.Selection, "Click a tile to change its layer (Z)");

        private void SelectionObjectMode_Checked(object sender, RoutedEventArgs e) =>
            SetMode(EditorMode.ObjectSelection, "Click a part of a placed object to override its layer for that copy only");

        private void SetMode(EditorMode mode, string hint)
        {
            // Leaving tile mode ends a tile selection for a new object
            if (mode != EditorMode.Tile && ToggleGroupSelection.IsChecked == true)
                ToggleGroupSelection.IsChecked = false;

            EditModeHelper.CurrentMode = mode;

            bool tiles = mode == EditorMode.Tile;
            bool objects = mode == EditorMode.Object;

            FolderBrowser.Visibility = tiles ? Visibility.Visible : Visibility.Collapsed;
            ObjectBrowser.Visibility = objects ? Visibility.Visible : Visibility.Collapsed;
            TilesheetToolsPanel.Visibility = tiles ? Visibility.Visible : Visibility.Collapsed;
            TileToolBar.Visibility = tiles ? Visibility.Visible : Visibility.Collapsed;
            AssetPreviewHost.Content = null;

            if (tiles)
                RefreshFolderBrowser();
            else if (objects)
            {
                RefreshObjectBrowser();
                if (ObjectDatabase.Definitions.TryGetValue(selectedObjectId, out var def))
                    AssetPreviewHost.Content = BuildObjectPreview(def);
            }

            MapCanvas.Focus();
            ShowStatusMessage(hint);
            RedrawHighlightLayer();
            UpdateStatusBar();
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
                        Header = def.IsNpc ? $"{def.Name}  (NPC)" : def.Name,
                        Tag = def.Id,
                        ToolTip = "Double-click to open the Object Editor",
                        IsSelected = def.Id == selectedObjectId
                    };

                    int defId = def.Id;
                    objItem.MouseDoubleClick += (s, e) =>
                    {
                        if (ObjectDatabase.Definitions.TryGetValue(defId, out var d))
                            OpenObjectEditor(d);
                        e.Handled = true;
                    };

                    categoryItem.Items.Add(objItem);
                }

                ObjectBrowser.Items.Add(categoryItem);
            }
        }

        private void ObjectBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Only in Place Objects mode: elsewhere the preview area shows something else (e.g. a tilesheet)
            if (!EditModeHelper.IsInObjectMode)
                return;

            if (e.NewValue is TreeViewItem item && item.Tag is int id && ObjectDatabase.Definitions.TryGetValue(id, out var def))
            {
                selectedObjectId = id;

                AssetPreviewHost.Content = BuildObjectPreview(def);
                ShowObjectProperties(def);
                UpdateStatusBar();
            }
        }

        private static readonly Brush PreviewCollisionFill = Frozen(new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)));
        private static readonly Brush PreviewZoneFill = Frozen(new SolidColorBrush(Color.FromArgb(70, 0, 150, 255)));

        /// <summary>
        /// Read-only preview of an object with its collision (green) and over-the-player
        /// zones (blue), plus a button to open the Object Editor.
        /// </summary>
        private UIElement BuildObjectPreview(ObjectDefinition def)
        {
            var panel = new DockPanel { Margin = new Thickness(10) };

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var editButton = new Button { Content = "Edit object (collision, over-player zones)...", Padding = new Thickness(8, 2, 8, 2) };
            editButton.Click += (s, e) => OpenObjectEditor(def);
            header.Children.Add(editButton);
            header.Children.Add(new TextBlock
            {
                Text = "Green = collision    Blue = drawn over the player",
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            });
            DockPanel.SetDock(header, Dock.Top);
            panel.Children.Add(header);

            if (def.Tiles == null || def.Tiles.Count == 0)
                return panel;

            var (_, _, width, height) = def.GetTileBounds();
            double pixelWidth = width * MapConstant.TileSize;
            double pixelHeight = height * MapConstant.TileSize;

            var canvas = new Canvas { Width = pixelWidth, Height = pixelHeight };
            canvas.Children.Add(new Image { Source = RenderObjectPreview(def), Width = pixelWidth, Height = pixelHeight });

            void AddShape(List<Point> shape, Brush stroke, Brush fill)
            {
                if (shape == null || shape.Count < 3) return;
                var polygon = new Polygon { Stroke = stroke, StrokeThickness = 1.5, Fill = fill };
                foreach (var p in shape)
                    polygon.Points.Add(new Point(p.X * pixelWidth, p.Y * pixelHeight));
                canvas.Children.Add(polygon);
            }

            AddShape(def.CollisionPolygon, Brushes.LimeGreen, PreviewCollisionFill);
            foreach (var zone in def.AbovePlayerZones ?? new List<List<Point>>())
                AddShape(zone, Brushes.DeepSkyBlue, PreviewZoneFill);

            // Shrink big objects to fit the panel, never enlarge small ones
            panel.Children.Add(new Viewbox
            {
                Child = canvas,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            });

            return panel;
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

            // Cut the sheet with its own grid (Tiles > Tileset Manager)
            var sheet = TilesheetDatabase.Get(path);
            viewer.SetGrid(sheet);

            if (EditModeHelper.IsInTileMode)
            {
                selectedTilesheet = Path.GetFileName(path);

                viewer.TileClicked += (tileX, tileY) =>
                {
                    var def = TileDatabase.FindSheetTile(path, tileX, tileY);
                    if (def != null)
                    {
                        selectedTileId = def.Id;
                    }
                    else
                    {
                        def = new TileDefinition
                        {
                            Id = TileDatabase.GetNextId(),
                            Name = $"{sheet.Name} {tileX},{tileY}",
                            IsWalkable = true,
                            Tilesheet = Path.GetFileName(path),
                            SheetX = tileX,
                            SheetY = tileY
                        };
                        TileDatabase.AddOrUpdate(def);
                        // Save right away: otherwise the tile only existed in memory and maps
                        // painted with it lost it after restarting the editor
                        TileDatabase.Save();
                        selectedTileId = def.Id;
                    }

                    ShowTileProperties(def);
                };
            }

            // Opening another sheet while selecting tiles for a new object: keep selecting
            if (EditModeHelper.IsInTileMode && ToggleGroupSelection.IsChecked == true)
                viewer.EnableGroupTileEdit();

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

            // (selectedTilesheet is kept: forgetting it here made "Create object" fail after
            // turning selection off and on again without re-opening the sheet)
        }

        // Category picked for the last created object, offered again for the next one
        private string lastObjectCategory = "General";

        private void CreateObjectFromGroupSelection()
        {
            if (!(AssetPreviewHost.Content is ZoomableImageViewer viewer) || string.IsNullOrEmpty(selectedTilesheet))
            {
                ShowStatusMessage("Open a tilesheet first (list on the left)");
                return;
            }

            var selectedPoints = viewer.GetSelectedGroupTiles();
            if (selectedPoints.Count == 0)
            {
                ShowStatusMessage("Turn on \"Select tiles for a new object\" and click the tiles of the object first");
                return;
            }

            var categories = ObjectDatabase.Definitions.Values.Select(d => d.Category);
            var dialog = new NewObjectWindow(selectedPoints.Count, categories, lastObjectCategory) { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            if (!TileGroupObjectCreator.TryCreateObjectFromSelection(selectedTilesheet, selectedPoints, out var newObj, dialog.ObjectName, dialog.Category))
            {
                MessageBox.Show(this, "Could not create the object from this selection.", "New Object", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lastObjectCategory = newObj.Category;

            // Stay on the sheet, still selecting, with an empty selection: ready for the next object.
            // (The object list is refreshed when switching to Place Objects mode; refreshing it here
            // used to select an object and replace the sheet with that object's preview.)
            viewer.EnableGroupTileEdit();
            ShowStatusMessage($"Created \"{newObj.Name}\" in {newObj.Category}. Select the tiles of the next object, or switch to Place Objects.");
        }

        private PlacedObject GetObjectUnderPointer(Point pointer)
        {
            double px = pointer.X / MapConstant.TileSize;
            double py = pointer.Y / MapConstant.TileSize;

            // Hit-test the object's actual tiles (obj.X/Y is its top-left corner, so the old
            // "within half a tile of X/Y" check only worked when clicking that corner).
            // Last match = drawn last = top-most.
            return map.Objects.LastOrDefault(obj =>
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def) || def.Tiles == null || def.Tiles.Count == 0)
                    return false;

                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);

                return def.Tiles.Any(entry =>
                {
                    double left = obj.X + (entry.X - minX);
                    double top = obj.Y + (entry.Y - minY);
                    return px >= left && px < left + 1 && py >= top && py < top + 1;
                });
            });
        }

        private static bool IsNpc(PlacedObject obj)
        {
            return obj.Meta != null && obj.Meta.TryGetValue("npc", out var tag) && tag == "true";
        }

        private void EditNpc(PlacedObject npc)
        {
            var npcWindow = new NpcEditWindow(npc.Meta) { Owner = this };
            if (npcWindow.ShowDialog() == true)
            {
                npc.Meta = npcWindow.ResultMeta;
                MarkDirty();
                RedrawMap();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HandleKeyboardInput(e);
            base.OnKeyDown(e);
        }

        private void HandleKeyboardInput(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
            {
                SaveAs();
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        Undo();
                        e.Handled = true;
                        break;
                    case Key.Y:
                        Redo();
                        e.Handled = true;
                        break;
                    case Key.D0:
                    case Key.NumPad0:
                        FitMapToView();
                        e.Handled = true;
                        break;
                    case Key.D1:
                    case Key.NumPad1:
                        ActualSize();
                        e.Handled = true;
                        break;
                    case Key.S:
                        Save();
                        e.Handled = true;
                        break;
                    case Key.O:
                        OpenMap();
                        e.Handled = true;
                        break;
                    case Key.N:
                        NewMap();
                        e.Handled = true;
                        break;
                    case Key.T:
                        OpenTilesetManager();
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.Delete && !selectionRect.IsEmpty)
            {
                DeleteSelectedObjects();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && EditModeHelper.IsInObjectMode)
            {
                // Delete the object under the mouse
                var target = GetObjectUnderPointer(Mouse.GetPosition(MapCanvas));
                if (target != null)
                    DeleteObject(target);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && EditModeHelper.IsInTileMode)
            {
                var (tileX, tileY) = GetCell(Mouse.GetPosition(MapCanvas));
                EraseTile(tileX, tileY);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && !selectionRect.IsEmpty)
            {
                ClearSelection();
                e.Handled = true;
            }
            else if (e.Key == Key.F1)
            {
                ShowControls();
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && !IsTypingInTextField && EditModeHelper.IsInTileMode &&
                     (e.Key == Key.B || e.Key == Key.R || e.Key == Key.F || e.Key == Key.OemOpenBrackets || e.Key == Key.OemCloseBrackets))
            {
                // Paint Tiles shortcuts: B / R / F = Brush / Rectangle / Fill, [ and ] = brush size
                switch (e.Key)
                {
                    case Key.B: BrushToolRadio.IsChecked = true; break;
                    case Key.R: RectToolRadio.IsChecked = true; break;
                    case Key.F: FillToolRadio.IsChecked = true; break;
                    case Key.OemOpenBrackets: ChangeBrushSize(-1); break;
                    case Key.OemCloseBrackets: ChangeBrushSize(+1); break;
                }
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && !IsTypingInTextField &&
                     (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down))
            {
                // Arrow keys move the camera (the map moves the opposite way)
                PanBy(e.Key == Key.Left ? ArrowPanStep : e.Key == Key.Right ? -ArrowPanStep : 0,
                      e.Key == Key.Up ? ArrowPanStep : e.Key == Key.Down ? -ArrowPanStep : 0);
                e.Handled = true;
            }
        }

        private static bool IsTypingInTextField =>
            Keyboard.FocusedElement is TextBox || Keyboard.FocusedElement is ComboBox || Keyboard.FocusedElement is ComboBoxItem;

        private void EditorMainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !IsTypingInTextField)
            {
                isSpaceHeld = true;
                e.Handled = true; // otherwise Space also "clicks" whichever toolbar button has focus
            }
        }

        private void EditorMainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                isSpaceHeld = false;
        }

        // === Camera (pan / zoom) ===

        private const double MinZoom = 0.1;
        private const double MaxZoom = 8.0;
        private const double ArrowPanStep = 150; // screen pixels

        private bool isPanning;
        private Point panLast;
        private bool viewInitialized;

        private void MapViewport_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Middle button, or Space + left button, drags the view (anywhere, even off the map)
            bool middle = e.ChangedButton == MouseButton.Middle;
            bool spaceLeft = e.ChangedButton == MouseButton.Left && isSpaceHeld;
            if (!middle && !spaceLeft)
                return;

            isPanning = true;
            panLast = e.GetPosition(MapViewport);
            MapViewport.CaptureMouse();
            MapViewport.Cursor = Cursors.SizeAll;
            e.Handled = true; // don't paint/place while panning
        }

        private void MapViewport_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning)
                return;

            var current = e.GetPosition(MapViewport);
            currentPanOffset += current - panLast;
            panLast = current;
            ApplyCanvasTransform();
            e.Handled = true;
        }

        private void MapViewport_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isPanning || (e.ChangedButton != MouseButton.Middle && e.ChangedButton != MouseButton.Left))
                return;

            isPanning = false;
            MapViewport.ReleaseMouseCapture();
            MapViewport.Cursor = null;
            e.Handled = true;
        }

        private void MapViewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.15 : 1 / 1.15;
            ZoomAt(e.GetPosition(MapViewport), zoom * factor);
            e.Handled = true;
        }

        private void MapViewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // The viewport has no size yet in the constructor: center the first map once it does
            if (!viewInitialized)
                FitMapToView();
        }

        /// <summary>Zooms while keeping the map point under <paramref name="viewportPoint"/> in place.</summary>
        private void ZoomAt(Point viewportPoint, double newZoom)
        {
            newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));

            var mapPoint = new Point(
                (viewportPoint.X - currentPanOffset.X) / zoom,
                (viewportPoint.Y - currentPanOffset.Y) / zoom);

            zoom = newZoom;
            currentPanOffset = new Vector(
                viewportPoint.X - mapPoint.X * zoom,
                viewportPoint.Y - mapPoint.Y * zoom);

            ApplyCanvasTransform();
        }

        /// <summary>Centers the whole map in the view (zooming out if needed, never above 100%).</summary>
        private void FitMapToView()
        {
            double viewWidth = MapViewport.ActualWidth;
            double viewHeight = MapViewport.ActualHeight;
            if (viewWidth <= 0 || viewHeight <= 0)
            {
                viewInitialized = false; // try again on SizeChanged
                return;
            }

            double mapPixelWidth = Math.Max(1, mapWidth * MapConstant.TileSize);
            double mapPixelHeight = Math.Max(1, mapHeight * MapConstant.TileSize);

            zoom = Math.Max(MinZoom, Math.Min(1.0, Math.Min(viewWidth / mapPixelWidth, viewHeight / mapPixelHeight) * 0.95));
            currentPanOffset = new Vector(
                (viewWidth - mapPixelWidth * zoom) / 2,
                (viewHeight - mapPixelHeight * zoom) / 2);

            ApplyCanvasTransform();
            viewInitialized = true;
        }

        private void ActualSize_Click(object sender, RoutedEventArgs e) => ActualSize();

        private void ActualSize()
        {
            ZoomAt(new Point(MapViewport.ActualWidth / 2, MapViewport.ActualHeight / 2), 1.0);
        }

        private void PanBy(double dx, double dy)
        {
            currentPanOffset += new Vector(dx, dy);
            ApplyCanvasTransform();
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            // (Panning is handled before this by the viewport: MapViewport_PreviewMouseDown)

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

                // Top-most real tile in the cell, on any layer (GetTileAt only looked at Z=0).
                // Id 0 tiles are just trigger holders.
                var tile = map.GetTilesAt(stileX, stileY).LastOrDefault(t => t.Id != 0);
                if (tile != null)
                {
                    var zEditor = new ZEditPopup(tile.Z);
                    if (zEditor.ShowDialog() == true)
                    {
                        tile.Z = zEditor.Result;
                        MarkDirty();
                        map.InvalidateTileIndex();
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
                                MarkDirty();
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
                    dragStartX = target.X;
                    dragStartY = target.Y;
                    dragOffset = new Point(target.X * MapConstant.TileSize - pos.X, target.Y * MapConstant.TileSize - pos.Y);
                    MapCanvas.CaptureMouse();
                    return;
                }

                // Clicking an existing NPC edits it (double-click used to place a second
                // object with its first click before opening the editor)
                if (target != null && IsNpc(target))
                {
                    EditNpc(target);
                    return;
                }

                // Otherwise place the selected object
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
                case EditorMode.Trigger:
                    // An object under the mouse gets an object event; otherwise the tile gets a trigger
                    var clickedObject = GetObjectUnderPointer(pos);
                    if (clickedObject != null)
                        EditObjectEvent(clickedObject);
                    else
                        HandleTriggerClick(tileX, tileY);
                    break;
            }
        }

        private void EditObjectEvent(PlacedObject obj)
        {
            ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def);

            var window = new ObjectEventWindow(def, obj.Event, currentFilePath) { Owner = this };
            if (window.ShowDialog() != true)
                return;

            if (string.IsNullOrEmpty(obj.Uid))
                obj.Uid = PlacedObject.NewUid();

            ExecuteEdit(new SetObjectEventCommand(obj, window.Result));
            RedrawMap();
            ShowStatusMessage(window.Result == null
                ? $"Removed the event of {def?.Name ?? "object"}"
                : $"{def?.Name ?? "Object"}: event saved ({window.Result.Actions.Count} action(s)). Save the map to keep it.");
        }

        private void MapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            var (tileX, tileY) = GetCell(pos);

            switch (EditModeHelper.CurrentMode)
            {
                case EditorMode.Tile:
                    if (tileTool == TileTool.Rectangle)
                    {
                        // Drag to erase a rectangle
                        BeginRect(StrokeKind.RectErase, tileX, tileY);
                    }
                    else
                    {
                        // Erase with the brush (drag to keep erasing)
                        BeginStroke(StrokeKind.Erase);
                        StrokeTo(tileX, tileY);
                    }
                    break;

                case EditorMode.Object:
                    var target = GetObjectUnderPointer(pos);
                    if (target != null)
                        DeleteObject(target);
                    break;

                case EditorMode.Trigger:
                    var eventObject = GetObjectUnderPointer(pos);
                    if (eventObject?.Event != null)
                    {
                        ExecuteEdit(new SetObjectEventCommand(eventObject, null));
                        RedrawMap();
                        ShowStatusMessage("Removed the object's event (Ctrl+Z to undo)");
                    }
                    else
                    {
                        RemoveTrigger(tileX, tileY);
                    }
                    break;
            }

            e.Handled = true;
        }

        private void MapCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (strokeKind == StrokeKind.Erase)
                EndStroke();
            else if (strokeKind == StrokeKind.RectErase)
                FinishRect();
        }

        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            hoverPoint = null;
            RedrawHighlightLayer();
            UpdateStatusBar();
        }

        // === Brush strokes (drag painting / erasing) ===

        private void BeginStroke(StrokeKind kind)
        {
            // A stroke whose mouse-up was missed (e.g. focus lost) is closed first so it stays undoable
            if (strokeKind != StrokeKind.None)
                EndStroke();

            strokeKind = kind;
            strokeCommands.Clear();
            lastStrokeCell = (-1, -1);
            MapCanvas.CaptureMouse();
        }

        private void EndStroke()
        {
            if (strokeCommands.Count > 0)
            {
                // Already applied while dragging: record the whole stroke as one undo step
                undoManager.Push(new BatchCommand(strokeCommands));
                MarkDirty();
            }

            strokeCommands.Clear();
            strokeKind = StrokeKind.None;
            MapCanvas.ReleaseMouseCapture();
        }

        /// <summary>
        /// Paints (or erases) with the brush from the last stroke position to this cell. Following
        /// the line in between means fast mouse moves leave no gaps.
        /// </summary>
        private void StrokeTo(int tileX, int tileY)
        {
            if ((tileX, tileY) == lastStrokeCell)
                return;

            var from = lastStrokeCell.x < 0 ? (tileX, tileY) : lastStrokeCell;
            bool erase = strokeKind == StrokeKind.Erase;
            bool changed = false;

            foreach (var (lx, ly) in LineCells(from, (tileX, tileY)))
            {
                foreach (var (bx, by) in BrushCells(lx, ly))
                    changed |= erase ? EraseCell(bx, by, strokeCommands) : PaintCell(bx, by, strokeCommands);
            }

            lastStrokeCell = (tileX, tileY);

            if (changed)
                RedrawMap(); // once per mouse move, not once per tile
        }

        /// <summary>Sets the selected tile on the current layer. Returns false if nothing changed.</summary>
        private bool PaintCell(int x, int y, List<IEditorCommand> into)
        {
            if (!IsInsideMap(x, y) || map.GetTileAt(x, y, selectedLayer)?.Id == selectedTileId)
                return false;

            var command = new SetTileCommand(map, x, y, selectedLayer, selectedTileId);
            command.Execute();
            into.Add(command);
            return true;
        }

        /// <summary>Erases the cell on the current layer. Returns false if nothing changed.</summary>
        private bool EraseCell(int x, int y, List<IEditorCommand> into)
        {
            if (!IsInsideMap(x, y))
                return false;

            var command = new RemoveTileCommand(map, x, y, selectedLayer);
            if (command.IsEmpty)
                return false;

            command.Execute();
            into.Add(command);
            return true;
        }

        /// <summary>Cells covered by the brush centered on (x, y). Even sizes extend right/down.</summary>
        private IEnumerable<(int x, int y)> BrushCells(int x, int y)
        {
            int start = -(brushSize - 1) / 2;
            for (int dy = 0; dy < brushSize; dy++)
                for (int dx = 0; dx < brushSize; dx++)
                    yield return (x + start + dx, y + start + dy);
        }

        /// <summary>Cells on the straight line between two cells (Bresenham).</summary>
        private static IEnumerable<(int x, int y)> LineCells((int x, int y) a, (int x, int y) b)
        {
            int x = a.x, y = a.y;
            int dx = Math.Abs(b.x - a.x), dy = -Math.Abs(b.y - a.y);
            int sx = a.x < b.x ? 1 : -1, sy = a.y < b.y ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                yield return (x, y);
                if (x == b.x && y == b.y) yield break;

                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x += sx; }
                if (e2 <= dx) { err += dx; y += sy; }
            }
        }

        // === Rectangle tool ===

        private void BeginRect(StrokeKind kind, int tileX, int tileY)
        {
            BeginStroke(kind);
            rectStartCell = (tileX, tileY);
            RedrawHighlightLayer();
        }

        private (int left, int top, int right, int bottom) CurrentRect()
        {
            var end = hoverPoint.HasValue ? GetCell(hoverPoint.Value) : rectStartCell;
            return (Math.Min(rectStartCell.x, end.x), Math.Min(rectStartCell.y, end.y),
                    Math.Max(rectStartCell.x, end.x), Math.Max(rectStartCell.y, end.y));
        }

        private void FinishRect()
        {
            var (left, top, right, bottom) = CurrentRect();
            bool erase = strokeKind == StrokeKind.RectErase;

            for (int y = top; y <= bottom; y++)
                for (int x = left; x <= right; x++)
                    _ = erase ? EraseCell(x, y, strokeCommands) : PaintCell(x, y, strokeCommands);

            EndStroke(); // records the whole rectangle as one undo step
            RedrawMap();
            RedrawHighlightLayer();
        }

        // === Whole layer ===

        private void FillLayer_Click(object sender, RoutedEventArgs e)
        {
            if (!TileDatabase.Definitions.ContainsKey(selectedTileId))
            {
                ShowStatusMessage("Pick a tile first (open a tilesheet and click a tile)");
                return;
            }

            ApplyToWholeLayer(PaintCell, "Filled");
        }

        private void ClearLayer_Click(object sender, RoutedEventArgs e) => ApplyToWholeLayer(EraseCell, "Cleared");

        private void ApplyToWholeLayer(Func<int, int, List<IEditorCommand>, bool> action, string verb)
        {
            var commands = new List<IEditorCommand>();
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    action(x, y, commands);

            if (commands.Count == 0)
            {
                ShowStatusMessage("Nothing to change on this layer");
                return;
            }

            // Already applied: record as one undo step
            undoManager.Push(new BatchCommand(commands));
            MarkDirty();
            RedrawMap();
            ShowStatusMessage($"{verb} layer {selectedLayer}: {commands.Count} tile(s). Ctrl+Z to undo.");
        }

        // === Tool / size selection ===

        private void TileTool_Checked(object sender, RoutedEventArgs e)
        {
            if (FillToolRadio == null) return; // during InitializeComponent

            tileTool = RectToolRadio.IsChecked == true ? TileTool.Rectangle
                     : FillToolRadio.IsChecked == true ? TileTool.Fill
                     : TileTool.Brush;

            MapCanvas?.Focus(); // keep keyboard shortcuts working
            RedrawHighlightLayer();
            UpdateStatusBar();
        }

        private void BrushSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrushSizeBox?.SelectedItem is ComboBoxItem item && int.TryParse(item.Content.ToString(), out int size))
                brushSize = size;

            if (MapCanvas == null) return; // during InitializeComponent
            MapCanvas.Focus();
            RedrawHighlightLayer();
            UpdateStatusBar();
        }

        private void ChangeBrushSize(int direction)
        {
            int index = Array.IndexOf(BrushSizes, brushSize) + direction;
            index = Math.Max(0, Math.Min(BrushSizes.Length - 1, index));
            BrushSizeBox.SelectedIndex = index;
        }

        private void EraseTile(int tileX, int tileY)
        {
            if (!IsInsideMap(tileX, tileY))
                return;

            var command = new RemoveTileCommand(map, tileX, tileY, selectedLayer);
            if (command.IsEmpty)
                return;

            ExecuteEdit(command);
            RedrawMap();
        }

        private void PickTile(int tileX, int tileY)
        {
            // Eyedropper: take the top-most visible tile in the cell, and switch to its layer
            var tile = map.GetTilesAt(tileX, tileY).LastOrDefault(t => t.Id != 0);
            if (tile == null || !TileDatabase.Definitions.TryGetValue(tile.Id, out var def))
            {
                ShowStatusMessage("Nothing to pick here");
                return;
            }

            selectedTileId = tile.Id;
            LayerInput.Text = tile.Z.ToString();
            ShowTileProperties(def);
            ShowStatusMessage($"Picked {def.Name} (#{tile.Id}) on layer {tile.Z}");
            UpdateStatusBar();
        }

        private void RemoveTrigger(int tileX, int tileY)
        {
            var tile = map.GetTileAt(tileX, tileY);
            if (tile?.Trigger == null)
                return;

            ExecuteEdit(new SetTriggerCommand(map, tileX, tileY, null));
            RedrawMap();
            ShowStatusMessage($"Removed trigger at {tileX}, {tileY}");
        }

        private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // === End paint stroke / rectangle ===
            if (strokeKind == StrokeKind.Paint)
            {
                EndStroke();
                return;
            }
            if (strokeKind == StrokeKind.RectPaint)
            {
                FinishRect();
                return;
            }

            // === End drag object ===
            if (draggedObject != null)
            {
                var pos = e.GetPosition(MapCanvas);
                draggedObject.X = (pos.X + dragOffset.X) / MapConstant.TileSize;
                draggedObject.Y = (pos.Y + dragOffset.Y) / MapConstant.TileSize;

                if (draggedObject.X != dragStartX || draggedObject.Y != dragStartY)
                {
                    // Already moved while dragging: record it so it can be undone
                    undoManager.Push(new MoveObjectCommand(draggedObject, dragStartX, dragStartY, draggedObject.X, draggedObject.Y));
                    MarkDirty();
                }

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
                RedrawHighlightLayer();
            }
        }

        private void HandleTileClick(int tileX, int tileY)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                PickTile(tileX, tileY);
                return;
            }

            // Fill tool, or Shift+click with any tool: flood fill
            if (tileTool == TileTool.Fill || Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ExecuteEdit(new FillTileCommand(map, tileX, tileY, selectedLayer, selectedTileId));
                RedrawMap();
                return;
            }

            if (tileTool == TileTool.Rectangle)
            {
                BeginRect(StrokeKind.RectPaint, tileX, tileY);
                return;
            }

            // Brush: paint while dragging
            BeginStroke(StrokeKind.Paint);
            StrokeTo(tileX, tileY);
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
                Uid = PlacedObject.NewUid(),
                Meta = isNpc ? new Dictionary<string, string> { ["npc"] = "true", ["name"] = "New NPC" } : null
            };

            ExecuteEdit(new PlaceObjectCommand(map, obj));
            RedrawMap();
        }

        private void HandleTriggerClick(int tileX, int tileY)
        {
            var existingTile = map.GetTileAt(tileX, tileY);
            var triggerEditWindow = new TriggerEditWindow(existingTile?.Trigger) { Owner = this };
            if (triggerEditWindow.ShowDialog() == true)
            {
                ExecuteEdit(new SetTriggerCommand(map, tileX, tileY, triggerEditWindow.Result));
                RedrawMap();
            }
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);

            hoverPoint = pos;
            UpdateStatusBar();

            // === Drag painting / erasing ===
            if ((strokeKind == StrokeKind.Paint && e.LeftButton == MouseButtonState.Pressed) ||
                (strokeKind == StrokeKind.Erase && e.RightButton == MouseButtonState.Pressed))
            {
                var (cx, cy) = GetCell(pos);
                StrokeTo(cx, cy);
                RedrawHighlightLayer();
                return;
            }
            if (strokeKind == StrokeKind.RectPaint || strokeKind == StrokeKind.RectErase)
            {
                RedrawHighlightLayer(); // the rectangle follows the mouse; applied on release
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
                RedrawMap(); // show the object following the mouse
                return;
            }

            if (isSelecting && selectionStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var current = e.GetPosition(MapCanvas);
                selectionRect = new Rect(selectionStart.Value, current);
                RedrawHighlightLayer();
                return;
            }

            // Brush preview / hover outline
            RedrawHighlightLayer();
        }

        private Point GetMouseMapPosition(MouseEventArgs e)
        {
            // GetPosition relative to MapCanvas already undoes the zoom/pan transform
            return e.GetPosition(MapCanvas);
        }

        private void LayerInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(LayerInput.Text, out int z))
            {
                selectedLayer = z;

                if (LayerPresetBox != null)
                    SyncLayerPreset();

                if (DimOtherLayersBox?.IsChecked == true)
                    RedrawMap();

                UpdateStatusBar();
            }
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
