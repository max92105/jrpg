using System.Windows;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>
/// The map settings dialog. OK hands the edited settings to the window's apply step, which returns false when they
/// weren't applied (the size is invalid or the user kept the old one), and the dialog then stays open.
/// </summary>
internal sealed partial class MapSettingsWindow : Window
{
    private readonly MapSettingsViewModel _settings;
    private readonly Func<MapSettingsViewModel, bool> _apply;

    /// <param name="settings">The open map's settings, edited here.</param>
    /// <param name="apply">Applies them to the map; false when it didn't.</param>
    public MapSettingsWindow(MapSettingsViewModel settings, Func<MapSettingsViewModel, bool> apply)
    {
        InitializeComponent();
        DataContext = _settings = settings;
        _apply = apply;
        Loaded += (_, _) => NameBox.Focus();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_apply(_settings))
        {
            DialogResult = true;
        }
    }
}
