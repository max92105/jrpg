using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Core.Maps;
using Jrpg.Editor.Hosting;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips page (decision 659, ADR 0045): the effects images on the left, the chosen strip's boxes and
/// preview on the right. Its host shows the problem line and keeps or drops the session. The chosen strip plays in the
/// preview at its typed frame time on the editor's animation clock, one square frame of the image at a time, while the
/// page is shown, so its pace is tuned by eye; an image is read once per page.
/// </summary>
internal sealed partial class EffectStripCatalogPage : UserControl
{
    private readonly RenderTick _renderTick;
    private readonly EffectStripCatalogViewModel _strips;
    private readonly Func<string, BitmapSource?> _loadImage;
    private readonly TileAnimationTime _animationTime;
    private readonly Dictionary<string, BitmapSource?> _images = new(StringComparer.Ordinal);
    private EffectStripViewModel? _watched;

    // Read again on every change to the chosen strip, so a render tick only works out its frame.
    private TileAnimation? _loop;
    private BitmapSource? _image;
    private int? _shownFrame;

    /// <param name="strips">The page's state, from <see cref="EffectStripEditingViewModel.Start"/>.</param>
    /// <param name="loadImage">Decodes an effects image at its own size; null when it can't.</param>
    /// <param name="animationTime">The editor's animation clock, which the preview plays on.</param>
    public EffectStripCatalogPage(
        EffectStripCatalogViewModel strips, Func<string, BitmapSource?> loadImage, TileAnimationTime animationTime)
    {
        InitializeComponent();
        _renderTick = new RenderTick(OnRendering);
        DataContext = _strips = strips;
        (_loadImage, _animationTime) = (loadImage, animationTime);
        strips.PropertyChanged += OnStripsChanged;
        WatchChosenStrip();
        _renderTick.RunWhileLoaded(this);
    }

    private void OnStripsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EffectStripCatalogViewModel.SelectedStrip))
        {
            WatchChosenStrip();
        }
    }

    private void WatchChosenStrip()
    {
        if (_watched is not null)
        {
            _watched.PropertyChanged -= OnChosenStripChanged;
        }

        _watched = _strips.SelectedStrip;
        if (_watched is not null)
        {
            _watched.PropertyChanged += OnChosenStripChanged;
        }

        RestartPreview();
    }

    // A typed box moves the pace or the hit: the shown frame and its line are worked out again.
    private void OnChosenStripChanged(object? sender, PropertyChangedEventArgs e) => RestartPreview();

    private void RestartPreview()
    {
        _loop = _watched?.PreviewLoop;
        _image = _watched is null ? null : ImageOf(_watched.File);
        _shownFrame = null;
        ShowFrame();
    }

    // WPF raises Rendering once per frame it draws; the image is cut again only when the strip moved to another frame.
    private void OnRendering(object? sender, EventArgs e) => ShowFrame();

    // A frame time that isn't a number holds the first frame; a missing image shows nothing.
    private void ShowFrame()
    {
        var frame = _loop?.FrameAt(_animationTime.Elapsed) ?? 0;
        if (frame == _shownFrame)
        {
            return;
        }

        _shownFrame = frame;
        var side = _image?.PixelHeight ?? 0;
        var isInside = side > 0 && (frame + 1) * side <= _image!.PixelWidth;
        StripPreview.Source = isInside ? new CroppedBitmap(_image, new Int32Rect(frame * side, 0, side, side)) : null;
        FrameText.Text = isInside ? _watched!.FrameLineAt(frame) : "";
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
