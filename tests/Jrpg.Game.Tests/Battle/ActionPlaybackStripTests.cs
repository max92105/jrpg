using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Which strip plays where damage landed, the action's own or its attacker's (decision 678), and which way it is drawn
// (decision 655): effect art faces right, as an enemy attacking the party, so a party member's action plays mirrored.
// On Sarxian against the goblin trio.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackStripTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];

    [Fact]
    public void Start_PartyMemberAttack_MirrorsTheStrip()
    {
        var playback = Hit(_sarxian, _goblin);

        Assert.True(Assert.Single(playback.Strips).IsMirrored);
    }

    [Fact]
    public void Start_EnemyAttack_KeepsTheStripAsDrawn()
    {
        var playback = Hit(_goblin, _sarxian);

        Assert.False(Assert.Single(playback.Strips).IsMirrored);
    }

    [Fact]
    public void Start_SkillNamingAStrip_PlaysItOnTheTargetDamaged()
    {
        var skill = new SkillCommand(SoundingSkills.WithSound("") with { EffectFile = "Own.png" }, [_goblin]);

        var playback = Landed(skill);

        Assert.Equal((_goblin, "Own.png"), (Assert.Single(playback.Strips).Target, playback.Strips[0].File));
    }

    [Fact]
    public void Start_ItemNamingAStrip_PlaysItOnTheTargetDamaged()
    {
        var bomb = new ItemDefinition
        {
            Id = "bomb",
            Name = "Bomb",
            Description = "",
            IconFile = "",
            MaxStack = 99,
            IsUsableFromMenu = false,
            Effect = new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy),
            EffectFile = "Own.png",
        };

        var playback = Landed(new ItemCommand(bomb, [_goblin]));

        Assert.Equal("Own.png", Assert.Single(playback.Strips).File);
    }

    [Fact]
    public void Start_SkillWithoutAStrip_PlaysTheAttackersStrip()
    {
        var playback = Landed(new SkillCommand(SoundingSkills.WithSound(""), [_goblin]));

        Assert.Equal(BattlerArt.Of(_sarxian).AttackEffectFile, Assert.Single(playback.Strips).File);
    }

    // Sarxian's action landing damage on the first goblin.
    private static ActionPlayback Landed(BattleCommand command) => ActionPlayback.Start(
        [new ActionStarted(_sarxian, command), new EffectApplied(_goblin, EffectKind.Damage, 13)],
        RepositoryScreenText.Text.Messages);

    private static ActionPlayback Hit(Combatant attacker, Combatant target) => ActionPlayback.Start(
        [new ActionStarted(attacker, new AttackCommand(target)), new EffectApplied(target, EffectKind.Damage, 13)],
        RepositoryScreenText.Text.Messages);
}
