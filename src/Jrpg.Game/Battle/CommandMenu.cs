using System.Collections.Immutable;
using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The acting party member's command choice, from the list down to a target. Attack opens the target cursor on Core's
/// legal targets, and Move on the actor's neighbors in the formation; Skills and Items open the actor's skills or the
/// party's consumables, and a usable one its targets; Defend and Run act at once. Cancel steps back one level; on the
/// list it does nothing: there is no menu in battle (decision 50).
/// </summary>
internal sealed class CommandMenu
{
    private readonly ListCursor _cursor;
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private BattleState? _battle;
    private bool _hasSkills;
    private bool _hasUsableItem;
    private bool _hasNeighbor;

    /// <param name="inventory">The party's items, as the battle uses them up.</param>
    /// <param name="items">Every item by id, from the catalog.</param>
    public CommandMenu(Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        _inventory = inventory;
        _items = items;
        _cursor = new ListCursor(Entries.Length, index => IsEnabled(Entries[index]));
    }

    public static ImmutableArray<CommandEntry> Entries { get; } =
        [CommandEntry.Attack, CommandEntry.Skills, CommandEntry.Items, CommandEntry.Move, CommandEntry.Defend, CommandEntry.Run];

    public CommandEntry Selected => Entries[_cursor.Selected];

    /// <summary>The actor's skills after Skills, until one is used or Cancel; null while the command list shows.</summary>
    public BattleSkillChoice? Skills { get; private set; }

    /// <summary>The party's consumables after Items, until one is used or Cancel; null while the command list shows.</summary>
    public BattleItemChoice? Items { get; private set; }

    /// <summary>The target being picked after Attack, a skill or an item; null while a list is open.</summary>
    public BattleTargetCursor? Targets { get; private set; }

    /// <summary>
    /// Skills is greyed for a member who knows none, Items when no owned item can be used now, and Move for a member
    /// alone in the line (decision 105).
    /// </summary>
    public bool IsEnabled(CommandEntry entry) => entry switch
    {
        CommandEntry.Skills => _hasSkills,
        CommandEntry.Items => _hasUsableItem,
        CommandEntry.Move => _hasNeighbor,
        _ => true,
    };

    /// <summary>Begins a party turn: the list open, on Attack.</summary>
    public void Start(BattleState battle)
    {
        _battle = battle;
        _hasSkills = battle.CurrentActor.Skills.Count > 0;
        _hasUsableItem = _inventory.Entries.Any(entry => battle.CanUse(_items[entry.ItemId]));
        _hasNeighbor = battle.TargetChoiceForMove().Candidates.Count > 0;
        _cursor.Select(0);
        Skills = null;
        Items = null;
        Targets = null;
    }

    /// <returns>The command chosen this update, or null.</returns>
    /// <exception cref="InvalidOperationException">No turn was started.</exception>
    public BattleCommand? Update(InputFrame input)
    {
        var battle = _battle ?? throw new InvalidOperationException("Start a turn before choosing a command.");
        if (Targets is not null)
        {
            return ChooseTarget(Targets, input);
        }

        if (Skills is not null)
        {
            ChooseSkill(battle, Skills, input);
            return null;
        }

        if (Items is not null)
        {
            ChooseItem(battle, Items, input);
            return null;
        }

        return ChooseFromList(battle, input);
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

        return input.IsPressed(GameButton.Confirm) ? Choose(Selected, battle) : null;
    }

    // Attack, Skills, Items and Move open the next step; Defend and Run are the command itself.
    private BattleCommand? Choose(CommandEntry entry, BattleState battle)
    {
        switch (entry)
        {
            case CommandEntry.Attack:
                Targets = new BattleTargetCursor(battle.TargetChoiceForAttack(), battle.CurrentActor);
                return null;
            case CommandEntry.Skills:
                Skills = new BattleSkillChoice(battle);
                return null;
            case CommandEntry.Items:
                Items = new BattleItemChoice(battle, _inventory, _items);
                return null;
            case CommandEntry.Move:
                Targets = new BattleTargetCursor(battle.TargetChoiceForMove(), battle.CurrentActor);
                return null;
            case CommandEntry.Defend:
                return new DefendCommand();
            case CommandEntry.Run:
                return new FleeCommand();
            default:
                throw new InvalidOperationException($"{entry} can't be chosen yet.");
        }
    }

    // A skill the actor can't pay for says so (decision 105); one with nobody to aim at stays greyed and does nothing.
    private void ChooseSkill(BattleState battle, BattleSkillChoice skills, InputFrame input)
    {
        MoveWithin(skills, input);
        if (input.IsPressed(GameButton.Cancel))
        {
            Skills = null;
        }
        else if (input.IsPressed(GameButton.Confirm) && !battle.CanAfford(skills.Selected))
        {
            skills.SayCannotAfford();
        }
        else if (input.IsPressed(GameButton.Confirm) && battle.CanUse(skills.Selected))
        {
            Targets = new BattleTargetCursor(battle.TargetChoiceFor(skills.Selected), battle.CurrentActor);
        }
    }

    // A greyed item does nothing on Confirm.
    private void ChooseItem(BattleState battle, BattleItemChoice items, InputFrame input)
    {
        MoveWithin(items, input);
        if (input.IsPressed(GameButton.Cancel))
        {
            Items = null;
        }
        else if (input.IsPressed(GameButton.Confirm) && battle.CanUse(items.Selected))
        {
            Targets = new BattleTargetCursor(battle.TargetChoiceFor(items.Selected), battle.CurrentActor);
        }
    }

    private static void MoveWithin(IBattleListChoice list, InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            list.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            list.MoveDown();
        }
    }

    // Up and Left go back along the formation, Down and Right forward (decision 56).
    private BattleCommand? ChooseTarget(BattleTargetCursor targets, InputFrame input)
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

        BattleCommand command = (Skills, Items) switch
        {
            ({ } skills, _) => new SkillCommand(skills.Selected, targets.Selected),
            (_, { } items) => new ItemCommand(items.Selected, targets.Selected),
            _ when Selected == CommandEntry.Move => new MoveCommand(targets.Selected[0]),
            _ => new AttackCommand(targets.Selected[0]),
        };
        Skills = null;
        Items = null;
        Targets = null;
        return command;
    }
}
