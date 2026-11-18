using System.Windows.Controls;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The Balance table page (decisions 992, 994): read only; the level, modifier and Power boxes with the line naming one
/// that can't be read, then the Skills, Classes and enemies, and Tiers tabs. Its host's Apply records nothing.
/// </summary>
internal sealed partial class BalanceTablePage : UserControl
{
    public BalanceTablePage(BalanceTableViewModel table)
    {
        InitializeComponent();
        DataContext = table;
    }
}
