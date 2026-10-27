using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class VitalsTests
{
    [Fact]
    public void Constructor_HpAboveMaxHp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vitals(hp: 101, maxHp: 100));
    }

    [Fact]
    public void TakeDamage_MoreThanHp_ClampsToZeroAndReturnsHpLost()
    {
        var vitals = new Vitals(hp: 30, maxHp: 100);

        var lost = vitals.TakeDamage(50);

        Assert.Equal(0, vitals.Hp);
        Assert.Equal(30, lost);
    }

    [Fact]
    public void TakeDamage_Fallen_ReturnsZero()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        var lost = vitals.TakeDamage(50);

        Assert.Equal(0, lost);
    }

    [Fact]
    public void RestoreHp_AboveMax_ClampsToMaxAndReturnsHpRestored()
    {
        var vitals = new Vitals(hp: 90, maxHp: 100);

        var restored = vitals.RestoreHp(50);

        Assert.Equal(100, vitals.Hp);
        Assert.Equal(10, restored);
    }

    [Fact]
    public void RestoreHp_Fallen_ReturnsZeroAndStaysFallen()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        var restored = vitals.RestoreHp(50);

        Assert.False(vitals.IsAlive);
        Assert.Equal(0, restored);
    }

    [Fact]
    public void Revive_Fallen_RestoresHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        var restored = vitals.Revive(15);

        Assert.Equal(15, vitals.Hp);
        Assert.Equal(15, restored);
    }

    [Fact]
    public void Revive_MoreThanMaxHp_ClampsToMaxHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        var restored = vitals.Revive(150);

        Assert.Equal(100, vitals.Hp);
        Assert.Equal(100, restored);
    }

    [Fact]
    public void Revive_Living_ReturnsZero()
    {
        var vitals = new Vitals(hp: 40, maxHp: 100);

        var restored = vitals.Revive(15);

        Assert.Equal(40, vitals.Hp);
        Assert.Equal(0, restored);
    }

    [Fact]
    public void RaiseMaxima_Alive_AddsToMaxAndCurrentHp()
    {
        var vitals = new Vitals(hp: 30, maxHp: 100);

        vitals.RaiseMaxima(maxHpGain: 10);

        Assert.Equal((40, 110), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void RaiseMaxima_Dead_StaysAtZeroHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        vitals.RaiseMaxima(maxHpGain: 10);

        Assert.Equal((0, 110), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void ResizeMax_BelowHp_ClampsHp()
    {
        var vitals = new Vitals(hp: 80, maxHp: 100);

        vitals.ResizeMax(60);

        Assert.Equal((60, 60), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void ResizeMax_AboveHp_KeepsHp()
    {
        var vitals = new Vitals(hp: 40, maxHp: 100);

        vitals.ResizeMax(150);

        Assert.Equal((40, 150), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void ResizeMax_LivingAtOneHpToOne_KeepsOne()
    {
        var vitals = new Vitals(hp: 1, maxHp: 100);

        vitals.ResizeMax(1);

        Assert.True(vitals.IsAlive);
        Assert.Equal((1, 1), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void ResizeMax_Fallen_StaysAtZero()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100);

        vitals.ResizeMax(150);

        Assert.Equal((0, 150), (vitals.Hp, vitals.MaxHp));
    }

    [Fact]
    public void ResizeMax_Zero_Throws()
    {
        var vitals = new Vitals(hp: 40, maxHp: 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => vitals.ResizeMax(0));
    }
}
