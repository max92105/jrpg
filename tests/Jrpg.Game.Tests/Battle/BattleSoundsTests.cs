using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Each event's sound, at the start of the beat that shows it (decision 423). The impact starts at 0.4 s here, so a
// sound placed at the start of the action can't pass for an impact's.
public sealed class BattleSoundsTests
{
    private static readonly TimeSpan _impact = TimeSpan.FromSeconds(0.4);

    private readonly Combatant _goblin = Combatant.FromEnemy(StartMapScenarios.RepositoryContent.Enemies["goblin"]);

    [Fact]
    public void Of_DamageLanded_IsTheHitAtTheImpactsStart()
    {
        var sounds = SoundsOf(new EffectApplied(_goblin, EffectKind.Damage, 13));

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Hit, _impact)], sounds);
    }

    [Fact]
    public void Of_TwoHits_IsOneHit()
    {
        var sounds = SoundsOf(new EffectApplied(_goblin, EffectKind.Damage, 13), new EffectApplied(_goblin, EffectKind.Damage, 7));

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Hit, _impact)], sounds);
    }

    [Fact]
    public void Of_CriticalAmongHits_IsTheCriticalOnly()
    {
        var sounds = SoundsOf(
            new EffectApplied(_goblin, EffectKind.Damage, 13),
            new EffectApplied(_goblin, EffectKind.Damage, 20, IsCritical: true));

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Critical, _impact)], sounds);
    }

    [Theory]
    [InlineData(EffectKind.RestoreHp)]
    [InlineData(EffectKind.Revive)]
    public void Of_HealLanded_IsTheHeal(EffectKind kind)
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Heal, _impact)], SoundsOf(new EffectApplied(_goblin, kind, 13)));
    }

    [Fact]
    public void Of_LifeStolen_IsTheHeal()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Heal, _impact)], SoundsOf(new LifeStolen(_goblin, 5)));
    }

    [Fact]
    public void Of_Miss_IsTheMiss()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Miss, _impact)], SoundsOf(new HitMissed(_goblin)));
    }

    [Fact]
    public void Of_Block_IsTheBlock()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Block, _impact)], SoundsOf(new HitBlocked(_goblin)));
    }

    [Fact]
    public void Of_StatusResisted_IsResisted()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Resisted, _impact)], SoundsOf(new StatusResisted(_goblin, StatusKind.Stun)));
    }

    [Fact]
    public void Of_StatusApplied_IsTheStatusSound()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.StatusApplied, _impact)], SoundsOf(new StatusApplied(_goblin, StatusKind.Stun, 1)));
    }

    [Fact]
    public void Of_BattleEnded_IsSilent()
    {
        Assert.Empty(SoundsOf(new BattleEnded(BattlePhase.Victory)));
    }

    [Fact]
    public void Of_EventItDoesNotKnow_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => SoundsOf(new UnknownEvent()));
    }

    private static IReadOnlyList<TimedSound> SoundsOf(params BattleEvent[] events) =>
        BattleSounds.Of(events, kind => kind == PlaybackBeatKind.Impact ? _impact : throw new InvalidOperationException($"No {kind} beat here."));

    private sealed record UnknownEvent : BattleEvent;
}
