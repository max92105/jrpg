using System.Windows.Controls;

namespace Jrpg.Editor.Loot;

/// <summary>
/// The Drop tables page (ADR 0045, decision 596): the tables left, the chosen table and its entries right. Its host
/// shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class DropTableCatalogPage : UserControl
{
    public DropTableCatalogPage(DropTableCatalogViewModel tables)
    {
        InitializeComponent();
        DataContext = tables;
    }
}
