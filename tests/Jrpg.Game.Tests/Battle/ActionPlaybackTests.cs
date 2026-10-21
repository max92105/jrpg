using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackTests
{
    private readonly Combatant _sarxian;
    private readonly Combatant _goblin;

    public ActionPlaybackTests()
    {
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
        (_sarxian, _goblin) = (battle.Combatants[0], battle.Combatants[1]);
    }

    [Fact]
    public void Start_AttackLanded_HasOneImpactBeat()
    {
        BattleEvent[] events = [new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)];

        var playback = Start(events);

        Assert.Equal([new PlaybackBeat(PlaybackBeatKind.Impact, TimeSpan.FromSeconds(1))], playback.Beats);
        Assert.Null(playback.BannerText);
    }

    [Fact]
    public void Start_TargetFell_AddsDefeatBeat()
    {
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new AttackCommand(_goblin)),
            new EffectApplied(_goblin, EffectKind.Damage, 40),
            new CombatantDefeated(_goblin),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Impact, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(TimeSpan.FromSeconds(2), playback.Duration);
    }

    [Fact]
    public void Start_Defend_HasNoHold()
    {
        var playback = Start([new ActionStarted(_sarxian, new DefendCommand())]);

        Assert.Equal(TimeSpan.Zero, playback.Duration);
        Assert.Equal($"{_sarxian.Name} defends.", playback.BannerText);
    }

    [Fact]
    public void Start_SkillCommand_ShowsSkillName()
    {
        var skill = _sarxian.Skills[0];

        var playback = Start([new ActionStarted(_sarxian, new SkillCommand(skill, [_goblin])), new EffectApplied(_goblin, EffectKind.Damage, 20)]);

        Assert.Equal(skill.Name, playback.BannerText);
        Assert.Equal([PlaybackBeatKind.Banner, PlaybackBeatKind.Impact], playback.Beats.Select(beat => beat.Kind));
    }

    [Fact]
    public void Start_TwoTargetsFell_HoldsOneImpactAndOneDefeat()
    {
        var second = Combatant.FromEnemy(BattleScenarios.Catalog.Enemies["goblin"]);
        BattleEvent[] events =
        [
            new ActionStarted(_sarxian, new SkillCommand(_sarxian.Skills[0], [_goblin, second])),
            new EffectApplied(_goblin, EffectKind.Damage, 40),
            new CombatantDefeated(_goblin),
            new EffectApplied(second, EffectKind.Damage, 40),
            new CombatantDefeated(second),
        ];

        var playback = Start(events);

        Assert.Equal([PlaybackBeatKind.Banner, PlaybackBeatKind.Impact, PlaybackBeatKind.Defeat], playback.Beats.Select(beat => beat.Kind));
        Assert.Equal(TimeSpan.FromSeconds(2), playback.Duration);
    }

    [Fact]
    public void Start_ItemCommand_ShowsItemName()
    {
        var potion = BattleScenarios.Catalog.Items["potion"];

        var playback = Start([new ActionStarted(_sarxian, new ItemCommand(potion, [_sarxian])), new EffectApplied(_sarxian, EffectKind.RestoreHp, 50)]);

        Assert.Equal(potion.Name, playback.BannerText);
    }

    [Fact]
    public void Start_AttackLanded_AddsDamageNumberOnTarget()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)]);

        Assert.Equal([new FloatingNumber(_goblin, "13", EffectKind.Damage)], playback.Numbers);
    }

    [Fact]
    public void Start_AttackLanded_PlaysAttackersStripOnTarget()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)]);

        Assert.Equal([new EffectOnTarget(_goblin, "AttackPlayer.png")], playback.Strips);
        Assert.Empty(playback.Cues);
    }

    [Fact]
    public void Start_HealLanded_ShowsPlaceholderCueNotStrip()
    {
        var potion = BattleScenarios.Catalog.Items["potion"];

        var playback = Start([new ActionStarted(_sarxian, new ItemCommand(potion, [_sarxian])), new EffectApplied(_sarxian, EffectKind.RestoreHp, 50)]);

        Assert.Equal([new CueOnTarget(_sarxian, potion.Name)], playback.Cues);
        Assert.Empty(playback.Strips);
    }

    // The fallen stay lit white for 1.0 s, then fade out over 1.0 s.
    [Fact]
    public void Update_FallenEnemy_FadesAfterTint()
    {
        var playback = Start([new ActionStarted(_sarxian, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 40), new CombatantDefeated(_goblin)]);
        playback.Update(TimeSpan.FromSeconds(0.9));
        var duringTint = playback.FallOpacityOf(_goblin);

        playback.Update(TimeSpan.FromSeconds(0.6));

        Assert.Equal(1, duringTint);
        Assert.Equal(0.75f, playback.FallOpacityOf(_goblin) ?? -1, 3); // Halfway through the fade, eased in: 1 - 0.5².
        playback.Update(TimeSpan.FromSeconds(0.5));
        Assert.Equal(0, playback.FallOpacityOf(_goblin));
        Assert.Null(playback.FallOpacityOf(_sarxian));
    }

    // BattleEvent stays open, unlike BattleCommand: new events come with new mechanics (ADR 0004), and the playback must
    // refuse what it can't show rather than show it silently wrong.
    [Fact]
    public void Start_UnknownEvent_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Start([new PoisonTicked()]));

        Assert.Contains(nameof(PoisonTicked), exception.Message, StringComparison.Ordinal);
    }

    private static ActionPlayback Start(IReadOnlyList<BattleEvent> events) => ActionPlayback.Start(events, RepositoryScreenText.Text.Messages);

    private sealed record PoisonTicked : BattleEvent;
}
