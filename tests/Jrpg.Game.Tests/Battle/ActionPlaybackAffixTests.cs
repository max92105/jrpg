using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// What the affix stats show in a battle's playback: kept apart from ActionPlaybackTests, near its size limit.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackAffixTests
{
    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;

    public ActionPlaybackAffixTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin) = (battle.Combatants[0], battle.Combatants[1]);
    }

    // The heal shows on the hitter in the impact, beside the damage on the goblin.
    [Fact]
    public void Start_LifeStolen_AddsHealNumberOnActor()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new LifeStolen(_sarxian, 2),
        ];

        var playback = ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);

        Assert.Equal(new FloatingNumber(_sarxian, "2", FloatingNumberTone.Heal), playback.Numbers[^1]);
        Assert.Equal([PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
    }
}
