using Jrpg.Core.Battle;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The acting member's skills, open in place of the commands. Every row can be selected, so a greyed skill can still be
/// read and say why it can't be used (decision 105).
/// </summary>
internal sealed class BattleSkillChoice : IBattleListChoice
{
    private readonly ListCursor _cursor;
    private readonly bool[] _isUsable;
    private readonly ResourceDefinition?[] _lacking;

    /// <param name="battle">On a party member's turn; that member knows at least one skill.</param>
    public BattleSkillChoice(BattleState battle)
    {
        Actor = battle.CurrentActor;
        Skills = Actor.Skills;
        _isUsable = [.. Skills.Select(battle.CanUse)];
        _lacking = [.. Skills.Select(battle.LackingResourceFor)];
        _cursor = new ListCursor(Skills.Count, _ => true, IBattleListChoice.VisibleRows);
    }

    /// <summary>The member whose skills these are; their class resources name the costs.</summary>
    public Combatant Actor { get; }

    /// <summary>The actor's skills in the order they learnt them.</summary>
    public IReadOnlyList<SkillDefinition> Skills { get; }

    public SkillDefinition Selected => Skills[_cursor.Selected];

    public int SelectedIndex => _cursor.Selected;

    /// <summary>The index of the first row in the visible window.</summary>
    public int FirstVisible => _cursor.FirstVisible;

    /// <summary>Whether Confirm was pressed on a skill the actor can't pay for, until the selection moves.</summary>
    public bool IsSayingCannotAfford { get; private set; }

    /// <summary>Whether the skill at <paramref name="index"/> can be used now; the rest are greyed.</summary>
    public bool IsUsable(int index) => _isUsable[index];

    /// <returns>
    /// The class resource the actor is short of for the skill at <paramref name="index"/>; null when it can be
    /// paid.
    /// </returns>
    public ResourceDefinition? LackingResourceAt(int index) => _lacking[index];

    public void MoveUp()
    {
        _cursor.MoveUp();
        IsSayingCannotAfford = false;
    }

    public void MoveDown()
    {
        _cursor.MoveDown();
        IsSayingCannotAfford = false;
    }

    public void SayCannotAfford() => IsSayingCannotAfford = true;
}
