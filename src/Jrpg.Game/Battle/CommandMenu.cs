using System.Collections.Immutable;
using Jrpg.Core.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The acting party member's command choice, from the list down to a target (legacy <c>CommandPanel</c>). Attack opens
/// the target cursor on Core's legal targets; Defend and Run act at once. Cancel on the list does nothing: there is no
/// menu in battle (decision 50).
/// </summary>
internal sealed class CommandMenu
{
    private readonly ListCursor _cursor = new(Entries.Length, index => IsEnabled(Entries[index]));
    private BattleState? _battle;

    public static ImmutableArray<CommandEntry> Entries { get; } =
        [CommandEntry.Attack, CommandEntry.Skills, CommandEntry.Items, CommandEntry.Defend, CommandEntry.Run];

    public CommandEntry Selected => Entries[_cursor.Selected];

    /// <summary>The target being picked after Attack; null while the list is open.</summary>
    public BattleTargetCursor? Targets { get; private set; }

    /// <summary>Skills and items stay greyed until the battle can use them (plan 2026-10-20-battle, B6a and B6b).</summary>
    public static bool IsEnabled(CommandEntry entry) => entry is not (CommandEntry.Skills or CommandEntry.Items);

    /// <summary>Begins a party turn: the list open, on Attack.</summary>
    public void Start(BattleState battle)
    {
        _battle = battle;
        _cursor.Select(0);
        Targets = null;
    }

    /// <returns>The command chosen this update, or null.</returns>
    /// <exception cref="InvalidOperationException">No turn was started.</exception>
    public BattleCommand? Update(InputFrame input)
    {
        var battle = _battle ?? throw new InvalidOperationException("Start a turn before choosing a command.");
        return Targets is null ? ChooseFromList(battle, input) : ChooseTarget(Targets, input);
    }

    private BattleCommand? ChooseFromList(BattleState battle, InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        switch (Selected)
        {
            case CommandEntry.Attack:
                Targets = new BattleTargetCursor(battle.TargetChoiceForAttack(), battle.CurrentActor);
                return null;
            case CommandEntry.Defend:
                return new DefendCommand();
            case CommandEntry.Run:
                return new FleeCommand();
            default:
                throw new InvalidOperationException($"{Selected} can't be chosen yet.");
        }
    }

    // Up and Left go back along the formation, Down and Right forward (decision 56).
    private AttackCommand? ChooseTarget(BattleTargetCursor targets, InputFrame input)
    {
        if (input.IsPressed(GameButton.Up) || input.IsPressed(GameButton.Left))
        {
            targets.MovePrevious();
        }

        if (input.IsPressed(GameButton.Down) || input.IsPressed(GameButton.Right))
        {
            targets.MoveNext();
        }

        if (input.IsPressed(GameButton.Cancel))
        {
            Targets = null;
            return null;
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        Targets = null;
        return new AttackCommand(targets.Selected[0]);
    }
}
