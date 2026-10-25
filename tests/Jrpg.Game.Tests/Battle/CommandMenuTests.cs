using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// Zanyr, a cleric, acts first and knows her class's skills, in order: Heal, Recovery, Heal All. The bag starts empty.
// The seed named for Sarxian keeps the listed order whoever fights, so it puts Zanyr first too.
public sealed class CommandMenuTests
{
    private const int RecoveryRow = 1;
    private const int HealAllRow = 2;

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
        Press(GameButton.Down, times: 2);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DefendCommand>(command);
    }

    [Fact]
    public void Update_MoveDownWithEmptyBag_SkipsGreyedItems()
    {
        Press(GameButton.Down, times: 2);

        Assert.Equal(CommandEntry.Defend, _menu.Selected);
    }

    [Fact]
    public void Update_ConfirmRun_ProducesFlee()
    {
        Press(GameButton.Down, times: 3);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<FleeCommand>(command);
    }

    [Fact]
    public void IsEnabled_ActorWithoutSkills_GreysSkills()
    {
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);

        menu.Start(BattleScenarios.MemberAgainstGoblinTrio("jazog", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed)));

        Assert.False(menu.IsEnabled(CommandEntry.Skills));
    }

    [Fact]
    public void Update_ConfirmSkills_ListsActorSkills()
    {
        OpenSkills();

        Assert.Equal(_battle.CurrentActor.Skills, _menu.Skills?.Skills);
    }

    [Fact]
    public void Update_UnaffordableSkill_IsGreyedAndSaysNotEnoughMp()
    {
        _battle.CurrentActor.Vitals.TrySpendMp(_battle.CurrentActor.Vitals.Mp - 5);
        OpenSkills();

        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.False(_menu.Skills?.IsUsable(0));
        Assert.True(_menu.Skills?.IsSayingCannotAfford);
        Assert.Null(_menu.Targets);
    }

    [Fact]
    public void Update_ConfirmMeteorWithThreeMana_SaysNotEnoughMana()
    {
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(BattleScenarios.MemberAgainstGoblinTrio("omer", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed)));
        menu.Update(InputFrames.Pressing(GameButton.Down));
        menu.Update(InputFrames.Pressing(GameButton.Confirm));
        SelectSkill(menu, "meteor");

        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.True(menu.Skills?.IsSayingCannotAfford);
        Assert.Equal("Mana", menu.Skills?.LackingResourceAt(menu.Skills.SelectedIndex)?.Name);
    }

    [Fact]
    public void Update_ConfirmSelfSkill_ProducesCommandOnActor()
    {
        OpenSkills();
        Press(GameButton.Down, times: RecoveryRow);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var skill = Assert.IsType<SkillCommand>(command);
        Assert.Equal("recovery", skill.Skill.Id);
        Assert.Equal([_battle.CurrentActor], skill.Targets);
    }

    [Fact]
    public void Update_AllAlliesSkill_ProducesCommandOnEveryLivingMember()
    {
        OpenSkills();
        Press(GameButton.Down, times: HealAllRow);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var skill = Assert.IsType<SkillCommand>(command);
        Assert.Equal("heal_all", skill.Skill.Id);
        Assert.Equal(_battle.LivingOn(BattleSide.Party), skill.Targets);
    }

    [Fact]
    public void Update_CancelOnSkillList_ReturnsToCommands()
    {
        OpenSkills();

        _menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(_menu.Skills);
        Assert.Equal(CommandEntry.Skills, _menu.Selected);
    }

    [Fact]
    public void Update_CancelOnSkillTargets_ReturnsToSkillList()
    {
        OpenSkills();
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        _menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(_menu.Targets);
        Assert.Equal("heal", _menu.Skills?.Selected.Id);
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
        Press(GameButton.Down, times: 2);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
    }

    private void OpenSkills()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Down));
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
    }

    // Bounded by the rows, so a list without the skill fails the test instead of looping forever.
    private static void SelectSkill(CommandMenu menu, string skillId)
    {
        var skills = menu.Skills!;
        for (var presses = 0; presses < skills.Skills.Count && skills.Selected.Id != skillId; presses++)
        {
            menu.Update(InputFrames.Pressing(GameButton.Down));
        }

        Assert.Equal(skillId, skills.Selected.Id);
    }

    private void Press(GameButton button, int times)
    {
        for (var press = 0; press < times; press++)
        {
            _menu.Update(InputFrames.Pressing(button));
        }
    }
}
