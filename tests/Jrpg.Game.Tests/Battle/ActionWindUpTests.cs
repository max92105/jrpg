using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// What one action shows and sounds before it lands (decisions 644, 647): an Attack, a skill or an item winds up;
// Defend, Move and Run land at once. On Sarxian against the goblin trio.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionWindUpTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];

    [Fact]
    public void Of_Attack_LastsTheWindUp()
    {
        Assert.Equal(BattlePacing.WindUp, WindUpOf(new AttackCommand(_goblin)).Duration);
    }

    [Fact]
    public void Of_Skill_LastsTheWindUp()
    {
        var bash = new SkillCommand(BattleScenarios.Skills["shield_bash"], [_goblin]);

        Assert.Equal(BattlePacing.WindUp, WindUpOf(bash).Duration);
    }

    [Fact]
    public void Of_Item_LastsTheWindUp()
    {
        var potion = new ItemCommand(BattleScenarios.Catalog.Items["potion"], [_sarxian]);

        Assert.Equal(BattlePacing.WindUp, WindUpOf(potion).Duration);
    }

    [Fact]
    public void Of_Defend_LastsNothing()
    {
        Assert.Equal(TimeSpan.Zero, WindUpOf(new DefendCommand()).Duration);
    }

    [Fact]
    public void Of_Move_LastsNothing()
    {
        Assert.Equal(TimeSpan.Zero, WindUpOf(new MoveCommand(_sarxian)).Duration);
    }

    [Fact]
    public void Of_Flee_LastsNothing()
    {
        Assert.Equal(TimeSpan.Zero, WindUpOf(new FleeCommand()).Duration);
    }

    [Fact]
    public void Of_Attack_NamesItAttack()
    {
        Assert.Equal("Attack", WindUpOf(new AttackCommand(_goblin)).BannerText);
    }

    [Fact]
    public void Of_Skill_NamesTheSkill()
    {
        var bash = BattleScenarios.Skills["shield_bash"];

        Assert.Equal(bash.Name, WindUpOf(new SkillCommand(bash, [_goblin])).BannerText);
    }

    [Fact]
    public void Sounds_Skill_IsItsSoundAtTheStart()
    {
        var windUp = WindUpOf(new SkillCommand(SoundingSkills.WithSound("Own.wav"), [_goblin]));

        Assert.Equal([new TimedSound("Own.wav", TimeSpan.Zero)], windUp.Sounds);
    }

    [Fact]
    public void Sounds_Attack_IsNothing()
    {
        Assert.Empty(WindUpOf(new AttackCommand(_goblin)).Sounds);
    }

    [Fact]
    public void Update_TwoDeltas_AddsThem()
    {
        var windUp = WindUpOf(new AttackCommand(_goblin));

        windUp.Update(TimeSpan.FromSeconds(0.2));
        windUp.Update(TimeSpan.FromSeconds(0.3));

        Assert.Equal(TimeSpan.FromSeconds(0.5), windUp.Elapsed);
    }

    // Built in one place, so a parameter a later slice adds changes one line.
    private static ActionWindUp WindUpOf(BattleCommand command) =>
        ActionWindUp.Of(_sarxian, command, RepositoryScreenText.Text.Messages);
}
