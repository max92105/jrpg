using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class PipFlashTests
{
    // Mana went from 3 to 5: the fourth and fifth pips flash, the third and sixth don't.
    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void Covers_ChangedPips_OnlyThose(int index, bool expected)
    {
        Assert.Equal(expected, new PipFlash(First: 3, End: 5, Share: 1).Covers(index));
    }

    [Fact]
    public void Covers_FlashOver_NoPip()
    {
        Assert.False(new PipFlash(First: 3, End: 5, Share: 0).Covers(3));
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 3)]
    public void Between_RiseOrDrop_FlashesTheChangedPips(int before, int after)
    {
        Assert.Equal(new PipFlash(First: 3, End: 5, Share: 1), PipFlash.Between(before, after));
    }
}
