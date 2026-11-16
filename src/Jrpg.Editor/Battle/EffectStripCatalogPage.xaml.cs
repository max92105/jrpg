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
/// Strip from sheet cells... under it opens the tool that picks a sheet's cells (decision 990); the strip its OK writes
/// enters the list as an import's.
/// </summary>
internal sealed partial class EffectStripCatalogPage : UserControl
{
    private readonly EffectStripCatalogViewModel _strips;
    private readonly Func<string, BitmapSource?> _loadImage;
    private readonly Dictionary<string, BitmapSource?> _images = new(StringComparer.Ordinal);
    private readonly Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> _import;
    private readonly Func<Window, SheetImportResult?> _makeStripFromCells;

    /// <param name="strips">The page's state, from <see cref="EffectStripEditingViewModel.Start"/>.</param>
    /// <param name="loadImage">Decodes an effects image at its own size; null when it can't.</param>
    /// <param name="import">
    /// The shell's import: copies the chosen images into the effects folder, a GIF as a strip by the options, and says
    /// what it did.
    /// </param>
    /// <param name="makeStripFromCells">
    /// Shows Strip from sheet cells over the given window, the page's own, and says what its OK wrote into the effects
    /// folder; null on Cancel.
    /// </param>
    public EffectStripCatalogPage(
        EffectStripCatalogViewModel strips,
        Func<string, BitmapSource?> loadImage,
        Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> import,
        Func<Window, SheetImportResult?> makeStripFromCells)
    {
        InitializeComponent();
        DataContext = _strips = strips;
        (_loadImage, _import, _makeStripFromCells) = (loadImage, import, makeStripFromCells);
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

    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (ArtImageFileDialog.Choose(owner, "Import effect strips", ArtFolders.Effects) is not { } paths
            || GifImportWindow.AskFor(owner, paths, GifImportOptionsViewModel.ForEffectStrips) is not { } options)
        {
            return;
        }

        TakeImport(_import(paths, options));
    }

    private void OnStripFromCellsClicked(object sender, RoutedEventArgs e)
    {
        if (_makeStripFromCells(Window.GetWindow(this)) is { } written)
        {
            TakeImport(written);
        }
    }

    // A replaced image is read again before its row is chosen, so the preview plays the new frames.
    private void TakeImport(SheetImportResult result)
    {
        ImportStatusText.Text = EffectStripCatalogViewModel.ImportLineOf(result);
        foreach (var file in result.Imported)
        {
            _images.Remove(file);
        }

        _strips.TakeImport(result);
    }

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
