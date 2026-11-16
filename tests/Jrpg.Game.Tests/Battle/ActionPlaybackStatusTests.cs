using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// The playback reads a status's record (decision 810): its name for the cue, its ticking part for a tick's tone.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackStatusTests
{
    private static readonly StatusDefinition _burning =
        TestStatuses.Record("burning_1", "Burning", new StatusPart(StatusPartKind.DamageEachTurn, 5));

    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;

    public ActionPlaybackStatusTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin) = (battle.Combatants[0], battle.Combatants[1]);
    }

    [Fact]
    public void Start_StatusApplied_ShowsTheRecordsName()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new StatusApplied(_goblin, _burning, 2)]);

        Assert.Equal([new CueOnTarget(_goblin, "Burning")], playback.Cues);
    }

    [Fact]
    public void Start_DamageTickOfAnyRecord_UsesPoisonTone()
    {
        var playback = Start([new ActionStarted(_sarxian, new DefendCommand()), new StatusTicked(_goblin, _burning, 5)]);

        Assert.Equal(FloatingNumberTone.Poison, Assert.Single(playback.Numbers).Tone);
    }

    private static ActionPlayback Start(IReadOnlyList<BattleEvent> events) =>
        ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);
}
