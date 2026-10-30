using Jrpg.Core.Exploration;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

// The inn always charges and rests everyone once paid (decision 382); it can't be paid short of gold, or twice.
public sealed class InnStayTests
{
    private const int Price = 30;
    private const int Gold = 100;
    private readonly PartyState _party = TestParties.LedBy("sarxian");

    public InnStayTests() => _party.Purse.Add(Gold);

    [Fact]
    public void TryPay_EnoughGold_TakesPriceAndRestsParty()
    {
        _party.Leader.Vitals.TakeDamage(10);
        var stay = new InnStay(Price, _party);

        var paid = stay.TryPay();

        Assert.True(paid);
        Assert.Equal(Gold - Price, _party.Purse.Gold);
        Assert.Equal(_party.Leader.Vitals.MaxHp, _party.Leader.Vitals.Hp);
    }

    [Fact]
    public void TryPay_NobodyNeedsRest_StillCharges()
    {
        var stay = new InnStay(Price, _party);
        Assert.False(_party.Leader.HasAnythingToRest);

        Assert.True(stay.TryPay());

        Assert.Equal(Gold - Price, _party.Purse.Gold);
    }

    [Fact]
    public void TryPay_ShortOfGold_ReturnsFalseAndChangesNothing()
    {
        _party.Leader.Vitals.TakeDamage(10);
        var stay = new InnStay(Gold + 1, _party);

        var paid = stay.TryPay();

        Assert.False(paid);
        Assert.False(stay.CanPay);
        Assert.Equal(Gold, _party.Purse.Gold);
        Assert.Equal(_party.Leader.Vitals.MaxHp - 10, _party.Leader.Vitals.Hp);
    }

    [Fact]
    public void TryPay_Twice_PaysOnce()
    {
        var stay = new InnStay(Price, _party);
        Assert.True(stay.TryPay());

        var again = stay.TryPay();

        Assert.False(again);
        Assert.False(stay.CanPay);
        Assert.Equal(Gold - Price, _party.Purse.Gold);
    }
}
