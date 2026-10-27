using Jrpg.Game.Menus;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Menus;

public sealed class ItemDetailLineTests
{
    private const int ListBottom = 520;
    private const int StripHeight = 40;

    private static readonly InventoryRow _potion = new("potion", "Potion", "potion.png", new MeasuredText("5", 10), "Restores HP.", IsUsable: true);

    [Fact]
    public void TextFor_NoNotice_IsTheSelectedDescription()
    {
        var line = new ItemDetailLine();

        Assert.Equal("Restores HP.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_DescriptionHidden_IsNull()
    {
        var line = new ItemDetailLine();

        line.ToggleDescription();

        Assert.Null(line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_DescriptionHiddenThenShown_IsTheDescription()
    {
        var line = new ItemDetailLine();
        line.ToggleDescription();

        line.ToggleDescription();

        Assert.Equal("Restores HP.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_NoticeWhileDescriptionHidden_IsTheNotice()
    {
        var line = new ItemDetailLine();
        line.ToggleDescription();

        line.ShowNotice("Potion was used.");

        Assert.Equal("Potion was used.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_AfterShowNotice_IsTheNotice()
    {
        var line = new ItemDetailLine();

        line.ShowNotice("Potion was used.");

        Assert.Equal("Potion was used.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_AfterClearNotice_IsTheDescriptionAgain()
    {
        var line = new ItemDetailLine();
        line.ShowNotice("Potion was used.");

        line.ClearNotice();

        Assert.Equal("Restores HP.", line.TextFor(_potion));
    }

    [Fact]
    public void TextFor_NothingSelectedAndNoNotice_IsNull()
    {
        var line = new ItemDetailLine();

        Assert.Null(line.TextFor(selected: null));
    }

    [Fact]
    public void TopFor_RowWithRoomBelow_IsUnderTheRow()
    {
        var row = new Rectangle(0, 100, 600, 52);

        Assert.Equal(row.Bottom, ItemDetailLine.TopFor(row, ListBottom, StripHeight));
    }

    [Fact]
    public void TopFor_RowExactlyFillingTheRoomBelow_IsUnderTheRow()
    {
        var row = new Rectangle(0, ListBottom - StripHeight - 52, 600, 52);

        Assert.Equal(row.Bottom, ItemDetailLine.TopFor(row, ListBottom, StripHeight));
    }

    [Fact]
    public void TopFor_LastVisibleRow_IsAboveTheRow()
    {
        var row = new Rectangle(0, ListBottom - 52, 600, 52);

        Assert.Equal(row.Y - StripHeight, ItemDetailLine.TopFor(row, ListBottom, StripHeight));
    }
}
