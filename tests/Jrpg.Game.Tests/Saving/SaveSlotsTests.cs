using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Saving;

public sealed class SaveSlotsTests : IDisposable
{
    private static readonly ContentCatalog _catalog = StartMapScenarios.AfterTheIntro;
    private static readonly DateTimeOffset _now = new(2026, 10, 19, 21, 5, 0, TimeSpan.FromHours(-4));

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-slots-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;

    public SaveSlotsTests()
    {
        _slots = new SaveSlots(new FileSaveStore(_directory, _catalog), _catalog, () => _now);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void NameOf_FirstSlot_IsSlot01()
    {
        Assert.Equal("slot_01", SaveSlots.NameOf(1));
    }

    [Fact]
    public void ReadAll_EmptyFolder_GivesTwentyEmptyEntriesInOrder()
    {
        var entries = _slots.ReadAll();

        Assert.Equal(Enumerable.Range(1, 20), entries.Select(entry => entry.Number));
        Assert.All(entries, entry => Assert.True(entry.IsEmpty));
    }

    [Fact]
    public void Write_ThenReadAll_SlotShowsMapNameAndPlayTime()
    {
        var session = StartMapScenarios.StartWithWholeParty(_catalog);
        session.AddPlayTime(TimeSpan.FromMinutes(75));
        session.Party.TryAddToActive(session.Party.MemberById("teelle"));

        _slots.Write(3, session);

        var summary = _slots.ReadAll()[2].Summary;
        Assert.NotNull(summary);
        Assert.Equal("start", summary.LocationName);
        Assert.Equal(TimeSpan.FromMinutes(75), summary.PlayTime);
        Assert.Equal(_now, summary.SavedAt);
        Assert.Equal([new PartyMemberSummary("Sarxian", 1), new PartyMemberSummary("Teelle", 1)], summary.ActiveParty);
    }

    [Fact]
    public void ReadAll_DamagedFile_GivesDamagedEntry()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "slot_02.json"), "{ not json");

        var entry = _slots.ReadAll()[1];

        Assert.True(entry.IsDamaged);
        Assert.Null(entry.Summary);
    }

    [Fact]
    public void ReadAll_FileHeldOpenElsewhere_GivesDamagedEntryForItOnly()
    {
        _slots.Write(4, GameSession.StartNew(_catalog, "sarxian", new SeededRandom(1)));
        using var lockedByAnotherProgram = new FileStream(Path.Combine(_directory, "slot_04.json"), FileMode.Open, FileAccess.Read, FileShare.None);

        var entries = _slots.ReadAll();

        Assert.True(entries[3].IsDamaged);
        Assert.True(entries[4].IsEmpty);
    }

    [Fact]
    public void Load_SavedSlot_GivesTheSave()
    {
        var session = GameSession.StartNew(_catalog, "sarxian", new SeededRandom(1));
        session.AddPlayTime(TimeSpan.FromMinutes(3));
        _slots.Write(7, session);

        var save = _slots.Load(7);

        Assert.NotNull(save);
        Assert.Equal(TimeSpan.FromMinutes(3), save.PlayTime);
        Assert.Equal(_now, save.SavedAt);
    }

    [Fact]
    public void Load_EmptyOrDamagedSlot_GivesNull()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "slot_02.json"), "{ not json");

        Assert.Null(_slots.Load(1));
        Assert.Null(_slots.Load(2));
    }

    [Fact]
    public void MostRecent_PicksLatestSavedAt()
    {
        var session = GameSession.StartNew(_catalog, "sarxian", new SeededRandom(1));
        _slots.Write(2, session);
        var later = new SaveSlots(new FileSaveStore(_directory, _catalog), _catalog, () => _now.AddDays(1));
        later.Write(9, session);

        var latest = SaveSlots.MostRecent(_slots.ReadAll());

        Assert.Equal(9, latest?.Number);
    }

    [Fact]
    public void MostRecent_NoReadableSave_ReturnsNull()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "slot_02.json"), "{ not json");

        Assert.Null(SaveSlots.MostRecent(_slots.ReadAll()));
    }
}
