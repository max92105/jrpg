using System.Windows.Controls;

namespace Jrpg.Editor.Skills;

/// <summary>
/// The Skills page (ADR 0045, decision 976): the skills left, the chosen skill's texts, files and users right. Its host
/// shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class SkillCatalogPage : UserControl
{
    public SkillCatalogPage(SkillCatalogViewModel skills)
    {
        InitializeComponent();
        DataContext = skills;
    }
}
