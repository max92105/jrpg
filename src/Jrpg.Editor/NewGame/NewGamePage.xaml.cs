using System.Windows.Controls;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// The New game page (decision 968): one record, so no list of them; its leader choices left, in the leader screen's
/// order, and right its starting items with their quantities, its starting gold, its start (map, cell position, facing)
/// and the player's field sprite. Its host shows the problem line and keeps or drops the session.
/// </summary>
internal sealed partial class NewGamePage : UserControl
{
    public NewGamePage(NewGameViewModel newGame)
    {
        InitializeComponent();
        DataContext = newGame;
    }
}
