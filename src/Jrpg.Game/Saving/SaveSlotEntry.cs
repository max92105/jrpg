namespace Jrpg.Game.Saving;

/// <summary>One of the manual save files as a list shows it: empty, damaged, or a save's summary.</summary>
internal sealed record SaveSlotEntry
{
    private SaveSlotEntry(int number, SaveSummary? summary, bool isDamaged)
    {
        Number = number;
        Summary = summary;
        IsDamaged = isDamaged;
    }

    /// <summary>1 to <see cref="SaveSlots.Count"/>, as the player sees it ("File 1").</summary>
    public int Number { get; }

    /// <summary>The save's summary; null when the file is empty or damaged.</summary>
    public SaveSummary? Summary { get; }

    /// <summary>The file exists but can't be loaded; the reason went to the debug output.</summary>
    public bool IsDamaged { get; }

    public bool IsEmpty => Summary is null && !IsDamaged;

    /// <summary>Whether the file holds a save that can be loaded.</summary>
    public bool IsReadable => Summary is not null;

    public static SaveSlotEntry Empty(int number) => new(number, null, isDamaged: false);

    public static SaveSlotEntry Damaged(int number) => new(number, null, isDamaged: true);

    public static SaveSlotEntry Saved(int number, SaveSummary summary) => new(number, summary, isDamaged: false);
}
