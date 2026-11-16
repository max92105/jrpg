using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips page (decision 659, ADR 0045): the effects images on the left, the chosen strip's boxes and
/// preview on the right. Its host shows the problem line and keeps or drops the session. The chosen strip plays in the
/// shared preview (<see cref="FramePreview"/>, decision 986) at its typed frame time, its hit frame marked, while the
/// page is shown, so its pace is tuned by eye; an image is read once per page.
/// </summary>
internal sealed partial class EffectStripCatalogPage : UserControl
{
    private readonly EffectStripCatalogViewModel _strips;
    private readonly Func<string, BitmapSource?> _loadImage;
    private readonly Dictionary<string, BitmapSource?> _images = new(StringComparer.Ordinal);

    /// <param name="strips">The page's state, from <see cref="EffectStripEditingViewModel.Start"/>.</param>
    /// <param name="loadImage">Decodes an effects image at its own size; null when it can't.</param>
    public EffectStripCatalogPage(EffectStripCatalogViewModel strips, Func<string, BitmapSource?> loadImage)
    {
        InitializeComponent();
        DataContext = _strips = strips;
        _loadImage = loadImage;
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
