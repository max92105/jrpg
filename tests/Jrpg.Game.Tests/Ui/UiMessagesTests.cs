using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiMessagesTests
{
    private readonly UiMessages _messages = new(new Dictionary<string, string>
    {
        ["title"] = "Load",
        ["found_items"] = "Obtained {item} x{count}!",
    });

    [Fact]
    public void Text_KnownId_ReturnsTheText()
    {
        Assert.Equal("Load", _messages.Text("title"));
    }

    [Fact]
    public void Text_UnknownId_Throws()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() => _messages.Text("tilte"));

        Assert.Contains("'tilte'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_NamedPlaceholder_IsReplaced()
    {
        var text = _messages.Format("found_items", new MessageArgument("count", "2"), new MessageArgument("item", "Potion"));

        Assert.Equal("Obtained Potion x2!", text);
    }

    [Fact]
    public void Format_MissingArgument_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => _messages.Format("found_items", new MessageArgument("item", "Potion")));

        Assert.Contains("{count}", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_NoPlaceholder_ReturnsTheText()
    {
        Assert.Equal("Load", _messages.Format("title"));
    }
}
