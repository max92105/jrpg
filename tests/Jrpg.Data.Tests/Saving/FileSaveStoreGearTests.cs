using Jrpg.Core.Items;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

// A save keeps each piece with its rolls (ADR 0029, format 5): the base's id, the rarity, each affix's id and value.
public sealed class FileSaveStoreGearTests : IDisposable
{
    private const string Slot = "slot_01";

    private static readonly SavedGear _rareSword = new("bronze_sword", Rarity.Rare, [new SavedAffix("power", 4), new SavedAffix("power", 2)]);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-saves-{Guid.NewGuid():N}");
    private readonly FileSaveStore _store;

    public FileSaveStoreGearTests()
    {
        _store = new FileSaveStore(_directory, CatalogWithPowerAffix);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // The stat isn't written: it comes from the pool on load.
    [Fact]
    public void SaveThenLoad_RolledPieceWorn_RoundTrips()
    {
        var worn = new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.Weapon] = _rareSword };
        var save = StoredSave with { Members = [StoredSave.Members[0] with { Worn = worn }, StoredSave.Members[1]] };
        _store.Write(Slot, save);

        var repaired = _store.Read(Slot)!;

        Assert.Equal(save.Members, repaired.Save.Members);
        Assert.Empty(repaired.Changes);
        var json = File.ReadAllText(Path.Combine(_directory, $"{Slot}.json"));
        Assert.Contains("\"rarity\": \"rare\"", json, StringComparison.Ordinal);
        Assert.Contains("\"affixId\": \"power\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"stat\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveThenLoad_OwnedPiecesInOrder_RoundTrips()
    {
        var save = StoredSave with { Gear = [Plain("power_ring"), _rareSword, Plain("bronze_sword")] };
        _store.Write(Slot, save);

        var read = _store.Read(Slot)!.Save;

        Assert.Equal(save.Gear, read.Gear);
    }
}
