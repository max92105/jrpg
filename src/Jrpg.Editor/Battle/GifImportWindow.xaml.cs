using System.Windows;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The GIF import's dialog, opened by Import... before a GIF becomes a battle strip: its height and background box. OK
/// is greyed while the height can't be read; Cancel imports nothing.
/// </summary>
internal sealed partial class GifImportWindow : Window
{
    private GifImportWindow(GifImportOptionsViewModel options)
    {
        InitializeComponent();
        DataContext = options;
        Loaded += (_, _) =>
        {
            HeightBox.Focus();
            HeightBox.SelectAll();
        };
    }

    /// <summary>Shows the dialog over its owner.</summary>
    /// <param name="owner">The window the dialog is centered on.</param>
    /// <param name="options">The boxes, edited here.</param>
    /// <returns>Whether OK was clicked, so the import goes on with the options.</returns>
    public static bool Ask(Window owner, GifImportOptionsViewModel options) =>
        new GifImportWindow(options) { Owner = owner }.ShowDialog() == true;

    // OK is greyed while a problem shows, so a click always applies.
    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
