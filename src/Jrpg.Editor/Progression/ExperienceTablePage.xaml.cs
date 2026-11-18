using System.Windows.Controls;

namespace Jrpg.Editor.Progression;

/// <summary>
/// The Experience page (decision 956): one table, so no list of records; the max level, each level step with its
/// experience box and total to reach, Add level and Remove last level, and the note on a max level below the saved one.
/// Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class ExperienceTablePage : UserControl
{
    public ExperienceTablePage(ExperienceTableViewModel table)
    {
        InitializeComponent();
        DataContext = table;
    }
}
