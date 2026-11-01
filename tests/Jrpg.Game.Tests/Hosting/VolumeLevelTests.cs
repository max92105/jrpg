using Jrpg.Game.Hosting;

namespace Jrpg.Game.Tests.Hosting;

public sealed class VolumeLevelTests
{
    [Fact]
    public void GainOf_Max_IsOne() => Assert.Equal(1f, VolumeLevel.GainOf(VolumeLevel.Max));

    [Fact]
    public void GainOf_Zero_IsSilent() => Assert.Equal(0f, VolumeLevel.GainOf(0));

    [Fact]
    public void GainOf_Five_IsAQuarter() => Assert.Equal(0.25f, VolumeLevel.GainOf(5));

    [Fact]
    public void GainOf_AboveMax_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => VolumeLevel.GainOf(VolumeLevel.Max + 1));

    [Fact]
    public void GainOf_BelowZero_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => VolumeLevel.GainOf(-1));
}
