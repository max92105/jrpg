using System.Windows.Controls;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// The Statuses page (ADR 0045, decisions 976 and 984): the statuses left, the chosen status's boxes, the skills
/// granting it and its part rows right. Its host shows the problem line and keeps or drops the whole session.
/// </summary>
internal sealed partial class StatusCatalogPage : UserControl
{
    public StatusCatalogPage(StatusCatalogViewModel statuses)
    {
        InitializeComponent();
        DataContext = statuses;
    }
}
