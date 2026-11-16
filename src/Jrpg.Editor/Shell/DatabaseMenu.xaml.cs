using System.Windows.Controls;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The main window's Database menu: one entry per catalog editor, each tagged with the title <see cref="MainWindow"/>
/// opens it by.
/// </summary>
internal sealed partial class DatabaseMenu : MenuItem
{
    public DatabaseMenu() => InitializeComponent();
}
