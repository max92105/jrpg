using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Party;

public sealed class PurseTests
{
    private readonly Purse _purse = new();

    [Fact]
    public void Add_WithinMax_RaisesGold()
    {
        var lost = _purse.Add(120);

        Assert.Equal(120, _purse.Gold);
        Assert.Equal(0, lost);
    }

    [Fact]
    public void Add_ExactlyToMax_LosesNothing()
    {
        _purse.Add(5);

        var lost = _purse.Add(Purse.MaxGold - 5);

        Assert.Equal(Purse.MaxGold, _purse.Gold);
        Assert.Equal(0, lost);
    }

    // One overflow rule (the plan's README): gold past the cap is lost, nothing is refused.
    [Fact]
    public void Add_PastMax_KeepsMaxAndReturnsWhatWasLost()
    {
        _purse.Add(Purse.MaxGold - 5);

        var lost = _purse.Add(12);

        Assert.Equal(Purse.MaxGold, _purse.Gold);
        Assert.Equal(7, lost);
    }

    [Fact]
    public void Add_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _purse.Add(-1));
    }

    [Fact]
    public void TrySpend_Enough_TakesIt()
    {
        _purse.Add(50);

        var spent = _purse.TrySpend(30);

        Assert.True(spent);
        Assert.Equal(20, _purse.Gold);
    }

    [Fact]
    public void TrySpend_MoreThanOwned_ReturnsFalseAndKeepsGold()
    {
        _purse.Add(50);

        var spent = _purse.TrySpend(51);

        Assert.False(spent);
        Assert.Equal(50, _purse.Gold);
    }

    [Fact]
    public void TrySpend_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _purse.TrySpend(-1));
    }

    [Fact]
    public void CanSpend_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _purse.CanSpend(-1));
    }

    [Fact]
    public void CanSpend_ExactlyOwned_IsTrue()
    {
        _purse.Add(50);

        Assert.True(_purse.CanSpend(50));
    }

    [Fact]
    public void CanSpend_OneMoreThanOwned_IsFalse()
    {
        _purse.Add(50);

        Assert.False(_purse.CanSpend(51));
    }
}
