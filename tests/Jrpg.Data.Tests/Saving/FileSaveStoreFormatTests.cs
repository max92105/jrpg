using Jrpg.Core.Items;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

/// <summary>The save file's envelope, and the damaged, foreign or older files the store refuses.</summary>
public sealed class FileSaveStoreFormatTests : IDisposable
{
    private const string Slot = "slot_01";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-saves-{Guid.NewGuid():N}");
    private readonly FileSaveStore _store;

    public FileSaveStoreFormatTests()
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
    public void Write_File_HoldsTheFormatFourEnvelope()
    {
        _store.Write(Slot, StoredSave);

        var json = File.ReadAllText(Path.Combine(_directory, "slot_01.json"));

        Assert.StartsWith("{\n  \"formatVersion\": 4,\n  \"game\": {", json.ReplaceLineEndings("\n"), StringComparison.Ordinal);
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
        _store.Write(Slot, StoredSave);
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
        _store.Write(Slot, StoredSave);
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
            "members" => StoredSave with { Members = [StoredSave.Members[0], null!] },
            "activeMemberIds" => StoredSave with { ActiveMemberIds = ["sarxian", null!] },
            "inventory" => StoredSave with { Inventory = [StoredSave.Inventory[0], null!] },
            "completedEvents" => StoredSave with { CompletedEvents = [null!] },
            _ => StoredSave with { ObjectSwaps = [null!] },
        };
        _store.Write(Slot, save);

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("the file is damaged (a list holds a null entry).", exception.Reason);
    }

    [Fact]
    public void Read_NullWornItem_ThrowsUnreadable()
    {
        var worn = new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = null! };
        _store.Write(Slot, StoredSave with { Members = [StoredSave.Members[0] with { Worn = worn }, StoredSave.Members[1]] });

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.Equal("the file is damaged (a worn slot holds a null item).", exception.Reason);
    }

    // A slot this build doesn't know is an unknown enum name, refused like any unknown field.
    [Fact]
    public void Read_UnknownSlotName_ThrowsUnreadable()
    {
        _store.Write(Slot, StoredSave);
        var path = Path.Combine(_directory, "slot_01.json");
        File.WriteAllText(path, File.ReadAllText(path).Replace("\"weapon\": \"bronze_sword\"", "\"thirdRing\": \"bronze_sword\"", StringComparison.Ordinal));

        var exception = Assert.Throws<UnreadableSaveException>(() => _store.Read(Slot));

        Assert.StartsWith("the file is damaged", exception.Reason, StringComparison.Ordinal);
    }

    private void WriteRaw(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, $"{Slot}.json"), json);
    }
}
