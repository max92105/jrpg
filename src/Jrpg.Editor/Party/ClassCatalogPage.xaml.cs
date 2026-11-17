using System.Windows.Controls;

namespace Jrpg.Editor.Party;

/// <summary>
/// The Classes page (decisions 942, 945): the classes left, the chosen class's tabs right, Stats its texts and its base
/// and growth boxes. Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class ClassCatalogPage : UserControl
{
    public ClassCatalogPage(ClassCatalogViewModel classes)
    {
        InitializeComponent();
        DataContext = classes;
    }
}
