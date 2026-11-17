using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Animation;

/// <summary>
/// The shared preview of an animation the editor writes (decision 986): its frames over black, Play or Pause, Step back
/// and Step forward, and the frame line. It reads its host's frame time and hit frame one-way, so the host's typed box
/// is the only one and what plays is what is written. Its rules are <see cref="FramePlayback"/>'s, moved by the render
/// clock from each Loaded to the next Unloaded. A frame fits its picture whole unless the host asks it to cover a fixed
/// area, as the Battle backgrounds page does (cover-fit, as the battle draws a background).
/// </summary>
internal sealed partial class FramePreview : UserControl
{
    public static readonly DependencyProperty FramesProperty = DependencyProperty.Register(
        nameof(Frames), typeof(IReadOnlyList<BitmapSource>), typeof(FramePreview), new PropertyMetadata(null, OnFramesChanged));

    public static readonly DependencyProperty FrameMillisecondsProperty = DependencyProperty.Register(
        nameof(FrameMilliseconds), typeof(int?), typeof(FramePreview), new PropertyMetadata(null, OnFrameMillisecondsChanged));

    public static readonly DependencyProperty HitFrameProperty = DependencyProperty.Register(
        nameof(HitFrame), typeof(int?), typeof(FramePreview), new PropertyMetadata(null, OnHitFrameChanged));

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
        nameof(Stretch), typeof(Stretch), typeof(FramePreview), new PropertyMetadata(Stretch.Uniform, OnStretchChanged));

    public static readonly DependencyProperty PictureHeightProperty = DependencyProperty.Register(
        nameof(PictureHeight), typeof(double), typeof(FramePreview), new PropertyMetadata(double.NaN, OnPictureHeightChanged));

    private readonly FramePlayback _playback = new();

    // Forgotten on Unloaded, so a page shown again goes on from its frame rather than by the time it was hidden.
    private TimeSpan? _lastRenderingTime;

    public FramePreview()
    {
        InitializeComponent();
        Root.DataContext = _playback;
        _playback.PropertyChanged += OnPlaybackChanged;
        new RenderTick(OnRendering).RunWhileLoaded(this);
        Unloaded += (_, _) => _lastRenderingTime = null;
    }

    /// <summary>
    /// The images it plays, from <see cref="PreviewFrames"/>; null or none shows nothing. A new list shows its first
    /// frame, playing, so a host sets it only when what it previews changes.
    /// </summary>
    public IReadOnlyList<BitmapSource>? Frames
    {
        get => (IReadOnlyList<BitmapSource>?)GetValue(FramesProperty);
        set => SetValue(FramesProperty, value);
    }

    /// <summary>The host's typed frame time, in whole milliseconds; null or below 1 holds the shown frame.</summary>
    public int? FrameMilliseconds
    {
        get => (int?)GetValue(FrameMillisecondsProperty);
        set => SetValue(FrameMillisecondsProperty, value);
    }

    /// <summary>
    /// The host's typed hit frame, from 0, which the frame line marks; null while its box isn't a number.
    /// </summary>
    public int? HitFrame
    {
        get => (int?)GetValue(HitFrameProperty);
        set => SetValue(HitFrameProperty, value);
    }

    /// <summary>
    /// How a frame fills the picture: <see cref="Stretch.Uniform"/>, whole inside it (the default), or
    /// <see cref="Stretch.UniformToFill"/>, covering it with the overflow cut evenly on both sides, as the battle draws
    /// a background.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// The picture's height in device-independent pixels, the buttons and frame line under it; unset, the picture takes
    /// what the host's height leaves. A host covering a fixed area sets it with the control's width.
    /// </summary>
    public double PictureHeight
    {
        get => (double)GetValue(PictureHeightProperty);
        set => SetValue(PictureHeightProperty, value);
    }

    // Shown at once, as the playback raises no frame change when the new list starts on the frame the old one showed.
    private static void OnFramesChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        var preview = (FramePreview)target;
        preview._playback.Show(preview.Frames?.Count ?? 0);
        preview.ShowFrame();
    }

    private static void OnFrameMillisecondsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) =>
        ((FramePreview)target)._playback.FrameMilliseconds = (int?)e.NewValue;

    private static void OnHitFrameChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) =>
        ((FramePreview)target)._playback.HitFrame = (int?)e.NewValue;

    private static void OnStretchChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) =>
        ((FramePreview)target).FrameImage.Stretch = (Stretch)e.NewValue;

    private static void OnPictureHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) =>
        ((FramePreview)target).Picture.Height = (double)e.NewValue;

    private void OnPlaybackChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FramePlayback.ShownFrame))
        {
            ShowFrame();
        }
    }

    private void ShowFrame() => FrameImage.Source = Frames is { Count: > 0 } frames ? frames[_playback.ShownFrame] : null;

    // WPF may raise Rendering more than once with one RenderingTime, so only a later time moves the preview; the first
    // tick after a load only sets the clock's start.
    private void OnRendering(object? sender, EventArgs e)
    {
        var now = ((RenderingEventArgs)e).RenderingTime;
        if (_lastRenderingTime is { } last && now > last)
        {
            _playback.Advance(now - last);
        }

        _lastRenderingTime = now;
    }
}
