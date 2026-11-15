using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The Cinematics window (ADR 0046): OK keeps the session's cinematics when nothing would be refused, Cancel drops
/// them. The problem line shows live and greys OK while it shows; the preview shows the chosen still's picture. Play
/// runs the cinematic on the render loop's clock, stopped when the window closes.
/// </summary>
internal sealed partial class CinematicCatalogWindow : Window
{
    /// <summary>
    /// The preview's size: the game's 1920 x 1080 screen at a quarter (the plan's tunable "Preview size").
    /// </summary>
    public const int PreviewWidth = 480;

    public const int PreviewHeight = 270;

    private readonly CinematicPreview _preview;

    // The render time of the last frame the preview advanced on; null until the first since Play.
    private TimeSpan? _lastFrame;

    /// <param name="cinematics">The window's state, from <see cref="CinematicEditingViewModel.Start"/>.</param>
    /// <param name="loadPicture">
    /// Decodes a picture under the cinematics folder for the preview; null when it can't.
    /// </param>
    public CinematicCatalogWindow(CinematicCatalogViewModel cinematics, Func<string, BitmapSource?> loadPicture)
    {
        Resources.Add("PictureOf", new CinematicPictureConverter(loadPicture));
        InitializeComponent();
        DataContext = cinematics;
        _preview = cinematics.Preview;
        _preview.PropertyChanged += OnPreviewChanged;
        Closed += (_, _) => StopTheClock();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnPreviewChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CinematicPreview.IsPlaying))
        {
            return;
        }

        StopTheClock();
        if (_preview.IsPlaying && IsVisible)
        {
            CompositionTarget.Rendering += OnRendering;
        }
    }

    // The preview's clock is the render loop's: one tick per frame while Play runs, none otherwise or once closed.
    private void StopTheClock()
    {
        CompositionTarget.Rendering -= OnRendering;
        _lastFrame = null;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = ((RenderingEventArgs)e).RenderingTime;
        if (_lastFrame is { } last && now > last)
        {
            _preview.Advance(now - last);
        }

        _lastFrame = now;
    }
}
