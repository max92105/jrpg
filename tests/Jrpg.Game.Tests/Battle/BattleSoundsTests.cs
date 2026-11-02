using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Each event's sound, at the start of the beat that shows it (decision 423). Every kind of beat starts at its own time
// here, so a sound placed at the wrong beat, or at the action's start, can't pass for the right one.
public sealed class BattleSoundsTests
{
    private static readonly TimeSpan _impact = StartOf(PlaybackBeatKind.Impact);

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
    public void Of_EnemyFell_IsTheFallAtTheDefeatBeat()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Fall, StartOf(PlaybackBeatKind.Defeat))], SoundsOf(new CombatantDefeated(_goblin)));
    }

    [Fact]
    public void Of_PoisonTicked_IsThePoisonTickAtTheTickBeat()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.PoisonTick, StartOf(PlaybackBeatKind.Tick))], SoundsOf(new StatusTicked(_goblin, StatusKind.Poison, 5)));
    }

    [Fact]
    public void Of_RegenTicked_IsTheRegenTick()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.RegenTick, StartOf(PlaybackBeatKind.Tick))], SoundsOf(new StatusTicked(_goblin, StatusKind.Regen, 5)));
    }

    [Fact]
    public void Of_AnotherStatusTicked_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => SoundsOf(new StatusTicked(_goblin, StatusKind.Stun, 5)));
    }

    [Fact]
    public void Of_Backfired_IsTheBackfireAtItsBeat()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Backfire, StartOf(PlaybackBeatKind.Backfire))], SoundsOf(new Backfired(_goblin, "instability", 5)));
    }

    [Fact]
    public void Of_TurnSkipped_IsStunnedAtTheSkipBeat()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Stunned, StartOf(PlaybackBeatKind.Skip))], SoundsOf(new TurnSkipped(_goblin, StatusKind.Stun)));
    }

    [Fact]
    public void Of_EnemyEntersAPhase_IsEnragedAtThePhaseBeat()
    {
        var phase = new EnemyPhase { BelowHpPercent = 50, Tactics = [new EnemyAction { Weight = 1 }] };

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Enraged, StartOf(PlaybackBeatKind.Phase))], SoundsOf(new EnemyPhaseEntered(_goblin, phase)));
    }

    [Fact]
    public void Of_ResourceGained_IsTheGain()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.ResourceGain, TimeSpan.Zero)], SoundsOf(new ResourceChanged(_goblin, "mana", 2)));
    }

    [Fact]
    public void Of_ResourceSpent_IsSilent()
    {
        Assert.Empty(SoundsOf(new ResourceChanged(_goblin, "mana", -2)));
    }

    [Fact]
    public void Of_StatusEnded_IsItsSound()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.StatusEnded, TimeSpan.Zero)], SoundsOf(new StatusEnded(_goblin, StatusKind.Stun)));
    }

    [Fact]
    public void Of_PlacesSwapped_IsTheMove()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Move, TimeSpan.Zero)], SoundsOf(new PlacesSwapped(_goblin, _goblin)));
    }

    [Fact]
    public void Of_ExtraTurnReadied_IsItsSound()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.ExtraTurn, TimeSpan.Zero)], SoundsOf(new ExtraTurnReadied(_goblin, _goblin)));
    }

    [Fact]
    public void Of_ItemUsed_IsTheItemSound()
    {
        var potion = StartMapScenarios.RepositoryContent.Items["potion"];

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.ItemUse, TimeSpan.Zero)], SoundsOf(new ActionStarted(_goblin, new ItemCommand(potion, [_goblin]))));
    }

    [Fact]
    public void Of_Defended_IsTheDefendSound()
    {
        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Defend, TimeSpan.Zero)], SoundsOf(new ActionStarted(_goblin, new DefendCommand())));
    }

    [Fact]
    public void Of_Attacked_IsSilentAsItStarts()
    {
        Assert.Empty(SoundsOf(new ActionStarted(_goblin, new AttackCommand(_goblin))));
    }

    [Fact]
    public void Of_LaterBeatFirstInTheEvents_IsSortedByTime()
    {
        var sounds = SoundsOf(new CombatantDefeated(_goblin), new EffectApplied(_goblin, EffectKind.Damage, 13));

        Assert.Equal([UiSoundFiles.Battle.Hit, UiSoundFiles.Battle.Fall], sounds.Select(sound => sound.File));
    }

    [Fact]
    public void Of_EventItDoesNotKnow_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => SoundsOf(new UnknownEvent()));
    }

    private static IReadOnlyList<TimedSound> SoundsOf(params BattleEvent[] events) =>
        BattleSounds.Of(events, (_, kind) => StartOf(kind));

    private static TimeSpan StartOf(PlaybackBeatKind kind) => TimeSpan.FromSeconds(0.1 * ((int)kind + 1));

    private sealed record UnknownEvent : BattleEvent;
}
