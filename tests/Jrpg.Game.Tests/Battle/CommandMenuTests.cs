using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

public sealed class CommandMenuTests
{
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
        _menu.Update(InputFrames.Pressing(GameButton.Down));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DefendCommand>(command);
    }

    [Fact]
    public void Update_MoveDown_SkipsGreyedSkillsAndItems()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Down));

        Assert.Equal(CommandEntry.Defend, _menu.Selected);
    }

    [Fact]
    public void Update_ConfirmRun_ProducesFlee()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Down));
        _menu.Update(InputFrames.Pressing(GameButton.Down));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<FleeCommand>(command);
    }
}
