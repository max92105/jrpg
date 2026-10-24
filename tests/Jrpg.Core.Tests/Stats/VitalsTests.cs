using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class VitalsTests
{
    [Fact]
    public void Constructor_HpAboveMaxHp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vitals(hp: 101, maxHp: 100, mp: 0, maxMp: 0));
    }

    [Fact]
    public void TakeDamage_MoreThanHp_ClampsToZeroAndReturnsHpLost()
    {
        var vitals = new Vitals(hp: 30, maxHp: 100, mp: 0, maxMp: 0);

        var lost = vitals.TakeDamage(50);

        Assert.Equal(0, vitals.Hp);
        Assert.Equal(30, lost);
    }

    [Fact]
    public void TakeDamage_Fallen_ReturnsZero()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100, mp: 0, maxMp: 0);

        var lost = vitals.TakeDamage(50);

        Assert.Equal(0, lost);
    }

    [Fact]
    public void RestoreHp_AboveMax_ClampsToMaxAndReturnsHpRestored()
    {
        var vitals = new Vitals(hp: 90, maxHp: 100, mp: 0, maxMp: 0);

        var restored = vitals.RestoreHp(50);

        Assert.Equal(100, vitals.Hp);
        Assert.Equal(10, restored);
    }

    [Fact]
    public void RestoreHp_Fallen_ReturnsZeroAndStaysFallen()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100, mp: 0, maxMp: 0);

        var restored = vitals.RestoreHp(50);

        Assert.False(vitals.IsAlive);
        Assert.Equal(0, restored);
    }

    [Fact]
    public void Revive_Fallen_RestoresHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100, mp: 0, maxMp: 0);

        var restored = vitals.Revive(15);

        Assert.Equal(15, vitals.Hp);
        Assert.Equal(15, restored);
    }

    [Fact]
    public void Revive_MoreThanMaxHp_ClampsToMaxHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100, mp: 0, maxMp: 0);

        var restored = vitals.Revive(150);

        Assert.Equal(100, vitals.Hp);
        Assert.Equal(100, restored);
    }

    [Fact]
    public void Revive_Living_ReturnsZero()
    {
        var vitals = new Vitals(hp: 40, maxHp: 100, mp: 0, maxMp: 0);

        var restored = vitals.Revive(15);

        Assert.Equal(40, vitals.Hp);
        Assert.Equal(0, restored);
    }

    [Fact]
    public void TrySpendMp_EnoughMp_ReturnsTrueAndSpendsMp()
    {
        var vitals = new Vitals(hp: 100, maxHp: 100, mp: 10, maxMp: 20);

        var spent = vitals.TrySpendMp(4);

        Assert.True(spent);
        Assert.Equal(6, vitals.Mp);
    }

    [Fact]
    public void TrySpendMp_ExactlyAllMp_ReturnsTrueAndLeavesZero()
    {
        var vitals = new Vitals(hp: 100, maxHp: 100, mp: 4, maxMp: 20);

        var spent = vitals.TrySpendMp(4);

        Assert.True(spent);
        Assert.Equal(0, vitals.Mp);
    }

    [Fact]
    public void TrySpendMp_NotEnoughMp_ReturnsFalseAndKeepsMp()
    {
        var vitals = new Vitals(hp: 100, maxHp: 100, mp: 3, maxMp: 20);

        var spent = vitals.TrySpendMp(4);

        Assert.False(spent);
        Assert.Equal(3, vitals.Mp);
    }

    [Fact]
    public void RestoreMp_AboveMax_ClampsToMaxAndReturnsMpRestored()
    {
        var vitals = new Vitals(hp: 100, maxHp: 100, mp: 15, maxMp: 20);

        var restored = vitals.RestoreMp(10);

        Assert.Equal(20, vitals.Mp);
        Assert.Equal(5, restored);
    }

    [Fact]
    public void RaiseMaxima_Alive_AddsToMaxAndCurrent()
    {
        var vitals = new Vitals(hp: 30, maxHp: 100, mp: 5, maxMp: 20);

        vitals.RaiseMaxima(maxHpGain: 10, maxMpGain: 3);

        Assert.Equal((40, 110, 8, 23), (vitals.Hp, vitals.MaxHp, vitals.Mp, vitals.MaxMp));
    }

    [Fact]
    public void RaiseMaxima_Dead_StaysAtZeroHp()
    {
        var vitals = new Vitals(hp: 0, maxHp: 100, mp: 5, maxMp: 20);

        vitals.RaiseMaxima(maxHpGain: 10, maxMpGain: 3);

        Assert.Equal((0, 110, 8, 23), (vitals.Hp, vitals.MaxHp, vitals.Mp, vitals.MaxMp));
    }
}
