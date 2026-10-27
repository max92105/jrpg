using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Saving;
using Jrpg.Core.World;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;

namespace Jrpg.Data.Tests.Saving;

public sealed class FileSaveStoreTests : IDisposable
{
    private const string Slot = "slot_01";

    private static readonly SaveGame _save = new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 17, 20, 30, 15, TimeSpan.FromHours(-4)),
        PlayTime = TimeSpan.FromSeconds(4_321.25),
        Location = new MapLocation("start", new Point2D(10.25, 3.5), Facing.Left),
        Members =
        [
            new PartyMemberProgress("sarxian", Level: 3, Experience: 42, Hp: 77, Worn: new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = "bronze_sword" }, LastingAmounts: new Dictionary<string, int>()),
            new PartyMemberProgress("jazog", Level: 1, Experience: 0, Hp: 120, Worn: new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Chest] = "wizard_robe" }, LastingAmounts: new Dictionary<string, int>()),
        ],
        ActiveMemberIds = ["sarxian", "jazog"],
        Inventory = [new InventoryEntry("potion", 4), new InventoryEntry("bomb", 3)],
        CompletedEvents = [new MapEventKey("start", "0bff6243_0")],
        ObjectSwaps = [new ObjectSwap("start", "0bff6243", "chest_opened")],
    };

    // The repository content, which the sample save fits, so reading it changes nothing.
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-saves-{Guid.NewGuid():N}");
    private readonly FileSaveStore _store;

    public FileSaveStoreTests()
    {
        _store = new FileSaveStore(_directory, _catalog);
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
        _store.Write(Slot, _save);

        var repaired = _store.Read(Slot)!;
        var read = repaired.Save;

        Assert.Equal((_save.SavedAt, _save.PlayTime, _save.Location), (read.SavedAt, read.PlayTime, read.Location));
        Assert.Equal(_save.Members, read.Members);
        Assert.Equal(_save.ActiveMemberIds, read.ActiveMemberIds);
        Assert.Equal(_save.Inventory, read.Inventory);
        Assert.Equal(_save.CompletedEvents, read.CompletedEvents);
        Assert.Equal(_save.ObjectSwaps, read.ObjectSwaps);
        Assert.Empty(repaired.Changes);
    }

    // The repository has no lasting resource yet (LR3), so the store reads against a copy where Sarxian's class has
    // Focus, a lasting resource.
    [Fact]
    public void Read_AfterWrite_KeepsLastingAmounts()
    {
        var classId = _catalog.PartyMembers["sarxian"].ClassId;
        var focus = new ResourceDefinition("focus", "Focus", Max: 3, StartsAt: 3) { IsLasting = true };
        var classes = new Dictionary<string, ClassDefinition>(_catalog.Classes) { [classId] = _catalog.Classes[classId] with { Resources = [focus] } };
        var store = new FileSaveStore(_directory, _catalog with { Classes = classes });
        var save = _save with { Members = [_save.Members[0] with { LastingAmounts = new Dictionary<string, int> { ["focus"] = 1 } }, _save.Members[1]] };
        store.Write(Slot, save);

        var read = store.Read(Slot)!.Save;

        Assert.Equal(save.Members, read.Members);
    }

    // The repository has no ring yet (G2), so the store reads against a copy where one exists; the slot keys are
    // written in camelCase, like every other name in the file.
    [Fact]
    public void SaveThenLoad_RingsInBothSlots_RoundTrips()
    {
        var ring = new ItemDefinition { Id = "power_ring", Name = "Power Ring", Description = "", IconFile = "Armor.png", MaxStack = 99, IsUsableFromMenu = false, Equipment = new EquipmentStats(EquipmentKind.Ring) };
        var store = new FileSaveStore(_directory, _catalog with { Items = new Dictionary<string, ItemDefinition>(_catalog.Items) { ["power_ring"] = ring } });
        var worn = new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = "bronze_sword", [EquipmentSlot.FirstRing] = "power_ring", [EquipmentSlot.SecondRing] = "power_ring" };
        var save = _save with { Members = [_save.Members[0] with { Worn = worn }, _save.Members[1]] };
        store.Write(Slot, save);

        var read = store.Read(Slot)!.Save;

        Assert.Equal(save.Members, read.Members);
        Assert.Contains("\"firstRing\": \"power_ring\"", File.ReadAllText(Path.Combine(_directory, "slot_01.json")), StringComparison.Ordinal);
    }

    [Fact]
    public void Write_File_HoldsTheFormatFourEnvelope()
    {
        _store.Write(Slot, _save);

        var json = File.ReadAllText(Path.Combine(_directory, "slot_01.json"));

        Assert.StartsWith("{\n  \"formatVersion\": 4,\n  \"game\": {", json.ReplaceLineEndings("\n"), StringComparison.Ordinal);
    }

    [Fact]
    public void Read_NoFile_ReturnsNull()
    {
        var read = _store.Read(Slot);

        Assert.Null(read);
    }

    [Fact]
    public void Read_FileWithoutFormatVersion_ThrowsUnreadable()
    {
        WriteRaw("""
            { "Characters": [], "Inventory": [], "CurrentMap": "start.json", "PlayerX": 10, "PlayerY": 3,
              "ActivePartyOrder": ["Sarxian"], "Gold": 0, "PartyAverageLevel": 1, "LocationName": "start",
              "SaveTimestamp": "2026-10-12T17:23:00", "PlayTimeSeconds": 60, "CompletedEvents": [], "ObjectSwaps": {} }
            """);

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("it's not a save from this version of the game.", exception.Reason);
        Assert.Equal(Slot, exception.SlotName);
    }

    // Saves before format 4 kept only a weapon and an armor (ADR 0028). Until the first release an older save is
    // refused, not migrated (ADR 0022).
    [Fact]
    public void Read_FormatThreeSave_ThrowsAsOlder()
    {
        WriteRaw("""{ "formatVersion": 3, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("it was made by an older version of the game (format 3).", exception.Reason);
    }

    [Fact]
    public void Read_NewerFormatVersion_ThrowsNamingTheVersion()
    {
        WriteRaw("""{ "formatVersion": 5, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("it was made by a newer version of the game (format 5).", exception.Reason);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("\"1\"")]
    [InlineData("1.5")]
    public void Read_InvalidFormatVersion_ThrowsNamingIt(string version)
    {
        WriteRaw($$"""{ "formatVersion": {{version}}, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal($"its format version '{version}' isn't valid.", exception.Reason);
    }

    [Fact]
    public void Read_MalformedJson_ThrowsNamingTheSlot()
    {
        WriteRaw("""{ "formatVersion": 4, "game": { """);

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.StartsWith("Save 'slot_01' can't be read: the file is damaged", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_MissingRequiredField_ThrowsNamingTheField()
    {
        _store.Write(Slot, _save);
        var path = Path.Combine(_directory, "slot_01.json");
        var withoutPlayTime = string.Join('\n', File.ReadAllLines(path).Where(line => !line.Contains("\"playTime\"", StringComparison.Ordinal)));
        File.WriteAllText(path, withoutPlayTime);

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Contains("missing required properties", exception.Reason, StringComparison.Ordinal);
        Assert.Contains("playTime", exception.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_UnknownField_ThrowsNamingTheField()
    {
        _store.Write(Slot, _save);
        var path = Path.Combine(_directory, "slot_01.json");
        File.WriteAllText(path, File.ReadAllText(path).Replace("\"playTime\"", "\"playtime\"", StringComparison.Ordinal));

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Contains("'playtime' could not be mapped", exception.Reason, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("formatVersion")]
    [InlineData("game")]
    public void Read_DuplicateProperty_ThrowsUnreadable(string property)
    {
        WriteRaw(property == "game"
            ? """{ "formatVersion": 4, "game": {}, "game": {} }"""
            : """{ "formatVersion": 4, "formatVersion": 4, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.StartsWith("the file is damaged", exception.Reason, StringComparison.Ordinal);
    }

    public static TheoryData<string> ListsWithANullEntry => ["members", "activeMemberIds", "inventory", "completedEvents", "objectSwaps"];

    [Theory]
    [MemberData(nameof(ListsWithANullEntry))]
    public void Read_NullListEntry_ThrowsUnreadable(string list)
    {
        var save = list switch
        {
            "members" => _save with { Members = [_save.Members[0], null!] },
            "activeMemberIds" => _save with { ActiveMemberIds = ["sarxian", null!] },
            "inventory" => _save with { Inventory = [_save.Inventory[0], null!] },
            "completedEvents" => _save with { CompletedEvents = [null!] },
            _ => _save with { ObjectSwaps = [null!] },
        };
        _store.Write(Slot, save);

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("the file is damaged (a list holds a null entry).", exception.Reason);
    }

    [Fact]
    public void Read_NullWornItem_ThrowsUnreadable()
    {
        var worn = new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = null! };
        _store.Write(Slot, _save with { Members = [_save.Members[0] with { Worn = worn }, _save.Members[1]] });

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("the file is damaged (a worn slot holds a null item).", exception.Reason);
    }

    // A slot this build doesn't know is an unknown enum name, refused like any unknown field.
    [Fact]
    public void Read_UnknownSlotName_ThrowsUnreadable()
    {
        _store.Write(Slot, _save);
        var path = Path.Combine(_directory, "slot_01.json");
        File.WriteAllText(path, File.ReadAllText(path).Replace("\"weapon\": \"bronze_sword\"", "\"thirdRing\": \"bronze_sword\"", StringComparison.Ordinal));

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.StartsWith("the file is damaged", exception.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ExistingSlot_ReplacesIt()
    {
        _store.Write(Slot, _save);

        _store.Write(Slot, _save with { PlayTime = TimeSpan.FromHours(2) });

        Assert.Equal(TimeSpan.FromHours(2), _store.Read(Slot)!.Save.PlayTime);
        Assert.Equal(["slot_01.json"], Directory.GetFiles(_directory).Select(Path.GetFileName));
    }

    [Theory]
    [InlineData("Slot1")]
    [InlineData("../escape")]
    [InlineData("")]
    public void Write_InvalidSlotName_Throws(string slotName)
    {
        Assert.Throws<ArgumentException>(() => _store.Write(slotName, _save));
    }

    private void WriteRaw(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, $"{Slot}.json"), json);
    }
}
