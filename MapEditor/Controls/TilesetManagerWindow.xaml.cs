using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using JRPG.Core.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEditor.Controls
{
    /// <summary>
    /// Imports tilesheet images, sets how each one is cut (Tilesheets.json) and edits the
    /// tiles cut from them (Tileset.json). Edits apply in memory; Save writes both files,
    /// closing without saving reloads them from disk.
    /// </summary>
    public partial class TilesetManagerWindow : Window
    {
        private string currentFile;          // sheet file name (in Assets/Tiles)
        private BitmapSource currentImage;   // the sheet, as Bgra32 (to detect empty cells)
        private int? selectedX, selectedY;   // selected cell
        private bool isDirty;
        private bool suppressEvents;

        /// <summary>True when something was saved, so the map editor knows to refresh.</summary>
        public bool ChangesSaved { get; private set; }

        private static string TilesFolder => AssetPathResolver.GetTilePath("");

        public TilesetManagerWindow(string initialSheet = null)
        {
            InitializeComponent();

            SheetViewer.TileClicked += (x, y) => SelectCell(x, y);

            RefreshSheetList(initialSheet != null ? Path.GetFileName(initialSheet) : null);
        }

        // === Sheets ===

        private void RefreshSheetList(string selectFile = null)
        {
            suppressEvents = true;
            SheetList.Items.Clear();

            if (Directory.Exists(TilesFolder))
            {
                foreach (var path in Directory.GetFiles(TilesFolder, "*.png").OrderBy(p => p))
                {
                    string file = Path.GetFileName(path);
                    SheetList.Items.Add(new ListBoxItem { Content = SheetLabel(file), Tag = file, ToolTip = file });
                }
            }

            suppressEvents = false;

            var toSelect = SheetList.Items.OfType<ListBoxItem>()
                .FirstOrDefault(i => string.Equals((string)i.Tag, selectFile ?? currentFile, StringComparison.OrdinalIgnoreCase))
                ?? SheetList.Items.OfType<ListBoxItem>().FirstOrDefault();

            if (toSelect != null)
            {
                if (SheetList.SelectedItem == toSelect)
                    LoadSheet((string)toSelect.Tag); // already selected: reload anyway
                else
                    SheetList.SelectedItem = toSelect;
            }
        }

        private void SheetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressEvents) return;

            if (SheetList.SelectedItem is ListBoxItem item)
                LoadSheet((string)item.Tag);
        }

        private void LoadSheet(string file)
        {
            BitmapSource image;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(AssetPathResolver.GetTilePath(file), UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                converted.Freeze();
                image = converted;
            }
            catch (Exception ex)
            {
                SetStatus($"Could not open {file}: {ex.Message}");
                return;
            }

            currentFile = file;
            selectedX = selectedY = null;
            currentImage = image;

            var sheet = TilesheetDatabase.Get(file);

            suppressEvents = true;
            SheetFileText.Text = $"File: {file}";
            SheetImageSizeText.Text = $"Image: {currentImage.PixelWidth} x {currentImage.PixelHeight} px";
            SheetNameBox.Text = sheet.Name ?? Path.GetFileNameWithoutExtension(file);
            TileWidthBox.Text = sheet.TileWidth.ToString();
            TileHeightBox.Text = sheet.TileHeight.ToString();
            MarginBox.Text = sheet.Margin.ToString();
            SpacingBox.Text = sheet.Spacing.ToString();
            suppressEvents = false;

            SheetViewer.SetImage(currentImage);
            SheetViewer.SetGrid(sheet);
            RefreshMarkedCells();
            UpdateGridSummary(sheet, false);
            ShowCell();
        }

        /// <summary>The current sheet's settings, registered so they get saved.</summary>
        private TilesheetDefinition CurrentSheet()
        {
            if (!TilesheetDatabase.Contains(currentFile))
                TilesheetDatabase.AddOrUpdate(TilesheetDatabase.Get(currentFile));
            return TilesheetDatabase.Get(currentFile);
        }

        private void SheetName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressEvents || currentFile == null) return;

            CurrentSheet().Name = SheetNameBox.Text.Trim();
            MarkDirty();
        }

        private void Grid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressEvents || currentFile == null) return;

            if (!int.TryParse(TileWidthBox.Text, out int w) || !int.TryParse(TileHeightBox.Text, out int h) ||
                !int.TryParse(MarginBox.Text, out int margin) || !int.TryParse(SpacingBox.Text, out int spacing) ||
                w < 1 || h < 1 || margin < 0 || spacing < 0 ||
                w > currentImage.PixelWidth || h > currentImage.PixelHeight)
            {
                GridSummaryText.Text = "Enter a valid tile size (and margin / spacing of 0 or more).";
                return;
            }

            var sheet = CurrentSheet();
            sheet.TileWidth = w;
            sheet.TileHeight = h;
            sheet.Margin = margin;
            sheet.Spacing = spacing;
            MarkDirty();

            SheetViewer.SetGrid(sheet);
            RefreshMarkedCells();
            UpdateGridSummary(sheet, true);
            ShowCell();
        }

        private void UpdateGridSummary(TilesheetDefinition sheet, bool gridChanged)
        {
            int columns = sheet.ColumnsFor(currentImage.PixelWidth);
            int rows = sheet.RowsFor(currentImage.PixelHeight);
            GridSummaryText.Text = $"{columns} x {rows} = {columns * rows} cells";

            int used = SheetTiles().Count();
            if (gridChanged && used > 0)
            {
                GridWarningText.Text = $"{used} tile(s) are already cut from this sheet. Changing the grid changes what they show on your maps.";
                GridWarningText.Visibility = Visibility.Visible;
            }
            else
            {
                GridWarningText.Visibility = Visibility.Collapsed;
            }
        }

        private IEnumerable<TileDefinition> SheetTiles()
        {
            return TileDatabase.Definitions.Values.Where(d => SameFile(d.Tilesheet, currentFile) && d.SheetX.HasValue && d.SheetY.HasValue);
        }

        private void RefreshMarkedCells()
        {
            SheetViewer.SetMarkedCells(SheetTiles().Select(d => new Point(d.SheetX.Value, d.SheetY.Value)));
        }

        private static bool SameFile(string a, string b)
        {
            return string.Equals(Path.GetFileName(a ?? ""), Path.GetFileName(b ?? ""), StringComparison.OrdinalIgnoreCase);
        }

        // === Import ===

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PNG images (*.png)|*.png",
                Title = "Import tilesheet image(s)",
                Multiselect = true
            };

            if (dialog.ShowDialog(this) != true)
                return;

            string lastImported = null;

            foreach (var source in dialog.FileNames)
            {
                string file = Path.GetFileName(source);
                string destination = Path.Combine(TilesFolder, file);

                try
                {
                    bool alreadyThere = string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase);
                    if (!alreadyThere)
                    {
                        if (File.Exists(destination))
                        {
                            var answer = MessageBox.Show(this,
                                $"Assets/Tiles/{file} already exists. Replace it?\n(Tiles already cut from it will use the new image.)",
                                "Import", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (answer != MessageBoxResult.Yes)
                                continue;
                        }

                        File.Copy(source, destination, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not import {file}:\n{ex.Message}", "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (!TilesheetDatabase.Contains(file))
                {
                    TilesheetDatabase.AddOrUpdate(new TilesheetDefinition
                    {
                        File = file,
                        Name = Path.GetFileNameWithoutExtension(file),
                        TileWidth = GuessTileSize(destination),
                        TileHeight = GuessTileSize(destination)
                    });
                    MarkDirty();
                }

                lastImported = file;
            }

            if (lastImported != null)
            {
                RefreshSheetList(lastImported);
                SetStatus($"Imported {lastImported}. Check its tile size, then add its tiles.");
            }
        }

        /// <summary>Common tile sizes that divide the image evenly, preferring the map's 48px.</summary>
        private static int GuessTileSize(string path)
        {
            try
            {
                var decoder = BitmapDecoder.Create(new Uri(path, UriKind.Absolute), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                int width = decoder.Frames[0].PixelWidth;
                int height = decoder.Frames[0].PixelHeight;

                foreach (int size in new[] { 48, 32, 16, 64, 24 })
                {
                    if (width % size == 0 && height % size == 0)
                        return size;
                }
            }
            catch
            {
                // fall through to the default
            }

            return JRPG.Core.Maps.MapConstant.TileSize;
        }

        // === Tiles ===

        private void SelectCell(int x, int y)
        {
            selectedX = x;
            selectedY = y;
            ShowCell();
        }

        private TileDefinition SelectedTile()
        {
            if (selectedX == null || selectedY == null) return null;
            return TileDatabase.FindSheetTile(currentFile, selectedX.Value, selectedY.Value);
        }

        private void ShowCell()
        {
            suppressEvents = true;

            if (selectedX == null || selectedY == null)
            {
                CellText.Text = "Click a cell in the sheet.";
                TilePreview.Source = null;
                AddTileButton.Visibility = Visibility.Collapsed;
                TilePanel.Visibility = Visibility.Collapsed;
                suppressEvents = false;
                return;
            }

            SheetViewer.SelectCell(selectedX.Value, selectedY.Value);
            TilePreview.Source = CropCell(selectedX.Value, selectedY.Value);

            var tile = SelectedTile();
            if (tile == null)
            {
                CellText.Text = $"Cell {selectedX}, {selectedY} is not a tile yet.";
                AddTileButton.Visibility = Visibility.Visible;
                TilePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                CellText.Text = $"Cell {selectedX}, {selectedY}";
                AddTileButton.Visibility = Visibility.Collapsed;
                TilePanel.Visibility = Visibility.Visible;
                TileIdText.Text = $"Tile id: {tile.Id}";
                TileNameBox.Text = tile.Name ?? "";
                WalkableBox.IsChecked = tile.IsWalkable;
                CollisionText.Text = tile.CollisionPolygon != null && tile.CollisionPolygon.Count >= 3
                    ? $"Collision shape: {tile.CollisionPolygon.Count} points"
                    : "No collision shape";
            }

            suppressEvents = false;
        }

        private BitmapSource CropCell(int x, int y)
        {
            var probe = new TileDefinition { Tilesheet = currentFile, SheetX = x, SheetY = y };
            var rect = TilesheetDatabase.GetSourceRect(probe, currentImage.PixelWidth, currentImage.PixelHeight);
            if (rect == null) return null;

            var cropped = new CroppedBitmap(currentImage, rect.Value);
            cropped.Freeze();
            return cropped;
        }

        private TileDefinition CreateTile(int x, int y)
        {
            var sheet = CurrentSheet();
            var tile = new TileDefinition
            {
                Id = TileDatabase.GetNextId(),
                Name = $"{sheet.Name} {x},{y}",
                IsWalkable = true,
                Tilesheet = currentFile,
                SheetX = x,
                SheetY = y
            };
            TileDatabase.AddOrUpdate(tile);
            return tile;
        }

        private void AddTile_Click(object sender, RoutedEventArgs e)
        {
            if (selectedX == null || selectedY == null || SelectedTile() != null) return;

            var tile = CreateTile(selectedX.Value, selectedY.Value);
            MarkDirty();
            RefreshMarkedCells();
            ShowCell();
            SetStatus($"Added tile #{tile.Id}");
        }

        private void AddAllTiles_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile == null) return;

            var sheet = CurrentSheet();
            int columns = sheet.ColumnsFor(currentImage.PixelWidth);
            int rows = sheet.RowsFor(currentImage.PixelHeight);
            int added = 0, empty = 0;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (TileDatabase.FindSheetTile(currentFile, x, y) != null)
                        continue;

                    if (IsCellEmpty(sheet, x, y))
                    {
                        empty++;
                        continue;
                    }

                    CreateTile(x, y);
                    added++;
                }
            }

            if (added > 0)
                MarkDirty();

            RefreshMarkedCells();
            ShowCell();
            RefreshSheetListLabels();
            SetStatus($"Added {added} tile(s), skipped {empty} empty cell(s).");
        }

        /// <summary>True when every pixel of the cell is fully transparent.</summary>
        private bool IsCellEmpty(TilesheetDefinition sheet, int x, int y)
        {
            var probe = new TileDefinition { Tilesheet = currentFile, SheetX = x, SheetY = y };
            var rect = TilesheetDatabase.GetSourceRect(probe, currentImage.PixelWidth, currentImage.PixelHeight);
            if (rect == null) return true;

            int stride = rect.Value.Width * 4;
            var pixels = new byte[stride * rect.Value.Height];
            currentImage.CopyPixels(rect.Value, pixels, stride, 0);

            for (int i = 3; i < pixels.Length; i += 4) // Bgra32: alpha is every 4th byte
            {
                if (pixels[i] != 0)
                    return false;
            }
            return true;
        }

        private void TileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suppressEvents) return;

            var tile = SelectedTile();
            if (tile == null) return;

            tile.Name = TileNameBox.Text.Trim();
            MarkDirty();
        }

        private void Walkable_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressEvents) return;

            var tile = SelectedTile();
            if (tile == null) return;

            tile.IsWalkable = WalkableBox.IsChecked == true;
            MarkDirty();
        }

        private void EditCollision_Click(object sender, RoutedEventArgs e)
        {
            var tile = SelectedTile();
            if (tile == null || selectedX == null || selectedY == null) return;

            var existing = tile.CollisionPolygon ?? new List<Point>();
            var window = new CollisionEditorWindow(existing) { Owner = this };
            window.SetImageAndPolygon(CropCell(selectedX.Value, selectedY.Value), existing);

            if (window.ShowDialog() == true)
            {
                tile.CollisionPolygon = window.Result.ToList();
                MarkDirty();
                ShowCell();
            }
        }

        private void DeleteTile_Click(object sender, RoutedEventArgs e)
        {
            var tile = SelectedTile();
            if (tile == null) return;

            var answer = MessageBox.Show(this,
                $"Delete tile #{tile.Id} \"{tile.Name}\"?\nMaps that use it will show an empty spot there.",
                "Delete tile", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            TileDatabase.Remove(tile.Id);
            MarkDirty();
            RefreshMarkedCells();
            ShowCell();
            RefreshSheetListLabels();
            SetStatus($"Deleted tile #{tile.Id}");
        }

        /// <summary>Updates the sheet names / tile counts in the list without changing the selection.</summary>
        private void RefreshSheetListLabels()
        {
            foreach (var item in SheetList.Items.OfType<ListBoxItem>())
                item.Content = SheetLabel((string)item.Tag);
        }

        private static string SheetLabel(string file)
        {
            var sheet = TilesheetDatabase.Get(file);
            int tileCount = TileDatabase.Definitions.Values.Count(d => SameFile(d.Tilesheet, file));

            string label = sheet.Name ?? file;
            if (!TilesheetDatabase.Contains(file))
                label += "  (grid not set)";
            return label + $"  - {tileCount} tile{(tileCount == 1 ? "" : "s")}";
        }

        // === Save / close ===

        private void MarkDirty()
        {
            if (isDirty) return;
            isDirty = true;
            Title = "Tileset Manager *";
        }

        private void SetStatus(string message) => StatusText.Text = message;

        private bool Save()
        {
            try
            {
                TilesheetDatabase.Save();
                TileDatabase.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save:\n{ex.Message}", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            isDirty = false;
            ChangesSaved = true;
            Title = "Tileset Manager";
            RefreshSheetListLabels();
            SetStatus($"Saved Tileset.json and Tilesheets.json at {DateTime.Now:HH:mm:ss}");
            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e) => Save();

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            if (isDirty)
            {
                var answer = MessageBox.Show(this, "Save changes to the tileset?", "Unsaved changes",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (answer == MessageBoxResult.Cancel || (answer == MessageBoxResult.Yes && !Save()))
                {
                    e.Cancel = true;
                    return;
                }

                if (answer == MessageBoxResult.No)
                {
                    // Discard: go back to what is on disk (also reloads the sheet settings)
                    TileDatabase.Load();
                }
            }

            base.OnClosing(e);
        }
    }
}
