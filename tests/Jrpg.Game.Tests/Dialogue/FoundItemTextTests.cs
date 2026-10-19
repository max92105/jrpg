using Jrpg.Core.Exploration;
using Jrpg.Core.Items;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Dialogue;

public sealed class FoundItemTextTests
{
    private static readonly UiMessages _messages = new(new Dictionary<string, string>
    {
        [MessageIds.ItemFound] = "Obtained {item}!",
        [MessageIds.ItemFoundMany] = "Obtained {item} x{count}!",
        [MessageIds.ItemFoundLost] = "{lost} didn't fit and were left behind.",
    });

    private static readonly ItemDefinition _potion = new()
    {
        Id = "potion",
        Name = "Potion",
        Description = "Heals.",
        IconFile = "Consumable.png",
        MaxStack = 99,
        IsUsableFromMenu = true,
    };

    [Fact]
    public void Of_OneCopy_UsesFoundMessage()
    {
        var text = FoundItemText.Of(new ItemReceipt(_potion, Added: 1, Lost: 0), _messages);

        Assert.Equal("Obtained Potion!", text);
    }

    [Fact]
    public void Of_TwoCopies_UsesManyMessage()
    {
        var text = FoundItemText.Of(new ItemReceipt(_potion, Added: 2, Lost: 0), _messages);

        Assert.Equal("Obtained Potion x2!", text);
    }

    [Fact]
    public void Of_SomeLost_MentionsLostCopies()
    {
        var text = FoundItemText.Of(new ItemReceipt(_potion, Added: 1, Lost: 2), _messages);

        Assert.Equal("Obtained Potion! 2 didn't fit and were left behind.", text);
    }

    [Fact]
    public void Of_AllLost_OnlyMentionsLostCopies()
    {
        var text = FoundItemText.Of(new ItemReceipt(_potion, Added: 0, Lost: 3), _messages);

        Assert.Equal("3 didn't fit and were left behind.", text);
    }
}
