using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// The playback's sounds use its own beats' times.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionPlaybackSoundTests
{
    private readonly Combatant _goblin = Combatant.FromEnemy(StartMapScenarios.RepositoryContent.Enemies["goblin"]);

    [Fact]
    public void Start_DamageLanded_SoundsTheHit()
    {
        var playback = ActionPlayback.Start(
            [new ActionStarted(_goblin, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)],
            RepositoryScreenText.Text.Messages);

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Hit, TimeSpan.Zero)], playback.Sounds);
    }

    [Fact]
    public void Start_HitThenFall_FallSoundsAtTheDefeatBeatsStart()
    {
        var playback = ActionPlayback.Start(
            [new ActionStarted(_goblin, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13), new CombatantDefeated(_goblin)],
            RepositoryScreenText.Text.Messages);

        Assert.Equal(
            [new TimedSound(UiSoundFiles.Battle.Hit, TimeSpan.Zero), new TimedSound(UiSoundFiles.Battle.Fall, BattlePacing.ImpactHold)],
            playback.Sounds);
    }

    // The hit fells the goblin (1.0 s), then the backfire fells the caster on a defeat beat of its own (3.0 s).
    [Fact]
    public void Start_TwoFallsOnTwoDefeatBeats_SoundsEachAtItsOwn()
    {
        var caster = Combatant.FromEnemy(StartMapScenarios.RepositoryContent.Enemies["goblin"]);
        var playback = ActionPlayback.Start(
            [
                new ActionStarted(caster, new AttackCommand(_goblin)),
                new EffectApplied(_goblin, EffectKind.Damage, 40),
                new CombatantDefeated(_goblin),
                new Backfired(caster, "instability", 16),
                new CombatantDefeated(caster),
            ],
            RepositoryScreenText.Text.Messages);

        var falls = playback.Sounds.Where(sound => sound.File == UiSoundFiles.Battle.Fall).Select(sound => sound.At);

        Assert.Equal([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)], falls);
    }

    // The actor's Regen ticks at 1.0 s; the stunned goblin's Poison ticks after its "Stunned" cue, at 2.6 s.
    [Fact]
    public void Start_TicksOnTwoTickBeats_SoundsEachAtItsOwn()
    {
        var actor = Combatant.FromEnemy(StartMapScenarios.RepositoryContent.Enemies["goblin"]);
        var playback = ActionPlayback.Start(
            [
                new ActionStarted(actor, new AttackCommand(_goblin)),
                new EffectApplied(_goblin, EffectKind.Damage, 13),
                new StatusTicked(actor, StatusKind.Regen, 5),
                new TurnSkipped(_goblin, StatusKind.Stun),
                new StatusTicked(_goblin, StatusKind.Poison, 9),
            ],
            RepositoryScreenText.Text.Messages);

        Assert.Contains(new TimedSound(UiSoundFiles.Battle.RegenTick, TimeSpan.FromSeconds(1)), playback.Sounds);
        Assert.Contains(new TimedSound(UiSoundFiles.Battle.PoisonTick, TimeSpan.FromSeconds(2.6)), playback.Sounds);
    }
}
