using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

public sealed class FileSaveStoreTests : IDisposable
{
    private const string Slot = "slot_01";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-saves-{Guid.NewGuid():N}");
    private readonly FileSaveStore _store;

    public FileSaveStoreTests()
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
    public void Read_AfterWrite_ReturnsEquivalentSave()
    {
        _store.Write(Slot, StoredSave);

        var repaired = _store.Read(Slot)!;
        var read = repaired.Save;

        Assert.Equal((StoredSave.SavedAt, StoredSave.PlayTime, StoredSave.Location), (read.SavedAt, read.PlayTime, read.Location));
        Assert.Equal(StoredSave.Members, read.Members);
        Assert.Equal(StoredSave.ActiveMemberIds, read.ActiveMemberIds);
        Assert.Equal(StoredSave.Inventory, read.Inventory);
        Assert.Equal(StoredSave.CompletedEvents, read.CompletedEvents);
        Assert.Equal(StoredSave.ObjectSwaps, read.ObjectSwaps);
        Assert.Empty(repaired.Changes);
    }

    // The repository has no lasting resource yet (LR3), so the store reads against a copy where Sarxian's class has
    // Focus, a lasting resource.
    [Fact]
    public void Read_AfterWrite_KeepsLastingAmounts()
    {
        var classId = Catalog.PartyMembers["sarxian"].ClassId;
        var focus = new ResourceDefinition("focus", "Focus", Max: 3, StartsAt: 3) { IsLasting = true };
        var classes = new Dictionary<string, ClassDefinition>(Catalog.Classes) { [classId] = Catalog.Classes[classId] with { Resources = [focus] } };
        var store = new FileSaveStore(_directory, Catalog with { Classes = classes });
        var save = StoredSave with { Members = [StoredSave.Members[0] with { LastingAmounts = new Dictionary<string, int> { ["focus"] = 1 } }, StoredSave.Members[1]] };
        store.Write(Slot, save);

        var read = store.Read(Slot)!.Save;

        Assert.Equal(save.Members, read.Members);
    }

    // The slot keys are written in camelCase, like every other name in the file.
    [Fact]
    public void SaveThenLoad_RingsInBothSlots_RoundTrips()
    {
        var worn = new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = "bronze_sword", [EquipmentSlot.FirstRing] = "power_ring", [EquipmentSlot.SecondRing] = "power_ring" };
        var save = StoredSave with { Members = [StoredSave.Members[0] with { Worn = worn }, StoredSave.Members[1]] };
        _store.Write(Slot, save);

        var read = _store.Read(Slot)!.Save;

        Assert.Equal(save.Members, read.Members);
        Assert.Contains("\"firstRing\": \"power_ring\"", File.ReadAllText(Path.Combine(_directory, "slot_01.json")), StringComparison.Ordinal);
    }

    [Fact]
    public void Read_NoFile_ReturnsNull()
    {
        var read = _store.Read(Slot);

        Assert.Null(read);
    }

    [Fact]
    public void Write_ExistingSlot_ReplacesIt()
    {
        _store.Write(Slot, StoredSave);

        _store.Write(Slot, StoredSave with { PlayTime = TimeSpan.FromHours(2) });

        Assert.Equal(TimeSpan.FromHours(2), _store.Read(Slot)!.Save.PlayTime);
        Assert.Equal(["slot_01.json"], Directory.GetFiles(_directory).Select(Path.GetFileName));
    }

    [Theory]
    [InlineData("Slot1")]
    [InlineData("../escape")]
    [InlineData("")]
    public void Write_InvalidSlotName_Throws(string slotName)
    {
        Assert.Throws<ArgumentException>(() => _store.Write(slotName, StoredSave));
    }
}
