namespace Jrpg.Game.Battle;

/// <summary>Where the cursor is in a list that opens in place of the commands: the skills or the items.</summary>
internal interface IBattleListChoice
{
    /// <summary>How many rows show at once: as many as the command list, so the panel keeps its size.</summary>
    const int VisibleRows = 5;

    int SelectedIndex { get; }

    /// <summary>The index of the first row in the visible window.</summary>
    int FirstVisible { get; }

    void MoveUp();

    void MoveDown();
}
