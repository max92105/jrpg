using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The Cinematics window (ADR 0046): OK keeps the session's cinematics when nothing would be refused, Cancel drops
/// them. The problem line shows live and greys OK while it shows; the preview shows the chosen still's picture, and a
/// click on it sets where its zoom heads. Play runs the cinematic on the render loop's clock; closing the window stops
/// the clock and the preview with it.
/// </summary>
internal sealed partial class CinematicCatalogWindow : Window
{
    private readonly RenderTick _renderTick;
    /// <summary>
    /// The preview's size: the game's 1920 x 1080 screen at a quarter (the plan's tunable "Preview size"). Doubles, as
    /// the XAML sets a <c>Width</c> from them and WPF refuses an int there, failing as the window opens.
    /// </summary>
    public const double PreviewWidth = 480;

    public const double PreviewHeight = 270;

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
        _renderTick = new RenderTick(OnRendering);
        DataContext = cinematics;
        _preview = cinematics.Preview;
        _preview.PropertyChanged += OnPreviewChanged;
        Closed += (_, _) => StopTheClock();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;

    // The still's panel is disabled with no still chosen, and Play's layer takes clicks while it shows, so a click here
    // always lands on the chosen still's picture.
    private void OnStillPictureClicked(object sender, MouseButtonEventArgs e)
    {
        var picture = (FrameworkElement)sender;
        var at = e.GetPosition(picture);
        ((CinematicStillRow)picture.DataContext).SetFocusAt(at.X / PreviewWidth, at.Y / PreviewHeight);
    }

    private void OnPreviewChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CinematicPreview.IsPlaying))
        {
            return;
        }

        StopTheClock();
        if (_preview.IsPlaying && IsVisible)
        {
            _renderTick.Start();
        }
    }

    // The preview's clock is the render loop's: one tick per frame while Play runs, none otherwise or once closed.
    private void StopTheClock()
    {
        _renderTick.Stop();
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
