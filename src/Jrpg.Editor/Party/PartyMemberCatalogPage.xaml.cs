using System.Windows.Controls;

namespace Jrpg.Editor.Party;

/// <summary>
/// The Party members page (decision 953): the members left with Add and Remove, the chosen member's boxes right: id,
/// name, class by name, summary, and its three pictures by file over their folders, with a mark for each the folders
/// lack (decision 442). Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class PartyMemberCatalogPage : UserControl
{
    public PartyMemberCatalogPage(PartyMemberCatalogViewModel members)
    {
        InitializeComponent();
        DataContext = members;
    }
}
