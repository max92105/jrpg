using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// Sarxian acts first and knows, in order: Fire Blast, Fireball, Heal, Recovery, Heal All.
public sealed class CommandMenuTests
{
    private const int RecoveryRow = 3;
    private const int HealAllRow = 4;

    private readonly BattleState _battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
    private readonly CommandMenu _menu = new();

    public CommandMenuTests() => _menu.Start(_battle);

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
    public void Update_MoveDownFromSkills_SkipsGreyedItems()
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
        var menu = new CommandMenu();

        menu.Start(BattleScenarios.MemberAgainstGoblinTrio("jazog", new SeededRandom(BattleScenarios.SarxianFirstSeed)));

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
        Assert.True(_menu.Skills?.IsSayingNotEnoughMp);
        Assert.Null(_menu.Targets);
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
        Assert.Equal("fire_blast", _menu.Skills?.Selected.Id);
    }

    private void OpenSkills()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Down));
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
    }

    private void Press(GameButton button, int times)
    {
        for (var press = 0; press < times; press++)
        {
            _menu.Update(InputFrames.Pressing(button));
        }
    }
}
