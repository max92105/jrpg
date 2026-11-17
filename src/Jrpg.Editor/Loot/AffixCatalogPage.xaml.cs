using System.Windows.Controls;

namespace Jrpg.Editor.Loot;

/// <summary>
/// The Affixes page (decision 884): the pool left, the chosen affix's fields and the gear kinds it rolls on right. Its
/// host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class AffixCatalogPage : UserControl
{
    public AffixCatalogPage(AffixCatalogViewModel affixes)
    {
        InitializeComponent();
        DataContext = affixes;
    }
}
