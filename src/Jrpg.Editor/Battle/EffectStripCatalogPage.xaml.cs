using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips page (decision 659, ADR 0045): the effects images on the left, the chosen strip's boxes and
/// preview on the right. Its host shows the problem line and keeps or drops the session. The chosen strip plays in the
/// shared preview (<see cref="FramePreview"/>, decision 986) at its typed frame time, its hit frame marked, while the
/// page is shown, so its pace is tuned by eye; an image is read once per page, again once an import replaced it.
/// Import... under the list copies PNGs and GIFs into the folder, a GIF's pace typed while it plays (decision 989);
/// Strip from sheet cells... under it opens the tool that picks a sheet's cells (decision 990).
/// </summary>
internal sealed partial class EffectStripCatalogPage : UserControl
{
    private readonly EffectStripCatalogViewModel _strips;
    private readonly Func<string, BitmapSource?> _loadImage;
    private readonly Dictionary<string, BitmapSource?> _images = new(StringComparer.Ordinal);
    private readonly Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> _import;
    private readonly Action<Window> _openStripFromCells;

    /// <param name="strips">The page's state, from <see cref="EffectStripEditingViewModel.Start"/>.</param>
    /// <param name="loadImage">Decodes an effects image at its own size; null when it can't.</param>
    /// <param name="import">
    /// The shell's import: copies the chosen images into the effects folder, a GIF as a strip by the options, and says
    /// what it did.
    /// </param>
    /// <param name="openStripFromCells">Shows Strip from sheet cells over the given window, the page's own.</param>
    public EffectStripCatalogPage(
        EffectStripCatalogViewModel strips,
        Func<string, BitmapSource?> loadImage,
        Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> import,
        Action<Window> openStripFromCells)
    {
        InitializeComponent();
        DataContext = _strips = strips;
        (_loadImage, _import, _openStripFromCells) = (loadImage, import, openStripFromCells);
        strips.PropertyChanged += OnStripsChanged;
        ShowChosenStrip();
    }

    private void OnStripsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EffectStripCatalogViewModel.SelectedStrip))
        {
            ShowChosenStrip();
        }
    }

    // Only a new choice hands the preview new frames, which start it over: a typed frame time or hit frame reaches it
    // through its bindings and keeps the shown frame. A missing image shows nothing.
    private void ShowChosenStrip() =>
        StripPreview.Frames = _strips.SelectedStrip is { FrameCount: { } frameCount } strip && ImageOf(strip.File) is { } image
            ? PreviewFrames.OfStrip(image, frameCount)
            : null;

    // A replaced image is read again before its row is chosen, so the preview plays the new frames.
    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (ArtImageFileDialog.Choose(owner, "Import effect strips", ArtFolders.Effects) is not { } paths
            || GifImportWindow.AskFor(owner, paths, GifImportOptionsViewModel.ForEffectStrips) is not { } options)
        {
            return;
        }

        var result = _import(paths, options);
        ImportStatusText.Text = EffectStripCatalogViewModel.ImportLineOf(result);
        foreach (var file in result.Imported)
        {
            _images.Remove(file);
        }

        _strips.TakeImport(result);
    }

    private void OnStripFromCellsClicked(object sender, RoutedEventArgs e) => _openStripFromCells(Window.GetWindow(this));

    private BitmapSource? ImageOf(string file)
    {
        if (!_images.TryGetValue(file, out var image))
        {
            image = _loadImage(file);
            _images.Add(file, image);
        }

        return image;
    }
}
