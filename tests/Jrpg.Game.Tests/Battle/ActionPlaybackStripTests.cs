using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Which way the attacker's strip is drawn (decision 655): effect art faces right, as an enemy attacking the party, so a
// party member's attack plays mirrored. On Sarxian against the goblin trio.
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

    private static ActionPlayback Hit(Combatant attacker, Combatant target) => ActionPlayback.Start(
        [new ActionStarted(attacker, new AttackCommand(target)), new EffectApplied(target, EffectKind.Damage, 13)],
        RepositoryScreenText.Text.Messages);
}
