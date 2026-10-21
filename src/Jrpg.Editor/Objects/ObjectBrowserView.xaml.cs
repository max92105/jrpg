using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects;

/// <summary>Lists the <see cref="ObjectBrowserViewModel"/>'s objects by category, with a preview of the chosen one.</summary>
internal sealed partial class ObjectBrowserView : UserControl
{
    private ObjectBrowserViewModel? _browser;

    public ObjectBrowserView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>Draws the previews; set by the window, which shares it with the map.</summary>
    public MapDrawing? Drawing { get; set; }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _browser?.PropertyChanged -= OnBrowserChanged;
        _browser = DataContext as ObjectBrowserViewModel;
        _browser?.PropertyChanged += OnBrowserChanged;
        ShowPreview();
    }

    /// <summary>Draws the chosen object again; the window calls it when a catalog change (a re-cut sheet) changes its art.</summary>
    public void ShowPreview() =>
        Preview.Source = _browser?.SelectedObject is { } definition && Drawing is not null ? Drawing.DrawObject(definition) : null;

    private void OnBrowserChanged(object? sender, PropertyChangedEventArgs e) => ShowPreview();
}
