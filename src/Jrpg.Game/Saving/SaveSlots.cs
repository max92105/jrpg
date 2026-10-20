using System.Diagnostics;
using System.Globalization;
using Jrpg.Core.Content;
using Jrpg.Core.Saving;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;

namespace Jrpg.Game.Saving;

/// <summary>
/// The player's manual save files, <c>slot_01</c> to <c>slot_20</c> (decisions 34 and 62), over the store that writes
/// and repairs them.
/// </summary>
/// <param name="store">Where the files are.</param>
/// <param name="catalog">Map and member names for the summaries.</param>
/// <param name="clock">The time a save is stamped with; the real clock in the game, a fixed one in tests (ADR 0008).</param>
internal sealed class SaveSlots(FileSaveStore store, ContentCatalog catalog, Func<DateTimeOffset> clock)
{
    public const int Count = 20;

    /// <param name="number">1 to <see cref="Count"/>.</param>
    public static string NameOf(int number) => string.Create(CultureInfo.InvariantCulture, $"slot_{number:00}");

    /// <summary>Every file's entry in order. Reads the disk, so it runs when a list opens, never every frame.</summary>
    public IReadOnlyList<SaveSlotEntry> ReadAll() => [.. Enumerable.Range(1, Count).Select(Read)];

    /// <summary>Saves the session into the file, replacing what was there.</summary>
    /// <param name="number">1 to <see cref="Count"/>.</param>
    /// <param name="session">Between events, as the menu only opens then (ADR 0007).</param>
    public void Write(int number, GameSession session) => store.Write(NameOf(number), session.Capture(clock()));

    /// <summary>
    /// The save in the file, fitted to the current content, for loading. What the repair changed goes to the debug output
    /// (decision 63).
    /// </summary>
    /// <param name="number">1 to <see cref="Count"/>.</param>
    /// <returns>Null when the file is empty or can't be read, which can happen after a list showed it readable.</returns>
    public SaveGame? Load(int number)
    {
        var (repaired, _) = ReadRepaired(number);
        foreach (var change in repaired?.Changes ?? [])
        {
            Debug.WriteLine($"Loading {NameOf(number)}: {change}");
        }

        return repaired?.Save;
    }

    private SaveSlotEntry Read(int number)
    {
        var (repaired, isDamaged) = ReadRepaired(number);
        if (repaired is not null)
        {
            return SaveSlotEntry.Saved(number, SummaryOf(repaired.Save));
        }

        return isDamaged ? SaveSlotEntry.Damaged(number) : SaveSlotEntry.Empty(number);
    }

    private (RepairedSave? Save, bool IsDamaged) ReadRepaired(int number)
    {
        try
        {
            return (store.Read(NameOf(number)), false);
        }
        catch (Exception exception) when (exception is UnreadableSaveException or IOException or UnauthorizedAccessException)
        {
            // The player sees only that the file is damaged, even when another program merely holds it open; the reason is
            // for whoever debugs it.
            Debug.WriteLine(exception.Message);
            return (null, true);
        }
    }

    // A repaired save fits the content, so its map and members exist in the catalog.
    private SaveSummary SummaryOf(SaveGame save)
    {
        var levels = save.Members.ToDictionary(member => member.Id, member => member.Level);
        var party = save.ActiveMemberIds.Select(id => new PartyMemberSummary(catalog.PartyMembers[id].Name, levels[id])).ToList();
        return new SaveSummary(catalog.Maps[save.Location.MapId].Settings.DisplayName, save.PlayTime, save.SavedAt, party);
    }
}
