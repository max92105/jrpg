using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;
using Microsoft.Win32;

namespace Jrpg.Editor.Animation;

/// <summary>
/// Strip from sheet cells (decision 990), over the Effect strips page: a PNG sheet opened from anywhere, shown zoomable
/// under its grid and numbers; with Pick on, a click adds the cell under it, or a row's number the whole row, and the
/// shared preview plays the picks at the typed pace. OK cuts the picks out of the sheet and hands them to the shell's
/// write, which copies the strip into the effects folder; a refusal keeps the tool open. Its rules are
/// <see cref="StripFromCellsViewModel"/>'s.
/// </summary>
internal sealed partial class StripFromCellsWindow : Window
{
    // A sheet's cell opens this many screen pixels wide: a row of eight fits the sheet's side of the window.
    private const int StartCellPixels = 64;

    private readonly StripFromCellsViewModel _strip = new();
    private readonly CellZoom _zoom = new(StartCellPixels);
    private readonly Func<CellStrip, IReadOnlyList<uint[]>, SheetImportResult> _write;
    private BitmapSource? _sheet;
    private double _scale = 1;

    // The cell width the zoom last scaled by, kept while a retyped cut has no cell, so the sheet doesn't jump in size.
    private int _scaledCellWidth = 1;

    /// <param name="write">
    /// The shell's write: copies the strip, its cells' pixels in play order, into the effects folder, asking before it
    /// replaces an image, and says what it did.
    /// </param>
    public StripFromCellsWindow(Func<CellStrip, IReadOnlyList<uint[]>, SheetImportResult> write)
    {
        InitializeComponent();
        DataContext = _strip;
        ZoomBar.DataContext = _zoom;
        _write = write;
        _ = new ZoomedScroller(SheetScroller, DrawSheet, () => _scale) { Zoom = _zoom };
        _strip.PicksChanged += OnPicksChanged;
        _strip.PropertyChanged += (_, _) => RefusalText.Text = "";
    }

    /// <summary>What OK wrote, for the page that opened the tool; null until a write copied the strip.</summary>
    public SheetImportResult? Written { get; private set; }

    // A refused file only names its refusal on the file line: the sheet open before stays, with its picks.
    private void OnOpenSheetClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Open a sheet", Filter = "PNG images (*.png)|*.png" };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var name = Path.GetFileName(dialog.FileName);
        BitmapImage sheet;
        try
        {
            sheet = SpriteImageFile.Read(dialog.FileName) ?? throw new InvalidDataException("not a PNG image.");
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException
            or NotSupportedException or FileFormatException or COMException)
        {
            FileText.Text = $"{name}: {exception.Message}";
            FileText.SetResourceReference(ForegroundProperty, "ProblemTextBrush");
            return;
        }

        FileText.Text = dialog.FileName;
        FileText.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        _sheet = sheet;
        _strip.TakeSheet(name, (sheet.PixelWidth, sheet.PixelHeight));
    }

    // Raised for the cut too, so the grid follows the typed columns and rows; new frames start the preview over.
    private void OnPicksChanged(object? sender, EventArgs e)
    {
        DrawSheet();
        PicksPreview.Frames = _sheet is not null && _strip.CellSize is { } cell
            ? PreviewFrames.OfCells(_sheet, cell, _strip.Picks)
            : null;
    }

    // The sheet at the zoom's size per cell, its grid and numbers, and the picks numbered over it.
    private void DrawSheet()
    {
        if (_sheet is null)
        {
            return;
        }

        var size = (_sheet.PixelWidth, _sheet.PixelHeight);
        var cell = _strip.CellSize;
        _scaledCellWidth = cell?.Width ?? _scaledCellWidth;
        _scale = _zoom.ScaleFor(_scaledCellWidth);
        SheetImage.Source = _sheet;
        SheetMarks.Source = SpriteSheetMarks.Draw(size, cell?.Width ?? 0, cell?.Height ?? 0, _scale);
        PickMarks.Source = cell is { } picked ? SequenceMarks.Draw(size, picked, _strip.Picks, _scale) : null;
        SheetImage.Width = SheetMarks.Width = PickMarks.Width = size.PixelWidth;
        SheetImage.Height = SheetMarks.Height = PickMarks.Height = size.PixelHeight;
        SheetView.LayoutTransform = new ScaleTransform(_scale, _scale);
    }

    // OK is greyed until the view model has no problem, so there is a sheet and the picks lie within it. A declined
    // replace has neither an image nor a refusal: the tool stays open as it was.
    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        var strip = _strip.ToStrip();
        var result = _write(strip, SheetCells.Cut(_sheet!, strip.CellSize, strip.Cells));
        if (result.Imported.Count == 0)
        {
            RefusalText.Text = result.Summary;
            return;
        }

        Written = result;
        DialogResult = true;
    }

    // The image is sized in its own pixels under the zoom's layout transform, so the point is in image pixels; a row's
    // number lies over its first cell, and picks the row instead.
    private void OnSheetClicked(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(SheetImage);
        if (_sheet is not null && _strip.CellSize is { } cell
            && SpriteSheetMarks.RowAt(point, _sheet.PixelHeight, cell.Height, _scale) is { } row)
        {
            _strip.PickRow(row);
            return;
        }

        _strip.PickCellAt(point.X, point.Y);
    }
}
