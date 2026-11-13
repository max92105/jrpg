using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A moveNpc step is read from a map layout's events (ADR 0044): a copy of the test world gives the villager's talk one
// before its line, and the loader reads its path in order.
public sealed class ContentCatalogLoaderMoveNpcTests : IDisposable
{
    private const string VillagerTalkSteps =
        "\"id\": \"villager_1_0\",\n          \"activation\": \"interact\",\n          \"prompt\": \"Talk\",\n"
        + "          \"isOnce\": false,\n          \"onlyWhenObjectId\": null,\n          \"steps\": [";

    private const string MoveStep =
        "\n            { \"type\": \"moveNpc\", \"objectUid\": \"villager_1\", \"path\": [ \"left\", \"left\", \"down\" ] },";

    private readonly CopiedContentRoot _content = new("move-npc", "test-world");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_EventWithMoveNpc_ReadsPathInOrder()
    {
        GiveTheVillagerAMoveStep();

        var catalog = ContentCatalogLoader.Load(_content.Path);

        var villager = catalog.Maps["start"].Layout.Objects.Single(placed => placed.Uid == "villager_1");
        var move = Assert.IsType<MoveNpcStep>(villager.Events[0].Steps[0]);
        Assert.Equal("villager_1", move.ObjectUid);
        Assert.Equal([Facing.Left, Facing.Left, Facing.Down], move.Path);
    }

    // The step goes in by text, so the file's indentation is normalized first; the loader doesn't mind the layout.
    private void GiveTheVillagerAMoveStep()
    {
        var file = _content.PathOf(Path.Combine("map-layouts", "start.json"));
        var text = File.ReadAllText(file).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Contains(VillagerTalkSteps, text, StringComparison.Ordinal);
        File.WriteAllText(file, text.Replace(VillagerTalkSteps, VillagerTalkSteps + MoveStep, StringComparison.Ordinal));
    }
}
