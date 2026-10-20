using Jrpg.Game.Menus;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Menus;

/// <summary>Two 560 px columns 16 px apart, one card height per row.</summary>
public sealed class ItemTargetPickerViewTests
{
    private static readonly Point _cardsTopLeft = new(384, 300);

    [Fact]
    public void CardArea_FirstMember_IsFirstColumnFirstRow()
    {
        var area = ItemTargetPickerView.CardArea(0, _cardsTopLeft);

        Assert.Equal(new Rectangle(384, 300, 560, MemberCardView.Height), area);
    }

    [Fact]
    public void CardArea_SecondMember_IsSecondColumnFirstRow()
    {
        var area = ItemTargetPickerView.CardArea(1, _cardsTopLeft);

        Assert.Equal(new Rectangle(384 + 560 + 16, 300, 560, MemberCardView.Height), area);
    }

    [Fact]
    public void CardArea_SeventhMember_IsFirstColumnFourthRow()
    {
        var area = ItemTargetPickerView.CardArea(6, _cardsTopLeft);

        Assert.Equal(new Rectangle(384, 300 + (3 * MemberCardView.Height), 560, MemberCardView.Height), area);
    }
}
