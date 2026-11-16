using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Hosting;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// The Character sprites page (decisions 568-570): the images on the left, the chosen one in the middle with its frame
/// grid, its setup on the right with the four facings walking at the editor's animation time while the page is shown.
/// While a facing picks, a click on the image adds the frame under it, and the image numbers that facing's cells
/// (decision 772). Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class FieldSpritePage : UserControl
{
    private readonly RenderTick _renderTick;
    private readonly FieldSpriteCatalogViewModel _sprites;
    private readonly string _artRoot;
    private readonly TileAnimationTime _animationTime;
    private readonly Func<IReadOnlyList<string>, SheetImportResult> _import;
    private readonly CellZoom _zoom = new((int)Maps.MapDrawing.CellPixels);
    private readonly WalkPreview _preview = new();
    private FieldSpriteSetupViewModel? _setup;
    private BitmapSource? _sheet;
    private double _scale = 1;

    // The chosen setup's record while it has no problem, built once per change so a render tick only compares cells.
    private FieldSpriteSheet? _previewSheet;

    /// <param name="sprites">The page's state, from <see cref="FieldSpriteEditingViewModel.Start"/>.</param>
    /// <param name="artRoot">The <c>art</c> folder, where the images are read from.</param>
    /// <param name="animationTime">The editor's animation time, which the preview walks on.</param>
    /// <param name="import">
    /// Copies chosen images into the field-sprite art and says what it did; the shell drops the replaced pictures.
    /// </param>
    public FieldSpritePage(
        FieldSpriteCatalogViewModel sprites,
        string artRoot,
        TileAnimationTime animationTime,
        Func<IReadOnlyList<string>, SheetImportResult> import)
    {
        InitializeComponent();
        _renderTick = new RenderTick(OnRendering);
        DataContext = _sprites = sprites;
        (_artRoot, _animationTime, _import) = (artRoot, animationTime, import);
        ZoomBar.DataContext = _zoom;
        _ = new ZoomedScroller(ImageScroller, DrawSheet, () => _scale) { Zoom = _zoom };
        sprites.PropertyChanged += OnSpritesChanged;
        _renderTick.RunWhileLoaded(this);
        ShowChosen();
    }

    private void OnSpritesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FieldSpriteCatalogViewModel.Setup))
        {
            ShowChosen();
        }
    }

    // The chosen image and its setup: the sheet is read again each time, since an import may have replaced it.
    private void ShowChosen()
    {
        if (_setup is not null)
        {
            _setup.PropertyChanged -= OnSetupChanged;
            _setup.Walk.PropertyChanged -= OnWalkChanged;
        }

        _setup = _sprites.Setup;
        if (_setup is not null)
        {
            _setup.PropertyChanged += OnSetupChanged;
            _setup.Walk.PropertyChanged += OnWalkChanged;
        }

        _sheet = _setup is null ? null : SpriteImageFile.Load(_artRoot, _setup.File);
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

    // The picking facing never reaches the setup's own notices (it changes nothing in the record), so the marks listen
    // to the walk itself.
    private void OnWalkChanged(object? sender, PropertyChangedEventArgs e) => DrawPickMarks();

    // The sheet at the zoom's size per frame, its grid, row and column numbers and the pick marks over it.
    private void DrawSheet()
    {
        if (_sheet is null || _setup is null)
        {
            SheetImage.Source = SheetMarks.Source = PickMarks.Source = null;
            return;
        }

        var size = (_sheet.PixelWidth, _sheet.PixelHeight);
        _scale = _zoom.ScaleFor(Math.Max(1, _setup.FrameWidth));
        SheetImage.Source = _sheet;
        SheetMarks.Source = SpriteSheetMarks.Draw(size, _setup.FrameWidth, _setup.FrameHeight, _scale);
        SheetImage.Width = SheetMarks.Width = PickMarks.Width = size.PixelWidth;
        SheetImage.Height = SheetMarks.Height = PickMarks.Height = size.PixelHeight;
        ImageView.LayoutTransform = new ScaleTransform(_scale, _scale);
        DrawPickMarks();
    }

    // Nothing while no facing picks or the cut gives no frame.
    private void DrawPickMarks()
    {
        var hasFrames = _setup is { FrameWidth: >= 1, FrameHeight: >= 1 };
        PickMarks.Source = _sheet is not null && hasFrames && _setup is { Walk.PickingFacing: { } picking }
            ? SequenceMarks.Draw(
                (_sheet.PixelWidth, _sheet.PixelHeight), (_setup.FrameWidth, _setup.FrameHeight), picking.Cells, _scale)
            : null;
    }

    // The image is sized in its own pixels under the zoom's layout transform, so the point is in image pixels.
    private void OnImageClicked(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(SheetImage);
        _setup?.PickCellAt(point.X, point.Y);
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

    // The dialog hosting the page owns the file dialog (the mode bar plan's page recipe).
    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var paths = ArtImageFileDialog.Choose(Window.GetWindow(this), "Import character sheets", ArtFolders.FieldSprites);
        if (paths is null)
        {
            return;
        }

        var result = _import(paths);
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
}
