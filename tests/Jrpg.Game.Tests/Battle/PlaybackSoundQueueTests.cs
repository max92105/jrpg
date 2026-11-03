using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class PlaybackSoundQueueTests
{
    private readonly Combatant _goblin = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["goblin"]);
    private readonly PlaybackSoundQueue _queue = new();
    private readonly AskedSounds _asked = new();

    [Fact]
    public void Follow_SoundDueAtTheStart_AsksIt()
    {
        var playback = Hit();

        _queue.Follow(playback, _asked);

        Assert.Equal([UiSoundFiles.Battle.Hit], _asked.Files);
    }

    // The tick's beat comes first, so the hit's sound is due only once that beat has played.
    [Fact]
    public void Follow_TimeNotYetReached_AsksNothing()
    {
        var playback = HitAfterATick();

        _queue.Follow(playback, _asked);

        Assert.DoesNotContain(UiSoundFiles.Battle.Hit, _asked.Files);
    }

    [Fact]
    public void Follow_TimeReachedExactly_AsksTheSound()
    {
        var playback = HitAfterATick();
        _queue.Follow(playback, _asked);

        playback.Update(BattlePacing.TickHold);
        _queue.Follow(playback, _asked);

        Assert.Contains(UiSoundFiles.Battle.Hit, _asked.Files);
    }

    [Fact]
    public void Follow_TimeReached_AsksItOnce()
    {
        var playback = Hit();
        _queue.Follow(playback, _asked);
        _asked.Clear();

        playback.Update(TimeSpan.FromSeconds(0.5));
        _queue.Follow(playback, _asked);

        Assert.Empty(_asked.Files);
    }

    [Fact]
    public void Follow_ANewPlayback_StartsFromItsFirstSound()
    {
        _queue.Follow(Hit(), _asked);
        _asked.Clear();

        _queue.Follow(Hit(), _asked);

        Assert.Equal([UiSoundFiles.Battle.Hit], _asked.Files);
    }

    [Fact]
    public void Follow_NoPlayback_AsksNothing()
    {
        _queue.Follow(null, _asked);

        Assert.Empty(_asked.Files);
    }

    private ActionPlayback Hit() => ActionPlayback.Start(
        [new ActionStarted(_goblin, new AttackCommand(_goblin)), new EffectApplied(_goblin, EffectKind.Damage, 13)],
        RepositoryScreenText.Text.Messages);

    private ActionPlayback HitAfterATick() => ActionPlayback.Start(
        [new StatusTicked(_goblin, StatusKind.Poison, 3), new EffectApplied(_goblin, EffectKind.Damage, 13)],
        RepositoryScreenText.Text.Messages);
}
