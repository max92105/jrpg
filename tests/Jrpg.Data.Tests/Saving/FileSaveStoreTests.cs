using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
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
            new PartyMemberProgress("sarxian", Level: 3, Experience: 42, Hp: 77, WeaponId: "bronze_sword", ArmorId: null),
            new PartyMemberProgress("jazog", Level: 1, Experience: 0, Hp: 120, WeaponId: null, ArmorId: "wizard_robe"),
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

    [Fact]
    public void Write_File_HoldsTheFormatTwoEnvelope()
    {
        _store.Write(Slot, _save);

        var json = File.ReadAllText(Path.Combine(_directory, "slot_01.json"));

        Assert.StartsWith("{\n  \"formatVersion\": 2,\n  \"game\": {", json.ReplaceLineEndings("\n"), StringComparison.Ordinal);
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

    // Saves before format 2 kept MP. Until the first release an older save is refused, not migrated (ADR 0022).
    [Fact]
    public void Read_FormatOneSave_ThrowsAsOlder()
    {
        WriteRaw("""{ "formatVersion": 1, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("it was made by an older version of the game (format 1).", exception.Reason);
    }

    [Fact]
    public void Read_NewerFormatVersion_ThrowsNamingTheVersion()
    {
        WriteRaw("""{ "formatVersion": 3, "game": {} }""");

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("it was made by a newer version of the game (format 3).", exception.Reason);
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
        WriteRaw("""{ "formatVersion": 2, "game": { """);

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
            ? """{ "formatVersion": 2, "game": {}, "game": {} }"""
            : """{ "formatVersion": 2, "formatVersion": 2, "game": {} }""");

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
