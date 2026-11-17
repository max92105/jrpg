using System.Windows.Controls;

namespace Jrpg.Editor.Party;

/// <summary>
/// The Classes page's Resources tab (decision 950), on the chosen <see cref="ClassViewModel"/>: its resources left with
/// Add, Up, Down and Remove, the chosen resource's id, name, color with a swatch, lasting box, amounts, gains and its
/// four rules, each ticked on with its boxes (decision 951), right.
/// </summary>
internal sealed partial class ClassResourcesView : UserControl
{
    public ClassResourcesView() => InitializeComponent();
}
