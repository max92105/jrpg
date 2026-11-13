using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;
using Microsoft.Win32;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// The Character sprites window (decisions 568-570): the images on the left, the chosen one in the middle with its
/// frame grid, its setup on the right with the four facings walking at the editor's animation time. OK applies the
/// session as one undo step, greyed while a problem shows; Cancel drops it.
/// </summary>
internal sealed partial class FieldSpriteWindow : Window
{
    private readonly FieldSpriteCatalogViewModel _sprites;
    private readonly string _artRoot;
    private readonly TileAnimationTime _animationTime;
    private readonly Func<IReadOnlyList<string>, SheetImportResult> _import;
    private readonly CellZoom _zoom = new((int)Maps.MapDrawing.CellPixels);
    private readonly ZoomedScroller _zooming;
    private readonly WalkPreview _preview = new();
    private FieldSpriteSetupViewModel? _setup;
    private BitmapSource? _sheet;
    private double _scale = 1;

    // The chosen setup's record while it has no problem, built once per change so a render tick only compares cells.
    private FieldSpriteSheet? _previewSheet;

    /// <param name="sprites">The window's state.</param>
    /// <param name="artRoot">The <c>art</c> folder, where the images are read from.</param>
    /// <param name="animationTime">The editor's animation time, which the preview walks on.</param>
    /// <param name="import">
    /// Copies chosen images into the field-sprite art and says what it did; the shell drops the replaced pictures.
    /// </param>
    public FieldSpriteWindow(
        FieldSpriteCatalogViewModel sprites,
        string artRoot,
        TileAnimationTime animationTime,
        Func<IReadOnlyList<string>, SheetImportResult> import)
    {
        InitializeComponent();
        DataContext = _sprites = sprites;
        (_artRoot, _animationTime, _import) = (artRoot, animationTime, import);
        ZoomBar.DataContext = _zoom;
        _zooming = new ZoomedScroller(ImageScroller, DrawSheet, () => _scale) { Zoom = _zoom };
        sprites.PropertyChanged += OnSpritesChanged;
        CompositionTarget.Rendering += OnRendering;
        Closed += OnClosed;
        ShowChosen();
    }

    private void OnSpritesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FieldSpriteCatalogViewModel.Setup))
        {
            ShowChosen();
        }
        else if (e.PropertyName == nameof(FieldSpriteCatalogViewModel.Problem))
        {
            OkButton.IsEnabled = _sprites.Problem is null;
        }
    }

    // The chosen image and its setup: the sheet is read again each time, since an import may have replaced it.
    private void ShowChosen()
    {
        if (_setup is not null)
        {
            _setup.PropertyChanged -= OnSetupChanged;
        }

        _setup = _sprites.Setup;
        if (_setup is not null)
        {
            _setup.PropertyChanged += OnSetupChanged;
        }

        _sheet = _setup is null ? null : SpriteImageFile.Load(_artRoot, _setup.File);
        OkButton.IsEnabled = _sprites.Problem is null;
        DrawSheet();
        DrawPreview();
    }

    private void OnSetupChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FieldSpriteSetupViewModel.FrameWidth) or nameof(FieldSpriteSetupViewModel.FrameHeight))
        {
            DrawSheet();
        }

        DrawPreview();
    }

    // The sheet at the zoom's size per frame, its grid and row numbers over it.
    private void DrawSheet()
    {
        if (_sheet is null || _setup is null)
        {
            SheetImage.Source = SheetMarks.Source = null;
            return;
        }

        var size = (_sheet.PixelWidth, _sheet.PixelHeight);
        _scale = _zoom.ScaleFor(Math.Max(1, _setup.FrameWidth));
        SheetImage.Source = _sheet;
        SheetMarks.Source = SpriteSheetMarks.Draw(size, _setup.FrameWidth, _setup.FrameHeight, _scale);
        SheetImage.Width = SheetMarks.Width = size.PixelWidth;
        SheetImage.Height = SheetMarks.Height = size.PixelHeight;
        ImageView.LayoutTransform = new ScaleTransform(_scale, _scale);
    }

    // Redrawn only when a facing's frame changed, as the composer's canvas does.
    private void OnRendering(object? sender, EventArgs e)
    {
        if (_previewSheet is { } sheet && _preview.HasMovedOn(sheet, _animationTime.Elapsed))
        {
            DrawPreview();
        }
    }

    // After a change: the record is worked out once here, never on the tick.
    private void DrawPreview()
    {
        _previewSheet = _setup is { Problem: null } && _sheet is not null ? _setup.ToSheet() : null;
        Preview.Source = _previewSheet is { } sheet ? _preview.Draw(sheet, _sheet!, _animationTime.Elapsed) : null;
    }

    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import character sheets",
            Filter = "PNG images (*.png)|*.png",
            Multiselect = true,
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var result = _import(dialog.FileNames);
        ImportStatusText.Text = result.Summary;
        _sprites.RefreshAfterImport(result.Imported);
    }

    private void OnCopySetupClicked(object sender, RoutedEventArgs e)
    {
        if (CopySourceBox.SelectedItem is string file)
        {
            _sprites.CopySetupFrom(file);
        }
    }

    // OK is greyed while a problem shows, so a click always applies.
    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnClosed(object? sender, EventArgs e)
    {
        CompositionTarget.Rendering -= OnRendering;
        _zooming.Zoom = null;
        _sprites.PropertyChanged -= OnSpritesChanged;
        if (_setup is not null)
        {
            _setup.PropertyChanged -= OnSetupChanged;
        }
    }
}
