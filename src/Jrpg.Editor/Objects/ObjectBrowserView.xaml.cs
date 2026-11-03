using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects;

/// <summary>Lists the <see cref="ObjectBrowserViewModel"/>'s objects by category, with a preview of the chosen one.</summary>
internal sealed partial class ObjectBrowserView : UserControl
{
    private ObjectBrowserViewModel? _browser;
    private TileAnimationTime? _animationTime;

    public ObjectBrowserView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>Draws the previews; set by the window, which shares it with the map.</summary>
    public MapDrawing? Drawing { get; set; }

    /// <summary>
    /// The editor's animation time, which an animated preview follows (decision 470); set by the window.
    /// </summary>
    public TileAnimationTime? AnimationTime
    {
        get => _animationTime;
        set
        {
            _animationTime?.FramesChanged -= OnFramesChanged;
            _animationTime = value;
            _animationTime?.FramesChanged += OnFramesChanged;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _browser?.PropertyChanged -= OnBrowserChanged;
        _browser = DataContext as ObjectBrowserViewModel;
        _browser?.PropertyChanged += OnBrowserChanged;
        ShowPreview();
    }

    /// <summary>Draws the chosen object again; the window calls it when a catalog change (a re-cut sheet) changes its art.</summary>
    public void ShowPreview() =>
        Preview.Source = _browser?.SelectedObject is { } definition && Drawing is not null ? Drawing.DrawObject(definition, _animationTime?.Elapsed ?? TimeSpan.Zero) : null;

    private void OnBrowserChanged(object? sender, PropertyChangedEventArgs e) => ShowPreview();

    // A still object's preview never changes, so only an animated one is drawn again.
    private void OnFramesChanged(object? sender, EventArgs e)
    {
        if (_browser?.SelectedObject is { } definition && Drawing is not null && Drawing.IsAnimated(definition))
        {
            ShowPreview();
        }
    }
}
