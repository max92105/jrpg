namespace Jrpg.Game.Ui;

/// <summary>
/// A menu page list's margins and row height, in one place (placeholders, decision 82): the values every page list uses
/// today. Only the Quests page reads it yet; the other lists switch when next touched (the backlog's line).
/// </summary>
internal static class PageListLayout
{
    /// <summary>The list's gap to its column's left and right edges.</summary>
    public const int Margin = 28;

    /// <summary>The list's gap to its column's top.</summary>
    public const int TopMargin = 20;

    /// <summary>One row's height.</summary>
    public const int RowHeight = 52;
}
