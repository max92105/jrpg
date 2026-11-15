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
    private readonly Combatant _goblin = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["goblin"]);

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

    // The hit fells the goblin after the impact's hold, then the backfire fells the caster on a defeat beat of its own.
    [Fact]
    public void Start_TwoFallsOnTwoDefeatBeats_SoundsEachAtItsOwn()
    {
        var caster = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["goblin"]);
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

        Assert.Equal([BattlePacing.ImpactHold, BattlePacing.DefeatHold + BattlePacing.BackfireHold], falls);
    }

    // The actor's Regen ticks after the impact's hold; the stunned goblin's Poison after its "Stunned" cue.
    [Fact]
    public void Start_TicksOnTwoTickBeats_SoundsEachAtItsOwn()
    {
        var actor = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["goblin"]);
        var playback = ActionPlayback.Start(
            [
                new ActionStarted(actor, new AttackCommand(_goblin)),
                new EffectApplied(_goblin, EffectKind.Damage, 13),
                new StatusTicked(actor, StatusKind.Regen, 5),
                new TurnSkipped(_goblin, StatusKind.Stun),
                new StatusTicked(_goblin, StatusKind.Poison, 9),
            ],
            RepositoryScreenText.Text.Messages);

        Assert.Contains(new TimedSound(UiSoundFiles.Battle.RegenTick, BattlePacing.ImpactHold), playback.Sounds);
        var poisonAt = BattlePacing.ImpactHold + BattlePacing.TickHold + BattlePacing.SkipHold;
        Assert.Contains(new TimedSound(UiSoundFiles.Battle.PoisonTick, poisonAt), playback.Sounds);
    }
}
