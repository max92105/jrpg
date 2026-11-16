using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Skills;

/// <summary>
/// The Skills page (ADR 0045, decision 976): the skills left, the chosen skill's texts, files, sections and users
/// right. Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class SkillCatalogPage : UserControl
{
    public SkillCatalogPage(SkillCatalogViewModel skills)
    {
        InitializeComponent();
        DataContext = skills;
    }

    // Selected on focus, as the Drop tables page's entries are: a trigger on IsKeyboardFocusWithin lets go as soon as
    // Remove takes the focus, so it would act on no row.
    private void OnTypedRowFocused(object sender, KeyboardFocusChangedEventArgs e) => ((ListBoxItem)sender).IsSelected = true;
}
