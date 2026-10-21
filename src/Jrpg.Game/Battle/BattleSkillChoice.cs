using Jrpg.Core.Battle;
using Jrpg.Core.Skills;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The acting member's skills, open in place of the commands. Every row can be selected, so a greyed skill can still be
/// read and say why it can't be used (decision 105).
/// </summary>
internal sealed class BattleSkillChoice
{
    /// <summary>How many rows show at once: as many as the command list, so the panel keeps its size.</summary>
    public const int VisibleRows = 5;

    private readonly ListCursor _cursor;
    private readonly bool[] _isUsable;

    /// <param name="battle">On a party member's turn; that member knows at least one skill.</param>
    public BattleSkillChoice(BattleState battle)
    {
        Skills = battle.CurrentActor.Skills;
        _isUsable = [.. Skills.Select(battle.CanUse)];
        _cursor = new ListCursor(Skills.Count, _ => true, VisibleRows);
    }

    /// <summary>The actor's skills in the order they learnt them.</summary>
    public IReadOnlyList<SkillDefinition> Skills { get; }

    public SkillDefinition Selected => Skills[_cursor.Selected];

    public int SelectedIndex => _cursor.Selected;

    /// <summary>The index of the first row in the visible window.</summary>
    public int FirstVisible => _cursor.FirstVisible;

    /// <summary>Whether Confirm was pressed on a skill the actor lacks the MP for, until the selection moves.</summary>
    public bool IsSayingNotEnoughMp { get; private set; }

    /// <summary>Whether the skill at <paramref name="index"/> can be used now; the rest are greyed.</summary>
    public bool IsUsable(int index) => _isUsable[index];

    public void MoveUp()
    {
        _cursor.MoveUp();
        IsSayingNotEnoughMp = false;
    }

    public void MoveDown()
    {
        _cursor.MoveDown();
        IsSayingNotEnoughMp = false;
    }

    public void SayNotEnoughMp() => IsSayingNotEnoughMp = true;
}
