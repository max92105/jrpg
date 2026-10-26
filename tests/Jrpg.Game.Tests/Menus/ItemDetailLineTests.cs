using Jrpg.Game.Menus;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Menus;

public sealed class ItemDetailLineTests
{
    private const int RowHeight = 52;
    private const int StripHeight = 40;
    private const int ListBottom = 10 * RowHeight;

    private static readonly InventoryRow _potion = new("potion", "Potion", "potion.png", new MeasuredText("3", 12), "Heals a little.", IsUsable: true);

    [Fact]
    public void TextFor_NoNotice_IsTheSelectedDescription()
    {
        var line = new ItemDetailLine();

        var text = line.TextFor(_potion);

        Assert.Equal("Heals a little.", text);
    }

    [Fact]
    public void TextFor_AfterShowNotice_IsTheNotice()
    {
        var line = new ItemDetailLine();

        line.ShowNotice("Sarxian recovers 30 HP.");

        Assert.Equal("Sarxian recovers 30 HP.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_AfterClearNotice_IsTheDescriptionAgain()
    {
        var line = new ItemDetailLine();
        line.ShowNotice("Sarxian recovers 30 HP.");

        line.ClearNotice();

        Assert.Equal("Heals a little.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_EmptyCategoryAfterANotice_IsTheNotice()
    {
        var line = new ItemDetailLine();

        line.ShowNotice("Potion was used.");

        Assert.Equal("Potion was used.", line.TextFor(selected: null));
    }

    [Fact]
    public void TopFor_RowWithRoomBelow_IsUnderTheRow()
    {
        var row = new Rectangle(0, 2 * RowHeight, 400, RowHeight);

        var top = ItemDetailLine.TopFor(row, ListBottom, StripHeight);

        Assert.Equal(row.Bottom, top);
    }

    [Fact]
    public void TopFor_LastVisibleRow_IsAboveTheRow()
    {
        var row = new Rectangle(0, 9 * RowHeight, 400, RowHeight);

        var top = ItemDetailLine.TopFor(row, ListBottom, StripHeight);

        Assert.Equal(row.Y - StripHeight, top);
    }

    [Fact]
    public void TopFor_StripEndingAtTheListBottom_IsUnderTheRow()
    {
        var row = new Rectangle(0, ListBottom - RowHeight - StripHeight, 400, RowHeight);

        var top = ItemDetailLine.TopFor(row, ListBottom, StripHeight);

        Assert.Equal(row.Bottom, top);
    }
}
