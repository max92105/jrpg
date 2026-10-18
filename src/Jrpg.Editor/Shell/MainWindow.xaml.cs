using System.ComponentModel;
using System.Windows;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window. Turning the open map into WPF images is a view concern, done here, so the view models hold
/// plain data and run in tests without a UI thread (ADR 0012).
/// </summary>
internal sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly MapDrawing? _drawing;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;
        _drawing = viewModel.Catalog is { } catalog ? new MapDrawing(catalog, viewModel.ArtRoot) : null;
        viewModel.PropertyChanged += OnViewModelChanged;
        ShowCurrentMap();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentMap))
        {
            ShowCurrentMap();
        }
    }

    private void ShowCurrentMap() =>
        MapImage.Source = _viewModel.CurrentMap is { } map && _drawing is not null ? _drawing.Draw(map.Map.Layout) : null;
}
