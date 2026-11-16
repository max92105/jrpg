using System.Windows.Controls;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Enemies page's Tactics tab (decision 849), on the chosen enemy's <see cref="EnemyTacticsViewModel"/>: its lists
/// left, the chosen list's rules right with the chosen rule's boxes under them.
/// </summary>
internal sealed partial class EnemyTacticsView : UserControl
{
    public EnemyTacticsView() => InitializeComponent();
}
