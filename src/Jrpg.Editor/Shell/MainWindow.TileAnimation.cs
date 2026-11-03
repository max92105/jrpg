using System.Windows;
using Microsoft.Win32;

namespace Jrpg.Editor.Shell;

/// <summary>The main window's animated tiles (ADR 0039): Tiles > Import animations.</summary>
internal sealed partial class MainWindow
{
    private void OnImportAnimationsClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Import animations", Filter = "Tiled tilesets (*.tsx)|*.tsx" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AnimationImport?.Import(dialog.FileName);
        }
    }
}
