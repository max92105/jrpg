using System.Windows.Controls;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The main window's status lines, on its view model: the load error, the art warning and the save error.
/// </summary>
internal sealed partial class StatusBand : UserControl
{
    public StatusBand() => InitializeComponent();
}
