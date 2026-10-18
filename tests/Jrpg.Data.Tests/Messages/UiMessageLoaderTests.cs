using Jrpg.Data.Content;
using Jrpg.Data.Messages;

namespace Jrpg.Data.Tests.Messages;

public sealed class UiMessageLoaderTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-messages-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_contentRoot))
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
    }

    [Fact]
    public void Load_RepositoryContent_Succeeds()
    {
        var messages = UiMessageLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.NotEmpty(messages);
    }

    [Fact]
    public void Load_ValidTable_ReturnsTheTexts()
    {
        WriteTable("""{ "found_item": "Obtained {item}!" }""");

        var messages = UiMessageLoader.Load(_contentRoot);

        Assert.Equal("Obtained {item}!", messages["found_item"]);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"  \"")]
    [InlineData("null")]
    public void Load_EmptyText_ThrowsNamingTheId(string text)
    {
        WriteTable($$"""{ "found_item": {{text}} }""");

        var exception = Assert.Throws<ContentValidationException>(() => UiMessageLoader.Load(_contentRoot));

        Assert.Equal("text/ui.json: message 'found_item' has no text.", exception.Message);
    }

    [Theory]
    [InlineData("Obtained {item!")]
    [InlineData("Obtained item}!")]
    [InlineData("Obtained {{item}}!")]
    [InlineData("Obtained {}!")]
    public void Load_UnbalancedBrace_ThrowsNamingTheId(string text)
    {
        WriteTable($$"""{ "found_item": "{{text}}" }""");

        var exception = Assert.Throws<ContentValidationException>(() => UiMessageLoader.Load(_contentRoot));

        Assert.Equal("text/ui.json: message 'found_item' has an unbalanced brace or an empty placeholder.", exception.Message);
    }

    private void WriteTable(string json)
    {
        var path = Path.Combine(_contentRoot, "text", "ui.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }
}
