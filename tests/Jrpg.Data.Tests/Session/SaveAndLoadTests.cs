using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;

namespace Jrpg.Data.Tests.Session;

public sealed class SaveAndLoadTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-session-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void SaveToFileAndRestore_RepositoryContent_ContinuesWhereSaved()
    {
        // Below the chest at (19.0, 20.05) on start, facing it.
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var belowChest = new MapLocation("start", new Point2D(19.5, 21.5), Facing.Up);
        var session = GameSession.StartNew(catalog with { NewGame = catalog.NewGame with { StartLocation = belowChest } }, "sarxian", new SeededRandom(1));
        session.Exploration.TryInteract();
        session.Exploration.ContinueEvent();
        session.Exploration.Walk(MoveIntent.FromAxes(-1, 0), TimeSpan.FromSeconds(0.5));
        session.AddPlayTime(TimeSpan.FromMinutes(3));
        var store = new FileSaveStore(_directory, catalog);

        store.Write("slot_01", session.Capture(new DateTimeOffset(2026, 10, 17, 21, 0, 0, TimeSpan.Zero)));
        var loaded = store.Read("slot_01")!;
        var restored = GameSession.Restore(loaded.Save, catalog, new SeededRandom(2));

        Assert.Empty(loaded.Changes);
        Assert.Equal(6, restored.Inventory.QuantityOf("potion"));
        Assert.Equal(session.Inventory.Entries, restored.Inventory.Entries);
        Assert.Equal(session.Exploration.Location, restored.Exploration.Location);
        Assert.Equal(TimeSpan.FromMinutes(3), restored.PlayTime);
    }

    [Fact]
    public void SaveToFileAndRestore_OpenedChest_StaysOpen()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var belowChest = new MapLocation("start", new Point2D(19.5, 21.5), Facing.Up);
        var session = GameSession.StartNew(catalog with { NewGame = catalog.NewGame with { StartLocation = belowChest } }, "sarxian", new SeededRandom(1));
        session.Exploration.TryInteract();
        session.Exploration.ContinueEvent();
        var store = new FileSaveStore(_directory, catalog);

        store.Write("slot_01", session.Capture(DateTimeOffset.UnixEpoch));
        var restored = GameSession.Restore(store.Read("slot_01")!.Save, catalog, new SeededRandom(2));

        Assert.False(restored.Exploration.TryInteract());
    }
}
