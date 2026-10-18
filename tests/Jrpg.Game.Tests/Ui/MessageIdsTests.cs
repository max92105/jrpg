using System.Reflection;
using Jrpg.Data.Messages;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class MessageIdsTests
{
    [Fact]
    public void EveryConstant_ExistsInRepositoryContent()
    {
        var messages = UiMessageLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var ids = typeof(MessageIds).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();

        Assert.NotEmpty(ids);
        Assert.All(ids, id => Assert.True(messages.ContainsKey(id), $"content/text/ui.json has no '{id}'."));
    }
}
