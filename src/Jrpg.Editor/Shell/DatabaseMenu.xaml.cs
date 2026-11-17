using System.Windows.Controls;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The main window's Database menu: one entry per catalog page, each showing it by its title in the Database mode
/// through <see cref="MainViewModel.ShowDatabasePageCommand"/>, which applies the shown page first.
/// </summary>
internal sealed partial class DatabaseMenu : MenuItem
{
    public DatabaseMenu() => InitializeComponent();
}
