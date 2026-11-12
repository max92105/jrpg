using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// Zanyr, a cleric, acts first; the bag starts empty. The seed named for Sarxian keeps the listed order whoever fights,
// so it puts Zanyr first too.
public sealed class CommandMenuTests
{
    private readonly Inventory _inventory = new();
    private readonly BattleState _battle;
    private readonly CommandMenu _menu;

    public CommandMenuTests()
    {
        _battle = BattleScenarios.MemberAgainstGoblinTrio("zanyr", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
        _menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        _menu.Start(_battle);
    }

    [Fact]
    public void Update_ConfirmAttack_OpensTargetsOnLivingEnemies()
    {
        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.Null(command);
        Assert.Equal(_battle.LivingOn(BattleSide.Enemies), _menu.Targets?.Candidates);
    }

    [Fact]
    public void Update_ConfirmTarget_ProducesAttackOnIt()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal(new AttackCommand(_battle.Combatants[^1]), command);
    }

    [Fact]
    public void Update_CancelOnTargets_ReturnsToCommands()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        _menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(_menu.Targets);
        Assert.Equal(CommandEntry.Attack, _menu.Selected);
    }

    [Fact]
    public void Update_ConfirmDefend_ProducesDefend()
    {
        CommandMenuDriver.SelectEntry(_menu, CommandEntry.Defend);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DefendCommand>(command);
    }

    [Fact]
    public void Update_MoveDownWithEmptyBagAlone_SkipsGreyedItemsAndMove()
    {
        CommandMenuDriver.Press(_menu, GameButton.Down, times: 2);

        Assert.Equal(CommandEntry.Defend, _menu.Selected);
    }

    [Fact]
    public void Update_ConfirmRun_ProducesFlee()
    {
        CommandMenuDriver.SelectEntry(_menu, CommandEntry.Run);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<FleeCommand>(command);
    }

    [Fact]
    public void Update_ConfirmItems_ListsOwnedConsumablesWithCounts()
    {
        Own(("potion", 3), ("bronze_sword", 1), ("bomb_all", 2));

        OpenItems();

        Assert.Equal(["potion", "bomb_all"], _menu.Items?.Items.Select(item => item.Id));
        Assert.Equal([3, 2], _menu.Items?.Quantities);
    }

    [Fact]
    public void Update_ReviveWithNobodyFallen_IsGreyed()
    {
        Own(("potion", 1), ("revive_feather", 1));

        OpenItems();

        Assert.True(_menu.Items?.IsUsable(0));
        Assert.False(_menu.Items?.IsUsable(1));
    }

    [Fact]
    public void Update_ConfirmGreyedRevive_StaysOnList()
    {
        Own(("potion", 1), ("revive_feather", 1));
        OpenItems();
        _menu.Update(InputFrames.Pressing(GameButton.Down));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.Null(command);
        Assert.Null(_menu.Targets);
        Assert.Equal("revive_feather", _menu.Items?.Selected.Id);
    }

    [Fact]
    public void IsEnabled_NoUsableItem_GreysItems()
    {
        Own(("revive_feather", 1), ("bronze_sword", 1));

        Assert.False(_menu.IsEnabled(CommandEntry.Items));
    }

    [Fact]
    public void Update_ConfirmBombAll_ProducesCommandOnEveryLivingEnemy()
    {
        Own(("bomb_all", 1));
        OpenItems();
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var item = Assert.IsType<ItemCommand>(command);
        Assert.Equal("bomb_all", item.Item.Id);
        Assert.Equal(_battle.LivingOn(BattleSide.Enemies), item.Targets);
    }

    [Fact]
    public void Update_ConfirmItemsAfterLastPotionUsed_NoLongerListsIt()
    {
        Own(("potion", 1), ("bomb", 1));
        OpenItems();
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
        _battle.Execute(Assert.IsType<ItemCommand>(_menu.Update(InputFrames.Pressing(GameButton.Confirm))));
        PassTheGoblinsTurns();
        _menu.Start(_battle);

        OpenItems();

        Assert.Equal(["bomb"], _menu.Items?.Items.Select(item => item.Id));
    }

    // The menu reads the bag when a turn starts, so the turn is started again after filling it.
    private void Own(params (string ItemId, int Quantity)[] entries)
    {
        foreach (var (itemId, quantity) in entries)
        {
            _inventory.Add(BattleScenarios.Catalog.Items[itemId], quantity);
        }

        _menu.Start(_battle);
    }

    // The three goblins act after Zanyr; defending passes their turns back to her without changing anything.
    private void PassTheGoblinsTurns()
    {
        while (_battle.CurrentActor.Side == BattleSide.Enemies)
        {
            _battle.Execute(new DefendCommand());
        }
    }

    private void OpenItems()
    {
        CommandMenuDriver.Press(_menu, GameButton.Down, times: 2);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
    }
}
