using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
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
}
