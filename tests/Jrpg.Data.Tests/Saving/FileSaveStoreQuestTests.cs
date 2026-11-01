using Jrpg.Core.Quests;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

// A save file keeps the quest log (format 7): each started quest's stage and completion, in start order.
public sealed class FileSaveStoreQuestTests : IDisposable
{
    private const string Slot = "slot_01";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-saves-{Guid.NewGuid():N}");
    private readonly FileSaveStore _store;

    public FileSaveStoreQuestTests()
    {
        _store = new FileSaveStore(_directory, Catalog);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Write_ThenRead_KeepsQuests()
    {
        IReadOnlyList<QuestProgress> quests = [new QuestProgress("placeholder_errand", "return_to_giver", IsCompleted: true)];
        _store.Write(Slot, StoredSave with { Quests = quests });

        var read = _store.Read(Slot);

        Assert.NotNull(read);
        Assert.Equal(quests, read.Save.Quests);
        Assert.Empty(read.Changes);
    }
}
