using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// The words the plaque shows for each kind of action (decision 646), on Sarxian against the goblin trio.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionBannerTextTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void For_Attack_IsAttack()
    {
        Assert.Equal("Attack", ActionBannerText.For(_sarxian, new AttackCommand(_goblin), _messages));
    }

    [Fact]
    public void For_Skill_IsTheSkillsName()
    {
        var ready = BattleScenarios.Catalog.Skills["ready"];

        Assert.Equal(ready.Name, ActionBannerText.For(_sarxian, new SkillCommand(ready, [_goblin]), _messages));
    }

    [Fact]
    public void For_Item_IsTheItemsName()
    {
        var potion = BattleScenarios.Catalog.Items["potion"];

        Assert.Equal(potion.Name, ActionBannerText.For(_sarxian, new ItemCommand(potion, [_sarxian]), _messages));
    }

    [Fact]
    public void For_Defend_NamesTheActor()
    {
        Assert.Equal($"{_sarxian.Name} defends.", ActionBannerText.For(_sarxian, new DefendCommand(), _messages));
    }

    [Fact]
    public void For_Move_IsMove()
    {
        Assert.Equal("Move", ActionBannerText.For(_sarxian, new MoveCommand(_sarxian), _messages));
    }

    [Fact]
    public void For_Flee_IsNothing()
    {
        Assert.Null(ActionBannerText.For(_sarxian, new FleeCommand(), _messages));
    }
}
